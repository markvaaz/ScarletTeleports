using Unity.Entities;
using System.Collections.Generic;
using ProjectM.Network;
using System.Linq;
using System;

namespace ScarletTeleports.Data;

public class PlayerDataOptions {
  public string CharacterName { get; set; } = default;
  public ulong PlatformID { get; set; } = 0;
  public bool IsOnline { get; set; } = false;
  public bool IsAdmin { get; set; } = false;
  public Entity UserEntity { get; set; } = default;
  public Entity CharacterEntity { get; set; } = default;
  public User User { get; set; }
}

public class PlayerData {
  public string Name { get; set; }
  public Entity UserEntity { get; set; }
  public Entity CharacterEntity { get; set; }
  public ulong PlatformID { get; set; }
  public bool IsOnline { get; set; }
  public int MaxTeleports { get; set; } = Settings.Get<int>("DefaultMaximumPersonalTeleports");
  public bool CanResquestTeleports { get; set; } = true;
  public bool BypassCost { get; set; } = false;
  public bool BypassCooldown { get; set; } = false;
  public bool BypassDraculaRoom { get; set; } = false;
  public bool BypassCombat { get; set; } = false;
  public bool BypassRestrictedZones { get; set; } = false;
  public HashSet<TeleportData> Teleports { get; set; } = [];
  public HashSet<TeleportRequestData> PendingRequests { get; set; } = [];
  public DateTime LastTeleportTime { get; set; } = DateTime.Now.AddHours(-1);
  public bool LoadedTeleports { get; set; } = false;
  public bool IsAdmin => UserEntity.Read<User>().IsAdmin;

  public PlayerData(PlayerDataOptions options) {
    Name = options.CharacterName;
    PlatformID = options.PlatformID;
    IsOnline = options.IsOnline;
    UserEntity = options.UserEntity;
    CharacterEntity = options.CharacterEntity;
  }

  public void AddTeleport(TeleportData teleport) {
    if (teleport == null) return;

    var existingTeleport = Teleports.FirstOrDefault(t => t.Name.Equals(teleport.Name, StringComparison.OrdinalIgnoreCase));

    if (existingTeleport != null && !existingTeleport.Equals(default(TeleportData))) {
      Teleports.Remove(existingTeleport);
    }

    Teleports.Add(teleport);
  }

  public TeleportData GetTeleport(string name) {
    return Teleports.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
  }

  public bool HasTeleport(string name) {
    return Teleports.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
  }

  public int RemoveTeleport(string name) {
    return Teleports.RemoveWhere(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
  }

  public void ClearTeleports() {
    Teleports.Clear();
  }
}
