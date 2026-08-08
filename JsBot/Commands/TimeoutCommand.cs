using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace JsBot.Commands;

public enum TimeUnit {
	Millisecond,
	Second,
	Minute,
	Hour,
	Day,
	Week,
	Month
}

file static class TimeUnitExtensions {
	public static long Multiplier(this TimeUnit unit) => unit switch {
		TimeUnit.Millisecond => 1,
		TimeUnit.Second => 1000,
		TimeUnit.Minute => 60000,
		TimeUnit.Hour => 3600000,
		TimeUnit.Day => 86400000,
		TimeUnit.Week => 604800000,
		TimeUnit.Month => 2592000000,
		_ => throw new ArgumentOutOfRangeException(nameof(unit))
	};
}

[RequireUserPermissions<ApplicationCommandContext>(Permissions.ModerateUsers)]
public class TimeoutCommand : ApplicationCommandModule<ApplicationCommandContext> {
	[SlashCommand("timeout", "타임아웃을 설정합니다.")]
	public async Task Handle(
		[SlashCommandParameter(Description = "유저")]
		GuildUser user,
		[SlashCommandParameter(Description = "시간")]
		int time,
		[SlashCommandParameter(Description = "시간단위(기본: 일)")]
		TimeUnit timeunit = TimeUnit.Day) {
		try {
			DateTimeOffset until = DateTimeOffset.UtcNow.AddMilliseconds(time * timeunit.Multiplier());
			await user.TimeOutAsync(until);
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("해당 유저에게 <t:" + until.ToUnixTimeSeconds() + ":R>까지 타임아웃을 설정했습니다.").WithFlags(MessageFlags.Ephemeral)));
		} catch (Exception e) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("오류가 발생했습니다: " + e.Message).WithFlags(MessageFlags.Ephemeral)));
			LogBuilder.NewError(Context.User)
				.AddField("EventType", "On Slash Command Interaction")
				.AddField("Command", "timeout")
				.AddField("Channel", Context.Channel)
				.AddField(e)
				.Send();
		}
	}
}