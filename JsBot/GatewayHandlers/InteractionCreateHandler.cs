using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace JsBot.GatewayHandlers;

public class InteractionCreateHandler : IInteractionCreateGatewayHandler {
	public async ValueTask HandleAsync(Interaction interaction) {
		if(interaction is not ButtonInteraction button) return;
		string[] parts = button.Data.CustomId.Split('-');
		try {
			switch(parts[0]) {
				case "progress":
					await ModAnnounce.ProgressButton(parts, button);
					break;
				case "release":
					await ModAnnounce.ReleaseButton(parts, button);
					break;
				case "beta":
					await ModAnnounce.BetaButton(parts, button);
					break;
				case "apply":
				{
					JMod? mod = JMod.GetMod(int.Parse(parts[1]));
					if(mod == null) {
						await button.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드를 찾을 수 없습니다.").WithFlags(MessageFlags.Ephemeral)));
					} else {
						await button.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties()
							.WithContent("모드 적용 방식이 변경되어 아래 버튼을 눌러 링크로 이동하셔야 됩니다.")
							.WithComponents([
								new ActionRowProperties([
									new LinkButtonProperties("https://jalib.jongyeol.kr/modApplicator/" + mod.Name + "/" + parts[2], "링크로 이동(서버 1)"),
									new LinkButtonProperties("https://jalib2.jongyeol.kr/modApplicator/" + mod.Name + "/" + parts[2], "링크로 이동(서버 2)")
								])
							])
							.WithFlags(MessageFlags.Ephemeral)));
						new LogBuilder(button.User, "모드 적용 요청 링크를 보냈습니다.")
							.AddField("mod", mod.Name)
							.AddBlankField(true)
							.AddField("version", parts[2])
							.AddField("user", button.User)
							.AddBlankField(true)
							.AddField("channel", button.Channel)
							.Send();
					}
					break;
				}
			}
		} catch (Exception e) {
			await button.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties()
				.WithEmbeds([new EmbedProperties().WithTitle("오류가 발생하였습니다.").WithDescription(e.Message).WithColor(new Color(0xFF0000))])
				.WithFlags(MessageFlags.Ephemeral)));
			LogBuilder.NewError(button.User)
				.AddField("EventType", "On Button Interaction")
				.AddField("Channel", button.Channel)
				.AddField("Custom Id", button.Data.CustomId)
				.AddField(e)
				.Send();
		}
	}
}