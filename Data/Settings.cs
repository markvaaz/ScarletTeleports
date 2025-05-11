using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace ScarletTeleports.Data;

public readonly struct Settings {
  public static readonly string ConfigPath = Path.Combine(Paths.ConfigPath, "ScarletTeleports");
  public static ConfigEntry<bool> EnablePersonalTeleports { get; private set; }
  public static ConfigEntry<bool> EnablePersonalCooldown { get; private set; }
  public static ConfigEntry<bool> EnableGlobalCooldown { get; private set; }
  public static ConfigEntry<bool> EnableDraculaRoom { get; private set; }
  public static ConfigEntry<bool> EnableTeleportInCombat { get; private set; }
  public static ConfigEntry<bool> EnableTeleportBetweenPlayers { get; private set; }
  public static ConfigEntry<int> DefaulMaximumPersonalTeleports { get; private set; }
  public static ConfigEntry<int> DefaultPersonalCooldown { get; private set; }
  public static ConfigEntry<int> DefaultGlobalCooldown { get; private set; }
  public static ConfigEntry<int> DefaultPersonalPrefabGUID { get; private set; }
  public static ConfigEntry<string> DefaultPersonalPrefabName { get; private set; }
  public static ConfigEntry<int> DefaultGlobalPrefabGUID { get; private set; }
  public static ConfigEntry<string> DefaultGlobalPrefabName { get; private set; }
  public static ConfigEntry<int> DefaultPersonalCost { get; private set; }
  public static ConfigEntry<int> DefaultGlobalCost { get; private set; }
  public static ConfigEntry<int> TeleportRequestExpiration { get; private set; }

  private static readonly List<string> OrderedSections = ["General", "Timers", "Prefabs", "Costs"];

  public static void Initialize() {
    EnablePersonalTeleports = InitConfigEntry("General", "EnablePersonalTeleports", true, "If enabled, allows players to create personal teleports.");
    EnablePersonalCooldown = InitConfigEntry("General", "EnablePersonalCooldown", true, "Enables cooldown for personal teleports.");
    EnableGlobalCooldown = InitConfigEntry("General", "EnableGlobalCooldown", true, "Enables cooldown for global teleports.");
    EnableDraculaRoom = InitConfigEntry("General", "EnableDraculaRoom", false, "Enables teleporting from and to the Dracula's room.");
    EnableTeleportInCombat = InitConfigEntry("General", "EnableTeleportInCombat", false, "Enables teleporting while in combat globally.");
    EnableTeleportBetweenPlayers = InitConfigEntry("General", "EnableTeleportBetweenPlayers", true, "Enables teleporting between players.");
    DefaulMaximumPersonalTeleports = InitConfigEntry("General", "DefaultMaximumPersonalTeleports", 3, "The maximum number of personal teleports a player can have.");
    TeleportRequestExpiration = InitConfigEntry("Timers", "TeleportRequestExpiration", 30, "The expiration time in seconds of a teleport request.");
    DefaultPersonalCooldown = InitConfigEntry("Timers", "DefaultPersonalCooldown", 30, "The cooldown in seconds for personal teleports.");
    DefaultGlobalCooldown = InitConfigEntry("Timers", "DefaultGlobalCooldown", 30, "The cooldown in seconds for global teleports.");
    DefaultPersonalPrefabName = InitConfigEntry("Prefabs", "DefaultPersonalPrefabName", "Blood Essence", "The name of the prefab that will be consumed when teleporting to a personal teleport.");
    DefaultPersonalPrefabGUID = InitConfigEntry("Prefabs", "DefaultPersonalPrefabGUID", 862477668, "The prefab that will be consumed when teleporting to a personal teleport.");
    DefaultGlobalPrefabGUID = InitConfigEntry("Prefabs", "DefaultGlobalPrefabGUID", 862477668, "The prefab that will be consumed when teleporting to a global teleport.");
    DefaultGlobalPrefabName = InitConfigEntry("Prefabs", "DefaultGlobalPrefabName", "Blood Essence", "The name of the prefab that will be consumed when teleporting to a global teleport.");
    DefaultPersonalCost = InitConfigEntry("Costs", "DefaultPersonalCost", 100, "The amount of the prefab that will be consumed when teleporting to a personal teleport.");
    DefaultGlobalCost = InitConfigEntry("Costs", "DefaultGlobalCost", 50, "The amount of the prefab that will be consumed when teleporting to a global teleport.");

    ReorderConfigSections();
  }

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
      if (sectionsContent.ContainsKey(section)) {
        foreach (var line in sectionsContent[section]) {
          writer.WriteLine(line);
        }
        writer.WriteLine();
      }
    }
  }
}