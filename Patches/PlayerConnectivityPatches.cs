using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ScarletTeleports.Services;
using Stunlock.Core;
using Stunlock.Network;
using System;
using Unity.Collections;
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
			var userData = userEntity.Read<User>();
			bool creatingCharacter = userData.CharacterName.IsEmpty;

			if (!creatingCharacter) {
				PlayerService.SetPlayerCache(userEntity);
			}
		} catch (Exception e) {
			Core.Log.LogError($"An error occurred while connecting player: {e.Message}");
		}
	}
}

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class OnUserDisconnected_Patch {
	private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData) {
		if (!Core.hasInitialized) Core.Initialize();

		try {
			var index = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			var client = __instance._ApprovedUsersLookup[index];
			var userData = client.UserEntity.Read<User>();
			bool creatingCharacter = userData.CharacterName.IsEmpty;

			if (!creatingCharacter) {
				PlayerService.SetPlayerCache(client.UserEntity, true);
			}
		} catch (Exception e) {
			Core.Log.LogError($"An error occurred while disconnecting player: {e.Message}");
		}
	}
}

[HarmonyPatch(typeof(Destroy_TravelBuffSystem), nameof(Destroy_TravelBuffSystem.OnUpdate))]
public class Destroy_TravelBuffSystem_Patch {
	private static PrefabGUID netherCoffinGUID = new(722466953);

	private static void Postfix(Destroy_TravelBuffSystem __instance) {
		if (!Core.hasInitialized) Core.Initialize();

		var entities = __instance.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PrefabGUID>(), ComponentType.ReadOnly<EntityOwner>()).ToEntityArray(Allocator.Temp);

		try {
			foreach (var entity in entities) {
				PrefabGUID GUID = entity.Read<PrefabGUID>();

				if (GUID.Equals(netherCoffinGUID)) {
					var owner = entity.Read<EntityOwner>().Owner;

					if (!owner.Has<PlayerCharacter>()) return;

					var userEntity = owner.Read<PlayerCharacter>().UserEntity;

					PlayerService.SetPlayerCache(userEntity);
				}
			}
		} catch (Exception e) {
			Core.Log.LogError($"An error occurred while destroying travel buff: {e.Message}");
		} finally {
			entities.Dispose();
		}
	}
}