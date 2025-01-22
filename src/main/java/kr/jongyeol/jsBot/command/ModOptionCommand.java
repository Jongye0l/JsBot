package kr.jongyeol.jsBot.command;

import kr.jongyeol.jaServer.data.ModData;
import kr.jongyeol.jsBot.JModData;
import kr.jongyeol.jsBot.SlashCommandAdapter;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.command.Option;
import mx.kenzie.eris.api.entity.command.callback.Autocomplete;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.MessageFlags;

import java.util.Arrays;
import java.util.stream.Stream;

public interface ModOptionCommand extends SlashCommandAdapter {
    default Option getModNameOption() {
        return Option.ofStrings("modname0", "모드 이름").autocomplete(true);
    }

    default Option getModNameOptionPublic() {
        return Option.ofStrings("modname", "모드 이름").autocomplete(true);
    }

    default Option getModNameOptionPrivate() {
        return Option.ofStrings("modname1", "모드 이름").autocomplete(true);
    }

    default boolean checkMod(Interaction event, JModData modData) {
        if(modData == null) {
            event.reply(new Message("모드 데이터가 존재하지 않습니다.").withFlag(MessageFlags.EPHEMERAL));
            return false;
        }
        return true;
    }

    default void onAutoComplete(Interaction event) throws Throwable {
        respond(event, event.data.options);
    }

    default boolean respond(Interaction event, Interaction.Option[] options) {
        for(Interaction.Option option : options) {
            if(option.options != null) {
                if(respond(event, option.options)) return true;
                continue;
            }
            if(!option.focused) continue;
            event.respond(switch(option.name) {
                case "modname" -> new Autocomplete(Stream.of(ModData.getModNames()).filter(name -> name.toLowerCase().startsWith(((String) option.value).toLowerCase())).map(name ->
                    new Option.Choice<>(name, name)).toArray(Option.Choice[]::new));
                case "modname0" -> new Autocomplete(Arrays.stream(JModData.getModList()).filter(mod -> !mod.isPrivateMod() && mod.getName().toLowerCase().startsWith(((String) option.value).toLowerCase())).map(mod ->
                    new Option.Choice<>(mod.getName(), mod.getName())).toArray(Option.Choice[]::new));
                case "modname1" -> new Autocomplete(Arrays.stream(JModData.getModList()).filter(mod -> mod.getName().toLowerCase().startsWith(((String) option.value).toLowerCase())).map(mod ->
                    new Option.Choice<>(mod.getName(), mod.getName())).toArray(Option.Choice[]::new));
                default -> null;
            });
            return true;
        }
        return false;
    }
}
