package kr.jongyeol.jsBot;

import kr.jongyeol.jaServer.Logger;
import kr.jongyeol.jsBot.command.*;
import mx.kenzie.eris.Bot;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.magic.Intents;

import java.util.ArrayList;
import java.util.List;

public class DiscordBot {
    public static final List<SlashCommandAdapter> commands = new ArrayList<>();
    public static final String SAMPLE_URL = "https://jongyeol.kr/";
    public static Bot bot;

    public static void main(String[] args) {
        JSettings.load();
        bot = new Bot(JSettings.getInstance().getToken(), Intents.GUILD_MESSAGES, Intents.MESSAGE_CONTENT, Intents.GUILD_MEMBERS);
        EventListener.register(bot);
        bot.start();
    }

    public static void loadCommands() {
        commands.add(new AddMod());
        commands.add(new SendAnnounce());
        commands.add(new RemoveMod());
        commands.add(new AnnounceMod());
        commands.add(new EditModData());
        commands.add(new Mod());
        commands.add(new Timeout());
        for(SlashCommandAdapter command : commands)
            for(String name : command.getCommandNames())
                if(command.guildOnly()) bot.registerCommand(command.getCommandData(Command.slash(name, null)), JSettings.getInstance().getGuildId(), command);
                else bot.registerCommand(command.getCommandData(Command.slash(name, null)), command);
    }
}
