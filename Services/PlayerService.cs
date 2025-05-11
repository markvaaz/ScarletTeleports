using ScarletTeleports.Data;
using System.Collections.Generic;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using System.Linq;

namespace ScarletTeleports.Services;

public class PlayerService {
  public readonly Dictionary<string, PlayerData> PlayerNames = new();
  public readonly Dictionary<ulong, PlayerData> PlayerIds = new();

  public PlayerService() {
    ClearCache();
    EntityQueryBuilder queryBuilder = new(Allocator.Temp);

    queryBuilder.AddAll(ComponentType.ReadOnly<User>());
    queryBuilder.WithOptions(EntityQueryOptions.IncludeDisabled);

    EntityQuery query = Core.EntityManager.CreateEntityQuery(ref queryBuilder);

    var userEntities = query.ToEntityArray(Allocator.Temp);

    foreach (var entity in userEntities) {
      SetPlayerCache(entity);
    }
  }

  public void ClearCache() {
    PlayerNames.Clear();
    PlayerIds.Clear();
  }

  public void SetPlayerCache(Entity userEntity, bool isOffline = false) {
    var userData = userEntity.Read<User>();
    var name = userData.CharacterName.ToString();

    if (!PlayerNames.ContainsKey(name.ToLower()) && !PlayerIds.ContainsKey(userData.PlatformId)) {
      var newData = new PlayerData(new());
      PlayerNames[name.ToLower()] = newData;
      PlayerIds[userData.PlatformId] = newData;
    }

    var playerData = PlayerNames[name.ToLower()];

    playerData.Name = name;
    playerData.PlatformID = userData.PlatformId;
    playerData.IsOnline = !isOffline && userData.IsConnected;
    playerData.UserEntity = userEntity;
    playerData.CharacterEntity = userData.LocalCharacter._Entity;

    TeleportService.LoadPersonalTeleports(playerData);
  }

  public List<PlayerData> GetAdmins() {
    return [.. GetAllPlayers().Where(p => p.IsAdmin())];
  }

  public bool TryGetById(ulong platformId, out PlayerData playerData) {
    return PlayerIds.TryGetValue(platformId, out playerData);
  }

  public bool TryGetByName(string name, out PlayerData playerData) {
    return PlayerNames.TryGetValue(name.ToLower(), out playerData);
  }

  public List<PlayerData> GetAllPlayers() => PlayerIds.Values.ToList();
}


