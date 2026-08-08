using System.Text.Json;
using JALib.Server.Models;

namespace JsBot.Data;

public class RawChannel : IEquatable<RawChannel> {
	public ulong Guild;
	public ulong Channel;
	public bool Beta;
	public bool Apply;
	public long LastAnnounce = -1;
	public readonly Dictionary<VersionStruct, long> ReleaseMessage = new();

	public RawChannel() {
	}

	public RawChannel(ulong guild, ulong channel, bool beta, bool apply) {
		Guild = guild;
		Channel = channel;
		Beta = beta;
		Apply = apply;
	}

	public string ChannelUrl => "https://discord.com/channels/" + Guild + "/" + Channel;

	public string GetMessageUrl(VersionStruct version) => ChannelUrl + "/" + ReleaseMessage[version];

	public bool Equals(RawChannel? other) => other != null && other.Guild == Guild && other.Channel == Channel;
	public override bool Equals(object? obj) => Equals(obj as RawChannel);
	// ReSharper disable NonReadonlyMemberInGetHashCode
	public override int GetHashCode() => HashCode.Combine(Guild, Channel);
	// ReSharper restore NonReadonlyMemberInGetHashCode

	public void Load(ref Utf8JsonReader reader) {
		while(reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
			if(reader.TokenType != JsonTokenType.PropertyName) continue;
			string propertyName = reader.GetString()!;
			reader.Read();
			switch(propertyName) {
				case "guild":
					Guild = reader.GetUInt64();
					break;
				case "channel":
					Channel = reader.GetUInt64();
					break;
				case "beta":
					Beta = reader.GetBoolean();
					break;
				case "apply":
					Apply = reader.GetBoolean();
					break;
				case "lastAnnounce":
					LastAnnounce = reader.GetInt64();
					break;
				case "releaseMessage":
					while(reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
						if(reader.TokenType != JsonTokenType.PropertyName) continue;
						VersionStruct version = new(reader.GetString()!);
						reader.Read();
						ReleaseMessage[version] = reader.GetInt64();
					}
					break;
				default:
					reader.Skip();
					break;
			}
		}
	}

	public void Save(Utf8JsonWriter writer) {
		writer.WriteStartObject();
		writer.WriteNumber("guild", Guild);
		writer.WriteNumber("channel", Channel);
		writer.WriteBoolean("beta", Beta);
		writer.WriteBoolean("apply", Apply);
		writer.WriteNumber("lastAnnounce", LastAnnounce);
		writer.WriteStartObject("releaseMessage");
		foreach((VersionStruct version, long id) in ReleaseMessage) writer.WriteNumber(version.ToString(), id);
		writer.WriteEndObject();
		writer.WriteEndObject();
	}
}