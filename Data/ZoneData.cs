using System.Collections.Generic;

namespace ScarletTeleports.Data;

public class ZoneData(string name, List<float> position, float radius, bool canTeleportTo = false, bool canTeleportFrom = false) {
  public string Name { get; set; } = name;

  public List<float> Position { get; set; } = position;

  public float Radius { get; set; } = radius;
  public bool CanTeleportTo { get; set; } = canTeleportTo;
  public bool CanTeleportFrom { get; set; } = canTeleportFrom;
}