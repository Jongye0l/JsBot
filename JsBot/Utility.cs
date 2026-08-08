using NetCord;

namespace JsBot;

public static class Utility {
	public static string GetUserName(User user) => user.Username;

	public static string GetAvatar(User user) => (user.HasAvatar ? user.GetAvatarUrl() : user.DefaultAvatarUrl)!.ToString();

	public static string GetChannelMention(ulong id) => "<#" + id + ">";
	public static string GetUserMention(ulong id) => "<@" + id + ">";
	public static string GetRoleMention(ulong id) => "<@&" + id + ">";
}