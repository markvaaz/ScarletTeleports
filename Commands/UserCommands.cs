using Command = VampireCommandFramework.CommandAttribute;
using CommandGroup = VampireCommandFramework.CommandGroupAttribute;
using ChatCommandContext = VampireCommandFramework.ChatCommandContext;
using ScarletTeleports.Services;
using System;
using ScarletTeleports.Data;
using ScarletTeleports.Utils;
using System.Linq;
using ProjectM.Network;
using ScarletTeleports.Systems;
using Stunlock.Core;

// TODO: REFACTOR THIS MESS

namespace ScarletTeleports.Commands;
[CommandGroup("st")]
internal static class UserCommands {
  [Command("setteleport", usage: "<teleport-name>", shortHand: "stp")]
  public static void SetTeleport(ChatCommandContext ctx, string teleportName) {
    if (!Settings.Get<bool>("EnablePersonalTeleports")) return;
    if (!TryGetPlayerById(ctx, out var player)) return;

    var zone = TeleportService.GetRestrictedZone(player.CharacterEntity.Position());

    if (!player.IsAdmin && zone != null && !zone.CanTeleportTo && !player.BypassRestrictedZones) {
      ctx.Reply($"You cannot set a teleport while in a restricted zone.".FormatError());
      return;
    }

    if (!player.IsAdmin && player.Teleports.Count >= player.MaxTeleports) {
      ctx.Reply($"You have reached the maximum number of teleports ({player.MaxTeleports}). Please remove one before adding a new one.".White());
      return;
    }

    if (player.Teleports.Any(t => t.Name.Equals(teleportName))) {
      ctx.Reply($"You already have a teleport with that name. Please choose a different one.".FormatError());
      return;
    }

    var position = player.CharacterEntity.Position();

    PersonalTeleportDataOptions teleport = new() {
      Name = teleportName,
      Position = [position.x, position.y, position.z]
    };

    TeleportService.CreatePersonalTeleport(player, teleport);

    ctx.Reply($"Teleport ~{teleportName}~ set at your position.".Format());
  }

  [Command("teleport", usage: "<teleport-name>", shortHand: "tp")]
  public static void Teleport(ChatCommandContext ctx, string teleportName) {
    if (!TryGetPlayerById(ctx, out var player)) return;

    if (!player.IsAdmin && !Settings.Get<bool>("EnableTeleportInCombat") && !player.BypassCombat && TeleportService.IsPlayerInCombat(player.CharacterEntity)) {
      ctx.Reply($"You cannot teleport while in combat.".FormatError());
      return;
    }

    if (!player.IsAdmin && !Settings.Get<bool>("EnableDraculaRoom") && !player.BypassDraculaRoom && TeleportService.IsInDraculaRoom(player.CharacterEntity)) {
      ctx.Reply($"You cannot teleport while in the ~Dracula~'s room.".FormatError());
      return;
    }

    var fromZone = TeleportService.GetRestrictedZone(player.CharacterEntity.Position());

    if (!player.IsAdmin && fromZone != null && !fromZone.CanTeleportFrom && !player.BypassRestrictedZones) {
      ctx.Reply($"You cannot teleport while in a restricted zone.".FormatError());
      return;
    }

    if (player.HasTeleport(teleportName)) {
      var personalTeleport = player.GetTeleport(teleportName);
      var personalToZone = TeleportService.GetRestrictedZone(personalTeleport.Position);

      if (!player.IsAdmin && personalToZone != null && !personalToZone.CanTeleportTo && !player.BypassRestrictedZones) {
        ctx.Reply($"You cannot teleport to a restricted zone.".FormatError());
        return;
      }


      if (TryTeleport(ctx, player, personalTeleport, teleportName)) return;
    }

    if (TeleportService.GlobalTeleports.TryGetValue(teleportName, out var globalTeleport)) {
      var globalToZone = TeleportService.GetRestrictedZone(globalTeleport.Position);

      if (globalToZone != null && !player.IsAdmin && !globalToZone.CanTeleportTo && !player.BypassRestrictedZones) {
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
    var timeLeft = player.LastTeleportTime.AddSeconds(cooldown) - DateTime.Now;

    if (!player.IsAdmin && !player.BypassCooldown && timeLeft > TimeSpan.Zero) {
      ctx.Reply($"You must wait ~{timeLeft.Minutes}~ minutes and ~{timeLeft.Seconds}~ seconds before teleporting again.".FormatError());
      return true;
    }

    if (!player.IsAdmin && !InventoryService.HasAmount(player.CharacterEntity, teleport.PrefabGUID, teleport.Cost)) {
      ctx.Reply($"You do not have enough ~{teleport.PrefabName}~ to teleport to ~{name}~.".FormatError());
      return true;
    }

    player.LastTeleportTime = DateTime.Now;

    TeleportService.TeleportToPosition(player, teleport.Position);

    if (!player.BypassCost) {
      InventoryService.RemoveItemFromInventory(player.CharacterEntity, teleport.PrefabGUID, teleport.Cost);
    }

    ctx.Reply($"Teleported to ~{name}~ successfully.".Format());
    return true;
  }

  [Command("removeteleport", usage: "<teleport-name>", shortHand: "rtp")]
  public static void RemoveTeleport(ChatCommandContext ctx, string teleportName) {
    if (!TryGetPlayerById(ctx, out var player)) return;

    if (TeleportService.RemovePersonalTeleport(player, teleportName)) {
      ctx.Reply($"Teleport ~{teleportName}~ removed successfully.".Format());
    } else {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
    }
  }

  [Command("listteleport", shortHand: "ltp")]
  public static void TeleportList(ChatCommandContext ctx) {
    if (!TryGetPlayerById(ctx, out var player)) return;

    var personalTeleports = player.Teleports;
    var globalTeleports = TeleportService.GlobalTeleports;

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

      foreach (var teleport in TeleportService.GlobalTeleports.Values) {
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

    if (!player.CanResquestTeleports) {
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

    playerTarget.PendingRequests.Add(new TeleportRequestData(player.PlatformID));

    player.CanResquestTeleports = false;

    var user = playerTarget.UserEntity.Read<User>();

    SystemMessages.Send(user, $"~{player.Name}~ has requested to teleport to you, you have ~{Settings.Get<int>("TeleportRequestExpiration")}~ seconds to respond.".Format());
    SystemMessages.Send(user, $"Use ~.st tpa {player.Name}~ to ~accept~ the request.".Format([null, "green"]));
    SystemMessages.Send(user, $"Use ~.st tpd {player.Name}~ to ~deny~ the request.".Format([null, "#ff4d4d"]));

    ctx.Reply($"Teleport request sent to ~{playerName}~.".Format());
  }

  [Command("teleportaccept", usage: "<player-name>", shortHand: "tpa")]
  public static void TeleportAccept(ChatCommandContext ctx, string playerName) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    if (!TryGetPlayerByName(ctx, playerName, out var playerTarget)) return;

    // Check if the player has a pending request
    if (!player.PendingRequests.Any(r => r.PlatformID == playerTarget.PlatformID)) {
      ctx.Reply($"You do not have a pending request from ~{playerName}~.".FormatError());
      return;
    }

    var prefabName = Settings.Get<string>("DefaultGlobalPrefabName");
    var prefabGUID = new PrefabGUID(Settings.Get<int>("DefaultGlobalPrefabGUID"));
    var cost = Settings.Get<int>("DefaultGlobalCost");

    // Check if the target player has enough items
    if (!playerTarget.IsAdmin && !InventoryService.HasAmount(player.CharacterEntity, prefabGUID, cost)) {
      SystemMessages.Send(playerTarget.UserEntity.Read<User>(), $"You do not have enough ~{Settings.Get<string>("DefaultGlobalPrefabName")}~ to teleport to ~{playerName}~.".FormatError());
      return;
    }

    // Check if the player is in combat
    if (!playerTarget.IsAdmin && !Settings.Get<bool>("EnableTeleportInCombat") && !playerTarget.BypassCombat && TeleportService.IsPlayerInCombat(playerTarget.CharacterEntity)) {
      ctx.Reply($"Player ~{player.Name}~ is currently in combat and cannot teleport.".FormatError());
      SystemMessages.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because you're in combat.".FormatError());
      return;
    }

    // Check if the target player is in Dracula's room
    if (!playerTarget.IsAdmin && !Settings.Get<bool>("EnableDraculaRoom") && !playerTarget.BypassDraculaRoom && TeleportService.IsInDraculaRoom(playerTarget.CharacterEntity)) {
      ctx.Reply($"Player ~{playerTarget.Name}~ is in Dracula's room and cannot teleport.".FormatError());
      SystemMessages.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because you're in Dracula's room.".FormatError());
      return;
    }

    // check if the player is in Dracula's room
    if (!playerTarget.IsAdmin && !Settings.Get<bool>("EnableDraculaRoom") && !playerTarget.BypassDraculaRoom && TeleportService.IsInDraculaRoom(player.CharacterEntity)) {
      ctx.Reply($"Teleport failed: you are in Dracula's room, which blocks incoming teleports.".FormatError());
      SystemMessages.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because they are in Dracula's room.".FormatError());
      return;
    }

    var zone = TeleportService.GetRestrictedZone(player.CharacterEntity.Position());

    // Check if the player is in a restricted zone
    if (!playerTarget.IsAdmin && !playerTarget.BypassRestrictedZones && zone != null && !zone.CanTeleportTo) {
      ctx.Reply($"Teleport failed: you are in a restricted zone, which blocks incoming teleports.".FormatError());
      SystemMessages.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied because they are in a restricted zone.".FormatError());
      return;
    }

    var position = player.CharacterEntity.Position();

    TeleportService.TeleportToPosition(playerTarget, position);

    playerTarget.CanResquestTeleports = true;

    if (!playerTarget.BypassCost) {
      InventoryService.RemoveItemFromInventory(playerTarget.CharacterEntity, prefabGUID, cost);
    }

    ctx.Reply($"Player ~{playerTarget.Name}~ has teleported to you.".Format());
    SystemMessages.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was accepted. Teleporting now...".Format());
  }

  [Command("teleportdeny", usage: "<player-name>", shortHand: "tpd")]
  public static void TeleportDeny(ChatCommandContext ctx, string playerName) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    if (!TryGetPlayerByName(ctx, playerName, out var playerTarget)) return;

    if (!player.PendingRequests.Any(r => r.PlatformID == playerTarget.PlatformID)) {
      ctx.Reply($"You do not have a pending request from ~{playerName}~.".FormatError());
      return;
    }

    player.PendingRequests.RemoveWhere(r => r.PlatformID == playerTarget.PlatformID);

    playerTarget.CanResquestTeleports = true;

    ctx.Reply($"Player ~{playerName}~ has been denied your teleport request.".Format());
    SystemMessages.Send(playerTarget.UserEntity.Read<User>(), $"Your teleport request to ~{player.Name}~ was denied.".Format());
  }

  public static bool TryGetPlayerById(ChatCommandContext ctx, out PlayerData player) {
    player = null;

    if (!Core.Players.TryGetById(ctx.User.PlatformId, out var playerData)) {
      ctx.Reply($"Unable to find your player data.".FormatError());
      return false;
    }

    player = playerData;

    return true;
  }

  public static bool TryGetPlayerByName(ChatCommandContext ctx, string name, out PlayerData player) {
    player = null;

    if (!Core.Players.TryGetByName(name, out var playerData)) {
      ctx.Reply($"Player ~{name}~ not found.".FormatError());
      return false;
    }

    player = playerData;

    return true;
  }
}