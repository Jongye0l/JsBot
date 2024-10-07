package kr.jongyeol.jsBot;

import mx.kenzie.eris.api.Lazy;
import mx.kenzie.eris.api.entity.Channel;
import mx.kenzie.eris.api.entity.User;

import java.awt.*;
import java.util.concurrent.CompletableFuture;

import static kr.jongyeol.jsBot.DiscordBot.bot;

public class Utility {
    public static Color getColor(String color) {
        return color == null ? null : switch(color) {
            case "red" -> Color.red;
            case "orange" -> Color.orange;
            case "yellow" -> Color.yellow;
            case "green" -> Color.green;
            case "blue" -> Color.blue;
            case "cyan" -> Color.cyan;
            case "black" -> Color.black;
            case "white" -> Color.white;
            case "gray" -> Color.gray;
            case "darkgray" -> Color.darkGray;
            case "lightgray" -> Color.lightGray;
            case "magenta" -> Color.magenta;
            case "pink" -> Color.pink;
            default -> null;
        };
    }

    public static String getUserName(User user) {
        return user.username;
    }

    public static String getAvatar(User user) {
        return user.avatar != null ? user.getAvatarURL() :
            "https://cdn.discordapp.com/embed/avatars/" + (user.discriminator() != 0 ? user.discriminator() % 5 : (user.id() >> 22) % 6) + ".png";
    }

    public static String getChannelMention(long id) {
        return "<#" + id + ">";
    }

    public static String getUserMention(long id) {
        return "<@" + id + ">";
    }

    public static String getRoleMention(long id) {
        return "<@&" + id + ">";
    }
}
