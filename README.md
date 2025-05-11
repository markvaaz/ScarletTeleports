# ScarletTeleports

ScarletTeleports adds a complete teleport system to V Rising servers. Players and admins can create, manage, and use personal or global teleport points, request teleports to other players, easily list or remove teleports, and set custom costs, cooldowns, and permissions. The mod also allows defining restricted zones where teleportation is limited, configuring consumable items required for each teleport, and setting individual limits per player. All features are accessible through chat commands, with permission support and advanced options for server administration.

---

## Features

- **Personal Teleports:**  
  Players can set, list, use, and remove their own teleport points, with configurable limits and cooldowns.

- **Global Teleports:**  
  Admins can create teleport points accessible to all players, with full control over location, cost, cooldown, and name.

- **Teleport Requests:**  
  Players can request to teleport to each other, with accept/deny mechanics for safe and consensual travel. *(Teleports between players are considered Global Teleports)*

- **Summon & Goto:**  
  Admins can instantly teleport to players or summon them, individually or all at once.

- **Restricted Zones:**  
  Define areas where teleportation is limited or blocked, with fine-grained control over entry and exit permissions. *(Dracula's room is restricted by default)*

- **Custom Prefabs & Costs:**  
  Assign custom prefabs, costs, and cooldowns to teleports for immersive or balanced gameplay.

- **Bypass & Permissions:**  
  Grant special permissions to players to bypass certain restrictions, such as teleport in combat, cost, zone limits and other restrictions.

- **Comprehensive Command System:**  
  All features are accessible via easy-to-use chat commands, with detailed feedback and error messages.

- **Data Persistence:**  
  All teleports and settings are saved and loaded automatically, ensuring nothing is lost on server restart.

---

## Usage

For a full list of commands and usage, expand the **Show Commands**.

<details>
<summary>Show Commands</summary>

## Admin Commands

### Teleportation

- `.st goto`
  - **Usage:** `.st goto <x> <y> <z>` or `.st goto <player-name>`
  - **Description:** Teleport to the specified coordinates or to the specified player.

- `.st summon`
  - **Usage:** `.st summon all` or `.st summon <player-name>`
  - **Description:** Teleport all players to you or teleport a specific player to you.

### Global Teleports

- `.st add global`
  - **Usage:** `.st add global <teleport-name> <x> <y> <z>` or `.st add global <teleport-name>`
  - **Description:** Add a global teleport at the specified coordinates or at your current position.

- `.st make global`
  - **Usage:**  
    - `.st make global <teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown> <x> <y> <z>`  
    - `.st make global <teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown>`
  - **Description:** Create a custom global teleport at the specified coordinates or at your current position.

- `.st remove global`
  - **Usage:** `.st remove global <teleport-name>`
  - **Description:** Remove a global teleport.

### Personal Teleports

- `.st add personal`
  - **Usage:** `.st add personal <player-name> <teleport-name> <x> <y> <z>` or `.st add personal <player-name> <teleport-name>`
  - **Description:** Add a personal teleport for a player at the specified coordinates or at your current position.

- `.st make personal`
  - **Usage:**  
    - `.st make personal <player-name> <teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown> <x> <y> <z>`  
    - `.st make personal <player-name> <teleport-name> <prefab-name> <prefab-guid> <cost> <cooldown>`
  - **Description:** Create a custom personal teleport for a player at the specified coordinates or at your current position.

- `.st remove personal`
  - **Usage:** `.st remove personal <player-name> <teleport-name>`
  - **Description:** Remove a player's personal teleport.

### Restricted Zones

- `.st add restricted`
  - **Usage:** `.st add restricted <name> <radius> <x> <y> <z>` or `.st add restricted <name> <radius>`
  - **Description:** Add a restricted zone at the specified coordinates or at your current position.

- `.st make restricted`
  - **Usage:**  
    - `.st make restricted <name> <radius> <can-teleport-to> <can-teleport-from> <x> <y> <z>`  
    - `.st make restricted <name> <radius> <can-teleport-to> <can-teleport-from>`
  - **Description:** Create a custom restricted zone at the specified coordinates or at your current position.

- `.st remove restricted`
  - **Usage:** `.st remove restricted <name>`
  - **Description:** Remove a restricted zone.

### Listing

- `.st list`
  - **Usage:**  
    - `.st list all`  
    - `.st list restricted`  
    - `.st list global`  
    - `.st list <player>`
  - **Description:** List all teleports, restricted zones, global teleports, or a specific player's teleports.

### Settings & Management

- `.st bypass`
  - **Usage:** `.st bypass <player-name> <cost|cooldown|dracularoom|combat> <true|false>`
  - **Description:** Set bypass options for a player.

- `.st set default`
  - **Usage:** `.st set default <global|personal> <prefabname|prefabguid|cost|cooldown> <value>`
  - **Description:** Set default values for teleports.

- `.st set maxteleports`
  - **Usage:** `.st set maxteleports <player-name> <max-teleports>`
  - **Description:** Set the maximum number of personal teleports for a player.

- `.st set cost`
  - **Usage:**  
    - `.st set cost <player-name> <teleport-name> <cost>`  
    - `.st set cost <global-teleport-name> <cost>`
  - **Description:** Set the cost for a personal or global teleport.

- `.st set cooldown`
  - **Usage:**  
    - `.st set cooldown <player-name> <teleport-name> <cooldown>`  
    - `.st set cooldown <teleport-name> <cooldown>`
  - **Description:** Set the cooldown for a personal or global teleport.

- `.st set prefab`
  - **Usage:**  
    - `.st set prefab <player-name> <teleport-name> <prefab-name> <prefab-guid>`  
    - `.st set prefab <teleport-name> <prefab-name> <prefab-guid>`
  - **Description:** Set the prefab for a personal or global teleport.

- `.st get info`
  - **Usage:**  
    - `.st get info <player-name> <teleport-name>`  
    - `.st get info <teleport-name>`
  - **Description:** Show detailed info for a personal or global teleport.

### Utilities

- `.st whereami`
  - **Usage:** `.st whereami` (`wai`)
  - **Description:** Show your current position.

- `.st iwanttoclearallglobalteleports`
  - **Usage:** `.st iwanttoclearallglobalteleports`
  - **Description:** Remove all global teleports.

- `.st iwanttoclearallplayerteleports`
  - **Usage:** `.st iwanttoclearallplayerteleports`
  - **Description:** Remove all personal teleports.

---

## User Commands

### Personal Teleports

- `.st setteleport` | `.st stp`
  - **Usage:** `.st setteleport <name>`
  - **Description:** Create a personal teleport at your current position. You cannot create teleports in restricted zones (unless you are admin or have permission), nor exceed your personal teleport limit.

- `.st removeteleport` | `.st rtp`
  - **Usage:** `.st removeteleport <name>`
  - **Description:** Remove a personal teleport by name.

### Teleportation

- `.st teleport` | `.st tp`
  - **Usage:** `.st teleport <name>`
  - **Description:** Teleport to a personal or global teleport by name. Respects combat, Dracula's room, and restricted zone limitations, except for admins or players with bypass permissions.

### Listing

- `.st listteleport` | `.st ltp`
  - **Usage:** `.st listteleport`
  - **Description:** List all available personal and global teleports.

### Teleport Requests

- `.st teleportrequest` | `.st tpr`
  - **Usage:** `.st teleportrequest <player>`
  - **Description:** Request to teleport to another player. Only one pending request at a time is allowed. Consumes the configured item, except for admins.

- `.st teleportaccept` | `.st tpa`
  - **Usage:** `.st teleportaccept <player>`
  - **Description:** Accept a teleport request from another player. The requester will be teleported to you, as long as both are not in combat, restricted zones, or Dracula's room (except admins or players with bypass).

- `.st teleportdeny` | `.st tpd`
  - **Usage:** `.st teleportdeny <player>`
  - **Description:** Deny a teleport request from another player.

</details>

---

## Installation

### Requirements

This mod requires the following dependencies to function correctly:

* **[BepInEx (RC2)](https://wiki.vrisingmods.com/user/bepinex_install.html)**
* **[VampireCommandFramework](https://github.com/decaprime/VampireCommandFramework/releases/tag/v0.10.0)**

Make sure both are installed and loaded **before** installing ScarletTeleports.

### Manual Installation

1. Download the latest release of **ScarletTeleports**.

2. Extract the contents into your `BepInEx/plugins` folder:

   ```
   <V Rising Server Directory>/BepInEx/plugins/
   ```

   Your folder should now include:

   ```
   BepInEx/plugins/ScarletTeleports.dll
   ```

3. Ensure **VampireCommandFramework** is also installed in the `plugins` folder.

4. Start or restart your server.

---

## Configuration

All settings can be adjusted in the `ScarletTeleports.cfg` file located in your server's `BepInEx/config` folder.

<details>
<summary>Show Settings</summary>

### General

- **EnablePersonalTeleports**: If enabled, allows players to create personal teleports.  
  *Default: true*

- **EnablePersonalCooldown**: Enables cooldown for personal teleports.  
  *Default: true*

- **EnableGlobalCooldown**: Enables cooldown for global teleports.  
  *Default: true*

- **EnableDraculaRoom**: Enables teleporting from and to the Dracula's room.  
  *Default: false*

- **EnableTeleportInCombat**: Enables teleporting while in combat globally.  
  *Default: false*

- **EnableTeleportBetweenPlayers**: Enables teleporting between players.  
  *Default: true*

- **DefaulMaximumPersonalTeleports**: The maximum number of personal teleports a player can have.  
  *Default: 3*

### Timers

- **TeleportRequestExpiration**: The expiration time in seconds of a teleport request.  
  *Default: 30*

- **DefaultPersonalCooldown**: The cooldown in seconds for personal teleports.  
  *Default: 30*

- **DefaultGlobalCooldown**: The cooldown in seconds for global teleports.  
  *Default: 30*

### Prefabs

- **DefaultPersonalPrefabName**: The name of the prefab that will be consumed when teleporting to a personal teleport.  
  *Default: Blood Essence*

- **DefaultPersonalPrefabGUID**: The GUID of the prefab that will be consumed when teleporting to a personal teleport.  
  *Default: 862477668*

- **DefaultGlobalPrefabName**: The name of the prefab that will be consumed when teleporting to a global teleport.  
  *Default: Blood Essence*

- **DefaultGlobalPrefabGUID**: The GUID of the prefab that will be consumed when teleporting to a global teleport.  
  *Default: 862477668*

### Costs

- **DefaultPersonalCost**: The amount of the prefab that will be consumed when teleporting to a personal teleport.  
  *Default: 100*

- **DefaultGlobalCost**: The amount of the prefab that will be consumed when teleporting to a global teleport.  
  *Default: 50*

</details>

---

## Support

For help, bug reports, or suggestions, please join the [Scarlet Mods Discord](https://discord.gg/xZfVnstcY2).