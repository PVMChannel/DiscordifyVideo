using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using FFMpegCore.Arguments;

public class ConfigManager
{
    private static string ConfigFileName = "config.json";
    private static string ConfigFileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiscordifyVideo");
    private static string ConfigFilePath =  Path.Combine(ConfigFileDirectory, ConfigFileName);
    public static Config CurrentConfig;
    public static void Load()
    {
        if(!File.Exists(ConfigFilePath)) {
            CurrentConfig = new Config();
            return;
        }

        string ConfigContents = File.ReadAllText(ConfigFilePath);
        CurrentConfig = JsonSerializer.Deserialize<Config>(ConfigContents);
    }
    private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions() { WriteIndented = true };
    public static void Save()
    {
        if(!Directory.Exists(ConfigFileDirectory)) Directory.CreateDirectory(ConfigFileDirectory);

        string json = JsonSerializer.Serialize<Config>(CurrentConfig, SerializerOptions);
        File.WriteAllText(ConfigFilePath, json);
    }
}