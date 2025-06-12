using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ScarletCore.Data;
using ScarletCore.Events;
using ScarletTeleports.Services;
using VampireCommandFramework;

namespace ScarletTeleports;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
[BepInDependency("markvaaz.ScarletCore")]
public class Plugin : BasePlugin {
  static Harmony _harmony;
  public static Harmony Harmony => _harmony;
  public static Plugin Instance { get; private set; }
  public static ManualLogSource LogInstance { get; private set; }
  public static Settings Settings { get; private set; }
  public static Database Database { get; private set; }

  public override void Load() {
    Instance = this;
    LogInstance = Log;

    Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

    _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

    Settings = new Settings(MyPluginInfo.PLUGIN_GUID, Instance);
    Database = new Database(MyPluginInfo.PLUGIN_GUID);

    EventManager.OnInitialize += TeleportManager.Initialize;

    LoadSettings();

    CommandRegistry.RegisterAll();
  }

  public override bool Unload() {
    CommandRegistry.UnregisterAssembly();
    _harmony?.UnpatchSelf();
    return true;
  }

  public static void LoadSettings() {
    var general = Settings.Section("General");

    general.Add("EnablePersonalTeleports", true, "If enabled, allows players to create personal teleports.");
    general.Add("EnablePersonalCooldown", true, "Enables cooldown for personal teleports.");
    general.Add("EnableGlobalCooldown", true, "Enables cooldown for global teleports.");
    general.Add("EnableDraculaRoom", false, "Enables teleporting from and to the Dracula's room.");
    general.Add("EnableTeleportInCombat", false, "Enables teleporting while in combat globally.");
    general.Add("EnableTeleportBetweenPlayers", true, "Enables teleporting between players.");
    general.Add("DefaultMaximumPersonalTeleports", 3, "The maximum number of personal teleports a player can have.");

    var timers = Settings.Section("Timers");

    timers.Add("TeleportRequestExpiration", 30, "The expiration time in seconds of a teleport request.");
    timers.Add("DefaultPersonalCooldown", 30, "The cooldown in seconds for personal teleports.");
    timers.Add("DefaultGlobalCooldown", 30, "The cooldown in seconds for global teleports.");

    var prefabs = Settings.Section("Prefabs");

    prefabs.Add("DefaultPersonalPrefabName", "Blood Essence", "The name of the prefab that will be consumed when teleporting to a personal teleport.");
    prefabs.Add("DefaultPersonalPrefabGUID", 862477668, "The prefab that will be consumed when teleporting to a personal teleport.");
    prefabs.Add("DefaultGlobalPrefabGUID", 862477668, "The prefab that will be consumed when teleporting to a global teleport.");
    prefabs.Add("DefaultGlobalPrefabName", "Blood Essence", "The name of the prefab that will be consumed when teleporting to a global teleport.");

    var costs = Settings.Section("Costs");

    costs.Add("DefaultPersonalCost", 100, "The amount of the prefab that will be consumed when teleporting to a personal teleport.");
    costs.Add("DefaultGlobalCost", 50, "The amount of the prefab that will be consumed when teleporting to a global teleport.");
  }
}
