using JsBot.Data;
using NetCord;
using NetCord.Rest;

namespace JsBot;

public static class ModAnnounce {
	public static async Task ProgressButton(string[] parts, ButtonInteraction interaction) {
		if(parts[1] == "all") {
			await ButtonRoleToggle(interaction, JSettings.Instance.AllProgressRole);
			return;
		}
		ModRoles? roles = JMod.GetMod(int.Parse(parts[1]))?.GetRoles(interaction.GuildId!.Value);
		if(roles != null) await ButtonRoleToggle(interaction, (ulong) roles.ProgressRole);
		else await interaction.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 역할 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
	}

	public static async Task ReleaseButton(string[] parts, ButtonInteraction interaction) {
		if(parts[1] == "all") {
			await ButtonRoleToggle(interaction, JSettings.Instance.AllReleaseRole);
			return;
		}
		if(parts[1] == "new") {
			await ButtonRoleToggle(interaction, JSettings.Instance.NewReleaseRole);
			return;
		}
		ModRoles? roles = JMod.GetMod(int.Parse(parts[1]))?.GetRoles(interaction.GuildId!.Value);
		if(roles != null) await ButtonRoleToggle(interaction, (ulong) roles.ReleaseRole);
		else await interaction.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 역할 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
	}

	public static async Task BetaButton(string[] parts, ButtonInteraction interaction) {
		if(parts[1] == "all") {
			await ButtonRoleToggle(interaction, JSettings.Instance.AllBetaReleaseRole);
			return;
		}
		ModRoles? roles = JMod.GetMod(int.Parse(parts[1]))?.GetRoles(interaction.GuildId!.Value);
		if(roles != null) await ButtonRoleToggle(interaction, (ulong) roles.BetaReleaseRole);
		else await interaction.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 역할 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
	}

	private static async Task ButtonRoleToggle(ButtonInteraction interaction, ulong role) {
		if(role == ulong.MaxValue) {
			await interaction.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent("모드 역할 데이터가 존재하지 않습니다.").WithFlags(MessageFlags.Ephemeral)));
			return;
		}
		ulong guildId = interaction.GuildId!.Value;
		GuildUser member = interaction.Guild!.Users[interaction.User.Id];
		if(member.RoleIds.Contains(role)) {
			await DiscordBot.Rest.RemoveGuildUserRoleAsync(guildId, member.Id, role);
			await interaction.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(Utility.GetRoleMention(role) + " 역할을 제거하였습니다.").WithFlags(MessageFlags.Ephemeral)));
			new LogBuilder(member, "역할을 제거하였습니다.")
				.AddField("role", Utility.GetRoleMention(role))
				.AddBlankField(true)
				.AddField("user", member)
				.AddField("guild", guildId)
				.AddField("channel", interaction.Channel)
				.AddField("id", interaction.Id)
				.Send();
		} else {
			await DiscordBot.Rest.AddGuildUserRoleAsync(guildId, member.Id, role);
			await interaction.SendResponseAsync(InteractionCallback.Message(new InteractionMessageProperties().WithContent(Utility.GetRoleMention(role) + " 역할을 추가하였습니다.").WithFlags(MessageFlags.Ephemeral)));
			new LogBuilder(member, "역할을 추가하였습니다.")
				.AddField("role", Utility.GetRoleMention(role))
				.AddBlankField(true)
				.AddField("user", member)
				.AddField("guild", guildId)
				.AddField("channel", interaction.Channel)
				.AddField("id", interaction.Id)
				.Send();
		}
	}
}