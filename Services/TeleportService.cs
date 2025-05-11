using System;
using Unity.Entities;
using Unity.Mathematics;
using ProjectM;
using ProjectM.Network;
using ScarletTeleports.Data;
using System.Collections.Generic;
using System.Linq;
using Stunlock.Core;

namespace ScarletTeleports.Services;

public class TeleportService {

  public static HashSet<TeleportData> PersonalTeleports { get; private set; } = [];
  public static Dictionary<string, TeleportData> GlobalTeleports { get; private set; } = [];
  public static Dictionary<string, ZoneData> RestrictedZones { get; private set; } = [];
  private static EntityManager entityManager => Core.EntityManager;
  public static void Initialize() {
    LoadAllPersonalTeleports();
    LoadGlobalTeleports();
    LoadRestrictedZones();
    CoroutineHandler.StartRepeatingCoroutine(CheckForExpiredRequests, 20);
  }

  public static void AddPersonalTeleport(TeleportData teleportData) {
    var existing = PersonalTeleports.FirstOrDefault(t =>
        t.Name.Equals(teleportData.Name, StringComparison.OrdinalIgnoreCase) &&
        t.PlatformID == teleportData.PlatformID);

    if (existing != null && !existing.Equals(default(TeleportData))) {
      PersonalTeleports.Remove(existing);
    }

    PersonalTeleports.Add(teleportData);
  }

  public static void AddGlobalTeleport(TeleportData teleportData) {
    GlobalTeleports[teleportData.Name] = teleportData;
  }

  public static bool IsPlayerInCombat(Entity player) {
    return BuffUtility.HasBuff(entityManager, player, new PrefabGUID(581443919)) || BuffUtility.HasBuff(entityManager, player, new PrefabGUID(697095869));
  }

  public static bool IsInDraculaRoom(Entity player) {
    var position = player.Position();
    var draculaRoom = new float3(720, 15, -2827);
    var distance = math.distance(position, draculaRoom);

    return distance < 80f;
  }

  public static bool TeleportToPosition(PlayerData player, float3 position) {
    var entity = entityManager.CreateEntity(
      ComponentType.ReadWrite<FromCharacter>(),
      ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
    );

    entityManager.SetComponentData<FromCharacter>(entity, new() {
      User = player.UserEntity,
      Character = player.CharacterEntity
    });

    entityManager.SetComponentData<PlayerTeleportDebugEvent>(entity, new() {
      Position = position,
      Target = PlayerTeleportDebugEvent.TeleportTarget.Self
    });

    return true;
  }

  public static void CreateGlobalTeleport(TeleportDataOptions teleportOptions) {
    if (HasGlobalTeleport(teleportOptions.Name)) {
      Core.Log.LogWarning($"Teleport {teleportOptions.Name} already exists.");
      return;
    }

    var teleportData = new TeleportData(teleportOptions);

    AddGlobalTeleport(teleportData);

    SaveGlobalTeleports();
  }

  public static void CreatePersonalTeleport(PlayerData player, TeleportDataOptions teleportOptions) {
    var teleportData = new TeleportData(teleportOptions, player.PlatformID, player.Name);

    player.AddTeleport(teleportData);

    AddPersonalTeleport(teleportData);

    SavePersonalTeleport(player);
  }

  public static void SaveGlobalTeleports() {
    Database.Save("GlobalTeleports", GetParsedTeleports(GlobalTeleports.Values.ToHashSet()));
  }

  public static void SaveAllPersonalTeleports() {
    foreach (var player in Core.Players.GetAllPlayers()) {
      SavePersonalTeleport(player);
    }
  }

  public static void SavePersonalTeleport(PlayerData player) {
    SaveModel saveModel = new() {
      MaxTeleports = player.MaxTeleports,
      BypassCost = player.BypassCost,
      BypassCooldown = player.BypassCooldown,
      BypassDraculaRoom = player.BypassDraculaRoom,
      BypassCombat = player.BypassCombat,
      Teleports = GetParsedTeleports(player.Teleports)
    };

    Database.Save($"PersonalTeleports/{player.PlatformID}", saveModel);
  }

  public static TeleportData GetGlobalTeleport(string name) {
    return GlobalTeleports[name];
  }

  public static bool HasGlobalTeleport(string name) {
    return GlobalTeleports.ContainsKey(name);
  }

  public static void LoadGlobalTeleports() {
    var teleports = Database.Get<List<GlobalTeleportDataOptions>>("GlobalTeleports");

    if (teleports == null) return;

    foreach (var teleport in teleports) {
      if (teleport.IsDefaultCost) {
        teleport.Cost = Settings.Get<int>("DefaultGlobalCost");
      }

      if (teleport.IsDefaultPrefab) {
        teleport.PrefabGUID = Settings.Get<int>("DefaultGlobalPrefabGUID");
      }

      if (teleport.IsDefaultCooldown) {
        teleport.Cooldown = Settings.Get<int>("DefaultGlobalCooldown");
      }

      AddGlobalTeleport(new TeleportData(teleport));
    }

    SaveGlobalTeleports();
  }

  public static void LoadAllPersonalTeleports() {
    var players = Core.Players.GetAllPlayers();

    if (players.Count == 0) return;

    foreach (var p in players) {
      if (!LoadPersonalTeleports(p)) continue;
    }
  }

  public static bool LoadPersonalTeleports(PlayerData player) {
    player.ClearTeleports();

    var data = Database.Get<SaveModel>($"PersonalTeleports/{player.PlatformID}");

    if (data == null) return false;

    var teleports = data.Teleports;

    player.MaxTeleports = data.MaxTeleports;
    player.BypassCost = data.BypassCost;
    player.BypassCooldown = data.BypassCooldown;
    player.BypassDraculaRoom = data.BypassDraculaRoom;
    player.BypassCombat = data.BypassCombat;

    if (teleports == null) return false;

    player.Teleports.Clear();

    foreach (var teleportOptions in teleports) {
      if (teleportOptions.IsDefaultCost) {
        teleportOptions.Cost = Settings.Get<int>("DefaultPersonalCost");
      }

      if (teleportOptions.IsDefaultPrefab) {
        teleportOptions.PrefabGUID = Settings.Get<int>("DefaultPersonalPrefabGUID");
      }

      if (teleportOptions.IsDefaultCooldown) {
        teleportOptions.Cooldown = Settings.Get<int>("DefaultPersonalCooldown");
      }

      var teleportData = new TeleportData(teleportOptions, player.PlatformID, player.Name);

      player.AddTeleport(teleportData);

      AddPersonalTeleport(teleportData);
    }

    SavePersonalTeleport(player);

    return true;
  }

  public static bool RemovePersonalTeleport(PlayerData player, string name) {
    if (!player.HasTeleport(name)) return false;

    var teleport = player.GetTeleport(name);

    player.RemoveTeleport(name);
    PersonalTeleports.Remove(teleport);
    SavePersonalTeleport(player);
    return true;
  }

  public static bool RemoveGlobalTeleport(string name) {
    var teleport = GetGlobalTeleport(name);

    if (!teleport.Equals(default(TeleportData))) {
      GlobalTeleports.Remove(teleport.Name);
      SaveGlobalTeleports();
      return true;
    } else {
      Core.Log.LogWarning($"Teleport {name} not found.");
      return false;
    }
  }

  public static HashSet<TeleportDataOptions> GetParsedTeleports(HashSet<TeleportData> teleports) {
    var teleportsResult = new HashSet<TeleportDataOptions>();

    foreach (var teleport in teleports) {
      teleportsResult.Add(new TeleportDataOptions {
        Name = teleport.Name,
        Position = [teleport.Position.x, teleport.Position.y, teleport.Position.z],
        PrefabName = teleport.PrefabName,
        PrefabGUID = teleport.PrefabGUID.GuidHash,
        Cost = teleport.Cost,
        Cooldown = teleport.Cooldown,
        IsDefaultCost = teleport.IsDefaultCost,
        IsDefaultPrefab = teleport.IsDefaultPrefab,
        IsDefaultCooldown = teleport.IsDefaultCooldown
      });
    }

    return teleportsResult;
  }

  public static HashSet<TeleportData> GetAllTeleports() {
    var result = new HashSet<TeleportData>();

    return result.Union(GlobalTeleports.Values.ToHashSet()).Union(PersonalTeleports).ToHashSet();
  }

  public static void CheckForExpiredRequests() {
    foreach (var player in Core.Players.GetAllPlayers()) {
      player.PendingRequests.RemoveWhere(request => {
        bool remove = request.ExpirationTime <= DateTime.Now;

        if (remove && Core.Players.TryGetById(request.PlatformID, out var p)) {
          p.CanResquestTeleports = true;
        }

        return remove;
      });
    }
  }

  public static void CreateRestrictedZone(ZoneData zone) {
    RestrictedZones.Add(zone.Name, zone);

    SaveRestrictedZones();
  }

  public static void LoadRestrictedZones() {
    var data = Database.Get<Dictionary<string, ZoneData>>("RestrictedZones");

    if (data != null) {
      RestrictedZones.Clear();
      RestrictedZones = data;
    }
  }

  public static ZoneData GetRestrictedZone(float3 position) {
    foreach (var zone in RestrictedZones.Values) {
      var zonePosition = new float3(zone.Position[0], zone.Position[1], zone.Position[2]);

      if (math.distance(position, zonePosition) < zone.Radius) {
        return zone;
      }
    }

    return null;
  }

  public static void SaveRestrictedZones() {
    Database.Save("RestrictedZones", RestrictedZones);
  }

  public static bool RemoveRestrictedZone(string name) {
    var removed = RestrictedZones.Remove(name);

    SaveRestrictedZones();

    return removed;
  }
}