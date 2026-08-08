using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace JsBot.Commands;

[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
public class AnnounceModCommand : ApplicationCommandModule<ApplicationCommandContext> {
	[SlashCommand("announcemod", "모드 기본사항을 전송합니다.")]
	public async Task Handle([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(AllJModNameAutocompleteProvider))] string name) {
		JMod? mod = JMod.GetMod(name);
		if(mod == null) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
			return;
		}
		await mod.Announce();
		new LogBuilder(Context.User, "모드 기본사항을 전송했습니다")
			.AddField("user", Context.User)
			.AddField("channel", Context.Channel)
			.AddBlankField()
			.AddField("id", mod.Id)
			.AddField("name", name)
			.AddField("modChannel", await mod.GetChannel())
			.AddBlankField()
			.AddField("latestVersion", mod.Version.ToString())
			.AddField("latestBetaVersion", mod.BetaVersion.ToString())
			.Send();
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(name + " 모드 기본사항을 전송했습니다.").WithFlags(MessageFlags.Ephemeral)));
	}
}