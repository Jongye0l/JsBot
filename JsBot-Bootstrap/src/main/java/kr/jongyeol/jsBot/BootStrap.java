package kr.jongyeol.jsBot;

import java.io.File;
import java.lang.reflect.InvocationTargetException;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.ArrayList;
import java.util.List;

public class BootStrap {
    public static void main(String[] args) throws ClassNotFoundException, IllegalAccessException, NoSuchMethodException, InvocationTargetException {
        File file = new File("library");
        if(!file.exists()) file.mkdir();
        List<URL> urls = new ArrayList<>();
        for(File f : file.listFiles()) {
            try {
                urls.add(f.toURI().toURL());
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        URLClassLoader loader = new URLClassLoader(urls.toArray(new URL[0]), BootStrap.class.getClassLoader());
        loader.loadClass("kr.jongyeol.jsBot.DiscordBot").getMethod("main", String[].class).invoke(null, (Object) args);
    }
}
