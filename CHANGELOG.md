## Updates

<details>
<summary>Update 1.2.0</summary>
  
- **New Feature**: Added waypoint teleportation command (`waypoint` / `wp`)
- Players can now open the game's native waypoint menu using the teleport system
- Waypoint teleports follow the same restrictions as regular teleports (combat, Dracula's room, restricted zones)
- No cooldown for waypoint teleports, but still consumes resources
- Added new configuration options:
  - `EnableWaypoints`: Toggle waypoint teleportation on/off
  - `DefaultWaypointCost`: Cost in resources to open waypoint menu (default: 75)
  - `DefaultWaypointPrefabGUID` and `DefaultWaypointPrefabName`: Configurable resource type
</details>

<details>
<summary>Update 1.1.2</summary>
  
- Fixed an issue where the player-to-player teleport incorrectly charged the target of the teleport 
</details>


<details>
<summary>Update 1.1.1</summary>
  
- Refactored teleport data management
- This update is needed for the latest ScarletCore version (to be released)
</details>


<details>
<summary>Update 1.1.0</summary>
  
- **Major Refactor**: Migrated most services and systems to use existing ScarletCore infrastructure.
- Improved performance and reduced code duplication by leveraging ScarletCore's established systems.
- Enhanced compatibility and stability through unified core architecture.
- Reduced mod footprint and potential conflicts with other ScarletCore-based mods.
</details>

<details>
<summary>Update 1.0.0</summary>
  
- Removed `Destroy_TravelBuffSystem.OnUpdate` patch, as it was causing lag. 
</details>

<details>
<summary>Update 0.1.41</summary>
  
- Teleport system optimized: resolved an issue that caused unnecessary memory allocation, which could lead to exponential growth over time.
- Thanks to **SirSaia** for pointing out the issue.
</details>

<details>
<summary>Update 0.1.40</summary>
  
- More performance and stability improvements.
</details>

<details>
<summary>Update 0.1.39</summary>
  
- Improved performance and stability.
</details>

<details>
<summary>Update 0.1.38</summary>
  
- Fixed an issue where settings were not displaying correctly in the config file.
</details>

<details>
<summary>Update 0.1.37</summary>
- Fixed a compatibility issue with **KindredCommands** due to a command prefix conflict (`.st`). Scarlet Teleports now uses **`.stp`** as its new prefix.
- Added the **restricted** option to the command bypass, allowing teleportation in restricted zones.
</details>
