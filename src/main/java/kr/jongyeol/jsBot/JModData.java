package kr.jongyeol.jsBot;

import kr.jongyeol.jaServer.ConnectOtherLib;
import kr.jongyeol.jaServer.Settings;
import kr.jongyeol.jaServer.data.DownloadLink;
import kr.jongyeol.jaServer.data.ModData;
import kr.jongyeol.jaServer.data.Version;
import kr.jongyeol.jsBot.data.ModRoles;
import kr.jongyeol.jsBot.data.RawChannel;
import lombok.Data;
import lombok.EqualsAndHashCode;
import lombok.SneakyThrows;
import mx.kenzie.eris.api.Lazy;
import mx.kenzie.eris.api.entity.Channel;
import mx.kenzie.eris.api.entity.Guild;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.Role;
import mx.kenzie.eris.api.entity.message.ActionRow;
import mx.kenzie.eris.api.entity.message.Button;
import mx.kenzie.eris.api.entity.message.Component;
import mx.kenzie.eris.api.magic.ButtonStyle;
import mx.kenzie.eris.api.magic.ChannelType;

import java.io.File;
import java.io.IOException;
import java.util.*;

import static kr.jongyeol.jsBot.DiscordBot.bot;

@EqualsAndHashCode(callSuper = true)
@Data
public class JModData extends ModData {
    private static int lastId = -1;
    private static List<JModData> modList = new ArrayList<>();

    @SneakyThrows
    public static void initialize() {
        clazz = JModData.class;
        LoadModData();

        lastId = Arrays.stream(getModList()).mapToInt(JModData::getId).max().orElse(-1);
    }

    public static JModData[] getModList() {
        return modList.toArray(new JModData[0]);
    }

    public static JModData getMod(String name) {
        return (JModData) ModData.getModData(name);
    }

    public static JModData getMod(int id) {
        return Arrays.stream(getModList()).filter(mod -> mod.getId() == id).findFirst().orElse(null);
    }

    public static JModData getMod(long channel) {
        return Arrays.stream(getModList()).filter(mod -> mod.getChannelId() == channel).findFirst().orElse(null);
    }

    transient private File datafile;
    private int id;
    private long channel;
    private ModRoles roles;
    private long lastAnnounce = -1;
    private boolean betaLinkable = false;
    private boolean deleted = false; //deleted
    private boolean privateMod = false; //private
    private String discordDL = null; //discorddl
    private final Map<Version, Long> releaseMessage = new HashMap<>();
    private final List<RawChannel> additionalChannels = new ArrayList<>();
    private final Map<Long, ModRoles> additionalRoles = new HashMap<>();

    public JModData() {
        modList.add(this);
    }

    public JModData(String name, DownloadLink link) throws IOException {
        modList.add(this);
        setName(name);
        setDownloadLink(link);
        id = ++lastId;
        Guild guild = new Guild();
        guild.api = bot.getAPI();
        guild.id = JSettings.getInstance().getGuildId() + "";
        List<Lazy> snowflakes = new ArrayList<>();
        roles = new ModRoles();
        Role role = guild.createRole(new Role(name));
        snowflakes.add(role);
        role.whenReady(lazy -> roles.setReleaseRole(((Role) lazy).id()));
        role = guild.createRole(new Role(name + " 근황"));
        snowflakes.add(role);
        role.whenReady(lazy -> roles.setProgressRole(((Role) lazy).id()));
        role = guild.createRole(new Role(name + " Beta"));
        snowflakes.add(role);
        role.whenReady(lazy -> roles.setBetaReleaseRole(((Role) lazy).id()));
        Channel modChannel = new Channel("\uD83D\uDCDFㅣ" + name, ChannelType.GUILD_TEXT);
        modChannel.topic = ":negative_squared_cross_mark: 아직 모드가 업로드 되지 않았습니다";
        modChannel.parent_id = JSettings.getInstance().getModCategory() + "";
        guild.createChannel(modChannel).whenReady(channel -> this.channel = ((Channel) channel).id());
        snowflakes.add(modChannel);
        for(Lazy lazy : snowflakes) lazy.await();
        datafile = new File(Settings.getInstance().getModDataPath(), name);
        save();
        announce();
    }

    public Channel getChannel() {
        return bot.getAPI().getChannel(channel);
    }

    public long getChannelId() {
        return channel;
    }

    public Channel getEmptyChannel() {
        Channel channel = new Channel();
        channel.api = bot.getAPI();
        channel.id = this.channel + "";
        return channel;
    }

    public String releasePing() {
        return "<@&" + roles.getReleaseRole() + ">";
    }

    public String progressPing() {
        return "<@&" + roles.getProgressRole() + ">";
    }

    public String betaReleasePing() {
        return "<@&" + roles.getBetaReleaseRole() + ">";
    }

    public String LastReleaseURL(boolean beta) {
        return String.format("https://discord.com/channels/%d/%d/%d", JSettings.getInstance().getGuildId(), channel, releaseMessage.get(beta ? getBetaVersion() : getVersion()));
    }

    public Message getLastRelease(boolean beta) {
        return getEmptyChannel().getMessage(getLastReleaseId(beta));
    }

    public long getLastReleaseId(boolean beta) {
        return releaseMessage.get(beta ? getBetaVersion() : getVersion());
    }

    public void addRelease(Version version, long id) {
        releaseMessage.put(version, id);
    }

    public void announce() {
        Message message = new Message(" ");
        message.setComponents(new ActionRow(
            new Button().label("최신버전으로 이동").style(ButtonStyle.LINK).url(getVersion() != null ?
                LastReleaseURL(false) : DiscordBot.SAMPLE_URL).disabled(getVersion() == null),
            new Button().label("최신버전으로 이동(베타)").style(ButtonStyle.LINK).url(getBetaVersion() != null ?
                LastReleaseURL(true) : DiscordBot.SAMPLE_URL).disabled(getBetaVersion() == null),
            new Button("release-" + id, "업로드 핑 받기"),
            new Button("progress-" + id, "근황 핑 받기"),
            new Button("beta-" + id, "베타 핑 받기")
        ));
        bot.getAPI().sendMessage(channel, message).whenReady(lazy -> {
            long last = lastAnnounce;
            lastAnnounce = message.id();
            try {
                save();
            } catch (IOException e) {
                throw new RuntimeException(e);
            } finally {
                if(last != -1) getEmptyChannel().deleteMessage(last);
            }
        });
    }

    public void announce(RawChannel rawChannel) {
        ModRoles roles1 = getRoles(rawChannel.getGuild());
        Message message = new Message(" ");
        List<Component> components = new ArrayList<>();
        String url = getVersion() != null && rawChannel.getReleaseMessage().containsKey(getVersion()) ? rawChannel.getMessageUrl(getVersion()) : DiscordBot.SAMPLE_URL;
        components.add(new Button().label("최신버전으로 이동").style(ButtonStyle.LINK).url(url).disabled(url.equals(DiscordBot.SAMPLE_URL)));
        if(rawChannel.isBeta()) {
            url = getBetaVersion() != null && rawChannel.getReleaseMessage().containsKey(getBetaVersion()) ? rawChannel.getMessageUrl(getBetaVersion()) : DiscordBot.SAMPLE_URL;
            components.add(new Button().label("최신버전으로 이동(베타)").style(ButtonStyle.LINK).url(url).disabled(url.equals(DiscordBot.SAMPLE_URL)));
        }
        if(roles1 != null) {
            if(roles1.getReleaseRole() != -1) components.add(new Button("release-" + id, "업로드 핑 받기"));
            if(roles1.getProgressRole() != -1) components.add(new Button("progress-" + id, "근황 핑 받기"));
            if(roles1.getBetaReleaseRole() != -1) components.add(new Button("beta-" + id, "베타 핑 받기"));
        }
        message.setComponents(new ActionRow(components.toArray(new Component[0])));
        bot.getAPI().sendMessage(rawChannel.getChannel(), message).whenReady(lazy -> {
            long last = rawChannel.getLastAnnounce();
            rawChannel.setLastAnnounce(message.id());
            try {
                save();
            } catch (IOException e) {
                throw new RuntimeException(e);
            } finally {
                if(last != -1) rawChannel.getEmptyChannel().deleteMessage(last);
            }
        });
    }

    public void delete() throws IOException {
        Role role = new Role();
        role.id = roles.getReleaseRole() + "";
        Guild guild = new Guild();
        guild.api = bot.getAPI();
        guild.id = JSettings.getInstance().getGuildId() + "";
        guild.deleteRole(role);
        role.id = roles.getProgressRole() + "";
        guild.deleteRole(role);
        role.id = roles.getBetaReleaseRole() + "";
        guild.deleteRole(role);
        bot.getAPI().sendMessage(channel, new Message("# 이 채널은 아카이브 되었습니다"));
        Channel ch = getEmptyChannel();
        ch.id = channel + "";
        ch.topic = ":no_entry_sign: 이 모드는 아카이브 되었습니다.";
        ch.modify();
        if(lastAnnounce != -1) ch.deleteMessage(lastAnnounce);
        deleted = true;
    }

    public void setLatestVersion(Version version) throws IOException {
        ConnectOtherLib.setVersion(this, version);
        setVersion(version);
        if(getBetaVersion() == null || version.isUpper(getBetaVersion())) setLatestBetaVersion(version);
    }

    public void setLatestBetaVersion(Version betaVersion) throws IOException {
        ConnectOtherLib.setBetaVersion(this, betaVersion);
        setBetaVersion(betaVersion);
    }

    public long getReleaseRole() {
        return roles.getReleaseRole();
    }

    public long getProgressRole() {
        return roles.getProgressRole();
    }

    public long getBetaReleaseRole() {
        return roles.getBetaReleaseRole();
    }

    public ModRoles getRoles(long guild) {
        return guild == JSettings.getInstance().getGuildId() ? roles : additionalRoles.get(guild);
    }

    public ModRoles getRolesOrNew(long guild) {
        ModRoles roles = getRoles(guild);
        if(roles == null) {
            roles = new ModRoles();
            additionalRoles.put(guild, roles);
        }
        return roles;
    }

    public void setRoles(long guild, ModRoles roles) throws IOException {
        additionalRoles.put(guild, roles);
        save();
    }

    public void setId(int id) throws IOException {
        this.id = id;
        save();
    }

    public void setChannel(long channel) throws IOException {
        this.channel = channel;
        save();
    }

    public void setReleaseRole(long releaseRole) throws IOException {
        roles.setReleaseRole(releaseRole);
        save();
    }

    public void setProgressRole(long progressRole) throws IOException {
        roles.setProgressRole(progressRole);
        save();
    }

    public void setBetaReleaseRole(long betaReleaseRole) throws IOException {
        roles.setBetaReleaseRole(betaReleaseRole);
        save();
    }

    public void setLastAnnounce(long lastAnnounce) throws IOException {
        this.lastAnnounce = lastAnnounce;
        save();
    }

    public void setBetaLinkable(boolean betaLinkable) throws IOException {
        this.betaLinkable = betaLinkable;
        save();
    }

    public boolean addChannel(RawChannel channel) throws IOException {
        if(additionalChannels.contains(channel)) return false;
        additionalChannels.add(channel);
        save();
        return true;
    }

    public boolean removeChannel(RawChannel channel) throws IOException {
        if(additionalChannels.remove(channel)) {
            save();
            return true;
        }
        return false;
    }

    public void removeRoles(long guild) throws IOException {
        additionalRoles.remove(guild);
        save();
    }

    public void setDiscordDL(String discordDL) throws IOException {
        this.discordDL = discordDL;
        save();
    }

    public void setPrivateMod(boolean privateMod) throws IOException {
        this.privateMod = privateMod;
        save();
    }

    public void setDeleted(boolean deleted) throws IOException {
        this.deleted = deleted;
        save();
    }
}
