using JsBot.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace JsBot;

public static class DiscordBot {
	public const string SampleUrl = "https://jongyeol.kr/";

	public static GatewayClient Client = null!;
	public static RestClient Rest => Client.Rest;

	public static void Main(string[] args) {
		JSettings.SetInstance();
		JMod.SetFactory();

		JALib.Server.Program.ConfigureBuilder = ConfigureBuilder;
		JALib.Server.Program.OnReady = OnReady;

		JALib.Server.Program.Main(args);
	}

	private static void ConfigureBuilder(WebApplicationBuilder builder) {
		builder.Services.AddDiscordGateway(options => {
			options.Token = JSettings.Instance.Token;
			options.Intents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildUsers | GatewayIntents.MessageContent;
		});
		builder.Services.AddApplicationCommands();
		builder.Services.AddGatewayHandlers(typeof(DiscordBot).Assembly);
	}

	private static void OnReady(WebApplication app) {
		JMod.AfterLoad();
		GatewayClient client = app.Services.GetRequiredService<GatewayClient>();
		Client = client;

		app.AddApplicationCommandModule<AddModCommand>();
		app.AddApplicationCommandModule<AnnounceModCommand>();
		app.AddApplicationCommandModule<EditModDataCommand>();
		app.AddApplicationCommandModule<ModCommand>();
		app.AddApplicationCommandModule<RemoveModCommand>();
		app.AddApplicationCommandModule<SendAnnounceCommand>();
		app.AddApplicationCommandModule<TimeoutCommand>();

		client.Ready += async _ => {
			ApplicationCommandService<ApplicationCommandContext, AutocompleteInteractionContext> commandService =
				app.Services.GetRequiredService<ApplicationCommandService<ApplicationCommandContext, AutocompleteInteractionContext>>();
			await commandService.RegisterCommandsAsync(client.Rest, client.Id);
		};
	}
}