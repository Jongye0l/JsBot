using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace JsBot.Commands;

[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
public class SendAnnounceCommand : ApplicationCommandModule<ApplicationCommandContext> {
	[SlashCommand("sendmodannounce", "핑 알림을 전송합니다.")]
	public async Task Handle() {
		MessageProperties message = new MessageProperties().WithContent("# 원하시는 알림을 선택해주세요").WithComponents([
			new ActionRowProperties([
				new ButtonProperties("release-all", "전체 모드핑", ButtonStyle.Primary),
				new ButtonProperties("release-new", "신규 모드핑", ButtonStyle.Primary),
				new ButtonProperties("progress-all", "전체 근황핑", ButtonStyle.Primary),
				new ButtonProperties("beta-all", "전체 베타핑", ButtonStyle.Primary)
			])
		]);
		await DiscordBot.Rest.SendMessageAsync(Context.Channel.Id, message);
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("알림 메시지를 전송했습니다.").WithFlags(MessageFlags.Ephemeral)));
		new LogBuilder(Context.User, "알림 메시지를 전송했습니다")
			.AddField("user", Context.User)
			.AddField("channel", Context.Channel)
			.Send();
	}
}