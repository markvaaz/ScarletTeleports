using HarmonyLib;
using ProjectM;
using ScarletTeleports.Services;
using Stunlock.Network;
using System;
using Unity.Entities;

namespace ScarletTeleports.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnected_Patch {
	public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId) {
		if (!Core.hasInitialized) Core.Initialize();

		try {
			var index = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			var client = __instance._ApprovedUsersLookup[index];
			var userEntity = client.UserEntity;

			if (client == null || client.UserEntity.Equals(Entity.Null)) {
				Core.Log.LogWarning("Failed to get user entity.");
				return;
			}

			PlayerService.SetPlayerCache(userEntity);
		} catch (Exception e) {
			Core.Log.LogError($"An error occurred while connecting player: {e.Message}");
		}
	}
}

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class OnUserDisconnected_Patch {
	private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason) {
		if (!Core.hasInitialized) Core.Initialize();

		if (connectionStatusReason == ConnectionStatusChangeReason.Banned) return;

		try {
			var index = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			var client = __instance._ApprovedUsersLookup[index];

			PlayerService.SetPlayerCache(client.UserEntity);
		} catch (Exception e) {
			Core.Log.LogError($"An error occurred while disconnecting player: {e.Message}");
		}
	}
}