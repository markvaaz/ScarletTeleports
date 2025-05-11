using Command = VampireCommandFramework.CommandAttribute;
using CommandGroup = VampireCommandFramework.CommandGroupAttribute;
using ChatCommandContext = VampireCommandFramework.ChatCommandContext;
using ScarletTeleports.Services;
using ScarletTeleports.Data;
using Stunlock.Core;
using Unity.Mathematics;
using ScarletTeleports.Utils;

namespace ScarletTeleports.Commands;
[CommandGroup("st")]
internal static class AdminCommands {
  [Command("goto", usage: "<x> <y> <z>", adminOnly: true)]
  public static void GotoTeleport(ChatCommandContext ctx, int x, int y, int z) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    TeleportService.TeleportToPosition(player, new float3(x, y, z));

    ctx.Reply($"Teleported you to ~({x}, {y}, {z})~.".Format());
  }

  [Command("goto", usage: "<player-name>", adminOnly: true)]
  public static void GotoTeleport(ChatCommandContext ctx, string playerName) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    if (!TryGetPlayerByName(ctx, playerName, out var playerTarget)) return;
    TeleportService.TeleportToPosition(player, playerTarget.CharacterEntity.Position());

    ctx.Reply($"Teleported you to ~{playerName}~.".Format());
  }

  [Command("summon all", adminOnly: true)]
  public static void SummonAllPlayers(ChatCommandContext ctx) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    var players = Core.Players.GetAllPlayers();

    foreach (var playerTarget in players) {
      if (!playerTarget.IsOnline) continue;
      TeleportService.TeleportToPosition(playerTarget, player.CharacterEntity.Position());
    }

    ctx.Reply($"Teleported ~All~ players to you.".Format());
  }

  [Command("summon", usage: "<player-name>", adminOnly: true)]
  public static void SummonPlayer(ChatCommandContext ctx, string playerName) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    if (!TryGetPlayerByName(ctx, playerName, out var playerTarget)) return;
    TeleportService.TeleportToPosition(playerTarget, player.CharacterEntity.Position());

    ctx.Reply($"Teleported ~{playerName}~ to you.".Format());
  }

  /* ** GLOBAL TELEPORTS ** */

  [Command("add global", usage: "<teleport-name> <x> <y> <z>", adminOnly: true)]
  public static void AddTeleport(ChatCommandContext ctx, string teleportName, float x, float y, float z) {
    CreateGlobalTeleport(ctx, teleportName, x, y, z);
  }

  [Command("add global", usage: "<teleport-name>", adminOnly: true)]
  public static void AddTeleport(ChatCommandContext ctx, string teleportName) {
    float3 position = ctx.Event.SenderCharacterEntity.Position();
    CreateGlobalTeleport(ctx, teleportName, position.x, position.y, position.z);
  }

  [Command("make global", usage: "<teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown> <x> <y> <z>", adminOnly: true)]
  public static void MakeTeleportGlobal(ChatCommandContext ctx, string teleportName, string perfabName, int prefabGUID, int cost, int cooldown, int x, int y, int z) {
    CreateGlobalTeleport(ctx, teleportName, x, y, z, perfabName, prefabGUID, cost, cooldown);
  }

  [Command("make global", usage: "<teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown>", adminOnly: true)]
  public static void MakeTeleportGlobal(ChatCommandContext ctx, string teleportName, string perfabName, int prefabGUID, int cost, int cooldown) {
    float3 position = ctx.Event.SenderCharacterEntity.Position();
    CreateGlobalTeleport(ctx, teleportName, position.x, position.y, position.z, perfabName, prefabGUID, cost, cooldown);
  }

  [Command("remove global", usage: "<teleport-name>", adminOnly: true)]
  public static void RemoveTeleport(ChatCommandContext ctx, string teleportName) {
    if (TeleportService.RemoveGlobalTeleport(teleportName)) {
      ctx.Reply($"Global Teleport ~{teleportName}~ removed successfully.".Format());
    } else {
      ctx.Reply($"Global Teleport ~{teleportName}~ not found.".FormatError());
    }
  }

  /* ** PERSONAL TELEPORTS ** */

  [Command("add personal", usage: "<player-name> <teleport-name> <x> <y> <z>", adminOnly: true)]
  public static void AddTeleport(ChatCommandContext ctx, string playerName, string teleportName, int x, int y, int z) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;
    CreatePersonalTeleport(ctx, player, playerName, teleportName, x, y, z);
  }

  [Command("add personal", usage: "<player-name> <teleport-name>", adminOnly: true)]
  public static void AddTeleport(ChatCommandContext ctx, string playerName, string teleportName) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;
    float3 position = ctx.Event.SenderCharacterEntity.Position();
    CreatePersonalTeleport(ctx, player, playerName, teleportName, position.x, position.y, position.z);
  }

  [Command("make personal", usage: "<player-name> <teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown> <x> <y> <z>", adminOnly: true)]
  public static void MakeTeleport(ChatCommandContext ctx, string playerName, string teleportName, string prefabName, int prefabGUID, int cost, int cooldown, int x, int y, int z) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;
    CreatePersonalTeleport(ctx, player, playerName, teleportName, x, y, z, prefabName, prefabGUID, cost, cooldown);
  }

  [Command("make personal", usage: "<player-name> <teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown>", adminOnly: true)]
  public static void MakeTeleport(ChatCommandContext ctx, string playerName, string teleportName, string perfabName, int prefabGUID, int cost, int cooldown) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;
    float3 position = ctx.Event.SenderCharacterEntity.Position();
    CreatePersonalTeleport(ctx, player, playerName, teleportName, position.x, position.y, position.z, perfabName, prefabGUID, cost, cooldown);
  }

  [Command("remove personal", usage: "<player-name> <teleport-name>", adminOnly: true)]
  public static void RemoveTeleport(ChatCommandContext ctx, string playerName, string teleportName) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    if (TeleportService.RemovePersonalTeleport(player, teleportName)) {
      ctx.Reply($"Teleport ~{teleportName}~ removed successfully.".Format());
    } else {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
    }
  }

  /* ** RESTRICTED ZONES ** */

  [Command("add restricted", usage: "<name> <radius> <x> <y> <z>", adminOnly: true)]
  public static void AddRestrictedZone(ChatCommandContext ctx, string name, float radius, int x, int y, int z) {
    CreateRestrictedZone(ctx, name, radius, x, y, z);
  }

  [Command("add restricted", usage: "<name> <radius>", adminOnly: true)]
  public static void AddRestrictedZone(ChatCommandContext ctx, string name, float radius) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    var position = player.CharacterEntity.Position();
    CreateRestrictedZone(ctx, name, radius, position.x, position.y, position.z);
  }

  [Command("make restricted", usage: "<name> <radius> <can-teleport-to> <can-teleport-from> <x> <y> <z>", adminOnly: true)]
  public static void MakeRestrictedZone(ChatCommandContext ctx, string name, float radius, bool canTeleportTo, bool canTeleportFrom, int x, int y, int z) {
    CreateRestrictedZone(ctx, name, radius, x, y, z, canTeleportTo, canTeleportFrom);
  }

  [Command("make restricted", usage: "<name> <radius> <can-teleport-to> <can-teleport-from>", adminOnly: true)]
  public static void MakeRestrictedZone(ChatCommandContext ctx, string name, float radius, bool canTeleportTo, bool canTeleportFrom) {
    if (!TryGetPlayerById(ctx, out var player)) return;
    var position = player.CharacterEntity.Position();
    CreateRestrictedZone(ctx, name, radius, position.x, position.y, position.z, canTeleportTo, canTeleportFrom);
  }

  [Command("remove restricted", usage: "<name>", adminOnly: true)]
  public static void RemoveRestricted(ChatCommandContext ctx, string name) {
    if (TeleportService.RemoveRestrictedZone(name)) {
      ctx.Reply($"Restricted area ~{name}~ removed successfully.".Format());
    } else {
      ctx.Reply($"Restricted area ~{name}~ not found.".FormatError());
    }
  }

  /* ** LIST ** */

  [Command("list all", adminOnly: true)]
  public static void ListAll(ChatCommandContext ctx) {
    var teleports = TeleportService.GetAllTeleports();
    ctx.Reply($"__**~All Teleports:~**__".Format());

    foreach (var t in teleports) {
      ctx.Reply($" • ~{t.Name}~ {(t.CharacterName != null ? $"- Owned by ~{t.CharacterName}~" : "- ~Global~")}".Format());
    }

    ctx.Reply($"Total: ~{teleports.Count}~ teleports.".Format());
  }

  [Command("list restricted", adminOnly: true)]
  public static void ListRestrictedZones(ChatCommandContext ctx) {
    var zones = TeleportService.RestrictedZones;
    ctx.Reply("__**~Restricted Zones:~**__".Format());

    foreach (var zone in zones.Values) {
      ctx.Reply($" • ~{zone.Name}~ - Radius: ~{zone.Radius}~ ({zone.Position[0]}, {zone.Position[1]}, {zone.Position[2]})".Format());
    }

    ctx.Reply($"Total: ~{zones.Count}~ zones.".Format());
  }

  [Command("list global", adminOnly: true)]
  public static void ListTeleports(ChatCommandContext ctx) {
    var teleports = TeleportService.GlobalTeleports;
    ctx.Reply("__**~Global Teleports:~**__".Format());

    foreach (var teleport in teleports.Values) {
      ctx.Reply($" • ~{teleport.Name}~ ({teleport.Position.x}, {teleport.Position.y}, {teleport.Position.z})".Format());
    }

    ctx.Reply($"Total: ~{teleports.Count}~ teleports.".Format());
  }

  [Command("list", adminOnly: true)]
  public static void ListPlayerTeleports(ChatCommandContext ctx, string playerName) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    ctx.Reply($"**Teleports for ~{playerName}~:**".Format());

    var teleports = player.Teleports;

    foreach (var teleport in teleports) {
      ctx.Reply($" • ~{teleport.Name}~ ({teleport.Position.x}, {teleport.Position.y}, {teleport.Position.z})".Format());
    }

    ctx.Reply($"Total: ~{teleports.Count}~ teleports.".Format());
  }

  [Command("bypass", usage: "<player-name> <cost|cooldown|dracularoom|combat> <true|false>", adminOnly: true)]
  public static void SetBypassOption(ChatCommandContext ctx, string playerName, string option, bool value) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    switch (option.ToLower()) {
      case "cost":
        player.BypassCost = value;
        break;
      case "cooldown":
        player.BypassCooldown = value;
        break;
      case "dracularoom":
        player.BypassDraculaRoom = value;
        break;
      case "combat":
        player.BypassCombat = value;
        break;
      default:
        ctx.Reply($"~Invalid option.~ Use: cost, cooldown, dracularoom, or combat.".FormatError());
        return;
    }

    TeleportService.SavePersonalTeleport(player);
    ctx.Reply($"Bypass ~{option.ToLower()}~ for ~{playerName}~ set to ~{value}~.".Format());
  }

  /* ** SETTINGS ** */

  [Command("set default", usage: "<global|personal> <prefabname|prefabguid|cost|cooldown> <value>", adminOnly: true)]
  public static void SetDefaults(ChatCommandContext ctx, string teleportType, string defaultType, string defaultValue) {
    teleportType = teleportType.ToLower();
    defaultType = defaultType.ToLower();
    defaultValue = defaultValue.ToLower();

    if (teleportType != "global" && teleportType != "personal") {
      ctx.Reply($"Invalid teleport type ~{teleportType}~.".FormatError());
      return;
    }

    if (defaultType != "prefabname" && defaultType != "prefabguid" && defaultType != "cost" && defaultType != "cooldown") {
      ctx.Reply($"Invalid default type ~{defaultType}~.".FormatError());
      return;
    }

    if (teleportType == "global") {
      switch (defaultType) {
        case "prefabname":
          Settings.Set("DefaultGlobalPrefabName", defaultValue);
          break;
        case "prefabguid":
          Settings.Set("DefaultGlobalPrefabGUID", int.Parse(defaultValue));
          break;
        case "cost":
          Settings.Set("DefaultGlobalCost", int.Parse(defaultValue));
          break;
        case "cooldown":
          Settings.Set("DefaultGlobalCooldown", int.Parse(defaultValue));
          break;
      }
    }

    if (teleportType == "personal") {
      switch (defaultType) {
        case "prefabname":
          Settings.Set("DefaultPersonalPrefabName", defaultValue);
          break;
        case "prefabguid":
          Settings.Set("DefaultPersonalPrefabGUID", int.Parse(defaultValue));
          break;
        case "cost":
          Settings.Set("DefaultPersonalCost", int.Parse(defaultValue));
          break;
        case "cooldown":
          Settings.Set("DefaultPersonalCooldown", int.Parse(defaultValue));
          break;
      }
    }
  }

  [Command("set maxteleports", usage: "<player-name> <max-teleports>", adminOnly: true)]
  public static void SetMaxTeleports(ChatCommandContext ctx, string playerName, int maxTeleports) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    if (maxTeleports < 0) {
      ctx.Reply($"Max teleports cannot be negative.".FormatError());
      return;
    }

    player.MaxTeleports = maxTeleports;

    TeleportService.SavePersonalTeleport(player);

    ctx.Reply($"Max teleports for ~{playerName}~ set to ~{maxTeleports}~.".Format());
  }

  [Command("set cost", usage: "<player-name> <teleport-name> <cost>", adminOnly: true)]
  public static void SetTeleportCost(ChatCommandContext ctx, string playerName, string teleportName, int cost) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    var teleport = player.GetTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    teleport.Cost = cost;
    teleport.IsDefaultCost = false;

    TeleportService.SavePersonalTeleport(player);

    ctx.Reply($"Teleport ~{teleportName}~ cost set to ~{cost}~.".Format());
  }

  [Command("set cost", usage: "<global-teleport-name> <cost>", adminOnly: true)]
  public static void SetTeleportCost(ChatCommandContext ctx, string teleportName, int cost) {
    var teleport = TeleportService.GetGlobalTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    teleport.Cost = cost;

    TeleportService.SaveGlobalTeleports();

    ctx.Reply($"Teleport ~{teleportName}~ cost set to ~{cost}~.".Format());
  }

  [Command("set cooldown", usage: "<player-name> <teleport-name> <cooldown>", adminOnly: true)]
  public static void SetTeleportCooldown(ChatCommandContext ctx, string playerName, string teleportName, int cooldown) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    var teleport = player.GetTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    teleport.Cooldown = cooldown;
    teleport.IsDefaultCooldown = false;

    TeleportService.SavePersonalTeleport(player);

    ctx.Reply($"Teleport ~{teleportName}~ cooldown set to ~{cooldown}~.".Format());
  }

  [Command("set cooldown", usage: "<teleport-name> <cooldown>", adminOnly: true)]
  public static void SetTeleportCooldown(ChatCommandContext ctx, string teleportName, int cooldown) {
    var teleport = TeleportService.GetGlobalTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    teleport.Cooldown = cooldown;
    teleport.IsDefaultCooldown = false;

    TeleportService.SaveGlobalTeleports();

    ctx.Reply($"Teleport ~{teleportName}~ cooldown set to ~{cooldown}~.".Format());
  }

  [Command("set prefab", usage: "<player-name> <teleport-name> <prefab-name> <prefab-guid>", adminOnly: true)]
  public static void SetTeleportPrefab(ChatCommandContext ctx, string playerName, string teleportName, string prefabName, string prefabGUID) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    var teleport = player.GetTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    if (!PrefabGUID.TryParse(prefabGUID, out var parsedPrefab)) {
      ctx.Reply($"Prefab ~{prefabGUID}~ not found.".FormatError());
      return;
    }

    teleport.PrefabName = prefabName;
    teleport.PrefabGUID = parsedPrefab;
    teleport.IsDefaultPrefab = false;

    TeleportService.SavePersonalTeleport(player);

    ctx.Reply($"Teleport ~{teleportName}~ prefab set to ~{prefabGUID}~.".Format());
  }

  [Command("set prefab", usage: "<teleport-name> <prefab-name> <prefab-guid>", adminOnly: true)]
  public static void SetTeleportPrefab(ChatCommandContext ctx, string teleportName, string prefabName, string prefabGUID) {
    var teleport = TeleportService.GetGlobalTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    if (!PrefabGUID.TryParse(prefabGUID, out var parsedPrefab)) {
      ctx.Reply($"Prefab {prefabGUID} is invalid.".FormatError());
      return;
    }

    teleport.PrefabName = prefabName;
    teleport.PrefabGUID = parsedPrefab;
    teleport.IsDefaultPrefab = false;

    TeleportService.SaveGlobalTeleports();

    ctx.Reply($"Teleport ~{teleportName}~ prefab set to ~{prefabName}~ (~{prefabGUID}~).".Format());
  }

  // ...existing code...

  [Command("get info", usage: "<player-name> <teleport-name>", adminOnly: true)]
  public static void GetTeleportInfo(ChatCommandContext ctx, string playerName, string teleportName) {
    if (!TryGetPlayerByName(ctx, playerName, out var player)) return;

    var teleport = player.GetTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    ctx.Reply($"Info for teleport ~{teleport.Name}~:");
    ctx.Reply($" • Prefab: ~{teleport.PrefabName}~ (~{teleport.PrefabGUID}~)".Format());
    ctx.Reply($" • Cost: ~{teleport.Cost}~".Format());
    ctx.Reply($" • Cooldown: ~{teleport.Cooldown}~".Format());
    ctx.Reply($" • Position: (~{teleport.Position.x}~, ~{teleport.Position.y}~, ~{teleport.Position.z}~)".Format());
  }

  [Command("get info", usage: "<teleport-name>", adminOnly: true)]
  public static void GetGlobalTeleportInfo(ChatCommandContext ctx, string teleportName) {
    var teleport = TeleportService.GetGlobalTeleport(teleportName);

    if (teleport.Equals(default(TeleportData))) {
      ctx.Reply($"Teleport ~{teleportName}~ not found.".FormatError());
      return;
    }

    ctx.Reply($"Info for global teleport ~{teleport.Name}~:");
    ctx.Reply($" • Prefab: ~{teleport.PrefabName}~ (~{teleport.PrefabGUID}~)".Format());
    ctx.Reply($" • Cost: ~{teleport.Cost}~".Format());
    ctx.Reply($" • Cooldown: ~{teleport.Cooldown}~".Format());
    ctx.Reply($" • Position: (~{teleport.Position.x}~, ~{teleport.Position.y}~, ~{teleport.Position.z}~)".Format());
  }

  [Command("whereami", "wai", adminOnly: true)]
  public static void WhereAmI(ChatCommandContext ctx) {
    if (!TryGetPlayerById(ctx, out var player)) return;

    var position = player.CharacterEntity.Position();

    ctx.Reply($"You are at (~{position.x}~, ~{position.y}~, ~{position.z}~).".Format());
  }

  [Command("iwanttoclearallglobalteleports", adminOnly: true)]
  public static void ClearAllGlobalTeleports(ChatCommandContext ctx) {
    TeleportService.GlobalTeleports.Clear();
    TeleportService.SaveGlobalTeleports();
    ctx.Reply($"All global teleports cleared.".Format());
  }

  [Command("iwanttoclearallplayerteleports", adminOnly: true)]
  public static void ClearAllPlayerTeleports(ChatCommandContext ctx) {
    Core.Players.GetAllPlayers().ForEach(p => p.Teleports.Clear());
    TeleportService.PersonalTeleports.Clear();
    TeleportService.SaveAllPersonalTeleports();
    ctx.Reply($"All global teleports cleared.".Format());
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

  public static void CreateRestrictedZone(ChatCommandContext ctx, string name, float radius, float x, float y, float z, bool canTeleportTo = false, bool canTeleportFrom = false) {
    TeleportService.CreateRestrictedZone(new ZoneData(
      name,
      [x, y, z],
      radius,
      canTeleportTo,
      canTeleportFrom
    ));

    ctx.Reply($"Restricted area ~{name}~ added successfully.".Format());
  }

  private static void CreatePersonalTeleport(ChatCommandContext ctx, PlayerData player, string name, string teleportName, float x, float y, float z, string prefabName = null, int? prefabGUID = null, int? cost = null, int? cooldown = null) {
    if (player.HasTeleport(teleportName)) {
      ctx.Reply($"Teleport ~{teleportName}~ already exists.".FormatError());
      return;
    }

    if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z)) {
      ctx.Reply("Invalid coordinates.".FormatError());
      return;
    }

    var teleportDataOptions = new PersonalTeleportDataOptions {
      Name = teleportName,
      Position = [x, y, z],
      PrefabName = prefabName ?? Settings.Get<string>("DefaultPersonalPrefabName"),
      PrefabGUID = prefabGUID ?? Settings.Get<int>("DefaultPersonalPrefabGUID"),
      Cost = cost ?? Settings.Get<int>("DefaultPersonalCost"),
      Cooldown = cooldown ?? Settings.Get<int>("DefaultPersonalCooldown")
    };

    TeleportService.CreatePersonalTeleport(player, teleportDataOptions);
    ctx.Reply($"Teleport ~{teleportName}~ added successfully for ~{name}~.".Format());
  }

  public static void CreateGlobalTeleport(ChatCommandContext ctx, string teleportName, float x, float y, float z, string prefabName = null, int? prefabGUID = null, int? cost = null, int? cooldown = null) {
    if (TeleportService.HasGlobalTeleport(teleportName)) {
      ctx.Reply($"Teleport ~{teleportName}~ already exists.".FormatError());
      return;
    }

    if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z)) {
      ctx.Reply($"Invalid coordinates.".FormatError());
      return;
    }

    GlobalTeleportDataOptions teleportDataOptions = new() {
      Name = teleportName,
      Position = [x, y, z],
      PrefabName = prefabName ?? Settings.Get<string>("DefaultGlobalPrefabName"),
      PrefabGUID = prefabGUID ?? Settings.Get<int>("DefaultGlobalPrefabGUID"),
      Cost = cost ?? Settings.Get<int>("DefaultGlobalCost"),
      Cooldown = cooldown ?? Settings.Get<int>("DefaultGlobalCooldown")
    };

    TeleportService.CreateGlobalTeleport(teleportDataOptions);
    ctx.Reply($"Global Teleport ~{teleportName}~ added successfully.".Format());
  }
}