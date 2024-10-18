package kr.jongyeol.jsBot.command;

import kr.jongyeol.jaServer.ConnectOtherLib;
import kr.jongyeol.jaServer.data.*;
import kr.jongyeol.jsBot.JModData;
import kr.jongyeol.jsBot.LogBuilder;
import kr.jongyeol.jsBot.SlashCommandAdapter;
import mx.kenzie.eris.api.entity.command.Command;
import mx.kenzie.eris.api.entity.command.Option;
import mx.kenzie.eris.api.event.Interaction;
import mx.kenzie.eris.api.magic.OptionType;
import mx.kenzie.eris.api.magic.Permission;

import java.util.HashMap;
import java.util.Map;

public class EditModData implements SlashCommandAdapter, ModOptionCommand {
    @Override
    public String[] getCommandNames() {
        return new String[] { "editmoddata" };
    }

    public Option[] getSubCommandData() {
        Option nameOption = getModNameOption();
        return new Option[] {
            Option.subCommand("channel", "채널", nameOption,
                new Option("channel", "채널", OptionType.CHANNEL)
            ),
            Option.subCommand("releaserole", "릴리즈 역할", nameOption,
                new Option("releaserole", "릴리즈 역할", OptionType.ROLE)
            ),
            Option.subCommand("progressrole", "근황 역할", nameOption,
                new Option("progressrole", "근황 역할", OptionType.ROLE)
            ),
            Option.subCommand("releasebetarole", "베타 역할", nameOption,
                new Option("releasebetarole", "베타 역할", OptionType.ROLE)
            ),
            Option.subCommand("lastannounce", "마지막 알림 메시지 ID", nameOption,
                new Option("lastannounce", "마지막 알림 메시지 ID", OptionType.INTEGER)
            ),
            Option.subCommand("betalinkable", "베타 링크 허용", nameOption,
                new Option("betalinkable", "베타 링크 허용", OptionType.BOOLEAN)
            ),
            Option.subCommand("deleted", "삭제됨", nameOption,
                new Option("deleted", "삭제됨", OptionType.BOOLEAN)
            ),
            Option.subCommand("private", "비공개", nameOption,
                new Option("private", "비공개", OptionType.BOOLEAN)
            ),
            Option.subCommand("discorddl", "디스코드 다운링크", nameOption,
                new Option("discorddl", "디스코드 다운링크", OptionType.STRING).required(false)
            ),
            Option.subCommand("releasemessage", "릴리즈 메시지 ID", nameOption,
                new Option("version", "버전", OptionType.STRING),
                new Option("releasemessage", "릴리즈 메시지 ID", OptionType.INTEGER).required(false)
            ),
            Option.subCommand("version", "버전", nameOption,
                new Option("version", "버전", OptionType.STRING).required(false)
            ),
            Option.subCommand("betaversion", "베타 버전", nameOption,
                new Option("betaversion", "베타 버전", OptionType.STRING).required(false)
            ),
            Option.subCommand("setbetainfo", "베타 정보 설정", nameOption,
                new Option("version", "버전", OptionType.STRING),
                new Option("beta", "베타 여부", OptionType.BOOLEAN).required(false)
            ),
            Option.subCommand("forceupdate", "강제 업데이트", nameOption,
                new Option("forceupdate", "강제 업데이트", OptionType.BOOLEAN)
            ),
            Option.subCommand("forceupdatebeta", "베타 강제 업데이트", nameOption,
                new Option("forceupdatebeta", "베타 강제 업데이트", OptionType.BOOLEAN)
            ),
            Option.subCommand("forceupdatehandle", "세부 강제 업데이트", nameOption,
                new Option("minversion", "최소 버전", OptionType.STRING),
                new Option("maxversion", "최대 버전", OptionType.STRING),
                new Option("forceupdate", "강제 업데이트", OptionType.BOOLEAN)
            ),
            Option.subCommand("removeforceupdatehandle", "세부 강제 업데이트 제거", nameOption,
                new Option("minversion", "최소 버전", OptionType.STRING),
                new Option("maxversion", "최대 버전", OptionType.STRING)
            ),
            Option.subCommand("reloadlocalization", "번역 재로딩", nameOption),
            Option.subCommand("homepage", "홈페이지", nameOption,
                new Option("homepage", "홈페이지", OptionType.STRING).required(false)
            ),
            Option.subCommand("discord", "디스코드", nameOption,
                new Option("discord", "디스코드", OptionType.STRING).required(false)
            ),
            Option.subCommand("downloadlink", "다운로드 링크", nameOption,
                Option.ofStrings("downloadlink", "다운로드 링크",
                    new Option.Choice<>("github", "github"),
                    new Option.Choice<>("custom", "custom")
                )
            ),
            Option.subCommand("customdownloadlink", "커스텀 다운로드 링크", nameOption,
                new Option("version", "버전", OptionType.STRING),
                new Option("link", "링크", OptionType.STRING).required(false)
            ),
            Option.subCommand("gid", "번역본 GID", nameOption,
                new Option("gid", "번역본 GID", OptionType.INTEGER)
            )
        };
    }

    @Override
    public Command getCommandData(Command data) {
        return data.permissions(Permission.MANAGE_GUILD)
            .description("모드를 수정합니다.")
            .options(getSubCommandData());
    }

    @Override
    public boolean guildOnly() {
        return true;
    }

    @Override
    public void onCommand(Interaction event) throws Exception {
        String subCommand = event.data.options[0].name;
        Map<String, Object> options = SlashCommandAdapter.getOptions(event.data.options[0].options);
        String name = (String) options.get("modname0");
        JModData modData = JModData.getMod(name);
        if(!checkMod(event, modData)) return;
        String original = "";
        switch(subCommand) {
            case "channel":
                original = modData.getChannelId() + "";
                modData.setChannel((long) options.get("channel"));
                break;
            case "releaserole":
                original = modData.getReleaseRole() + "";
                modData.setReleaseRole((long) options.get("releaserole"));
                break;
            case "progressrole":
                original = modData.getProgressRole() + "";
                modData.setProgressRole((long) options.get("progressrole"));
                break;
            case "releasebetarole":
                original = modData.getBetaReleaseRole() + "";
                modData.setBetaReleaseRole((long) options.get("releasebetarole"));
                break;
            case "lastannounce":
                original = modData.getLastAnnounce() + "";
                modData.setLastAnnounce((long) options.get("lastannounce"));
                break;
            case "betalinkable":
                original = modData.isBetaLinkable() + "";
                modData.setBetaLinkable((boolean) options.get("betalinkable"));
                break;
            case "deleted":
                original = modData.isDeleted() + "";
                modData.setDeleted((boolean) options.get("deleted"));
                break;
            case "private":
                original = modData.isPrivateMod() + "";
                modData.setPrivateMod((boolean) options.get("private"));
                break;
            case "discorddl":
                original = modData.getDiscordDL();
                modData.setDiscordDL(options.containsKey("discorddl") ? (String) options.get("discorddl") : null);
                break;
            case "releasemessage":
                String version = (String) options.get("version");
                Version versionData = new Version(version);
                original = modData.getReleaseMessage().get(versionData).toString();
                if(options.containsKey("releasemessage")) modData.getReleaseMessage().put(versionData, (long) options.get("releasemessage"));
                else modData.getReleaseMessage().remove(versionData);
                modData.save();
                break;
            case "version":
                original = modData.getVersion().toString();
                modData.setVersion(options.containsKey("version") ? new Version((String) options.get("version")) : null);
                ConnectOtherLib.setVersion(modData, modData.getVersion());
                break;
            case "betaversion":
                original = modData.getBetaVersion().toString();
                modData.setBetaVersion(new Version((String) options.get("betaversion")));
                ConnectOtherLib.setBetaVersion(modData, modData.getBetaVersion());
                break;
            case "setbetainfo":
                version = (String) options.get("version");
                versionData = new Version(version);
                Map<Version, Boolean> betaMap = modData.getBetaMap();
                original = betaMap.containsKey(versionData) ? betaMap.get(versionData) + "" : "null";
                if(options.containsKey("beta")) modData.getBetaMap().put(versionData, (boolean) options.get("beta"));
                else modData.getBetaMap().remove(versionData);
                ConnectOtherLib.setBetaMap(modData, versionData, options.containsKey("beta") ? (boolean) options.get("beta") : null);
                break;
            case "forceupdate":
                original = modData.isForceUpdate() + "";
                modData.setForceUpdate((boolean) options.get("forceupdate"));
                ConnectOtherLib.setForceUpdate(modData, modData.isForceUpdate());
                break;
            case "forceupdatebeta":
                original = modData.isForceUpdateBeta() + "";
                modData.setForceUpdateBeta((boolean) options.get("forceupdatebeta"));
                ConnectOtherLib.setForceUpdateBeta(modData, modData.isForceUpdateBeta());
                break;
            case "forceupdatehandle":
                original = null;
                ForceUpdateHandle handle = new ForceUpdateHandle(new Version((String) options.get("minversion")),
                    new Version((String) options.get("maxversion")), (boolean) options.get("forceupdate"));
                modData.addForceUpdateHandles(handle);
                ConnectOtherLib.addForceUpdateHandle(modData, handle);
                break;
            case "removeforceupdatehandle":
                for(int i = 0; i < modData.getForceUpdateHandles().length; i++) {
                    handle = modData.getForceUpdateHandles()[i];
                    if(handle.version1.equals(new Version((String) options.get("minversion")))
                        && handle.version2.equals(new Version((String) options.get("maxversion")))) {
                        original = "minversion: " + handle.version1 + ", maxversion: " + handle.version2 + ", forceupdate: " + handle.forceUpdate;
                        modData.removeForceUpdateHandles(i--);
                        ConnectOtherLib.removeForceUpdateHandle(modData, i);
                    }
                }
                break;
            case "reloadlocalization":
                original = null;
                modData.loadLocalizations();
                ConnectOtherLib.loadLocalizations(modData);
                break;
            case "homepage":
                original = modData.getHomepage();
                modData.setHomepage(options.containsKey("homepage") ? (String) options.get("homepage") : null);
                ConnectOtherLib.setHomepage(modData, modData.getHomepage());
                break;
            case "discord":
                original = modData.getDiscord();
                modData.setDiscord(options.containsKey("discord") ? (String) options.get("discord") : null);
                ConnectOtherLib.setDiscord(modData, modData.getDiscord());
                break;
            case "downloadlink":
                original = modData.getDownloadLink().toString();
                modData.setDownloadLink(options.get("downloadlink").equals("github") ?
                    new GithubDownloadLink(modData.getName()) : new CustomDownloadLink(new HashMap<>()));
                ConnectOtherLib.setDownloadLink(modData, modData.getDownloadLink());
                break;
            case "customdownloadlink":
                Version ver = new Version((String) options.get("version"));
                if(modData.getDownloadLink() instanceof GithubDownloadLink) {
                    event.reply("이 모드는 Github 다운로드 링크를 사용합니다.");
                    return;
                }
                original = modData.getDownloadLink().getLink(ver);
                Map<Version, String> link = ((CustomDownloadLink) modData.getDownloadLink()).links;
                if(options.containsKey("link")) link.put(ver, (String) options.get("link"));
                else link.remove(ver);
                break;
            case "gid":
                original = modData.getGid() + "";
                modData.setGid((int) options.get("gid"));
                ConnectOtherLib.setGid(modData, modData.getGid());
                break;
        }
        event.reply("모드 데이터를 수정했습니다.");
        LogBuilder log = new LogBuilder(event.getSource(), "모드 데이터를 수정했습니다.")
            .addField("user", event.getSource())
            .addBlankField(true)
            .addField("channel", event.getChannel())
            .addField("id", modData.getId())
            .addField("name", name)
            .addField("modChannel", modData.getChannel())
            .addField("changed", subCommand)
            .addField("original", original);
        for(String key : options.keySet()) {
            if(key.equals("modname")) continue;
            log.addField(key, options.get(key));
        }
        log.send();
    }
}
