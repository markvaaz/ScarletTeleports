using HarmonyLib;
using ProjectM;
using Unity.Scenes;

namespace ScarletTeleports.Patches;

[HarmonyPatch(typeof(SceneSystem), nameof(SceneSystem.ShutdownStreamingSupport))]
public static class InitializationPatch {
	[HarmonyPostfix]
	public static void OneShot_AfterLoad_InitializationPatch() {
		if (!Core.hasInitialized) Core.Initialize();
		Plugin.Harmony.Unpatch(typeof(SpawnTeamSystem_OnPersistenceLoad).GetMethod("OnUpdate"), typeof(InitializationPatch).GetMethod("OneShot_AfterLoad_InitializationPatch"));
	}
}
