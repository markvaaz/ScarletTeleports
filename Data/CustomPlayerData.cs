using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.Json.Serialization;

namespace ScarletTeleports.Data;

public class CustomPlayerData {
  public int MaxTeleports { get; set; } = Plugin.Settings.Get<int>("DefaultMaximumPersonalTeleports");
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