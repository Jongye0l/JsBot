package kr.jongyeol.jsBot;

import mx.kenzie.eris.api.command.CommandHandler;
import mx.kenzie.eris.api.entity.Embed;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.MessageFlags;

import java.util.HashMap;
import java.util.Map;

public interface SlashCommandAdapter extends CommandHandler {
    String[] getCommandNames();
    Command getCommandData(Command data);
    boolean guildOnly();

    static Map<String, Object> getOptions(Interaction.Option[] options) {
        Map<String, Object> map = new HashMap<>();
        for(Interaction.Option option : options) map.put(option.name, option.value);
        return map;
    }

    default void on(Interaction event) throws Throwable {
        try {
            if(event.type == 4) onAutoComplete(event);
            else onCommand(event);
        } catch (Throwable e) {
            if(event.type != 4) {
                Embed embed = new Embed("오류가 발생하였습니다.", e.getMessage());
                embed.color = 0xFF0000;
                event.reply(new Message(embed).withFlag(MessageFlags.EPHEMERAL));
            }
            LogBuilder.newError(event.getSource())
                .addField("EventType", "On Slash Command Interaction")
                .addField("Type", event.type)
                .addField("Command", event.data.name)
                .addField("Channel", event.getChannel())
                .addField((Exception) e)
                .send();
        }
    }

    void onCommand(Interaction event) throws Throwable;

    default void onAutoComplete(Interaction event) throws Throwable {
    }
}
