package kr.jongyeol.jsBot;

import kr.jongyeol.jaServer.ConnectOtherLib;
import kr.jongyeol.jaServer.Logger;
import kr.jongyeol.jaServer.Variables;
import kr.jongyeol.jaServer.data.*;
import kr.jongyeol.jsBot.data.ModRoles;
import kr.jongyeol.jsBot.data.RawChannel;
import mx.kenzie.eris.Bot;
import mx.kenzie.eris.api.entity.Channel;
import mx.kenzie.eris.api.entity.Embed;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.guild.ModifyMember;
import mx.kenzie.eris.api.entity.message.ActionRow;
import mx.kenzie.eris.api.entity.message.Button;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.event.Ready;
import mx.kenzie.eris.api.event.guild.member.AddGuildMember;
import mx.kenzie.eris.api.event.guild.member.RemoveGuildMember;
import mx.kenzie.eris.api.event.guild.member.UpdateGuildMember;
import mx.kenzie.eris.api.event.message.ReceiveMessage;
import mx.kenzie.eris.api.magic.ButtonStyle;
import mx.kenzie.eris.api.magic.MessageFlags;

import java.io.IOException;
import java.util.*;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import static kr.jongyeol.jsBot.DiscordBot.bot;

public class EventListener {
    private static boolean loaded;
    private static final Pattern pattern = Pattern.compile("<(\\d+)>");
    private static Map<Long, String> boostMap = new HashMap<>();
    private static final String MAGIC_MESSAGE = " ||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B||||\u200B|| _ _ _ _ _ _ ";

    public static void register(Bot bot) {
        bot.registerListener(AddGuildMember.class, EventListener::onGuildMemberJoin);
        bot.registerListener(RemoveGuildMember.class, EventListener::onGuildMemberRemove);
        bot.registerListener(UpdateGuildMember.class, EventListener::onGuildMemberUpdateBoostTime);
        bot.registerListener(Interaction.class, EventListener::onInteraction);
        bot.registerListener(Ready.class, EventListener::onReady);
        bot.registerListener(ReceiveMessage.class, EventListener::onMessageReceived);
    }

    public static void onGuildMemberJoin(AddGuildMember event) {
        JSettings settings = JSettings.getInstance();
        if(Long.parseLong(event.guild_id) != settings.getGuildId()) return;
        Embed embed = new Embed();
        embed.author.name = Utility.getUserName(event.user);
        embed.author.icon_url = Utility.getAvatar(event.user);
        embed.description = event.user.mention() + " 님이 서버에 들어오셨습니다.";
        embed.color(0x00FF00);
        bot.getAPI().sendMessage(JSettings.getInstance().getJoinLogChannel(), new Message(embed));
        ModifyMember member = new ModifyMember();
        long[] roles = settings.getRoles();
        member.roles = new String[roles.length + 1];
        for(int i = 0; i < roles.length; i++) member.roles[i] = roles[i] + "";
        member.roles[roles.length] = event.user.bot ? settings.getBotRole() + "" : settings.getUserRole() + "";
        bot.getAPI().modifyMember(event.guild_id, event.user, member);
    }

    public static void onGuildMemberRemove(RemoveGuildMember event) {
        if(Long.parseLong(event.guild_id) != JSettings.getInstance().getGuildId()) return;
        Embed embed = new Embed();
        embed.author.name = Utility.getUserName(event.user);
        embed.author.icon_url = Utility.getAvatar(event.user);
        embed.description = event.user.mention() + " 님이 서버를 떠나셨습니다.";
        embed.color(0xFF0000);
        bot.getAPI().sendMessage(JSettings.getInstance().getJoinLogChannel(), new Message(embed));
    }

    public static void onGuildMemberUpdateBoostTime(UpdateGuildMember event) {
        if(event.premium_since == null) return;
        if(boostMap.containsKey(event.user.id()) && boostMap.get(event.user.id()).equals(event.premium_since)) return;
        boostMap.put(event.user.id(), event.premium_since);
        Embed embed = new Embed();
        embed.author.name = Utility.getUserName(event.user);
        embed.author.icon_url = Utility.getAvatar(event.user);
        embed.description = event.user.mention() + " 님이 서버 부스트를 해주셨습니다!";
        bot.getAPI().sendMessage(JSettings.getInstance().getBoostLogChannel(), new Message(embed));
    }

    public static void onInteraction(Interaction event) {
        if(event.data.custom_id == null) return;
        if(event.data.component_type == null) {
            //onModalInteraction(event);
            return;
        }
        switch(event.data.component_type) {
            case 2 -> onButtonInteraction(event);
            //case 3 -> onStringSelectInteraction(event);
        }
    }

    public static void onButtonInteraction(Interaction event) {
        Variables.executor.execute(() -> {
            try {
                String[] strings = event.data.custom_id.split("-");
                try {
                    if(strings[0].equals("progress")) ModAnnounce.progressButton(strings, event);
                    if(strings[0].equals("release")) ModAnnounce.releaseButton(strings, event);
                    if(strings[0].equals("beta")) ModAnnounce.betaButton(strings, event);
                    if(strings[0].equals("apply")) {
                        JModData mod = JModData.getMod(Integer.parseInt(strings[1]));
                        if(mod == null) event.reply(new Message("모드를 찾을 수 없습니다.").withFlag(MessageFlags.EPHEMERAL));
                        else {
                            event.reply(new Message("모드 적용 방식이 변경되어 아래 버튼을 눌러 링크로 이동하셔야 됩니다.",
                                new Button().label("링크로 이동(서버 1)").style(ButtonStyle.LINK).url("https://jalib.jongyeol.kr/modApplicator/" + mod.getName() + "/" + strings[2]),
                                new Button().label("링크로 이동(서버 2)").style(ButtonStyle.LINK).url("https://jalib2.jongyeol.kr/modApplicator/" + mod.getName() + "/" + strings[2]))
                                .withFlag(MessageFlags.EPHEMERAL));
                            new LogBuilder(event.getSource(), "모드 적용 요청 링크를 보냈습니다.")
                                .addField("mod", mod.getName())
                                .addBlankField(true)
                                .addField("version", strings[2])
                                .addField("user", event.getSource())
                                .addBlankField(true)
                                .addField("channel", event.getChannel())
                                .send();
                        }
                    }
                } catch (Exception e) {
                    Logger.MAIN_LOGGER.error("Error In Id: " + strings[1]);
                    throw e;
                }
            } catch (Exception e) {
                event.reply(new Message(new Embed("오류가 발생하였습니다.", e.getMessage()).color(0xFF0000)).withFlag(MessageFlags.EPHEMERAL));
                Logger.MAIN_LOGGER.error(e);
                LogBuilder.newError(event.getSource())
                    .addField("EventType", "On Button Interaction")
                    .addField("Interaction Type", event.type)
                    .addField("Channel", event.getChannel())
                    .addField("Message", event.getMessage())
                    .addField("Custom Id", event.data.custom_id)
                    .addField(e)
                    .send();
            }
        });
    }

    public static void onReady(Ready event) {
        if(loaded) return;
        try {
            JModData.initialize();
            DiscordBot.loadCommands();
            loaded = true;
        } catch (Exception e) {
            Logger.MAIN_LOGGER.error(e);
            LogBuilder.newError()
                .addField("EventType", "On Ready")
                .addField(e)
                .send();
        }
    }

    public static void onMessageReceived(ReceiveMessage event) {
        try {
            checkModMessage(event);
            checkPingMessage(event);
        } catch (Exception e) {
            Logger.MAIN_LOGGER.error(e);
            Channel channel = bot.getAPI().getChannel(event.channel_id);
            channel.await();
            LogBuilder.newError(event.member.user)
                .addField("EventType", "On Message Received")
                .addField("Channel", channel)
                .addField("Message", event)
                .addField("Author", event.member.user)
                .addField(e)
                .send();
        }
    }

    private static void checkPingMessage(ReceiveMessage event) throws Exception {
        if(event.webhook_id != null || !event.content.startsWith("!") || Long.parseLong(event.guild_id) != JSettings.getInstance().getGuildId()) return;
        if(Arrays.stream(event.member.roles).noneMatch(s -> Long.parseLong(s) == 1287366163667357829L)) return;
        String[] line = event.content.split("\n");
        String[] data = line[0].split(" ");
        switch(data[0]) {
            case "!release", "!beta" -> {
                boolean beta = data[0].equals("!beta");
                JModData mod = JModData.getMod(Long.parseLong(event.channel_id));
                if(mod == null) return;
                Version version = new Version(data[1]);
                Version showVersion = version.build == 0 ? new Version(version.major, version.minor) : version.revision == -1 ? version : new Version(version.major, version.minor, version.build);
                String behind = beta ? " beta" + data[2] : "";
                Version latestVersion = beta ? mod.getBetaVersion() : mod.getVersion();
                if(latestVersion != null && version.isUpper(latestVersion)) return;
                String link = mod.getDiscordDL();
                DownloadLink link1 = mod.getDownloadLink();
                if(link == null) {
                    if(mod.isBetaLinkable() && beta) link = "";
                    else {
                        if(link1 instanceof GithubDownloadLink link2) link = link2.getLink(version);
                        else if(link1 instanceof CustomDownloadLink link2) {
                            if(!link2.links.containsKey(version) || data.length > (beta ? 3 : 2)) {
                                link = data[beta ? 3 : 2];
                                link2.links.put(version, link);
                                ConnectOtherLib.setDownloadLink(mod, link1);
                            } else link = "";
                        } else link = "";
                    }
                }
                StringBuilder builder = new StringBuilder(String.format("# %s %s%s\n", mod.getName(), showVersion, behind));
                for(int i = 1; i < line.length; i++) builder.append("\n").append(customMessage(line[i]));
                String text = builder.toString();
                builder.append(MAGIC_MESSAGE).append(beta ? "" : mod.releasePing()).append(mod.betaReleasePing())
                    .append(beta ? "" : JSettings.getAllReleasePing()).append(JSettings.getAllBetaReleasePing());
                if(!beta && latestVersion == null) builder.append(JSettings.getNewReleasePing());
                boolean source = link1 instanceof GithubDownloadLink;
                Message message = new Message(builder.toString());
                Button sourceButton = new Button().label("소스 코드").url(source ? ((GithubDownloadLink) link1).getSourceLink(version) :
                    DiscordBot.SAMPLE_URL).style(ButtonStyle.LINK).disabled(!source);
                Button downloadButton = new Button().label("다운로드").url(link).style(ButtonStyle.LINK).disabled(link.isEmpty());
                Button applyButton1 = new Button().label("모드 적용(서버 1)").url("https://jalib.jongyeol.kr/modApplicator/" + mod.getName() + "/" + version).style(ButtonStyle.LINK);
                Button applyButton2 = new Button().label("모드 적용(서버 2)").url("https://jalib2.jongyeol.kr/modApplicator/" + mod.getName() + "/" + version).style(ButtonStyle.LINK);
                message.setComponents(new ActionRow(sourceButton, downloadButton, applyButton1, applyButton2));
                message.attachments = event.attachments;
                message.withFlag(MessageFlags.SUPPRESS_EMBEDS);
                if(version.equals(latestVersion)) {
                    message.api = bot.getAPI();
                    message.id = mod.getLastReleaseId(beta) + "";
                    message.channel_id = event.channel_id;
                    message.edit();
                    Channel channel = mod.getChannel();
                    event.delete();
                    sendReleaseMessageAdditionChannel(beta, mod, text, message, true, version, sourceButton, downloadButton, applyButton1, applyButton2);
                    message.await();
                    channel.await();
                    new LogBuilder(event.member.user, "모드 업로드를 수정하였습니다.")
                        .addField("name", mod.getName())
                        .addField("id", mod.getId())
                        .addField("beta", mod.betaReleasePing())
                        .addField("user", event.member.user)
                        .addField("channel", channel)
                        .addField("message", message)
                        .addField("latestVersion", latestVersion)
                        .addField("version", version)
                        .addField("link", link)
                        .addField("attachment size", event.attachments.length)
                        .send();
                    break;
                }
                bot.getAPI().sendMessage(event.channel_id, message);
                Channel channel = mod.getEmptyChannel();
                channel.topic = String.format(":white_check_mark: 최신버전 : [%s](%s)", showVersion, link);
                channel.modify();
                event.delete();
                if(beta) mod.setLatestBetaVersion(version);
                else mod.setLatestVersion(version);
                sendReleaseMessageAdditionChannel(beta, mod, text, message, false, version, sourceButton, downloadButton, applyButton1, applyButton2);
                message.await();
                channel.await();
                mod.announce();
                mod.addRelease(version, message.id());
                new LogBuilder(event.member.user, "모드를 업로드 하였습니다.")
                    .addField("name", mod.getName())
                    .addField("id", mod.getId())
                    .addField("beta", mod.betaReleasePing())
                    .addField("user", event.member.user)
                    .addField("channel", channel)
                    .addField("message", message)
                    .addField("latestVersion", latestVersion)
                    .addField("version", version)
                    .addField("link", link)
                    .addField("attachment size", event.attachments.length)
                    .send();
                mod.save();
            }
            case "!progress" -> {
                JModData mod = JModData.getMod(Long.parseLong(event.channel_id));
                if(mod == null) return;
                StringBuilder builder = new StringBuilder();
                for(int i = 1; i < line.length; i++) builder.append(line[i]).append("\n");
                builder.replace(builder.length() - 1, builder.length() - 1, "");
                String text = builder.toString();
                builder.append(MAGIC_MESSAGE).append(mod.progressPing()).append(JSettings.getAllProgressPing());
                Message message = new Message(builder.toString());
                message.attachments = event.attachments;
                bot.getAPI().sendMessage(event.channel_id, message);
                Channel channel = mod.getChannel();
                event.delete();
                mod.getAdditionalChannels().forEach(ch -> {
                    StringBuilder builder1 = new StringBuilder(text);
                    ModRoles roles = mod.getRoles(ch.getGuild());
                    if(roles != null && roles.getProgressRole() != -1) builder1.append(MAGIC_MESSAGE).append(roles.getProgressPing());
                    Message message1 = new Message(builder1.toString());
                    message1.attachments = event.attachments;
                    bot.getAPI().sendMessage(ch.getChannel(), message1);
                });
                message.await();
                channel.await();
                new LogBuilder(event.member.user, "모드 근황을 업로드하였습니다.")
                    .addField("name", mod.getName())
                    .addField("id", mod.getId())
                    .addField("user", event.member.user)
                    .addField("channel", channel)
                    .addField("message", message)
                    .addField("attachment size", event.attachments.length)
                    .send();
            }
            default -> {
                if(Long.parseLong(event.guild_id) != JSettings.getInstance().getGuildId()) return;
                List<String> pingList = new ArrayList<>();
                for(String st : data) {
                    switch(st) {
                        case "!everyone" -> pingList.add("@everyone");
                        case "!here" -> pingList.add("@here");
                        case "!announce" -> pingList.add(JSettings.getAnnouncePing());
                        case "!simsim" -> pingList.add(JSettings.getSimsimPing());
                    }
                }
                if(pingList.isEmpty()) return;
                StringBuilder builder = new StringBuilder();
                for(int i = 1; i < line.length; i++) builder.append("\n").append(line[i]);
                builder.append(MAGIC_MESSAGE);
                for(String st : pingList) builder.append(st);
                Message message = new Message(builder.toString());
                message.attachments = event.attachments;
                bot.getAPI().sendMessage(event.channel_id, message);
                Channel channel = bot.getAPI().getChannel(event.channel_id);
                event.delete();
                message.await();
                LogBuilder logBuilder = new LogBuilder(event.member.user, "맨션을 하였습니다.")
                    .addField("user", event.member.user)
                    .addField("channel", channel)
                    .addField("message", message)
                    .addField("attachment size", event.attachments.length);
                for(String st : pingList) logBuilder.addField("ping", st);
                logBuilder.send();
            }
        }
    }

    private static void sendReleaseMessageAdditionChannel(boolean beta, JModData mod, String text, Message message, boolean edit, Version version,
                                                             Button sourceButton, Button downloadButton, Button applyButton1, Button applyButton2) {
        mod.getAdditionalChannels().forEach(ch -> {
            if(!ch.isBeta() && beta) return;
            Variables.executor.execute(() -> {
                StringBuilder builder1 = new StringBuilder(text);
                ModRoles roles = mod.getRoles(ch.getGuild());
                boolean magic = false;
                if(roles != null) {
                    if(roles.getReleaseRole() != -1 && !beta) {
                        builder1.append(MAGIC_MESSAGE).append(roles.getReleasePing());
                        magic = true;
                    }
                    if(roles.getBetaReleaseRole() != -1 && ch.isBeta()) {
                        if(!magic) builder1.append(MAGIC_MESSAGE);
                        builder1.append(roles.getBetaReleasePing());
                    }
                }
                Message message1 = new Message(builder1.toString());
                message1.setComponents(ch.isApply() ? new ActionRow(sourceButton, downloadButton, applyButton1, applyButton2) : new ActionRow(sourceButton, downloadButton));
                message1.attachments = message.attachments;
                message1.withFlag(MessageFlags.SUPPRESS_EMBEDS);
                if(edit) {
                    Long messageId = ch.getReleaseMessage().get(version);
                    if(messageId != null) {
                        message1.api = bot.getAPI();
                        message1.id = messageId + "";
                        message1.channel_id = ch.getChannel() + "";
                        message1.edit();
                        mod.announce(ch);
                        return;
                    }
                }
                bot.getAPI().sendMessage(ch.getChannel(), message1);
                message1.await();
                ch.getReleaseMessage().put(version, message1.id());
                mod.announce(ch);
                try {
                    mod.save();
                } catch (IOException e) {
                    Logger.MAIN_LOGGER.error(e);
                    LogBuilder.newError()
                        .addField("EventType", "Send Release Message Addition Channel")
                        .addField("Channel", ch.getChannel())
                        .addField("Message", message1)
                        .addField("Version", version)
                        .addField(e)
                        .send();
                }
            });
        });
    }

    private static void checkModMessage(ReceiveMessage event) {
        if(event.content.startsWith("!")) return;
        if(event.member != null && event.author.id() == bot.getAPI().getSelf().id()) return;
        long channelId = Long.parseLong(event.channel_id);
        JModData modData = JModData.getMod(channelId);
        if(modData != null) {
            modData.announce();
            return;
        }
        Variables.executor.execute(() -> {
            for(JModData mod : JModData.getModList()) {
                for(RawChannel rawChannel : mod.getAdditionalChannels()) {
                    if(rawChannel.getChannel() == channelId) {
                        mod.announce(rawChannel);
                        break;
                    }
                }
            }
        });
    }

    private static String customMessage(String message) {
        Matcher matcher = pattern.matcher(message);
        while(matcher.find()) {
            String st = matcher.group(1);
            message = message.replaceFirst("<" + st + ">", "<@" + st + ">");
        }
        return message;
    }
}
