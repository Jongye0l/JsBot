using System.Text.Json;

namespace JsBot.Data;

public class ModRoles {
	public long ReleaseRole = -1;
	public long ProgressRole = -1;
	public long BetaReleaseRole = -1;

	public bool NotSet() => ReleaseRole == -1 && ProgressRole == -1 && BetaReleaseRole == -1;

	public string ReleasePing => "<@&" + ReleaseRole + ">";
	public string ProgressPing => "<@&" + ProgressRole + ">";
	public string BetaReleasePing => "<@&" + BetaReleaseRole + ">";

	public void Load(ref Utf8JsonReader reader) {
		while(reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
			if(reader.TokenType != JsonTokenType.PropertyName) continue;
			string propertyName = reader.GetString()!;
			reader.Read();
			switch(propertyName) {
				case "releaseRole":
					ReleaseRole = reader.GetInt64();
					break;
				case "progressRole":
					ProgressRole = reader.GetInt64();
					break;
				case "betaReleaseRole":
					BetaReleaseRole = reader.GetInt64();
					break;
				default:
					reader.Skip();
					break;
			}
		}
	}

	public void Save(Utf8JsonWriter writer) {
		writer.WriteStartObject();
		writer.WriteNumber("releaseRole", ReleaseRole);
		writer.WriteNumber("progressRole", ProgressRole);
		writer.WriteNumber("betaReleaseRole", BetaReleaseRole);
		writer.WriteEndObject();
	}
}
