using JALib.Server.Models;
using JALib.Server.Models.DL;
using JsBot.Data;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using Serilog;

namespace JsBot.Commands;

public enum WantRole {
	Release,
	Progress,
	Beta
}

[SlashCommand("mod", "모드 관련 명령어입니다.")]
[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
public class ModCommand : ApplicationCommandModule<ApplicationCommandContext> {
	[SubSlashCommand("addchannel", "채널 추가")]
	public async Task AddChannel(
		[SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))]
		string modname,
		[SlashCommandParameter(Description = "채널")]
		IGuildChannel channel,
		[SlashCommandParameter(Description = "베타 추가")]
		bool beta = false,
		[SlashCommandParameter(Description = "자동 적용 버튼 추가")]
		bool apply = true) {
		JMod? mod = JMod.GetMod(modname);
		if(mod == null) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
			return;
		}
		RawChannel rawChannel = new(Context.Guild!.Id, channel.Id, beta, apply);
		if(!mod.AddChannel(rawChannel)) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(Utility.GetChannelMention(channel.Id) + " 채널에 이미 " + modname + " 모드 알림이 있습니다.")));
			return;
		}
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(Utility.GetChannelMention(channel.Id) + " 채널에 " + modname + " 모드 알림을 추가했습니다.")));
		new LogBuilder(Context.User, "채널에 모드 알림을 추가했습니다")
			.AddField("user", Context.User)
			.AddBlankField(true)
			.AddField("channel", Context.Channel)
			.AddField("id", mod.Id)
			.AddField("name", modname)
			.AddField("guild", Context.Guild.Id)
			.AddField("targetChannel", rawChannel.ChannelUrl)
			.AddField("beta", rawChannel.Beta)
			.Send();

		_ = Task.Run(async () => {
			try {
				ModRoles? roles = mod.GetRoles(Context.Guild.Id);
				string release = roles is { ReleaseRole: not -1 } ? roles.ReleasePing : "";
				string progress = roles is { ProgressRole: not -1 } ? roles.ProgressPing : "";
				string betaPing = roles is { BetaReleaseRole: not -1 } ? roles.BetaReleasePing : "";
				List<RestMessage> messages = [];
				await foreach(RestMessage message in DiscordBot.Rest.GetMessagesAsync(mod.Channel)) messages.Add(message);
				for(int i = messages.Count - 1; i >= 0; i--) {
					RestMessage message = messages[i];
					if(message.Author.Id != DiscordBot.Client.Id || message.Id == (ulong) mod.LastAnnounce) continue;
					string content = message.Content;
					content = content.Replace(Utility.GetRoleMention(JSettings.Instance.AllReleaseRole), "");
					content = content.Replace(Utility.GetRoleMention(JSettings.Instance.NewReleaseRole), "");
					content = content.Replace(Utility.GetRoleMention(JSettings.Instance.AllBetaReleaseRole), "");
					content = content.Replace(Utility.GetRoleMention(JSettings.Instance.AllProgressRole), "");
					content = content.Replace(mod.ReleasePing, release);
					content = content.Replace(mod.ProgressPing, progress);
					content = content.Replace(mod.BetaReleasePing, betaPing);
					MessageProperties newMessage = new MessageProperties().WithContent(content).WithFlags(MessageFlags.SuppressEmbeds);
					if(message.Components.Count != 0) {
						VersionStruct? version = null;
						foreach((VersionStruct v, ulong id) in mod.ReleaseMessage) {
							if(id == message.Id) version = v;
						}
						if(version is {} ver) {
							bool isBeta = mod.BetaMap.TryGetValue(ver, out bool b) && b;
							if(!isBeta || rawChannel.Beta) {
								string? link = mod.DiscordDl;
								DownloadLink downloadLink = mod.DownloadLink;
								if(link == null) link = mod.BetaLinkable && isBeta ? "" : downloadLink.GetLink(ver) ?? "";
								bool source = downloadLink is GithubDownloadLink;
								List<IActionRowComponentProperties> components = [
									new LinkButtonProperties(source ? ((GithubDownloadLink) downloadLink).GetSourceLink(ver) : DiscordBot.SampleUrl, "소스 코드") { Disabled = !source },
									new LinkButtonProperties(link.Length == 0 ? DiscordBot.SampleUrl : link, "다운로드") { Disabled = link.Length == 0 }
								];
								if(rawChannel.Apply) {
									components.Add(new LinkButtonProperties("https://jalib.jongyeol.kr/modApplicator/" + mod.Name + "/" + ver, "모드 적용(서버 1)"));
									components.Add(new LinkButtonProperties("https://jalib2.jongyeol.kr/modApplicator/" + mod.Name + "/" + ver, "모드 적용(서버 2)"));
								}
								newMessage.Components = [new ActionRowProperties(components)];
							}
						}
					}
					await DiscordBot.Rest.SendMessageAsync(channel.Id, newMessage);
					await Task.Delay(500);
				}
				await mod.Announce(rawChannel);
			} catch (Exception e) {
				Log.Logger.Error(e, "Failed to announce mod {ModName} to channel {ChannelId} in guild {GuildId}", modname, channel.Id, Context.Guild!.Id);
			}
		});
	}

	[SubSlashCommand("removechannel", "채널 제거")]
	public async Task RemoveChannel(
		[SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))]
		string modname,
		[SlashCommandParameter(Description = "채널")]
		IGuildChannel channel) {
		JMod? mod = JMod.GetMod(modname);
		if(mod == null) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
			return;
		}
		RawChannel rawChannel = new(Context.Guild!.Id, channel.Id, false, true);
		if(!mod.RemoveChannel(rawChannel)) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(Utility.GetChannelMention(channel.Id) + " 채널에 " + modname + " 모드 알림이 없습니다.")));
			return;
		}
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(Utility.GetChannelMention(channel.Id) + " 채널에 " + modname + " 모드 알림을 제거했습니다.")));
		new LogBuilder(Context.User, "채널에 모드 알림을 제거했습니다")
			.AddField("user", Context.User)
			.AddBlankField(true)
			.AddField("channel", Context.Channel)
			.AddField("id", mod.Id)
			.AddField("name", modname)
			.AddField("guild", Context.Guild.Id)
			.AddBlankField(true)
			.AddField("targetChannel", rawChannel.ChannelUrl)
			.Send();
	}

	[SubSlashCommand("setrole", "역할 추가")]
	public async Task SetRole(
		[SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))]
		string modname,
		[SlashCommandParameter(Description = "추가할 역할")]
		WantRole wantrole,
		[SlashCommandParameter(Description = "역할")]
		Role role) {
		JMod? mod = JMod.GetMod(modname);
		if(mod == null) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
			return;
		}
		ulong guild = Context.Guild!.Id;
		ModRoles roles = mod.GetRolesOrNew(guild);
		switch(wantrole) {
			case WantRole.Release:
				roles.ReleaseRole = (long) role.Id;
				break;
			case WantRole.Progress:
				roles.ProgressRole = (long) role.Id;
				break;
			case WantRole.Beta:
				roles.BetaReleaseRole = (long) role.Id;
				break;
		}
		foreach(RawChannel ch in mod.AdditionalChannels.Where(ch => ch.Guild == guild)) await mod.Announce(ch);
		mod.Save();
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("역할을 설정했습니다.")));
		new LogBuilder(Context.User, "역할을 설정했습니다.")
			.AddField("user", Context.User)
			.AddBlankField(true)
			.AddField("channel", Context.Channel)
			.AddField("id", mod.Id)
			.AddField("name", modname)
			.AddField("guild", guild)
			.AddField("role", role)
			.AddField("wantRole", wantrole.ToString())
			.Send();
	}

	[SubSlashCommand("removerole", "역할 제거")]
	public async Task RemoveRole(
		[SlashCommandParameter(Description = "모드 이름", AutocompleteProviderType = typeof(PublicJModNameAutocompleteProvider))]
		string modname,
		[SlashCommandParameter(Description = "추가할 역할")]
		WantRole wantrole) {
		JMod? mod = JMod.GetMod(modname);
		if(mod == null) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
			return;
		}
		ulong guild = Context.Guild!.Id;
		ModRoles? roles = mod.GetRoles(guild);
		bool removed = false;
		if(roles != null) {
			switch(wantrole) {
				case WantRole.Release when roles.ReleaseRole != -1:
					roles.ReleaseRole = -1;
					removed = true;
					break;
				case WantRole.Progress when roles.ProgressRole != -1:
					roles.ProgressRole = -1;
					removed = true;
					break;
				case WantRole.Beta when roles.BetaReleaseRole != -1:
					roles.BetaReleaseRole = -1;
					removed = true;
					break;
			}
		}
		if(!removed) {
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("역할이 없습니다.")));
			return;
		}
		foreach(RawChannel ch in mod.AdditionalChannels.Where(ch => ch.Guild == guild)) await mod.Announce(ch);
		if(roles!.NotSet()) mod.RemoveRoles(guild);
		else mod.Save();
		await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("역할을 제거했습니다.")));
		new LogBuilder(Context.User, "역할을 제거했습니다.")
			.AddField("user", Context.User)
			.AddBlankField(true)
			.AddField("channel", Context.Channel)
			.AddField("id", mod.Id)
			.AddField("name", modname)
			.AddField("guild", guild)
			.AddField("wantRole", wantrole.ToString())
			.Send();
	}
}