using System.Collections.Generic;

namespace ScarletTeleports.Data;

internal class SaveModel {
  public int MaxTeleports { get; set; } = 1;
  public bool BypassCost { get; set; } = false;
  public bool BypassCooldown { get; set; } = false;
  public bool BypassDraculaRoom { get; set; } = false;
  public bool BypassCombat { get; set; } = false;
  public bool BypassRestrictedZones { get; set; } = false;

  public HashSet<TeleportData> Teleports { get; set; } = [];
}