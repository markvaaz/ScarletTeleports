using Command = VampireCommandFramework.CommandAttribute;
using CommandGroup = VampireCommandFramework.CommandGroupAttribute;
using ChatCommandContext = VampireCommandFramework.ChatCommandContext;
using ScarletTeleports.Services;
using System;
using ScarletTeleports.Data;
using ScarletTeleports.Utils;
using System.Linq;
using ProjectM.Network;
using Stunlock.Core;
using ScarletCore.Data;
using ScarletCore.Services;

namespace ScarletTeleports.Commands;

public static class Constants {
  public const string Prefix = "stp";
}

[CommandGroup(Constants.Prefix)]
public static class UserCommands {
  private static Settings Settings => Plugin.Settings;
  private static readonly string Prefix = Constants.Prefix;

  [Command("setteleport", usage: "<teleport-name>", shortHand: "stp")]
  public static void SetTeleport(ChatCommandContext ctx, string teleportName) {
    if (!Settings.Get<bool>("EnablePersonalTeleports")) return;
    if (!TryGetPlayerById(ctx, out var player)) return;

    var zone = TeleportManager.GetRestrictedZone(player.CharacterEntity.GetPosition());
    var playerCD = TeleportManager.GetCustomPlayerData(player);

    if (!player.IsAdmin && zone != null && !zone.CanTeleportTo && !playerCD.BypassRestrictedZones) {
      ctx.Reply($"You cannot set a teleport while in a restricted zone.".FormatError());
      return;
    }

    if (!player.IsAdmin && playerCD.Teleports.Count >= playerCD.MaxTeleports) {
      ctx.Reply($"You have reached the maximum number of teleports ({playerCD.MaxTeleports}). Please remove one before adding a new one.".White());
      return;
    }

    if (playerCD.Teleports.Any(t => t.Name.Equals(teleportName))) {
      ctx.Reply($"You already have a teleport with that name. Please choose a different one.".FormatError());
      return;
    }

    var position = player.CharacterEntity.GetPosition();

    TeleportData teleport = new() {
      Name = teleportName,
      Position = position,
      PrefabGUID = new(Settings.Get<int>("DefaultPersonalPrefabGUID")),
      PrefabName = Settings.Get<string>("DefaultPersonalPrefabName"),
      Cost = Settings.Get<int>("DefaultPersonalCost"),
      Cooldown = Settings.Get<int>("DefaultPersonalCooldown"),
      CharacterName = player.Name,
    };

    TeleportManager.CreatePersonalTeleport(player, teleport);

    ctx.Reply($"Teleport ~{teleportName}~ set at your position.".Format());
  }

  [Command("teleport", usage: "<teleport-name>", shortHand: "tp")]
  public static void Teleport(ChatCommandContext ctx, string teleportName) {
    if (!TryGetPlayerById(ctx, out var player)) return;

    var playerCD = TeleportManager.GetCustomPlayerData(player);

    if (!player.IsAdmin && !Settings.Get<bool>("EnableTeleportInCombat") && !playerCD.BypassCombat && TeleportManager.IsPlayerInCombat(player.CharacterEntity)) {
      ctx.Reply($"You cannot teleport while in combat.".FormatError());
      return;
    }

    if (!player.IsAdmin && !Settings.Get<bool>("EnableDraculaRoom") && !playerCD.BypassDraculaRoom && TeleportManager.IsInDraculaRoom(player.CharacterEntity)) {
      ctx.Reply($"You cannot teleport while in the ~Dracula~'s room.".FormatError());
      return;
    }

    var fromZone = TeleportManager.GetRestrictedZone(player.CharacterEntity.GetPosition());

    if (!player.IsAdmin && fromZone != null && !fromZone.CanTeleportFrom && !playerCD.BypassRestrictedZones) {
      ctx.Reply($"You cannot teleport while in a restricted zone.".FormatError());
      return;
    }

    if (playerCD.HasTeleport(teleportName)) {
      var personalTeleport = playerCD.GetTeleport(teleportName);
      var personalToZone = TeleportManager.GetRestrictedZone(personalTeleport.Position);

      if (!player.IsAdmin && personalToZone != null && !personalToZone.CanTeleportTo && !playerCD.BypassRestrictedZones) {
        ctx.Reply($"You cannot teleport to a restricted zone.".FormatError());
        return;
      }

      if (TryTeleport(ctx, player, personalTeleport, teleportName)) return;
    }

    if (TeleportManager.GlobalTeleports.TryGetValue(teleportName, out var globalTeleport)) {
      var globalToZone = TeleportManager.GetRestrictedZone(globalTeleport.Position);

      if (globalToZone != null && !player.IsAdmin && !globalToZone.CanTeleportTo && !playerCD.BypassRestrictedZones) {
        ctx.Reply($"You cannot teleport to a restricted zone.".FormatError());
        return;
      }

      if (TryTeleport(ctx, player, globalTeleport, teleportName))
        return;
    }

    ctx.Reply($"Teleport ~{teleportName}~ not found.".Format());
  }

  private static bool TryTeleport(ChatCommandContext ctx, PlayerData player, TeleportData teleport, string name) {
    int cooldown = teleport.Cooldown;
    var playerCD = TeleportManager.GetCustomPlayerData(player);
    var timeLeft = playerCD.LastTeleportTime.AddSeconds(cooldown) - DateTime.Now;

    if (!player.IsAdmin && !playerCD.BypassCooldown && timeLeft > TimeSpan.Zero) {
      ctx.Reply($"You must wait ~{timeLeft.Minutes}~ minutes and ~{timeLeft.Seconds}~ seconds before teleporting again.".FormatError());
      return true;
    }

    if (!player.IsAdmin && !InventoryService.HasAmount(player.CharacterEntity, teleport.PrefabGUID, teleport.Cost)) {
      ctx.Reply($"You do not have enough ~{teleport.PrefabName}~ to teleport to ~{name}~.".FormatError());
      return true;
    }

    playerCD.LastTeleportTime = DateTime.Now;

    TeleportService.TeleportToPosition(player.CharacterEntity, teleport.Position);

    if (!playerCD.BypassCost) {
      InventoryService.RemoveItem(player.CharacterEntity, teleport.PrefabGUID, teleport.Cost);
    }

    ctx.Reply($"Teleported to ~{name}~ successfully.".Format());
    return true;
  }

  [Command("removeteleport", usage: "<teleport-name>", shortHand: "rtp")]
  public static void RemoveTeleport(ChatCommandContext ctx, string teleportName) {
    if (!TryGetPlayerById(ctx, out var player)) return;

    if (TeleportManager.RemovePersonalTeleport(player, teleportName)) {
      ctx.Reply($"Teleport ~{teleportName}~ removed successfully.".Format());
    } else {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
    }
  }

  [Command("listteleport", shortHand: "ltp")]
  public static void TeleportList(ChatCommandContext ctx) {
    if (!TryGetPlayerById(ctx, out var player)) return;

    var playerCD = TeleportManager.GetCustomPlayerData(player);
    var personalTeleports = playerCD.Teleports;
    var globalTeleports = TeleportManager.GlobalTeleports;

    if (personalTeleports.Count == 0 && globalTeleports.Count == 0) {
      ctx.Reply($"No teleports available.".Format());
      return;
    }

    ctx.Reply($"~Available Teleports:~".Underline().Bold().Format());

    if (personalTeleports.Count > 0) {
      ctx.Reply($"Personal".Format().Bold());

      foreach (var teleport in personalTeleports) {
        ctx.Reply($" • ~{teleport.Name}~".Format());
      }
    }

    if (globalTeleports.Count > 0) {
      ctx.Reply($"Global".Bold().Format());

      foreach (var teleport in TeleportManager.GlobalTeleports.Values) {
        ctx.Reply($" • ~{teleport.Name}~".Format());
      }
    }
  }

  [Command("teleportrequest", usage: "<player-name>", shortHand: "tpr")]
  public static void TeleportRequest(ChatCommandContext ctx, string playerName) {
    if (!Settings.Get<bool>("EnableTeleportBetweenPlayers")) {
      ctx.Reply($"Teleport between players are disabled.".FormatError());
      return;
    }
    if (!TryGetPlayerById(ctx, out var player)) return;
    if (!TryGetPlayerByName(ctx, playerName, out var playerTarget)) return;

    var playerCD = TeleportManager.GetCustomPlayerData(player);

    if (!playerCD.CanResquestTeleports) {
      ctx.Reply($"You already have a pending teleport request.".FormatError());
      return;
    }

    var prefabName = Settings.Get<string>("DefaultGlobalPrefabName");
    var prefabGUID = new PrefabGUID(Settings.Get<int>("DefaultGlobalPrefabGUID"));
    var cost = Settings.Get<int>("DefaultGlobalCost");

    if (!player.IsAdmin && !InventoryService.HasAmount(player.CharacterEntity, prefabGUID, cost)) {
      ctx.Reply($"You do not have enough ~{prefabName}~ to teleport to ~{playerName}~.".FormatError());
      return;
    }

    var targetCD = TeleportManager.GetCustomPlayerData(playerTarget);

    targetCD.PendingRequests.Add(new TeleportRequestData(player.PlatformId));

    playerCD.CanResquestTeleports = false;

    var user = playerTarget.UserEntity.Read<User>();

    MessageService.Send(user, $"~{player.Name}~ has requested to teleport to you, you have ~{Settings.Get<int>("TeleportRequestExpiration")}~ seconds to respond.".Format());
    MessageService.Send(user, $"Use ~.{Prefix} tpa {player.Name}~ to ~accept~ the request.".Format([null, "green"]));
    MessageService.Send(user, $"Use ~.{Prefix} tpd {player.Name}~ to ~deny~ the request.".Format([null, "#ff4d4d"]));

    ctx.Reply($"Teleport request sent to ~{playerName}~.".Format());
  }

  [Command("teleportaccept", usage: "<player-name>", shortHand: "tpa")]
  public static void TeleportAccept(ChatCommandContext ctx, string playerName) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    if (!TryGetPlayerByName(ctx, playerName, out var playerTarget)) return;

    var playerCD = TeleportManager.GetCustomPlayerData(player);
    var playerTargetCD = TeleportManager.GetCustomPlayerData(playerTarget);

    // Check if the player has a pending request
    if (!playerCD.PendingRequests.Any(r => r.PlatformID == playerTarget.PlatformId)) {
      ctx.Reply($"You do not have a pending request from ~{playerName}~.".FormatError());
      return;
    }

    var prefabName = Settings.Get<string>("DefaultGlobalPrefabName");
    var prefabGUID = new PrefabGUID(Settings.Get<int>("DefaultGlobalPrefabGUID"));
    var cost = Settings.Get<int>("DefaultGlobalCost");

    // Check if the target player has enough items
    if (!playerTarget.IsAdmin && !InventoryService.HasAmount(player.CharacterEntity, prefabGUID, cost)) {
      MessageService.Send(playerTarget.UserEntity.Read<User>(), $"You do not have enough ~{Settings.Get<string>("DefaultGlobalPrefabName")}~ to teleport to ~{playerName}~.".FormatError());
      return;
    }

    // Check if the player is in combat
    if (!playerTarget.IsAdmin && !Settings.Get<bool>("EnableTeleportInCombat") && !playerTargetCD.BypassCombat && TeleportManager.IsPlayerInCombat(playerTarget.CharacterEntity)) {
      ctx.Reply($"Player ~{player.Name}~ is currently in combat and cannot teleport.".FormatError());
      MessageService.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because you're in combat.".FormatError());
      return;
    }

    // Check if the target player is in Dracula's room
    if (!playerTarget.IsAdmin && !Settings.Get<bool>("EnableDraculaRoom") && !playerTargetCD.BypassDraculaRoom && TeleportManager.IsInDraculaRoom(playerTarget.CharacterEntity)) {
      ctx.Reply($"Player ~{playerTarget.Name}~ is in Dracula's room and cannot teleport.".FormatError());
      MessageService.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because you're in Dracula's room.".FormatError());
      return;
    }

    // check if the player is in Dracula's room
    if (!playerTarget.IsAdmin && !Settings.Get<bool>("EnableDraculaRoom") && !playerTargetCD.BypassDraculaRoom && TeleportManager.IsInDraculaRoom(player.CharacterEntity)) {
      ctx.Reply($"Teleport failed: you are in Dracula's room, which blocks incoming teleports.".FormatError());
      MessageService.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because they are in Dracula's room.".FormatError());
      return;
    }

    var zone = TeleportManager.GetRestrictedZone(player.CharacterEntity.GetPosition());

    // Check if the player is in a restricted zone
    if (!playerTarget.IsAdmin && !playerTargetCD.BypassRestrictedZones && zone != null && !zone.CanTeleportTo) {
      ctx.Reply($"Teleport failed: you are in a restricted zone, which blocks incoming teleports.".FormatError());
      MessageService.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because they are in a restricted zone.".FormatError());
      return;
    }

    var position = player.CharacterEntity.GetPosition();

    TeleportService.TeleportToPosition(playerTarget.CharacterEntity, position);

    playerTargetCD.CanResquestTeleports = true;

    if (!playerTargetCD.BypassCost) {
      InventoryService.RemoveItem(playerTarget.CharacterEntity, prefabGUID, cost);
    }

    ctx.Reply($"Player ~{playerTarget.Name}~ has teleported to you.".Format());
    MessageService.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was accepted. Teleporting now...".Format());
  }

  [Command("teleportdeny", usage: "<player-name>", shortHand: "tpd")]
  public static void TeleportDeny(ChatCommandContext ctx, string playerName) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    if (!TryGetPlayerByName(ctx, playerName, out var playerTarget)) return;

    var playerCD = TeleportManager.GetCustomPlayerData(player);

    if (!playerCD.PendingRequests.Any(r => r.PlatformID == playerTarget.PlatformId)) {
      ctx.Reply($"You do not have a pending request from ~{playerName}~.".FormatError());
      return;
    }

    var playerTargetCD = TeleportManager.GetCustomPlayerData(playerTarget);

    playerCD.PendingRequests.RemoveWhere(r => r.PlatformID == playerTarget.PlatformId);

    playerTargetCD.CanResquestTeleports = true;

    ctx.Reply($"Player ~{playerName}~ has been denied your teleport request.".Format());
    MessageService.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied.".Format());
  }

  public static bool TryGetPlayerById(ChatCommandContext ctx, out PlayerData player) {
    player = null;

    if (!PlayerService.TryGetById(ctx.User.PlatformId, out var playerData)) {
      ctx.Reply($"Unable to find your player data.".FormatError());
      return false;
    }

    player = playerData;

    return true;
  }

  public static bool TryGetPlayerByName(ChatCommandContext ctx, string name, out PlayerData player) {
    player = null;

    if (!PlayerService.TryGetByName(name, out var playerData)) {
      ctx.Reply($"Player ~{name}~ not found.".FormatError());
      return false;
    }

    player = playerData;

    return true;
  }
}