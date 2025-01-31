package kr.jongyeol.jsBot.command;

import kr.jongyeol.jsBot.DiscordBot;
import kr.jongyeol.jsBot.LogBuilder;
import kr.jongyeol.jsBot.SlashCommandAdapter;
import mx.kenzie.eris.api.entity.Member;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.entity.command.Option;
import mx.kenzie.eris.api.entity.guild.ModifyMember;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.MessageFlags;
import mx.kenzie.eris.api.magic.OptionType;
import mx.kenzie.eris.api.magic.Permission;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Map;

public class Timeout implements SlashCommandAdapter {
    @Override
    public String[] getCommandNames() {
        return new String[] { "timeout", "타임아웃" };
    }

    @Override
    public Command getCommandData(Command data) {
        return data.permissions(Permission.MODERATE_MEMBERS)
            .description("타임아웃을 설정합니다.")
            //.dm_permission(false)
            .options(new Option[]{
                new Option("user", "유저", OptionType.USER),
                new Option("time", "시간", OptionType.INTEGER),
                Option.ofStrings("timeunit", "시간단위(기본: 일)",
                    new Option.Choice<>("밀리초", "milisecond"),
                    new Option.Choice<>("초", "second"),
                    new Option.Choice<>("분", "minute"),
                    new Option.Choice<>("시간", "hour"),
                    new Option.Choice<>("일", "day"),
                    new Option.Choice<>("주", "week"),
                    new Option.Choice<>("월", "month")
                ).required(false)
            });
    }

    @Override
    public boolean guildOnly() {
        return false;
    }

    @Override
    public void onCommand(Interaction event) {
        try {
            Map<String, Object> options = SlashCommandAdapter.getOptions(event.data.options);
            String user = (String) options.get("user");
            int time = (int) options.get("time");
            ModifyMember modifyMember = new ModifyMember();
            TimeUnit timeUnit = TimeUnit.valueOf(((String) options.get("timeunit")).toUpperCase());
            Date date = new Date(System.currentTimeMillis() + time * timeUnit.multiplier);
            SimpleDateFormat isoFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSXXX");
            modifyMember.communication_disabled_until = isoFormat.format(date);
            Member member = DiscordBot.bot.getAPI().modifyMember(event.guild_id, user, modifyMember);
            member.await();
            if(member.error() != null) {
                event.reply(new Message("오류가 발생했습니다: " + member.error()).withFlag(MessageFlags.EPHEMERAL));
                LogBuilder.newError(event.getSource())
                    .addField("EventType", "On Slash Command Interaction(API Request)")
                    .addField("Type", event.type)
                    .addField("Command", event.data.name)
                    .addField("Channel", event.getChannel())
                    .addField(member.error())
                    .send();
                return;
            }
            event.reply(new Message("해당 유저에게 <t:" + date.getTime() / 1000 + ":R>까지 타임아웃을 설정했습니다.").withFlag(MessageFlags.EPHEMERAL));
        } catch (Exception e) {
            event.reply(new Message("오류가 발생했습니다: " + e.getMessage()).withFlag(MessageFlags.EPHEMERAL));
            LogBuilder.newError(event.getSource())
                .addField("EventType", "On Slash Command Interaction")
                .addField("Type", event.type)
                .addField("Command", event.data.name)
                .addField("Channel", event.getChannel())
                .addField(e)
                .send();
        }
    }

    public enum TimeUnit {
        MILISECOND(1),
        SECOND(1000),
        MINUTE(60000),
        HOUR(3600000),
        DAY(86400000),
        WEEK(604800000),
        MONTH(2592000000L);

        public final long multiplier;

        TimeUnit(long multiplier) {
            this.multiplier = multiplier;
        }
    }
}
