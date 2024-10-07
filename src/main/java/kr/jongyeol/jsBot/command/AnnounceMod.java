package kr.jongyeol.jsBot.command;

import kr.jongyeol.jsBot.JModData;
import kr.jongyeol.jsBot.LogBuilder;
import kr.jongyeol.jsBot.SlashCommandAdapter;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.entity.command.Option;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.MessageFlags;
import mx.kenzie.eris.api.magic.Permission;

public class AnnounceMod implements SlashCommandAdapter, ModOptionCommand {
    @Override
    public String[] getCommandNames() {
        return new String[] { "announcemod" };
    }

    @Override
    public Command getCommandData(Command data) {
        return data.permissions(Permission.MANAGE_GUILD)
            .description("모드 기본사항을 전송합니다.")
            .options(new Option[]{ getModNameOptionPrivate() });
    }

    @Override
    public boolean guildOnly() {
        return true;
    }

    @Override
    public void onCommand(Interaction event) throws Exception {
        String name = (String) event.data.options[0].value;
        JModData modData = JModData.getMod(name);
        if(!checkMod(event, modData)) return;
        modData.announce();
        new LogBuilder(event.getSource(), "모드 기본사항을 전송했습니다")
            .addField("user", event.getSource())
            .addField("channel", event.getChannel())
            .addBlankField()
            .addField("id", modData.getId())
            .addField("name", name)
            .addField("modChannel", modData.getChannel())
            .addBlankField()
            .addField("latestVersion", modData.getVersion())
            .addField("latestBetaVersion", modData.getBetaVersion())
            .send();
        event.reply(new Message(name + "모드 기본사항을 전송했습니다.").withFlag(MessageFlags.EPHEMERAL));
    }
}
