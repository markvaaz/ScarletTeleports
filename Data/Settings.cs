using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace ScarletTeleports.Data;

public static class Settings {
  public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "ScarletAuras");
  private static readonly Dictionary<string, object> Entries = [];
  private static readonly List<string> OrderedSections = ["General"];

  public static void Initialize() {
    Add("General", "EnablePersonalTeleports", true, "If enabled, allows players to create personal teleports.");
    Add("General", "EnablePersonalCooldown", true, "Enables cooldown for personal teleports.");
    Add("General", "EnableGlobalCooldown", true, "Enables cooldown for global teleports.");
    Add("General", "EnableDraculaRoom", false, "Enables teleporting from and to the Dracula's room.");
    Add("General", "EnableTeleportInCombat", false, "Enables teleporting while in combat globally.");
    Add("General", "EnableTeleportBetweenPlayers", true, "Enables teleporting between players.");
    Add("General", "DefaultMaximumPersonalTeleports", 3, "The maximum number of personal teleports a player can have.");
    Add("Timers", "TeleportRequestExpiration", 30, "The expiration time in seconds of a teleport request.");
    Add("Timers", "DefaultPersonalCooldown", 30, "The cooldown in seconds for personal teleports.");
    Add("Timers", "DefaultGlobalCooldown", 30, "The cooldown in seconds for global teleports.");
    Add("Prefabs", "DefaultPersonalPrefabName", "Blood Essence", "The name of the prefab that will be consumed when teleporting to a personal teleport.");
    Add("Prefabs", "DefaultPersonalPrefabGUID", 862477668, "The prefab that will be consumed when teleporting to a personal teleport.");
    Add("Prefabs", "DefaultGlobalPrefabGUID", 862477668, "The prefab that will be consumed when teleporting to a global teleport.");
    Add("Prefabs", "DefaultGlobalPrefabName", "Blood Essence", "The name of the prefab that will be consumed when teleporting to a global teleport.");
    Add("Costs", "DefaultPersonalCost", 100, "The amount of the prefab that will be consumed when teleporting to a personal teleport.");
    Add("Costs", "DefaultGlobalCost", 50, "The amount of the prefab that will be consumed when teleporting to a global teleport.");

    ReorderConfigSections();
  }

  private static void Add<T>(string section, string key, T defaultValue, string description) {
    var entry = InitConfigEntry(section, key, defaultValue, description);
    Entries[key] = entry;
  }

  public static T Get<T>(string key) => (Entries[key] as ConfigEntry<T>).Value;

  public static void Set<T>(string key, T value) => (Entries[key] as ConfigEntry<T>).Value = value;

  static ConfigEntry<T> InitConfigEntry<T>(string section, string key, T defaultValue, string description) {
    var entry = Plugin.Instance.Config.Bind(section, key, defaultValue, description);

    var pluginConfigPath = Path.Combine(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");

    if (File.Exists(pluginConfigPath)) {
      var config = new ConfigFile(pluginConfigPath, true);
      if (config.TryGetEntry(section, key, out ConfigEntry<T> existingEntry)) {
        entry.Value = existingEntry.Value;
      }
    }

    return entry;
  }

  private static void ReorderConfigSections() {
    var configPath = Path.Combine(Paths.ConfigPath, $"{MyPluginInfo.PLUGIN_GUID}.cfg");
    if (!File.Exists(configPath)) return;

    var lines = File.ReadAllLines(configPath).ToList();
    var sectionsContent = new Dictionary<string, List<string>>();
    string currentSection = "";

    foreach (var line in lines) {
      if (line.StartsWith("[")) {
        currentSection = line.Trim('[', ']');
        sectionsContent[currentSection] = new List<string> { line };
      } else if (!string.IsNullOrWhiteSpace(currentSection)) {
        sectionsContent[currentSection].Add(line);
      }
    }

    using var writer = new StreamWriter(configPath, false);
    foreach (var section in OrderedSections) {
      if (sectionsContent.TryGetValue(section, out var content)) {
        foreach (var line in content) writer.WriteLine(line);
        writer.WriteLine();
      }
    }
  }
}
