using JALib.Server.Models.DL;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace JsBot.Commands;

public enum DownloadLinkType {
	Github,
	Custom
}

[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
public class AddModCommand : ApplicationCommandModule<ApplicationCommandContext> {
	[SlashCommand("addmod", "모드를 생성합니다.")]
	public async Task Handle(
		[SlashCommandParameter(Description = "이름")]
		string name,
		[SlashCommandParameter(Description = "링크")]
		DownloadLinkType link) {
		JMod mod = await JMod.CreateAsync(name, link == DownloadLinkType.Github ? new GithubDownloadLink(name) : new CustomDownloadLink());
		new LogBuilder(Context.User, "모드를 생성했습니다")
			.AddField("user", Context.User)
			.AddBlankField()
			.AddField("channel", Context.Channel)
			.AddField("id", mod.Id)
			.AddBlankField()
			.AddField("name", name)
			.AddField("modChannel", await mod.GetChannel())
			.AddBlankField()
			.AddField("releaseRole", mod.ReleasePing)
			.AddField("progressRole", mod.ProgressPing)
			.AddField("releaseBetaRole", mod.BetaReleasePing)
			.Send();
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(name + " 모드를 생성했습니다.").WithFlags(MessageFlags.Ephemeral)));
	}
}