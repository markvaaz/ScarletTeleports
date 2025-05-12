using ScarletTeleports.Data;
using System.Collections.Generic;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using System.Linq;

namespace ScarletTeleports.Services;

public class PlayerService {
  public readonly Dictionary<string, PlayerData> PlayerNames = [];
  public readonly Dictionary<ulong, PlayerData> PlayerIds = [];
  public readonly List<PlayerData> AllPlayers = [];

  public PlayerService() {
    ClearCache();
    EntityQueryBuilder queryBuilder = new(Allocator.Temp);

    queryBuilder.AddAll(ComponentType.ReadOnly<User>());
    queryBuilder.WithOptions(EntityQueryOptions.IncludeDisabled);

    EntityQuery query = Core.EntityManager.CreateEntityQuery(ref queryBuilder);

    try {
      var userEntities = query.ToEntityArray(Allocator.Temp);

      foreach (var entity in userEntities) {
        SetPlayerCache(entity);
      }
    } catch (System.Exception e) {
      Core.Log.LogError(e);
    } finally {
      query.Dispose();
      queryBuilder.Dispose();
    }
  }

  public void ClearCache() {
    PlayerNames.Clear();
    PlayerIds.Clear();
  }

  public void SetPlayerCache(Entity userEntity, bool isOffline = false) {
    var userData = userEntity.Read<User>();
    var name = userData.CharacterName.ToString();

    if (!PlayerIds.ContainsKey(userData.PlatformId)) {
      PlayerData newData = new();
      PlayerNames[name.ToLower()] = newData;
      PlayerIds[userData.PlatformId] = newData;
      AllPlayers.Add(newData);
    }

    var playerData = PlayerIds[userData.PlatformId];

    playerData.Name = name;
    playerData.PlatformID = userData.PlatformId;
    playerData.IsOnline = !isOffline && userData.IsConnected;
    playerData.UserEntity = userEntity;
    playerData.CharacterEntity = userData.LocalCharacter._Entity;

    TeleportService.LoadPersonalTeleports(playerData);
  }

  public List<PlayerData> GetAdmins() {
    return [.. AllPlayers.Where(p => p.IsAdmin)];
  }

  public bool TryGetById(ulong platformId, out PlayerData playerData) {
    return PlayerIds.TryGetValue(platformId, out playerData);
  }

  public bool TryGetByName(string name, out PlayerData playerData) {
    return PlayerNames.TryGetValue(name.ToLower(), out playerData);
  }
}


