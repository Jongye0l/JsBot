using System.Collections.Concurrent;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace JsBot.GatewayHandlers;

public class GuildUserUpdateHandler : IGuildUserUpdateGatewayHandler {
	private static readonly ConcurrentDictionary<ulong, DateTimeOffset> BoostMap = new();

	public ValueTask HandleAsync(GuildUser user) {
		if(user.GuildId != JSettings.Instance.GuildId) return ValueTask.CompletedTask;
		if(user.GuildBoostStart is not {} boostStart) return ValueTask.CompletedTask;
		if(BoostMap.TryGetValue(user.Id, out DateTimeOffset existing) && existing == boostStart) return ValueTask.CompletedTask;
		BoostMap[user.Id] = boostStart;
		EmbedProperties embed = new EmbedProperties()
			.WithAuthor(new EmbedAuthorProperties().WithName(Utility.GetUserName(user)).WithIconUrl(Utility.GetAvatar(user)))
			.WithDescription(Utility.GetUserMention(user.Id) + " 님이 서버 부스트를 해주셨습니다!");
		_ = DiscordBot.Rest.SendMessageAsync(JSettings.Instance.BoostLogChannel, new MessageProperties().WithEmbeds([embed]));
		return ValueTask.CompletedTask;
	}
}