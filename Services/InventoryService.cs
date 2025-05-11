using System.Collections.Generic;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using ProjectM.Scripting;

namespace ScarletTeleports.Services;

public class InventoryService {
  private static ServerGameManager GameManager = Core.GameManager;
  private static EntityManager EntityManager = Core.EntityManager;

  public static Entity AddItemToInventory(Entity entity, PrefabGUID guid, int amount) {
    if (GameManager.HasFullInventory(entity)) return new Entity();

    try {
      var response = GameManager.TryAddInventoryItem(entity, guid, amount);

      return response.NewEntity;
    } catch (System.Exception e) {
      Core.Log.LogError(e);
    }

    return new Entity();
  }

  public static List<InventoryBuffer> GetInventoryItems(Entity entity) {
    int totalSlots = InventoryUtilities.GetInventorySize(EntityManager, entity);
    List<InventoryBuffer> items = [];

    for (int i = 0; i < totalSlots; i++) {
      if (InventoryUtilities.TryGetItemAtSlot(EntityManager, entity, i, out var item)) {
        items.Add(item);
      }
    }

    return items;
  }

  public static int GetInventorySize(Entity entity) {
    return InventoryUtilities.GetInventorySize(EntityManager, entity);
  }

  public static int GetItemAmount(Entity entity, PrefabGUID guid) {
    int totalSlots = GetInventorySize(entity);
    int amount = 0;

    for (int i = 0; i < totalSlots; i++) {
      if (InventoryUtilities.TryGetItemAtSlot(EntityManager, entity, i, out var item)) {
        if (item.ItemType.Equals(guid)) amount += item.Amount;
      }
    }

    return amount;
  }

  public static bool IsFull(Entity entity) {
    return GameManager.HasFullInventory(entity);
  }

  public static bool HasAmount(Entity entity, PrefabGUID guid, int amount) {
    return GameManager.GetInventoryItemCount(entity, guid) >= amount;
  }

  public static bool RemoveItemFromInventory(Entity entity, PrefabGUID guid, int amount) {
    if (!InventoryUtilities.TryGetInventoryEntity(EntityManager, entity, out var inventoryEntity)) return false;

    if (GetItemAmount(entity, guid) < amount) return false;

    GameManager.TryRemoveInventoryItem(inventoryEntity, guid, amount);

    return true;
  }
}