using System.Text.Json;
using JALib.Server.Helpers;
using JALib.Server.Models;

namespace JsBot;

public class JSettings : Settings {
	public static new JSettings Instance => Settings.Instance.As<JSettings>();

	public ulong GuildId = 1201804820579622933;
	public ulong ModCategory = 1201815747819753492;
	public ulong[] Roles = [1201806313022292008, 1201807541718482954, 1201807827589672980];
	public ulong BotRole = 1201810089611108372;
	public ulong UserRole = 1201808991311888404;
	public ulong AllReleaseRole = 1201809565625094204;
	public ulong AllProgressRole = 1204459016416264242;
	public ulong AllBetaReleaseRole = 1226058805356531762;
	public ulong NewReleaseRole = 1201809843372179476;
	public ulong JoinLogChannel = 1202115930881003540;
	public ulong BoostLogChannel = 1203022996135219271;
	public ulong LogChannel = 1204464547587428383;
	public ulong AnnounceRole = 1206250401477955694;
	public ulong SimsimRole = 1204810318694453338;
	public string Token = "";

	public static void SetInstance() {
		Settings.Instance = new JSettings();
	}

	public override void Load(string key, ref Utf8JsonReader reader) {
		switch(key) {
			case "guildId":
				GuildId = reader.GetUInt64();
				break;
			case "modCategory":
				ModCategory = reader.GetUInt64();
				break;
			case "roles":
				List<ulong> roles = [];
				while(reader.Read() && reader.TokenType != JsonTokenType.EndArray) roles.Add(reader.GetUInt64());
				Roles = roles.ToArray();
				break;
			case "botRole":
				BotRole = reader.GetUInt64();
				break;
			case "userRole":
				UserRole = reader.GetUInt64();
				break;
			case "allReleaseRole":
				AllReleaseRole = reader.GetUInt64();
				break;
			case "allProgressRole":
				AllProgressRole = reader.GetUInt64();
				break;
			case "allBetaReleaseRole":
				AllBetaReleaseRole = reader.GetUInt64();
				break;
			case "newReleaseRole":
				NewReleaseRole = reader.GetUInt64();
				break;
			case "joinLogChannel":
				JoinLogChannel = reader.GetUInt64();
				break;
			case "boostLogChannel":
				BoostLogChannel = reader.GetUInt64();
				break;
			case "logChannel":
				LogChannel = reader.GetUInt64();
				break;
			case "announceRole":
				AnnounceRole = reader.GetUInt64();
				break;
			case "simsimRole":
				SimsimRole = reader.GetUInt64();
				break;
			case "token":
				Token = reader.GetString()!;
				break;
			default:
				base.Load(key, ref reader);
				break;
		}
	}

	public override void Save(Utf8JsonWriter writer) {
		writer.WriteStartObject();
		writer.WriteString(nameof(LogPath), LogPath);
		writer.WriteString(nameof(ModDataPath), ModDataPath);
		writer.WriteString(nameof(AdminManagerPath), AdminManagerPath);
		writer.WriteString(nameof(TokenPath), TokenPath);
		writer.WriteString("otherLibURL", OtherLibUrl);
		writer.WriteString(nameof(ModFilePath), ModFilePath);
		writer.WriteNumber("guildId", GuildId);
		writer.WriteNumber("modCategory", ModCategory);
		writer.WriteStartArray("roles");
		foreach(ulong role in Roles) writer.WriteNumberValue(role);
		writer.WriteEndArray();
		writer.WriteNumber("botRole", BotRole);
		writer.WriteNumber("userRole", UserRole);
		writer.WriteNumber("allReleaseRole", AllReleaseRole);
		writer.WriteNumber("allProgressRole", AllProgressRole);
		writer.WriteNumber("allBetaReleaseRole", AllBetaReleaseRole);
		writer.WriteNumber("newReleaseRole", NewReleaseRole);
		writer.WriteNumber("joinLogChannel", JoinLogChannel);
		writer.WriteNumber("boostLogChannel", BoostLogChannel);
		writer.WriteNumber("logChannel", LogChannel);
		writer.WriteNumber("announceRole", AnnounceRole);
		writer.WriteNumber("simsimRole", SimsimRole);
		writer.WriteString("token", Token);
		writer.WriteEndObject();
	}
}