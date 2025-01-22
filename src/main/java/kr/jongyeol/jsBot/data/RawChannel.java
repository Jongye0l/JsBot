package kr.jongyeol.jsBot.data;

import kr.jongyeol.jaServer.data.Version;
import lombok.Data;
import lombok.NoArgsConstructor;
import mx.kenzie.eris.api.entity.Channel;

import java.util.HashMap;
import java.util.Map;

import static kr.jongyeol.jsBot.DiscordBot.bot;

@Data
@NoArgsConstructor
public class RawChannel {
    private long guild;
    private long channel;
    private boolean beta;
    private boolean apply;
    private long lastAnnounce = -1;
    private final Map<Version, Long> releaseMessage = new HashMap<>();

    public RawChannel(long guild, long channel, boolean beta, boolean apply) {
        this.guild = guild;
        this.channel = channel;
        this.beta = beta;
        this.apply = apply;
    }

    public String getChannelUrl() {
        return "https://discord.com/channels/" + guild + "/" + channel;
    }

    public String getMessageUrl(Version version) {
        return getChannelUrl() + "/" + releaseMessage.get(version);
    }

    public Channel getEmptyChannel() {
        Channel channel = new Channel();
        channel.api = bot.getAPI();
        channel.id = this.channel + "";
        return channel;
    }

    @Override
    public boolean equals(Object o) {
        return o instanceof RawChannel rawChannel && rawChannel.guild == guild && rawChannel.channel == channel;
    }
}
