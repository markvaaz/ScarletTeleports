using Unity.Entities;
using System.Collections.Generic;
using ProjectM.Network;
using System.Linq;
using System;
using System.Text.Json.Serialization;

namespace ScarletTeleports.Data;

public class PlayerData {
  [JsonIgnore]
  public Entity UserEntity;
  [JsonIgnore]
  public User User => UserEntity.Read<User>();
  [JsonIgnore]
  public string Name => User.CharacterName.ToString();
  [JsonIgnore]
  public Entity CharacterEntity => User.LocalCharacter._Entity;
  [JsonIgnore]
  public ulong PlatformId => User.PlatformId;
  [JsonIgnore]
  public bool IsOnline => User.IsConnected;
  [JsonIgnore]
  public bool IsAdmin => User.IsAdmin;
  [JsonIgnore]
  public DateTime ConnectedSince => DateTimeOffset.FromUnixTimeSeconds(User.TimeLastConnected).DateTime;
  public int MaxTeleports { get; set; } = Settings.Get<int>("DefaultMaximumPersonalTeleports");
  public bool BypassCost { get; set; } = false;
  public bool BypassCooldown { get; set; } = false;
  public bool BypassDraculaRoom { get; set; } = false;
  public bool BypassCombat { get; set; } = false;
  [JsonIgnore]
  public bool CanResquestTeleports { get; set; } = true;
  public bool BypassRestrictedZones { get; set; } = false;
  public HashSet<TeleportData> Teleports { get; set; } = [];
  [JsonIgnore]
  public HashSet<TeleportRequestData> PendingRequests { get; set; } = [];
  [JsonIgnore]
  public DateTime LastTeleportTime { get; set; } = DateTime.Now.AddHours(-1);
  [JsonIgnore]
  public bool LoadedTeleports { get; set; } = false;

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
