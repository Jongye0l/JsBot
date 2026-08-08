using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace JsBot.GatewayHandlers;

public class GuildUserAddHandler : IGuildUserAddGatewayHandler {
	public async ValueTask HandleAsync(GuildUser user) {
		if(user.GuildId != JSettings.Instance.GuildId) return;
		EmbedProperties embed = new EmbedProperties()
			.WithAuthor(new EmbedAuthorProperties().WithName(Utility.GetUserName(user)).WithIconUrl(Utility.GetAvatar(user)))
			.WithDescription(Utility.GetUserMention(user.Id) + " 님이 서버에 들어오셨습니다.")
			.WithColor(new Color(0x00FF00));
		_ = DiscordBot.Rest.SendMessageAsync(JSettings.Instance.JoinLogChannel, new MessageProperties().WithEmbeds([embed]));

		ulong[] roles = [.. JSettings.Instance.Roles, user.IsBot ? JSettings.Instance.BotRole : JSettings.Instance.UserRole];
		await DiscordBot.Rest.ModifyGuildUserAsync(JSettings.Instance.GuildId, user.Id, options => options.RoleIds = roles);
	}
}