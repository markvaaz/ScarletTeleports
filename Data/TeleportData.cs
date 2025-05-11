using System.Collections.Generic;
using Unity.Mathematics;
using Stunlock.Core;
using System;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Text.Json;

namespace ScarletTeleports.Data;

[JsonSerializable(typeof(TeleportData))]
public class TeleportData {
  public string Name { get; set; }
  public string CharacterName { get; set; }

  [JsonIgnore]
  public float3 Position { get; set; }

  [JsonPropertyName("Position")]
  public List<float> PositionSerialized {
    get => [Position.x, Position.y, Position.z];
    set {
      if (value is { Count: 3 }) {
        Position = new float3(value[0], value[1], value[2]);
      }
    }
  }

  [JsonIgnore]
  public PrefabGUID PrefabGUID { get; set; }

  [JsonPropertyName("PrefabGUID")]
  public int PrefabGUIDSerialized {
    get => PrefabGUID.GuidHash;
    set => PrefabGUID = new PrefabGUID(value);
  }

  public string PrefabName { get; set; }
  public int Cost { get; set; }
  public int Cooldown { get; set; }
  public bool IsDefaultCost { get; set; } = true;
  public bool IsDefaultPrefab { get; set; } = true;
  public bool IsDefaultCooldown { get; set; } = true;
  public ulong PlatformID { get; set; }

  public override bool Equals(object obj) {
    if (obj is not TeleportData other) return false;
    return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
  }

  public override int GetHashCode() {
    return Name.ToLowerInvariant().GetHashCode();
  }
}

public class TeleportRequestData {
  public ulong PlatformID { get; set; }
  public DateTime ExpirationTime { get; set; } = DateTime.Now.AddSeconds(Settings.Get<int>("TeleportRequestExpiration"));

  public TeleportRequestData(ulong platformID) {
    PlatformID = platformID;
  }
}