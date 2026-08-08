using NetCord;
using NetCord.Rest;

namespace JsBot;

public class LogBuilder {
	private const string NullMessage = "null";

	public static LogBuilder NewError(User user) => new(user, "오류가 발생하였습니다.", true);
	public static LogBuilder NewError() => NewError(DiscordBot.Client.Cache.User!);

	private bool _inline = true;
	private readonly bool _error;
	private readonly EmbedProperties _embed;

	public LogBuilder(User user, string message) : this(user, message, false) {
	}

	public LogBuilder(string message) : this(DiscordBot.Client.Cache.User!, message, false) {
	}

	private LogBuilder(User user, string message, bool error) {
		_error = error;
		string name = Utility.GetUserName(user);
		string avatar = Utility.GetAvatar(user);
		_embed = new EmbedProperties()
			.WithAuthor(new EmbedAuthorProperties().WithName(name).WithIconUrl(avatar))
			.WithColor(new Color(error ? 0xFF0000 : 0x00FF00))
			.WithDescription(message)
			.WithFooter(new EmbedFooterProperties().WithText(name).WithIconUrl(avatar))
			.WithTimestamp(DateTimeOffset.UtcNow);
	}

	public LogBuilder SetDefaultInline(bool inline) {
		_inline = inline;
		return this;
	}

	public LogBuilder AddField(string name, string? value) => AddField(name, value, _inline);

	public LogBuilder AddField(string name, string? value, bool inline) {
		_embed.AddFields(new EmbedFieldProperties().WithName(name).WithValue(value ?? NullMessage).WithInline(inline));
		return this;
	}

	public LogBuilder AddField(string name, object? value) => AddField(name, value, _inline);
	public LogBuilder AddField(string name, object? value, bool inline) => AddField(name, value?.ToString(), inline);

	public LogBuilder AddField(string name, bool value) => AddField(name, value, _inline);
	public LogBuilder AddField(string name, bool value, bool inline) => AddField(name, value ? "true" : "false", inline);

	public LogBuilder AddField(string name, int value) => AddField(name, value, _inline);
	public LogBuilder AddField(string name, int value, bool inline) => AddField(name, value.ToString(), inline);

	public LogBuilder AddField(string name, ulong value) => AddField(name, value, _inline);
	public LogBuilder AddField(string name, ulong value, bool inline) => AddField(name, value.ToString(), inline);

	public LogBuilder AddField(string name, User user) => AddField(name, user, _inline);
	public LogBuilder AddField(string name, User user, bool inline) => AddField(name, Utility.GetUserMention(user.Id) + "(" + Utility.GetUserName(user) + ")", inline);

	public LogBuilder AddField(string name, TextChannel channel) => AddField(name, channel, _inline);
	public LogBuilder AddField(string name, TextChannel channel, bool inline) => AddField(name, Utility.GetChannelMention(channel.Id), inline);

	public LogBuilder AddField(string name, RestMessage message) => AddField(name, message, _inline);
	public LogBuilder AddField(string name, RestMessage message, bool inline) =>
		AddField(name, "https://discord.com/channels/" + JSettings.Instance.GuildId + "/" + message.ChannelId + "/" + message.Id, inline);

	public LogBuilder AddField(string name, ButtonProperties button) => AddField(name, button, _inline);
	public LogBuilder AddField(string name, ButtonProperties button, bool inline) => AddField(name, button.Label + "(" + button.CustomId + ")", inline);

	public LogBuilder AddField(string name, Role role) => AddField(name, role, _inline);
	public LogBuilder AddField(string name, Role role, bool inline) => AddField(name, Utility.GetRoleMention(role.Id) + "(" + role.Name + ")", inline);

	public LogBuilder AddField(Exception e) {
		List<string> traces = [];
		System.Text.StringBuilder trace = new();
		traces.Add("");
		foreach(string line in (e.StackTrace ?? "").Split('\n')) {
			if(trace.Length + line.Length > 1024) {
				traces[^1] = trace.ToString();
				trace.Clear();
				traces.Add("");
			}
			trace.Append(line).Append('\n');
		}
		traces[^1] = trace.ToString();
		_embed.AddFields(new EmbedFieldProperties().WithName("Exception Type").WithValue(e.GetType().FullName ?? e.GetType().Name).WithInline(_inline));
		_embed.AddFields(new EmbedFieldProperties().WithName("Exception Message").WithValue(e.Message).WithInline(_inline));
		foreach(string tracer in traces) _embed.AddFields(new EmbedFieldProperties().WithName("Exception Stack Trace").WithValue(tracer.Length == 0 ? NullMessage : tracer).WithInline(_inline));
		return this;
	}

	public LogBuilder AddBlankField() => AddBlankField(false);

	public LogBuilder AddBlankField(bool inline) {
		_embed.AddFields(new EmbedFieldProperties().WithName("").WithValue("").WithInline(inline));
		return this;
	}

	public void Send() {
		MessageProperties message = new MessageProperties().WithEmbeds([_embed]);
		if(_error) message.Content = "<@447333460382842880>";
		_ = DiscordBot.Rest.SendMessageAsync(JSettings.Instance.LogChannel, message);
	}
}