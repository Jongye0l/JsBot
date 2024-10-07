package kr.jongyeol.jsBot;

import kr.jongyeol.jsBot.data.ModRoles;
import mx.kenzie.eris.api.entity.Guild;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.MessageFlags;

import java.util.Arrays;

public class ModAnnounce {

    public static void progressButton(String[] strings, Interaction event) {
        if(strings[1].equals("all")) {
            buttonInteraction(event, JSettings.getInstance().getAllProgressRole());
            return;
        }
        ModRoles roles = JModData.getMod(Integer.parseInt(strings[1])).getRoles(Long.parseLong(event.guild_id));
        if(roles != null) buttonInteraction(event, roles.getProgressRole());
        else event.reply(new Message("모드 역할 데이터가 존재하지 않습니다.").withFlag(MessageFlags.EPHEMERAL));
    }

    public static void releaseButton(String[] strings, Interaction event) {
        if(strings[1].equals("all")) {
            buttonInteraction(event, JSettings.getInstance().getAllReleaseRole());
            return;
        }
        if(strings[1].equals("new")) {
            buttonInteraction(event, JSettings.getInstance().getNewReleaseRole());
            return;
        }
        ModRoles roles = JModData.getMod(Integer.parseInt(strings[1])).getRoles(Long.parseLong(event.guild_id));
        if(roles != null) buttonInteraction(event, roles.getReleaseRole());
        else event.reply(new Message("모드 역할 데이터가 존재하지 않습니다.").withFlag(MessageFlags.EPHEMERAL));
    }

    public static void betaButton(String[] strings, Interaction event) {
        if(strings[1].equals("all")) {
            buttonInteraction(event, JSettings.getInstance().getAllBetaReleaseRole());
            return;
        }
        ModRoles roles = JModData.getMod(Integer.parseInt(strings[1])).getRoles(Long.parseLong(event.guild_id));
        if(roles != null) buttonInteraction(event, roles.getBetaReleaseRole());
        else event.reply(new Message("모드 역할 데이터가 존재하지 않습니다.").withFlag(MessageFlags.EPHEMERAL));
    }

    private static void buttonInteraction(Interaction event, long role) {
        if(role == -1) {
            event.reply(new Message("모드 역할 데이터가 존재하지 않습니다.").withFlag(MessageFlags.EPHEMERAL));
            return;
        }
        Guild guild = new Guild();
        guild.api = event.api;
        guild.id = event.guild_id;
        if(Arrays.stream(event.member.roles).anyMatch(s -> s.equals(role + ""))) {
            guild.removeRole(event.member, role);
            event.reply(new Message(Utility.getRoleMention(role) + " 역할을 제거하였습니다.").withFlag(MessageFlags.EPHEMERAL));
            new LogBuilder(event.getSource(), "역할을 제거하였습니다.")
                .addField("role", Utility.getRoleMention(role))
                .addBlankField(true)
                .addField("user", event.getSource())
                .addField("guild", event.guild_id)
                .addField("channel", event.getChannel())
                .addField("id", event.id)
                .send();
        } else {
            guild.addRole(event.member, role);
            event.reply(new Message(Utility.getRoleMention(role) + " 역할을 추가하였습니다.").withFlag(MessageFlags.EPHEMERAL));
            new LogBuilder(event.getSource(), "역할을 추가하였습니다.")
                .addField("role", Utility.getRoleMention(role))
                .addBlankField(true)
                .addField("user", event.getSource())
                .addField("guild", event.guild_id)
                .addField("channel", event.getChannel())
                .addField("id", event.id)
                .send();
        }

    }
}
