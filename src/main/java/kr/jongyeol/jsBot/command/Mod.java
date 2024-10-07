package kr.jongyeol.jsBot.command;

import kr.jongyeol.jsBot.JModData;
import kr.jongyeol.jsBot.LogBuilder;
import kr.jongyeol.jsBot.SlashCommandAdapter;
import kr.jongyeol.jsBot.Utility;
import kr.jongyeol.jsBot.data.ModRoles;
import kr.jongyeol.jsBot.data.RawChannel;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.entity.command.Option;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.OptionType;
import mx.kenzie.eris.api.magic.Permission;

import java.util.Map;

public class Mod implements SlashCommandAdapter, ModOptionCommand {
    @Override
    public String[] getCommandNames() {
        return new String[] { "mod" };
    }

    @Override
    public Command getCommandData(Command data) {
        Option nameOption = getModNameOptionPublic();
        Option wantRole = Option.ofStrings("wantrole", "추가할 역할",
            new Option.Choice<>("release", "업로드 핑"),
            new Option.Choice<>("progress", "근황 핑"),
            new Option.Choice<>("beta", "베타 핑")
        );
        Option channel = new Option("channel", "채널", OptionType.CHANNEL);
        return data.description("모드 관련 명령어입니다.").dm_permission(false)
            .permissions(Permission.MANAGE_GUILD)
            .options(new Option[]{
                Option.subCommand("addchannel", "채널 추가", nameOption, channel,
                    new Option("beta", "베타 추가", OptionType.BOOLEAN).required(false)
                ),
                Option.subCommand("removechannel", "채널 제거", nameOption, channel),
                Option.subCommand("setrole", "역할 추가", nameOption, wantRole,
                    new Option("role", "역할", OptionType.ROLE)
                ),
                Option.subCommand("removerole", "역할 제거", nameOption, wantRole)
            });
    }

    @Override
    public boolean guildOnly() {
        return false;
    }

    @Override
    public void onCommand(Interaction event) throws Throwable {
        String subCommand = event.data.options[0].name;
        Map<String, Object> options = SlashCommandAdapter.getOptions(event.data.options[0].options);
        String name = (String) options.get("modname");
        JModData modData = JModData.getMod(name);
        if(!checkMod(event, modData)) return;
        switch(subCommand) {
            case "addchannel":
                RawChannel channel = new RawChannel(Long.parseLong(event.guild_id), Long.parseLong((String) options.get("channel")), Boolean.TRUE.equals(options.get("beta")));
                if(modData.addChannel(channel)) {
                    modData.announce(channel);
                    event.reply(Utility.getChannelMention(channel.getChannel()) + " 채널에 " + name + " 모드 알림을 추가했습니다.");
                    new LogBuilder(event.getSource(), "채널에 모드 알림을 추가했습니다")
                        .addField("user", event.getSource())
                        .addBlankField(true)
                        .addField("channel", event.getChannel())
                        .addField("id", modData.getId())
                        .addField("name", name)
                        .addField("modChannel", modData.getChannel())
                        .addField("guild", event.guild_id)
                        .addField("targetChannel", channel.getChannelUrl())
                        .addField("beta", channel.isBeta())
                        .send();
                } else event.reply(Utility.getChannelMention(channel.getChannel()) + " 채널에 이미 " + name + " 모드 알림이 있습니다.");
                break;
            case "removechannel":
                RawChannel removeChannel = new RawChannel(Long.parseLong(event.guild_id), Long.parseLong((String) options.get("channel")), false);
                if(modData.removeChannel(removeChannel)) {
                    event.reply(Utility.getChannelMention(removeChannel.getChannel()) + " 채널에 " + name + " 모드 알림을 제거했습니다.");
                    new LogBuilder(event.getSource(), "채널에 모드 알림을 제거했습니다")
                        .addField("user", event.getSource())
                        .addBlankField(true)
                        .addField("channel", event.getChannel())
                        .addField("id", modData.getId())
                        .addField("name", name)
                        .addField("modChannel", modData.getChannel())
                        .addField("guild", event.guild_id)
                        .addBlankField(true)
                        .addField("targetChannel", removeChannel.getChannelUrl())
                        .send();
                } else event.reply(Utility.getChannelMention(removeChannel.getChannel()) + " 채널에 " + name + " 모드 알림이 없습니다.");
                break;
            case "setrole":
                long guild = Long.parseLong(event.guild_id);
                ModRoles roles = modData.getRolesOrNew(guild);
                switch((String) options.get("wantrole")) {
                    case "release":
                        roles.setReleaseRole((long) options.get("role"));
                        break;
                    case "progress":
                        roles.setProgressRole((long) options.get("role"));
                        break;
                    case "beta":
                        roles.setBetaReleaseRole((long) options.get("role"));
                        break;
                }
                modData.setRoles(guild, roles);
                modData.getAdditionalChannels().stream().filter(addChannel -> addChannel.getGuild() == guild).forEach(modData::announce);
                modData.save();
                event.reply("역할을 설정했습니다.");
                new LogBuilder(event.getSource(), "역할을 설정했습니다.")
                    .addField("user", event.getSource())
                    .addBlankField(true)
                    .addField("channel", event.getChannel())
                    .addField("id", modData.getId())
                    .addField("name", name)
                    .addField("modChannel", modData.getChannel())
                    .addField("guild", event.guild_id)
                    .addField("role", options.get("role"))
                    .addField("wantRole", (String) options.get("wantrole"))
                    .send();
                break;
            case "removerole":
                long guild0 = Long.parseLong(event.guild_id);
                ModRoles roles1 = modData.getRoles(guild0);
                boolean removed = false;
                if(roles1 != null) {
                    switch((String) options.get("wantrole")) {
                        case "release":
                            if(roles1.getReleaseRole() != -1) {
                                roles1.setReleaseRole(-1);
                                removed = true;
                            }
                            break;
                        case "progress":
                            if(roles1.getProgressRole() != -1) {
                                roles1.setProgressRole(-1);
                                removed = true;
                            }
                            break;
                        case "beta":
                            if(roles1.getBetaReleaseRole() != -1) {
                                roles1.setBetaReleaseRole(-1);
                                removed = true;
                            }
                            break;
                    }
                    if(removed) {
                        modData.getAdditionalChannels().stream().filter(addChannel -> addChannel.getGuild() == guild0).forEach(modData::announce);
                        if(roles1.notSet()) modData.removeRoles(Long.parseLong(event.guild_id));
                        else modData.save();
                        event.reply("역할을 제거했습니다.");
                        new LogBuilder(event.getSource(), "역할을 제거했습니다.")
                            .addField("user", event.getSource())
                            .addBlankField(true)
                            .addField("channel", event.getChannel())
                            .addField("id", modData.getId())
                            .addField("name", name)
                            .addField("modChannel", modData.getChannel())
                            .addField("guild", event.guild_id)
                            .addField("wantRole", (String) options.get("wantrole"))
                            .send();
                    } else event.reply("역할이 없습니다.");
                }
                break;
        }
    }
}
