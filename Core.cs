using System;
using System.Linq;
using Unity.Entities;
using ProjectM.Scripting;
using BepInEx.Logging;
using ProjectM;
using ScarletTeleports.Services;

namespace ScarletTeleports;

internal static class Core {
  public static World Server { get; } = GetServerWorld() ?? throw new Exception("There is no Server world (yet)...");
  public static PrefabCollectionSystem PrefabCollectionSystem => Server.GetExistingSystemManaged<PrefabCollectionSystem>();
  public static EntityManager EntityManager => Server.EntityManager;
  public static ServerGameManager GameManager => Server.GetExistingSystemManaged<ServerScriptMapper>().GetServerGameManager();
  public static UnitSpawnerUpdateSystem UnitSpawnerUpdateSystem => Core.Server.GetExistingSystemManaged<UnitSpawnerUpdateSystem>();
  public static PlayerService Players { get; set; }
  public static bool hasInitialized = false;

  public static ManualLogSource Log { get; } = Plugin.LogInstance;

  public static void Initialize() {
    if (hasInitialized) return;

    hasInitialized = true;

    Players = new PlayerService();
    TeleportService.Initialize();
  }

  static World GetServerWorld() {
    return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
  }
}