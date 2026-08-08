using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace JsBot.GatewayHandlers;

public class GuildUserRemoveHandler : IGuildUserRemoveGatewayHandler {
	public ValueTask HandleAsync(GuildUserRemoveEventArgs args) {
		if(args.GuildId != JSettings.Instance.GuildId) return ValueTask.CompletedTask;
		EmbedProperties embed = new EmbedProperties()
			.WithAuthor(new EmbedAuthorProperties().WithName(Utility.GetUserName(args.User)).WithIconUrl(Utility.GetAvatar(args.User)))
			.WithDescription(Utility.GetUserMention(args.User.Id) + " 님이 서버를 떠나셨습니다.")
			.WithColor(new Color(0xFF0000));
		_ = DiscordBot.Rest.SendMessageAsync(JSettings.Instance.JoinLogChannel, new MessageProperties().WithEmbeds([embed]));
		return ValueTask.CompletedTask;
	}
}