using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ScarletCore;
using ScarletCore.Services;
using ScarletCore.Systems;
using ScarletCore.Utils;
using Unity.Mathematics;

namespace ScarletTeleports.Patches;

[HarmonyPatch]
public static class WaypointPatch {
  public static readonly float3 Offset = new(0, 1, 0);
  [HarmonyPatch(typeof(TeleportToWaypointEventSystem), nameof(TeleportToWaypointEventSystem.OnUpdate))]
  [HarmonyPrefix]
  public static void Prefix(TeleportToWaypointEventSystem __instance) {
    var query = __instance._TeleportToWaypointEventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
    foreach (var entity in query) {
      var tpEvent = entity.Read<TeleportEvents_ToServer.TeleportToWaypointEvent>();
      var character = entity.Read<FromCharacter>().Character;
      var niem = GameSystems.NetworkIdSystem._NetworkIdLookupMap;

      if (!character.Exists() || !character.IsPlayer()) continue;

      if (!niem.TryGetValue(tpEvent.TargetChunkWaypoint, out var waypointEntity) || !waypointEntity.Exists()) {
        continue;
      }

      var position = waypointEntity.Position();

      TeleportService.Teleport(character, position + Offset);
    }
  }
}