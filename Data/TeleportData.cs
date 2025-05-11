using System.Collections.Generic;
using Unity.Mathematics;
using Stunlock.Core;
using System;

namespace ScarletTeleports.Data;

public class TeleportDataOptions {
  public string Name { get; set; }
  public List<float> Position { get; set; }
  public int PrefabGUID { get; set; }
  public string PrefabName { get; set; }
  public int Cost { get; set; }
  public int Cooldown { get; set; }
  public bool IsDefaultCost { get; set; } = true;
  public bool IsDefaultPrefab { get; set; } = true;
  public bool IsDefaultCooldown { get; set; } = true;
}

public class GlobalTeleportDataOptions : TeleportDataOptions {
  public GlobalTeleportDataOptions() {
    PrefabGUID = Settings.Get<int>("DefaultGlobalPrefabGUID");
    PrefabName = Settings.Get<string>("DefaultGlobalPrefabName");
    Cost = Settings.Get<int>("DefaultGlobalCost");
    Cooldown = Settings.Get<int>("DefaultGlobalCooldown");
  }
}

public class PersonalTeleportDataOptions : TeleportDataOptions {
  public PersonalTeleportDataOptions() {
    PrefabGUID = Settings.Get<int>("DefaultPersonalPrefabGUID");
    PrefabName = Settings.Get<string>("DefaultPersonalPrefabName");
    Cost = Settings.Get<int>("DefaultPersonalCost");
    Cooldown = Settings.Get<int>("DefaultPersonalCooldown");
  }
}

public class TeleportData {
  public string Name { get; set; }
  public string CharacterName { get; set; }
  public float3 Position { get; set; }
  public PrefabGUID PrefabGUID { get; set; }
  public string PrefabName { get; set; }
  public int Cost { get; set; }
  public int Cooldown { get; set; }
  public bool IsDefaultCost { get; set; }
  public bool IsDefaultPrefab { get; set; }
  public bool IsDefaultCooldown { get; set; }
  public ulong PlatformID { get; set; }

  public TeleportData(TeleportDataOptions options, ulong platformID = 0, string characterName = null) {
    Name = options.Name;
    Position = new(options.Position[0], options.Position[1], options.Position[2]);
    PrefabGUID = new(options.PrefabGUID);
    PrefabName = options.PrefabName;
    Cost = options.Cost;
    Cooldown = options.Cooldown;
    IsDefaultCost = options.IsDefaultCost;
    IsDefaultPrefab = options.IsDefaultPrefab;
    IsDefaultCooldown = options.IsDefaultCooldown;
    PlatformID = platformID;
    CharacterName = characterName;
  }
}

public class TeleportRequestData {
  public ulong PlatformID { get; set; }
  public DateTime ExpirationTime { get; set; } = DateTime.Now.AddSeconds(Settings.Get<int>("TeleportRequestExpiration"));

  public TeleportRequestData(ulong platformID) {
    PlatformID = platformID;
  }
}