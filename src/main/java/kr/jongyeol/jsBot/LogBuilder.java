package kr.jongyeol.jsBot;

import mx.kenzie.eris.api.entity.*;
import mx.kenzie.eris.api.entity.message.Button;

import java.time.Instant;
import java.util.ArrayList;

import static kr.jongyeol.jsBot.DiscordBot.bot;

public class LogBuilder {
    private static final String nullMessage = "null";

    public static LogBuilder newError(User user) {
        return new LogBuilder(user, "오류가 발생하였습니다.", true);
    }

    public static LogBuilder newError() {
        return newError(bot.getAPI().getSelf());
    }

    private boolean inline = true;

    private final boolean error;
    private final Embed embed;

    public LogBuilder(User user, String message) {
        this(user, message, false);
    }

    public LogBuilder(String message) {
        this(bot.getAPI().getSelf(), message, false);
    }

    private LogBuilder(User user, String message, boolean error) {
        this.error = error;
        embed = new Embed();
        embed.author.name = Utility.getUserName(user);
        embed.author.icon_url = Utility.getAvatar(user);
        embed.color = error ? 0xFF0000 : 0x00FF00;
        embed.description = message;
        embed.footer.text = embed.author.name;
        embed.footer.icon_url = embed.author.icon_url;
        embed.timestamp = Instant.now().toString();
    }

    public LogBuilder setColor(int color) {
        embed.color = color;
        return this;
    }

    public LogBuilder setDefaultInline(boolean inline) {
        this.inline = inline;
        return this;
    }

    public LogBuilder addField(String name, String value) {
        return addField(name, value, inline);
    }

    public LogBuilder addField(String name, String value, boolean inline) {
        if(value == null) value = nullMessage;
        embed.addField(new Embed.Field(name, value, inline));
        return this;
    }

    public LogBuilder addField(String name, Object object) {
        return addField(name, object, inline);
    }

    public LogBuilder addField(String name, Object object, boolean inline) {
        return addField(name, object == null ? nullMessage : object.toString(), inline);
    }

    public LogBuilder addField(String name, Channel channel) {
        return addField(name, channel, inline);
    }

    public LogBuilder addField(String name, Channel channel, boolean inline) {
        if(channel.name == null) channel.await(60000);
        return addField(name, channel.mention() + "(" + channel.name + ")", inline);
    }

    public LogBuilder addField(String name, User user) {
        return addField(name, user, inline);
    }

    public LogBuilder addField(String name, User user, boolean inline) {
        return addField(name, user.mention() + "(" + Utility.getUserName(user) + ")", inline);
    }

    public LogBuilder addField(String name, Member member) {
        return addField(name, member.user, inline);
    }

    public LogBuilder addField(String name, Member member, boolean inline) {
        return addField(name, member.user, inline);
    }

    public LogBuilder addField(String name, int value) {
        return addField(name, value, inline);
    }

    public LogBuilder addField(String name, int value, boolean inline) {
        return addField(name, value + "", inline);
    }

    public LogBuilder addField(String name, mx.kenzie.eris.api.entity.Message message) {
        return addField(name, message, inline);
    }

    public LogBuilder addField(String name, Message message, boolean inline) {
        Channel channel = bot.getAPI().getChannel(message.channel_id);
        channel.await();
        return addField(name, "https://discord.com/channels/" + channel.guild_id + "/" +
            message.channel_id + "/" + message.id, inline);
    }

    public LogBuilder addField(String name, Button button) {
        return addField(name, button, inline);
    }

    public LogBuilder addField(String name, Button button, boolean inline) {
        return addField(name, button.label + "(" + button.custom_id + ")", inline);
    }

    public LogBuilder addField(String name, boolean value) {
        return addField(name, value, inline);
    }

    public LogBuilder addField(String name, boolean value, boolean inline) {
        return addField(name, value ? "true" : "false", inline);
    }

    public LogBuilder addField(String name, Role role) {
        return addField(name, role, inline);
    }

    public LogBuilder addField(String name, Role role, boolean inline) {
        return addField(name, role.mention() + "(" + role.name + ")", inline);
    }

    public LogBuilder addField(Exception e) {
        ArrayList<StringBuilder> builders = new ArrayList<>();
        StringBuilder trace = new StringBuilder();
        builders.add(trace);
        for(StackTraceElement element : e.getStackTrace()) {
            trace.append("\tat ").append(element).append("\n");
            if(trace.length() > 900) {
                trace = new StringBuilder();
                builders.add(trace);
            }
        }
        Embed.Field[] fields = new Embed.Field[builders.size() + 2];
        int i = 0;
        fields[i++] = new Embed.Field("Exception Type", e.getClass().getName());
        fields[i++] = new Embed.Field("Exception Message", e.getMessage());
        for(StringBuilder tracer : builders) fields[i++] = new Embed.Field("Exception Stack Trace", tracer.toString());
        embed.addField(fields);
        return this;
    }

    public LogBuilder addBlankField() {
        return addBlankField(false);
    }

    public LogBuilder addBlankField(boolean inline) {
        embed.addField(new Embed.Field("", "", inline));
        return this;
    }

    public void send() {
        bot.getAPI().sendMessage(JSettings.getInstance().getLogChannel(), new Message(error ? "<@447333460382842880>" : null, embed));
    }
}
