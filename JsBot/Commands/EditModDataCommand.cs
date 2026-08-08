using JALib.Server;
using JALib.Server.Models;
using JALib.Server.Models.DL;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace JsBot.Commands;

[SlashCommand("editmoddata", "모드를 수정합니다.")]
[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
public class EditModDataCommand : ApplicationCommandModule<ApplicationCommandContext> {
	private async Task<JMod?> Resolve(string name) {
		JMod? mod = JMod.GetMod(name);
		if(mod == null) await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
		return mod;
	}

	private async Task Finish(JMod mod, string name, string subcommand, string? original, params (string Key, string? Value)[] extra) {
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 데이터를 수정했습니다.")));
		LogBuilder log = new LogBuilder(Context.User, "모드 데이터를 수정했습니다.")
			.AddField("user", Context.User)
			.AddBlankField(true)
			.AddField("channel", Context.Channel)
			.AddField("id", mod.Id)
			.AddField("name", name)
			.AddField("changed", subcommand)
			.AddField("original", original);
		foreach((string key, string? value) in extra) log.AddField(key, value);
		log.Send();
	}

	[SubSlashCommand("channel", "채널")]
	public async Task Channel([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, IGuildChannel channel) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.Channel.ToString();
		mod.Channel = channel.Id;
		mod.Save();
		await Finish(mod, modname0, "channel", original, ("channel", channel.Id.ToString()));
	}

	[SubSlashCommand("releaserole", "릴리즈 역할")]
	public async Task ReleaseRole([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, Role releaserole) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.Roles.ReleaseRole.ToString();
		mod.Roles.ReleaseRole = (long) releaserole.Id;
		mod.Save();
		await Finish(mod, modname0, "releaserole", original, ("releaserole", releaserole.Id.ToString()));
	}

	[SubSlashCommand("progressrole", "근황 역할")]
	public async Task ProgressRole([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, Role progressrole) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.Roles.ProgressRole.ToString();
		mod.Roles.ProgressRole = (long) progressrole.Id;
		mod.Save();
		await Finish(mod, modname0, "progressrole", original, ("progressrole", progressrole.Id.ToString()));
	}

	[SubSlashCommand("releasebetarole", "베타 역할")]
	public async Task ReleaseBetaRole([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, Role releasebetarole) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.Roles.BetaReleaseRole.ToString();
		mod.Roles.BetaReleaseRole = (long) releasebetarole.Id;
		mod.Save();
		await Finish(mod, modname0, "releasebetarole", original, ("releasebetarole", releasebetarole.Id.ToString()));
	}

	[SubSlashCommand("lastannounce", "마지막 알림 메시지 ID")]
	public async Task LastAnnounce([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, long lastannounce) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.LastAnnounce.ToString();
		mod.LastAnnounce = lastannounce;
		mod.Save();
		await Finish(mod, modname0, "lastannounce", original, ("lastannounce", lastannounce.ToString()));
	}

	[SubSlashCommand("betalinkable", "베타 링크 허용")]
	public async Task BetaLinkable([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, bool betalinkable) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.BetaLinkable.ToString();
		mod.BetaLinkable = betalinkable;
		mod.Save();
		await Finish(mod, modname0, "betalinkable", original, ("betalinkable", betalinkable.ToString()));
	}

	[SubSlashCommand("deleted", "삭제됨")]
	public async Task Deleted([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, bool deleted) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.Deleted.ToString();
		mod.Deleted = deleted;
		mod.Save();
		await Finish(mod, modname0, "deleted", original, ("deleted", deleted.ToString()));
	}

	[SubSlashCommand("private", "비공개")]
	public async Task Private([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, bool @private) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.PrivateMod.ToString();
		mod.PrivateMod = @private;
		mod.Save();
		await Finish(mod, modname0, "private", original, ("private", @private.ToString()));
	}

	[SubSlashCommand("discorddl", "디스코드 다운링크")]
	public async Task DiscordDl([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string? discorddl = null) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string? original = mod.DiscordDl;
		mod.DiscordDl = discorddl;
		mod.Save();
		await Finish(mod, modname0, "discorddl", original, ("discorddl", discorddl));
	}

	[SubSlashCommand("releasemessage", "릴리즈 메시지 ID")]
	public async Task ReleaseMessage([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string version, long? releasemessage = null) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		VersionStruct versionData = new(version);
		string original = mod.ReleaseMessage.TryGetValue(versionData, out ulong old) ? old.ToString() : "null";
		if(releasemessage != null) mod.ReleaseMessage[versionData] = (ulong) releasemessage.Value;
		else mod.ReleaseMessage.Remove(versionData);
		mod.Save();
		await Finish(mod, modname0, "releasemessage", original, ("version", version), ("releasemessage", releasemessage?.ToString()));
	}

	[SubSlashCommand("version", "버전")]
	public async Task Version([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string? version = null) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.Version.ToString();
		mod.Version = version != null ? new VersionStruct(version) : VersionStruct.Null;
		ConnectOtherLib.SetVersion(mod, mod.Version);
		mod.Save();
		await Finish(mod, modname0, "version", original, ("version", version));
	}

	[SubSlashCommand("betaversion", "베타 버전")]
	public async Task BetaVersion([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string betaversion) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.BetaVersion.ToString();
		mod.BetaVersion = new VersionStruct(betaversion);
		ConnectOtherLib.SetBetaVersion(mod, mod.BetaVersion);
		mod.Save();
		await Finish(mod, modname0, "betaversion", original, ("betaversion", betaversion));
	}

	[SubSlashCommand("setbetainfo", "베타 정보 설정")]
	public async Task SetBetaInfo([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string version, bool? beta = null) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		VersionStruct versionData = new(version);
		string original = mod.BetaMap.TryGetValue(versionData, out bool old) ? old.ToString() : "null";
		if(beta != null) mod.BetaMap[versionData] = beta.Value;
		else mod.BetaMap.Remove(versionData);
		ConnectOtherLib.SetBetaMap(mod, versionData, beta);
		mod.Save();
		await Finish(mod, modname0, "setbetainfo", original, ("version", version), ("beta", beta?.ToString()));
	}

	[SubSlashCommand("forceupdate", "강제 업데이트")]
	public async Task ForceUpdate([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, bool forceupdate) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.IsForceUpdate.ToString();
		mod.IsForceUpdate = forceupdate;
		ConnectOtherLib.SetForceUpdate(mod, mod.IsForceUpdate);
		mod.Save();
		await Finish(mod, modname0, "forceupdate", original, ("forceupdate", forceupdate.ToString()));
	}

	[SubSlashCommand("forceupdatebeta", "베타 강제 업데이트")]
	public async Task ForceUpdateBeta([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, bool forceupdatebeta) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.IsForceUpdateBeta.ToString();
		mod.IsForceUpdateBeta = forceupdatebeta;
		ConnectOtherLib.SetForceUpdateBeta(mod, mod.IsForceUpdateBeta);
		mod.Save();
		await Finish(mod, modname0, "forceupdatebeta", original, ("forceupdatebeta", forceupdatebeta.ToString()));
	}

	[SubSlashCommand("forceupdatehandle", "세부 강제 업데이트")]
	public async Task ForceUpdateHandle([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string minversion, string maxversion, bool forceupdate) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		ForceUpdateHandle handle = new() { LowVersion = new VersionStruct(minversion), HighVersion = new VersionStruct(maxversion), ForceUpdate = forceupdate };
		mod.ForceUpdateHandles = [.. mod.ForceUpdateHandles, handle];
		ConnectOtherLib.AddForceUpdateHandle(mod, handle);
		mod.Save();
		await Finish(mod, modname0, "forceupdatehandle", null, ("minversion", minversion), ("maxversion", maxversion), ("forceupdate", forceupdate.ToString()));
	}

	[SubSlashCommand("removeforceupdatehandle", "세부 강제 업데이트 제거")]
	public async Task RemoveForceUpdateHandle([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string minversion, string maxversion) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		VersionStruct low = new(minversion), high = new(maxversion);
		string? original = null;
		List<ForceUpdateHandle> handles = mod.ForceUpdateHandles.ToList();
		for(int i = 0; i < handles.Count; i++) {
			ForceUpdateHandle handle = handles[i];
			if(handle.LowVersion.Equals(low) && handle.HighVersion.Equals(high)) {
				original = $"minversion: {handle.LowVersion}, maxversion: {handle.HighVersion}, forceupdate: {handle.ForceUpdate}";
				handles.RemoveAt(i);
				ConnectOtherLib.RemoveForceUpdateHandle(mod, i);
				break;
			}
		}
		mod.ForceUpdateHandles = handles.ToArray();
		mod.Save();
		await Finish(mod, modname0, "removeforceupdatehandle", original, ("minversion", minversion), ("maxversion", maxversion));
	}

	[SubSlashCommand("reloadlocalization", "번역 재로딩")]
	public async Task ReloadLocalization([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		ConnectOtherLib.LoadLocalizations(mod);
		await Finish(mod, modname0, "reloadlocalization", null);
	}

	[SubSlashCommand("homepage", "홈페이지")]
	public async Task Homepage([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string? homepage = null) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string? original = mod.Homepage;
		mod.Homepage = homepage;
		ConnectOtherLib.SetHomepage(mod, mod.Homepage);
		mod.Save();
		await Finish(mod, modname0, "homepage", original, ("homepage", homepage));
	}

	[SubSlashCommand("discord", "디스코드")]
	public async Task Discord([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string? discord = null) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string? original = mod.Discord;
		mod.Discord = discord;
		ConnectOtherLib.SetDiscord(mod, mod.Discord);
		mod.Save();
		await Finish(mod, modname0, "discord", original, ("discord", discord));
	}

	[SubSlashCommand("downloadlink", "다운로드 링크")]
	public async Task DownloadLinkCommand([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, DownloadLinkType downloadlink) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.DownloadLink.ToString() ?? "";
		mod.DownloadLink = downloadlink == DownloadLinkType.Github ? new GithubDownloadLink(mod.Name) : new CustomDownloadLink();
		ConnectOtherLib.SetDownloadLink(mod, mod.DownloadLink);
		mod.Save();
		await Finish(mod, modname0, "downloadlink", original, ("downloadlink", downloadlink.ToString()));
	}

	[SubSlashCommand("customdownloadlink", "커스텀 다운로드 링크")]
	public async Task CustomDownloadLinkCommand([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, string version, string? link = null) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		if(mod.DownloadLink is GithubDownloadLink) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("이 모드는 Github 다운로드 링크를 사용합니다.")));
			return;
		}
		VersionStruct ver = new(version);
		CustomDownloadLink downloadLink = (CustomDownloadLink) mod.DownloadLink;
		string? original = downloadLink.GetLink(ver);
		if(link != null) downloadLink.Links[ver] = link;
		else downloadLink.Links.Remove(ver);
		mod.Save();
		await Finish(mod, modname0, "customdownloadlink", original, ("version", version), ("link", link));
	}

	[SubSlashCommand("gid", "번역본 GID")]
	public async Task Gid([SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))] string modname0, int gid) {
		JMod? mod = await Resolve(modname0);
		if(mod == null) return;
		string original = mod.Gid.ToString();
		mod.Gid = gid;
		ConnectOtherLib.SetGid(mod, mod.Gid);
		mod.Save();
		await Finish(mod, modname0, "gid", original, ("gid", gid.ToString()));
	}
}