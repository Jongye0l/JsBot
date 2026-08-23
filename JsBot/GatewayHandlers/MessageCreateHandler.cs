using System.Text;
using System.Text.RegularExpressions;
using JALib.Server;
using JALib.Server.Models;
using JALib.Server.Models.DL;
using JsBot.Data;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using Serilog;

namespace JsBot.GatewayHandlers;

public partial class MessageCreateHandler : IMessageCreateGatewayHandler {
	private const string MagicMessagePrefix = "\n-# ||";
	private const string MagicMessageSuffix = "||";

	[GeneratedRegex(@"<(\d+)>")]
	private static partial Regex MentionPattern();

	public async ValueTask HandleAsync(Message message) {
		try {
			Log.Logger.Debug("Message received: {Message} from {Author} in {Channel}", message.Content, message.Author, message.Channel);
			await CheckModMessage(message);
			await CheckPingMessage(message);
		} catch (Exception e) {
			LogBuilder.NewError(message.Author)
				.AddField("EventType", "On Message Received")
				.AddField("Channel", message.Channel!)
				.AddField("Message", message.Content)
				.AddField("Author", message.Author)
				.AddField(e)
				.Send();
		}
	}

	private static async Task CheckModMessage(Message message) {
		if(message.Content.StartsWith('!')) return;
		if(message.Author.Id == DiscordBot.Client.Id) return;
		ulong channelId = message.ChannelId;
		JMod? mod = JMod.GetMod(channelId);
		if(mod != null) {
			await mod.Announce();
			return;
		}
		foreach(JMod other in JMod.GetModList()) {
			foreach(RawChannel rawChannel in other.AdditionalChannels) {
				if(rawChannel.Channel == channelId) {
					await other.Announce(rawChannel);
					break;
				}
			}
		}
	}

	private static async Task CheckPingMessage(Message message) {
		if(message.WebhookId != null || !message.Content.StartsWith('!') || message.GuildId != JSettings.Instance.GuildId) return;
		Log.Logger.Information("Ping message received: {Message} from {Author} in {Channel}", message.Content, message.Author, message.Channel);
		GuildUser? member = message.Author as GuildUser;
		if(member == null || !member.RoleIds.Contains(1287366163667357829UL)) return;
		Log.Logger.Information("Ping message from authorized user: {User}", member);
		string[] lines = message.Content.Split('\n');
		string[] data = lines[0].Split(' ');
		Log.Logger.Information("Ping message command: {Command}, arguments: {Arguments}", data[0], data.Skip(1).ToArray());
		switch(data[0]) {
			case "!release" or "!beta":
				await HandleRelease(message, lines, data);
				break;
			case "!progress":
				await HandleProgress(message, lines);
				break;
			default:
				await HandlePing(message, lines, data);
				break;
		}
	}

	private static async Task HandleRelease(Message message, string[] lines, string[] data) {
		bool beta = data[0] == "!beta";
		JMod? mod = JMod.GetMod(message.ChannelId);
		if(mod == null) return;

		VersionStruct version = new(data[1]);
		VersionStruct showVersion = version;
		showVersion.Revision = -1;

		string behind = beta ? " beta" + data[2] : "";
		VersionStruct latestVersion = beta ? mod.BetaVersion : mod.Version;

		if(!latestVersion.IsNull() && version.IsUpper(latestVersion)) return;
		string? link = mod.DiscordDl;
		DownloadLink downloadLink = mod.DownloadLink;

		if(link == null) {
			if(mod.BetaLinkable && beta) link = "";
			else if(downloadLink is GithubDownloadLink githubLink) link = githubLink.GetLink(version);
			else if(downloadLink is CustomDownloadLink customLink) {
				if(!customLink.Links.ContainsKey(version) || data.Length > (beta ? 3 : 2)) {
					link = data[beta ? 3 : 2];
					customLink.Links[version] = link;
					ConnectOtherLib.SetDownloadLink(mod, downloadLink);
				} else link = "";
			} else link = "";
		}

		StringBuilder builder = new($"# {mod.Name} {showVersion}{behind}\n");
		for(int i = 1; i < lines.Length; i++) builder.Append('\n').Append(CustomMessage(lines[i]));

		string text = builder.ToString();
		builder.Append(MagicMessagePrefix).Append(beta ? "" : mod.ReleasePing).Append(mod.BetaReleasePing)
			.Append(beta ? "" : Pings.AllReleasePing).Append(Pings.AllBetaReleasePing);

		if(!beta && latestVersion.IsNull()) builder.Append(Pings.NewReleasePing);
		builder.Append(MagicMessageSuffix);
		bool source = downloadLink is GithubDownloadLink;

		MessageProperties messageProperties = new MessageProperties().WithContent(builder.ToString()).WithFlags(MessageFlags.SuppressEmbeds);
		LinkButtonProperties sourceButton = new(source ? ((GithubDownloadLink) downloadLink).GetSourceLink(version) : DiscordBot.SampleUrl, "소스 코드") { Disabled = !source };
		LinkButtonProperties downloadButton = new(link.Length == 0 ? DiscordBot.SampleUrl : link, "다운로드") { Disabled = link.Length == 0 };
		LinkButtonProperties applyButton1 = new("https://jalib.jongyeol.kr/modApplicator/" + mod.Name + "/" + version, "모드 적용(서버 1)");
		LinkButtonProperties applyButton2 = new("https://jalib2.jongyeol.kr/modApplicator/" + mod.Name + "/" + version, "모드 적용(서버 2)");
		messageProperties.Components = [new ActionRowProperties([sourceButton, downloadButton, applyButton1, applyButton2])];

		if(version.Equals(latestVersion)) {
			await DiscordBot.Rest.ModifyMessageAsync(message.ChannelId, mod.GetLastReleaseId(beta), o => {
				o.Content = messageProperties.Content;
				o.Components = messageProperties.Components;
				o.Flags = MessageFlags.SuppressEmbeds;
			});
			await message.DeleteAsync();
			await SendReleaseToAdditionalChannels(beta, mod, text, true, version, sourceButton, downloadButton, applyButton1, applyButton2);
			new LogBuilder(message.Author, "모드 업로드를 수정하였습니다.")
				.AddField("name", mod.Name)
				.AddField("id", mod.Id)
				.AddField("user", message.Author)
				.AddField("latestVersion", latestVersion.ToString())
				.AddField("version", version.ToString())
				.AddField("link", link)
				.AddField("attachment size", message.Attachments.Count)
				.Send();
			return;
		}

		RestMessage sent = await DiscordBot.Rest.SendMessageAsync(message.ChannelId, messageProperties);
		await DiscordBot.Rest.ModifyGuildChannelAsync(message.ChannelId, o => o.Topic = $":white_check_mark: 최신버전 : [{showVersion}]({link})");
		await message.DeleteAsync();
		if(beta) mod.SetLatestBetaVersion(version);
		else mod.SetLatestVersion(version);
		mod.AddRelease(version, sent.Id);
		await SendReleaseToAdditionalChannels(beta, mod, text, false, version, sourceButton, downloadButton, applyButton1, applyButton2);
		await mod.Announce();
		new LogBuilder(message.Author, "모드를 업로드 하였습니다.")
			.AddField("name", mod.Name)
			.AddField("id", mod.Id)
			.AddField("user", message.Author)
			.AddField("latestVersion", latestVersion.ToString())
			.AddField("version", version.ToString())
			.AddField("link", link)
			.AddField("attachment size", message.Attachments.Count)
			.Send();
		mod.Save();
	}

	private static Task SendReleaseToAdditionalChannels(bool beta, JMod mod, string text, bool edit, VersionStruct version,
		LinkButtonProperties sourceButton, LinkButtonProperties downloadButton, LinkButtonProperties applyButton1, LinkButtonProperties applyButton2) {
		foreach(RawChannel channel in mod.AdditionalChannels) {
			if(!channel.Beta && beta) continue;
			_ = Task.Run(async () => {
				try {
					StringBuilder builder = new(text);
					ModRoles? roles = mod.GetRoles(channel.Guild);
					bool magic = false;
					if(roles != null) {
						if(roles.ReleaseRole != -1 && !beta) {
							builder.Append(MagicMessagePrefix).Append(roles.ReleasePing);
							magic = true;
						}
						if(roles.BetaReleaseRole != -1 && channel.Beta) {
							if(!magic) {
								builder.Append(MagicMessagePrefix);
								magic = true;
							}
							builder.Append(roles.BetaReleasePing);
						}
						if(magic) builder.Append(MagicMessageSuffix);
					}
					MessageProperties message1 = new MessageProperties().WithContent(builder.ToString()).WithFlags(MessageFlags.SuppressEmbeds);
					message1.Components = channel.Apply ? [new ActionRowProperties([sourceButton, downloadButton, applyButton1, applyButton2])] : [new ActionRowProperties([sourceButton, downloadButton])];
					if(edit && channel.ReleaseMessage.TryGetValue(version, out long messageId)) {
						await DiscordBot.Rest.ModifyMessageAsync(channel.Channel, (ulong) messageId, o => {
							o.Content = message1.Content;
							o.Components = message1.Components;
						});
						await mod.Announce(channel);
						return;
					}
					RestMessage sent = await DiscordBot.Rest.SendMessageAsync(channel.Channel, message1);
					channel.ReleaseMessage[version] = (long) sent.Id;
					await mod.Announce(channel);
					mod.Save();
				} catch (Exception e) {
					Log.Logger.Error(e, "Failed to send release message for mod {ModName} version {Version} to additional channel {ChannelId} in guild {GuildId}", mod.Name, version, channel.Channel, channel.Guild);
				}
			});
		}
		return Task.CompletedTask;
	}

	private static async Task HandleProgress(Message message, string[] lines) {
		JMod? mod = JMod.GetMod(message.ChannelId);
		if(mod == null) return;
		StringBuilder builder = new();
		for(int i = 1; i < lines.Length; i++) builder.Append(CustomMessage(lines[i])).Append('\n');
		if(builder.Length > 0) builder.Length -= 1;
		string text = builder.ToString();
		builder.Append(MagicMessagePrefix).Append(mod.ProgressPing).Append(Pings.AllProgressPing).Append(MagicMessageSuffix);
		await DiscordBot.Rest.SendMessageAsync(message.ChannelId, builder.ToString());
		await message.DeleteAsync();
		foreach(RawChannel channel in mod.AdditionalChannels) {
			StringBuilder builder1 = new(text);
			ModRoles? roles = mod.GetRoles(channel.Guild);
			if(roles != null && roles.ProgressRole != -1) builder1.Append(MagicMessagePrefix).Append(roles.ProgressPing).Append(MagicMessageSuffix);
			_ = DiscordBot.Rest.SendMessageAsync(channel.Channel, builder1.ToString());
		}
		await mod.Announce();
		new LogBuilder(message.Author, "모드 근황을 업로드하였습니다.")
			.AddField("name", mod.Name)
			.AddField("id", mod.Id)
			.AddField("user", message.Author)
			.AddField("attachment size", message.Attachments.Count)
			.Send();
	}

	private static async Task HandlePing(Message message, string[] lines, string[] data) {
		if(message.GuildId != JSettings.Instance.GuildId) return;
		List<string> pingList = [];
		foreach(string st in data) {
			switch(st) {
				case "!everyone":
					pingList.Add("@everyone");
					break;
				case "!here":
					pingList.Add("@here");
					break;
				case "!announce":
					pingList.Add(Pings.AnnouncePing);
					break;
				case "!simsim":
					pingList.Add(Pings.SimsimPing);
					break;
			}
		}
		if(pingList.Count == 0) return;
		StringBuilder builder = new();
		for(int i = 1; i < lines.Length; i++) builder.Append('\n').Append(lines[i]);
		builder.Append(MagicMessagePrefix);
		foreach(string st in pingList) builder.Append(st);
		builder.Append(MagicMessageSuffix);
		await DiscordBot.Rest.SendMessageAsync(message.ChannelId, builder.ToString());
		await message.DeleteAsync();
		LogBuilder logBuilder = new LogBuilder(message.Author, "맨션을 하였습니다.")
			.AddField("user", message.Author)
			.AddField("attachment size", message.Attachments.Count);
		foreach(string st in pingList) logBuilder.AddField("ping", st);
		logBuilder.Send();
	}

	private static string CustomMessage(string message) {
		return MentionPattern().Replace(message, m => "<@" + m.Groups[1].Value + ">");
	}
}

file static class Pings {
	public static string AllReleasePing => Utility.GetRoleMention(JSettings.Instance.AllReleaseRole);
	public static string AllProgressPing => Utility.GetRoleMention(JSettings.Instance.AllProgressRole);
	public static string AllBetaReleasePing => Utility.GetRoleMention(JSettings.Instance.AllBetaReleaseRole);
	public static string NewReleasePing => Utility.GetRoleMention(JSettings.Instance.NewReleaseRole);
	public static string AnnouncePing => Utility.GetRoleMention(JSettings.Instance.AnnounceRole);
	public static string SimsimPing => Utility.GetRoleMention(JSettings.Instance.SimsimRole);
}