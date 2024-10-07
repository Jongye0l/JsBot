package kr.jongyeol.jsBot.command;

import kr.jongyeol.jsBot.*;
import mx.kenzie.eris.api.entity.Message;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.entity.message.ActionRow;
import mx.kenzie.eris.api.entity.message.Button;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.MessageFlags;
import mx.kenzie.eris.api.magic.Permission;

public class SendAnnounce implements SlashCommandAdapter {
    @Override
    public String[] getCommandNames() {
        return new String[] { "sendmodannounce" };
    }

    @Override
    public Command getCommandData(Command data) {
        return data.permissions(Permission.MANAGE_GUILD)
            .description("핑 알림을 전송합니다.");
    }

    @Override
    public boolean guildOnly() {
        return true;
    }

    @Override
    public void onCommand(Interaction event) throws Exception {
        Message message = new Message("# 원하시는 알림을 선택해주세요");
        message.setComponents(new ActionRow(
            new Button("release-all", "전체 모드핑"),
            new Button("release-new", "신규 모드핑"),
            new Button("progress-all", "전체 근황핑"),
            new Button("beta-all", "전체 베타핑")
        ));
        event.getChannel().send(message);
        event.reply(new Message("알림 메시지를 전송했습니다.").withFlag(MessageFlags.EPHEMERAL));
        new LogBuilder(event.getSource(), "알림 메시지를 전송했습니다")
            .addField("user", event.getSource())
            .addField("channel", event.getChannel()).send();
    }
}
