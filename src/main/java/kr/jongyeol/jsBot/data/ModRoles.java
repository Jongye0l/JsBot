package kr.jongyeol.jsBot.data;

import lombok.Data;

@Data
public class ModRoles {
    private long releaseRole = -1;
    private long progressRole = -1;
    private long betaReleaseRole = -1;

    public boolean notSet() {
        return releaseRole == -1 && progressRole == -1 && betaReleaseRole == -1;
    }

    public String getReleasePing() {
        return "<@&" + releaseRole + ">";
    }

    public String getProgressPing() {
        return "<@&" + progressRole + ">";
    }

    public String getBetaReleasePing() {
        return "<@&" + betaReleaseRole + ">";
    }
}
