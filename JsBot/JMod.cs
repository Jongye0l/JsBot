using System.Text.Json;
using JALib.Server;
using JALib.Server.Models;
using JALib.Server.Models.DL;
using JsBot.Data;
using NetCord;
using NetCord.Rest;
using Serilog;

namespace JsBot;

public class JMod : ModData {
	private static readonly List<JMod> ModList = [];
	private static int _lastId = -1;

	public static JMod[] GetModList() => ModList.ToArray();
	public static JMod? GetMod(string name) => GetModData(name) as JMod;
	public static JMod? GetMod(int id) => ModList.FirstOrDefault(mod => mod.Id == id);
	public static JMod? GetMod(ulong channel) => ModList.FirstOrDefault(mod => mod.Channel == channel);

	public static void SetFactory() {
		Factory = () => new JMod();
		Log.Logger.Information("JMod factory set");
	}

	public static void AfterLoad() {
		_lastId = ModList.Count == 0 ? -1 : ModList.Max(mod => mod.Id);
	}

	public int Id;
	public ulong Channel;
	public ModRoles Roles = new();
	public long LastAnnounce = -1;
	public bool BetaLinkable;
	public bool Deleted;
	public bool PrivateMod;
	public string? DiscordDl;
	public readonly Dictionary<VersionStruct, ulong> ReleaseMessage = new();
	public readonly List<RawChannel> AdditionalChannels = [];
	public readonly Dictionary<ulong, ModRoles> AdditionalRoles = new();

	public JMod() {
		ModList.Add(this);
	}

	public static async Task<JMod> CreateAsync(string name, DownloadLink link) {
		JMod mod = new() {
			Name = name,
			DownloadLink = link,
			Id = ++_lastId
		};
		ulong guildId = JSettings.Instance.GuildId;
		Role releaseRole = await DiscordBot.Rest.CreateGuildRoleAsync(guildId, new RoleProperties { Name = name });
		Role progressRole = await DiscordBot.Rest.CreateGuildRoleAsync(guildId, new RoleProperties { Name = name + " 근황" });
		Role betaRole = await DiscordBot.Rest.CreateGuildRoleAsync(guildId, new RoleProperties { Name = name + " Beta" });
		mod.Roles = new ModRoles { ReleaseRole = (long) releaseRole.Id, ProgressRole = (long) progressRole.Id, BetaReleaseRole = (long) betaRole.Id };
		IGuildChannel channel = await DiscordBot.Rest.CreateGuildChannelAsync(guildId, new GuildChannelProperties("📟ㅣ" + name, ChannelType.TextGuildChannel) {
			Topic = ":negative_squared_cross_mark: 아직 모드가 업로드 되지 않았습니다",
			ParentId = JSettings.Instance.ModCategory
		});
		mod.Channel = channel.Id;
		mod.Save();
		await mod.Announce();
		return mod;
	}

	public Task<TextChannel> GetChannel() => DiscordBot.Rest.GetChannelAsync(Channel).ContinueWith(t => (TextChannel) t.Result);

	public string ReleasePing => "<@&" + Roles.ReleaseRole + ">";
	public string ProgressPing => "<@&" + Roles.ProgressRole + ">";
	public string BetaReleasePing => "<@&" + Roles.BetaReleaseRole + ">";

	public string LastReleaseUrl(bool beta) =>
		$"https://discord.com/channels/{JSettings.Instance.GuildId}/{Channel}/{ReleaseMessage[beta ? BetaVersion : Version]}";

	public ulong GetLastReleaseId(bool beta) => ReleaseMessage[beta ? BetaVersion : Version];

	public void AddRelease(VersionStruct version, ulong id) => ReleaseMessage[version] = id;

	public async Task Announce() {
		MessageProperties message = new MessageProperties().WithContent(" ").WithComponents([
			new ActionRowProperties([
				new LinkButtonProperties(!Version.IsNull() ? LastReleaseUrl(false) : DiscordBot.SampleUrl, "최신버전으로 이동") { Disabled = Version.IsNull() },
				new LinkButtonProperties(!BetaVersion.IsNull() ? LastReleaseUrl(true) : DiscordBot.SampleUrl, "최신버전으로 이동(베타)") { Disabled = BetaVersion.IsNull() },
				new ButtonProperties("release-" + Id, "업로드 핑 받기", ButtonStyle.Primary),
				new ButtonProperties("progress-" + Id, "근황 핑 받기", ButtonStyle.Primary),
				new ButtonProperties("beta-" + Id, "베타 핑 받기", ButtonStyle.Primary)
			])
		]);
		RestMessage sent = await DiscordBot.Rest.SendMessageAsync(Channel, message);
		long last = LastAnnounce;
		LastAnnounce = (long) sent.Id;
		Save();
		if(last != -1) {
			try {
				await DiscordBot.Rest.DeleteMessageAsync(Channel, (ulong) last);
			} catch {
				// message already gone
			}
		}
	}

	public async Task Announce(RawChannel rawChannel) {
		ModRoles? roles = GetRoles(rawChannel.Guild);
		List<IActionRowComponentProperties> components = [];
		string url = !Version.IsNull() && rawChannel.ReleaseMessage.ContainsKey(Version) ? rawChannel.GetMessageUrl(Version) : DiscordBot.SampleUrl;
		components.Add(new LinkButtonProperties(url, "최신버전으로 이동") { Disabled = url == DiscordBot.SampleUrl });
		if(rawChannel.Beta) {
			url = !BetaVersion.IsNull() && rawChannel.ReleaseMessage.ContainsKey(BetaVersion) ? rawChannel.GetMessageUrl(BetaVersion) : DiscordBot.SampleUrl;
			components.Add(new LinkButtonProperties(url, "최신버전으로 이동(베타)") { Disabled = url == DiscordBot.SampleUrl });
		}
		if(roles != null) {
			if(roles.ReleaseRole != -1) components.Add(new ButtonProperties("release-" + Id, "업로드 핑 받기", ButtonStyle.Primary));
			if(roles.ProgressRole != -1) components.Add(new ButtonProperties("progress-" + Id, "근황 핑 받기", ButtonStyle.Primary));
			if(roles.BetaReleaseRole != -1) components.Add(new ButtonProperties("beta-" + Id, "베타 핑 받기", ButtonStyle.Primary));
		}
		MessageProperties message = new MessageProperties().WithContent(" ").WithComponents([new ActionRowProperties(components)]);
		RestMessage sent = await DiscordBot.Rest.SendMessageAsync(rawChannel.Channel, message);
		long last = rawChannel.LastAnnounce;
		rawChannel.LastAnnounce = (long) sent.Id;
		Save();
		if(last != -1) {
			try {
				await DiscordBot.Rest.DeleteMessageAsync(rawChannel.Channel, (ulong) last);
			} catch {
				// message already gone
			}
		}
	}

	public async Task Delete() {
		ulong guildId = JSettings.Instance.GuildId;
		if(Roles.ReleaseRole != -1) await DiscordBot.Rest.DeleteGuildRoleAsync(guildId, (ulong) Roles.ReleaseRole);
		if(Roles.ProgressRole != -1) await DiscordBot.Rest.DeleteGuildRoleAsync(guildId, (ulong) Roles.ProgressRole);
		if(Roles.BetaReleaseRole != -1) await DiscordBot.Rest.DeleteGuildRoleAsync(guildId, (ulong) Roles.BetaReleaseRole);
		await DiscordBot.Rest.SendMessageAsync(Channel, "# 이 채널은 아카이브 되었습니다");
		await DiscordBot.Rest.ModifyGuildChannelAsync(Channel, o => o.Topic = ":no_entry_sign: 이 모드는 아카이브 되었습니다.");
		if(LastAnnounce != -1) {
			try {
				await DiscordBot.Rest.DeleteMessageAsync(Channel, (ulong) LastAnnounce);
			} catch {
				// message already gone
			}
		}
		Deleted = true;
		Save();
	}

	public void SetLatestVersion(VersionStruct version) {
		ConnectOtherLib.SetVersion(this, version);
		Version = version;
		if(BetaVersion.IsNull() || version.IsUpper(BetaVersion)) SetLatestBetaVersion(version);
	}

	public void SetLatestBetaVersion(VersionStruct betaVersion) {
		ConnectOtherLib.SetBetaVersion(this, betaVersion);
		BetaVersion = betaVersion;
	}

	public ModRoles? GetRoles(ulong guild) => guild == JSettings.Instance.GuildId ? Roles : AdditionalRoles.GetValueOrDefault(guild);

	public ModRoles GetRolesOrNew(ulong guild) {
		ModRoles? roles = GetRoles(guild);
		if(roles != null) return roles;
		roles = new ModRoles();
		AdditionalRoles[guild] = roles;
		return roles;
	}

	public bool AddChannel(RawChannel channel) {
		if(AdditionalChannels.Contains(channel)) return false;
		AdditionalChannels.Add(channel);
		Save();
		return true;
	}

	public bool RemoveChannel(RawChannel channel) {
		if(!AdditionalChannels.Remove(channel)) return false;
		Save();
		return true;
	}

	public void RemoveRoles(ulong guild) {
		AdditionalRoles.Remove(guild);
		Save();
	}

	protected override void Load(string propertyName, ref Utf8JsonReader reader) {
		switch(propertyName) {
			case "id":
				Id = reader.GetInt32();
				break;
			case "channel":
				Channel = reader.GetUInt64();
				break;
			case "roles":
				Roles = new ModRoles();
				Roles.Load(ref reader);
				break;
			case "lastAnnounce":
				LastAnnounce = reader.GetInt64();
				break;
			case "betaLinkable":
				BetaLinkable = reader.GetBoolean();
				break;
			case "deleted":
				Deleted = reader.GetBoolean();
				break;
			case "privateMod":
				PrivateMod = reader.GetBoolean();
				break;
			case "discordDL":
				DiscordDl = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
				break;
			case "releaseMessage":
				while(reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
					if(reader.TokenType != JsonTokenType.PropertyName) continue;
					VersionStruct version = new(reader.GetString()!);
					reader.Read();
					ReleaseMessage[version] = reader.GetUInt64();
				}
				break;
			case "additionalChannels":
				while(reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
					if(reader.TokenType != JsonTokenType.StartObject) continue;
					RawChannel channel = new();
					channel.Load(ref reader);
					AdditionalChannels.Add(channel);
				}
				break;
			case "additionalRoles":
				while(reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
					if(reader.TokenType != JsonTokenType.PropertyName) continue;
					ulong guild = ulong.Parse(reader.GetString()!);
					reader.Read();
					ModRoles additionalRoles = new();
					additionalRoles.Load(ref reader);
					AdditionalRoles[guild] = additionalRoles;
				}
				break;
			default:
				base.Load(propertyName, ref reader);
				break;
		}
	}

	protected override void SaveExtra(Utf8JsonWriter writer) {
		writer.WriteNumber("id", Id);
		writer.WriteNumber("channel", Channel);
		writer.WritePropertyName("roles");
		Roles.Save(writer);
		writer.WriteNumber("lastAnnounce", LastAnnounce);
		writer.WriteBoolean("betaLinkable", BetaLinkable);
		writer.WriteBoolean("deleted", Deleted);
		writer.WriteBoolean("privateMod", PrivateMod);
		writer.WriteString("discordDL", DiscordDl);
		writer.WriteStartObject("releaseMessage");
		foreach((VersionStruct version, ulong id) in ReleaseMessage) writer.WriteNumber(version.ToString(), id);
		writer.WriteEndObject();
		writer.WriteStartArray("additionalChannels");
		foreach(RawChannel channel in AdditionalChannels) channel.Save(writer);
		writer.WriteEndArray();
		writer.WriteStartObject("additionalRoles");
		foreach((ulong guild, ModRoles roles) in AdditionalRoles) {
			writer.WritePropertyName(guild.ToString());
			roles.Save(writer);
		}
		writer.WriteEndObject();
	}
}