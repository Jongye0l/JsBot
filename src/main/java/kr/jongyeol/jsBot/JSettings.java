package kr.jongyeol.jsBot;

import kr.jongyeol.jaServer.Settings;
import lombok.Data;
import lombok.EqualsAndHashCode;
import lombok.Getter;
import lombok.Setter;
import mx.kenzie.eris.api.entity.Channel;
import mx.kenzie.eris.api.entity.Guild;

import java.io.IOException;

import static kr.jongyeol.jsBot.DiscordBot.bot;

@EqualsAndHashCode(callSuper = true)
@Data
public class JSettings extends Settings {
    private long guildId = 1201804820579622933L;
    private long modCategory = 1201815747819753492L;
    private long[] roles = new long[] {
        1201806313022292008L,
        1201807541718482954L,
        1201807827589672980L
    };
    private long botRole = 1201810089611108372L;
    private long userRole = 1201808991311888404L;
    private long allReleaseRole = 1201809565625094204L;
    private long allProgressRole = 1204459016416264242L;
    private long allBetaReleaseRole = 1226058805356531762L;
    private long newReleaseRole = 1201809843372179476L;
    private long joinLogChannel = 1202115930881003540L;
    private long boostLogChannel = 1203022996135219271L;
    private long logChannel = 1204464547587428383L;
    private long announceRole = 1206250401477955694L;
    private long simsimRole = 1204810318694453338L;
    private String token;

    public static JSettings getInstance() {
        return (JSettings) Settings.getInstance();
    }

    public static void load() {
        clazz = JSettings.class;
    }

    public static Guild getEmptyGuild() {
        Guild guild = new Guild();
        guild.api = bot.getAPI();
        guild.id = getInstance().getGuildId() + "";
        return guild;
    }

    public static String getAllReleasePing() {
        return Utility.getRoleMention(getInstance().getAllReleaseRole());
    }

    public static String getAllProgressPing() {
        return Utility.getRoleMention(getInstance().getAllProgressRole());
    }

    public static String getAllBetaReleasePing() {
        return Utility.getRoleMention(getInstance().getAllBetaReleaseRole());
    }

    public static String getNewReleasePing() {
        return Utility.getRoleMention(getInstance().getNewReleaseRole());
    }

    public static String getAnnouncePing() {
        return Utility.getRoleMention(getInstance().getAnnounceRole());
    }

    public static String getSimsimPing() {
        return Utility.getRoleMention(getInstance().getSimsimRole());
    }

    private JSettings() {
    }
}
