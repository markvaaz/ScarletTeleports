using System;
using Unity.Entities;
using Unity.Mathematics;
using ScarletTeleports.Data;
using System.Collections.Generic;
using System.Linq;
using Stunlock.Core;
using ScarletCore.Events;
using ScarletCore.Data;
using ScarletCore.Services;
using ScarletCore.Utils;
using ScarletCore.Systems;

namespace ScarletTeleports.Services;

public class TeleportManager {
  private static Settings Settings => Plugin.Settings;
  private static Database Database => Plugin.Database;
  public static HashSet<TeleportData> PersonalTeleports { get; private set; } = [];
  public static Dictionary<string, TeleportData> GlobalTeleports { get; private set; } = [];
  public static Dictionary<string, ZoneData> RestrictedZones { get; private set; } = [];
  public static void Initialize(object _, InitializeEventArgs args) {
    LoadGlobalTeleports();
    LoadRestrictedZones();
    CoroutineHandler.StartRepeatingCoroutine(CheckForExpiredRequests, 20);
    EventManager.OnUserConnected += LoadPersonalTeleportEvent;
  }

  public static void LoadPersonalTeleportEvent(object _, UserConnectedEventArgs args) {
    var playerData = args.Player;

    LoadPersonalTeleports(playerData);
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
    return BuffService.HasBuff(player, new PrefabGUID(581443919)) || BuffService.HasBuff(player, new PrefabGUID(697095869));
  }

  public static bool IsInDraculaRoom(Entity player) {
    var position = player.GetPosition();
    var draculaRoom = new float3(720, 15, -2827);
    var distance = math.distance(position, draculaRoom);

    return distance < 80f;
  }

  public static void CreateGlobalTeleport(TeleportData teleportData) {
    if (HasGlobalTeleport(teleportData.Name)) {
      Log.Warning($"Teleport {teleportData.Name} already exists.");
      return;
    }

    AddGlobalTeleport(teleportData);

    SaveGlobalTeleports();
  }

  public static CustomPlayerData GetCustomPlayerData(PlayerData player) {
    var customData = player.GetData<CustomPlayerData>();

    if (customData == null) {
      customData = new CustomPlayerData();
      player.SetData(customData);
    }

    return customData;
  }

  public static void CreatePersonalTeleport(PlayerData player, TeleportData teleportData) {
    var customData = GetCustomPlayerData(player);

    customData.AddTeleport(teleportData);

    AddPersonalTeleport(teleportData);

    SavePersonalTeleport(player);
  }

  public static void SaveGlobalTeleports() {
    Database.Save("GlobalTeleports", GlobalTeleports.Values.ToList());
  }

  public static void SaveAllPersonalTeleports() {
    foreach (var player in PlayerService.AllPlayers) {
      SavePersonalTeleport(player);
    }
  }

  public static void SavePersonalTeleport(PlayerData player) {
    Database.Save($"PersonalTeleports/{player.PlatformId}", player.GetData<CustomPlayerData>());
  }

  public static TeleportData GetGlobalTeleport(string name) {
    return GlobalTeleports[name];
  }

  public static bool HasGlobalTeleport(string name) {
    return GlobalTeleports.ContainsKey(name);
  }

  public static void LoadGlobalTeleports() {
    var teleports = Database.Load<List<TeleportData>>("GlobalTeleports");

    if (teleports == null) return;

    foreach (var teleport in teleports) {
      if (teleport.IsDefaultCost) {
        teleport.Cost = Settings.Get<int>("DefaultGlobalCost");
      }

      if (teleport.IsDefaultPrefab) {
        teleport.PrefabGUID = new(Settings.Get<int>("DefaultGlobalPrefabGUID"));
      }

      if (teleport.IsDefaultCooldown) {
        teleport.Cooldown = Settings.Get<int>("DefaultGlobalCooldown");
      }

      AddGlobalTeleport(teleport);
    }

    SaveGlobalTeleports();
  }

  public static bool LoadPersonalTeleports(PlayerData player) {
    var customData = GetCustomPlayerData(player);

    customData.Teleports.Clear();

    var data = Database.Load<CustomPlayerData>($"PersonalTeleports/{player.PlatformId}");

    if (data == null) return false;

    var teleports = data.Teleports;

    customData.MaxTeleports = data.MaxTeleports;
    customData.BypassCost = data.BypassCost;
    customData.BypassCooldown = data.BypassCooldown;
    customData.BypassDraculaRoom = data.BypassDraculaRoom;
    customData.BypassCombat = data.BypassCombat;

    if (teleports == null) return false;

    foreach (var teleportData in teleports) {
      if (teleportData.IsDefaultCost) {
        teleportData.Cost = Settings.Get<int>("DefaultPersonalCost");
      }

      if (teleportData.IsDefaultPrefab) {
        teleportData.PrefabGUID = new(Settings.Get<int>("DefaultPersonalPrefabGUID"));
      }

      if (teleportData.IsDefaultCooldown) {
        teleportData.Cooldown = Settings.Get<int>("DefaultPersonalCooldown");
      }

      customData.AddTeleport(teleportData);

      AddPersonalTeleport(teleportData);
    }

    SavePersonalTeleport(player);

    return true;
  }

  public static bool RemovePersonalTeleport(PlayerData player, string name) {
    var customData = GetCustomPlayerData(player);

    if (!customData.HasTeleport(name)) return false;

    var teleport = customData.GetTeleport(name);

    customData.RemoveTeleport(name);
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
      Log.Warning($"Teleport {name} not found.");
      return false;
    }
  }

  public static HashSet<TeleportData> GetAllTeleports() {
    var result = new HashSet<TeleportData>();

    return result.Union(GlobalTeleports.Values.ToHashSet()).Union(PersonalTeleports).ToHashSet();
  }

  public static void CheckForExpiredRequests() {
    foreach (var player in PlayerService.AllPlayers) {
      var customData = GetCustomPlayerData(player);
      customData.PendingRequests.RemoveWhere(request => {
        bool remove = request.ExpirationTime <= DateTime.Now;

        if (remove && PlayerService.TryGetById(request.PlatformID, out var p)) {
          var pCustomData = GetCustomPlayerData(p);
          pCustomData.CanResquestTeleports = true;
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
    var data = Database.Load<Dictionary<string, ZoneData>>("RestrictedZones");

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