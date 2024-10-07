package kr.jongyeol.jsBot.command;

import kr.jongyeol.jaServer.data.CustomDownloadLink;
import kr.jongyeol.jaServer.data.GithubDownloadLink;
import kr.jongyeol.jsBot.LogBuilder;
import kr.jongyeol.jsBot.JModData;
import kr.jongyeol.jsBot.SlashCommandAdapter;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.entity.command.Option;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.MessageFlags;
import mx.kenzie.eris.api.magic.OptionType;
import mx.kenzie.eris.api.magic.Permission;

import java.util.HashMap;
import java.util.Map;

public class AddMod implements SlashCommandAdapter {
    @Override
    public String[] getCommandNames() {
        return new String[] { "addmod" };
    }

    @Override
    public Command getCommandData(Command data) {
        return data.permissions(Permission.MANAGE_GUILD)
            .description("모드를 생성합니다.")
            .options(new Option[]{
                new Option("name", "이름", OptionType.STRING),
                Option.ofStrings("link", "링크",
                    new Option.Choice<>("github", "github"),
                    new Option.Choice<>("custom", "custom"))
            });
    }

    @Override
    public boolean guildOnly() {
        return true;
    }

    @Override
    public void onCommand(Interaction event) throws Exception {
        Map<String, Object> options = SlashCommandAdapter.getOptions(event.data.options);
        String name = (String) options.get("name");
        String link = (String) options.get("link");
        JModData modData = new JModData(name, link.equals("github") ? new GithubDownloadLink(name) : new CustomDownloadLink(new HashMap<>()));
        new LogBuilder(event.getSource(), "모드를 생성했습니다")
            .addField("user", event.getSource())
            .addBlankField()
            .addField("channel", event.getChannel())
            .addField("id", modData.getId())
            .addBlankField()
            .addField("name", name)
            .addField("modChannel", modData.getChannel())
            .addBlankField()
            .addField("releaseRole", modData.releasePing())
            .addField("progressRole", modData.progressPing())
            .addField("releaseBetaRole", modData.betaReleasePing())
            .send();
        event.reply(new Message(name + "모드를 생성했습니다.").withFlag(MessageFlags.EPHEMERAL));
    }
}
