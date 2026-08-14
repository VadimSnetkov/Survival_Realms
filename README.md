# Survival Realms

**Survival Realms** is a 3D first-person survival and RPG game developed in **Unity** as a university project. The game combines ideas and gameplay elements inspired by survival sandbox games such as *Minecraft*, *Muck*, and *Rust*, focusing on exploration, resource gathering, crafting, building, combat, and survival in a procedurally generated voxel world.

The main goal of the project is to create a playable survival sandbox where the player can explore a dynamically generated environment, collect resources, craft useful items, build structures, fight enemies, and manage basic survival parameters.

The project also serves as a practical implementation of different game-development concepts, including procedural generation, chunk-based world management, object interaction, inventory systems, data persistence, enemy AI, user interfaces, and modular gameplay systems.

---

## Project Overview

The player enters a procedurally generated voxel world consisting of different terrain types, resources, vegetation, enemies, and biomes.

There is no strictly linear path that the player must follow. Instead, the gameplay is based around a survival loop:

**Explore → Gather → Craft → Build → Fight → Survive → Explore further**

The world is divided into chunks that are dynamically generated and loaded depending on the player's position. Terrain generation uses procedural noise to create natural variations in elevation and biome distribution.

The project is designed with modularity in mind, allowing additional resources, blocks, crafting recipes, enemies, biomes, buildings, and gameplay mechanics to be added later.

---

# Main Features

## Procedural Voxel World Generation

The game world is generated procedurally instead of being manually created as one large static map.

The terrain consists of individual voxel blocks and is divided into chunks. Chunks around the player are dynamically generated and activated depending on the player's current position.

Procedural generation determines:

* terrain height;
* hills and mountains;
* underground layers;
* biome distribution;
* trees and vegetation;
* ores and resources;
* caves;
* environmental objects.

Using procedural generation makes every generated world different while avoiding the need to manually design the entire map.

---

## Chunk-Based World System

The world is separated into smaller areas called **chunks**.

The `ChunkManager` monitors the player's position and determines which chunks should currently exist around the player.

When the player approaches a new area:

1. The player's current chunk coordinates are calculated.
2. The game determines which surrounding chunks are required.
3. Missing chunks are instantiated.
4. `VoxelWorldGenerator` generates terrain for these chunks.
5. Chunks outside the configured view distance can be disabled.

This approach allows the world to be significantly larger than a single manually created Unity scene while reducing the amount of world geometry that needs to be active simultaneously.

---

# Terrain Generation

Terrain height is generated using procedural noise.

Instead of creating a completely flat world, different noise values are used to determine the surface height at each X/Z coordinate.

This creates:

* flat areas;
* small hills;
* valleys;
* larger elevated regions;
* mountains.

Multiple noise layers can be combined to make terrain less repetitive and more natural.

The terrain follows a logical vertical structure.

For example:

```text
Vegetation / objects
---------------------
Grass / Sand / Snow
Dirt
Dirt
Dirt
Stone
Stone
Stone + Ores
Stone
...
Bedrock
```

Surface blocks therefore depend on the biome, while underground materials depend primarily on depth.

---

# Biome System

The world contains multiple environmental regions generated using procedural biome noise.

The primary biomes are:

### Forest

The forest biome represents the standard survival environment.

It mainly contains:

* grass;
* dirt;
* trees;
* bushes;
* common resources.

Trees are procedurally distributed across suitable terrain, while bushes appear more frequently as smaller environmental objects.

### Desert

Desert areas use sand as their primary surface material.

The biome contains:

* sand;
* cacti;
* reduced vegetation;
* occasional lava areas.

Normal forest bushes and trees are prevented from spawning on desert terrain.

Cacti are generated instead, giving the biome its own recognizable environment.

### Snow

Snow regions use snow-covered terrain and different vegetation distribution.

This allows visually different parts of the world to exist without requiring separate manually created maps.

Biome generation is deterministic based on world coordinates and the selected world seed.

---

# Underground Generation

The underground environment uses several material layers.

Near the surface, the world primarily contains dirt.

At greater depths, dirt transitions into stone.

The lowest layer contains bedrock, preventing the player from simply digging indefinitely through the bottom of the generated world.

This structure provides a more logical underground environment compared with randomly distributing every block type at every height.

---

# Ore Generation

Resources such as iron are generated underground.

Ore generation considers depth, meaning valuable underground resources cannot normally appear directly on the surface.

For example, iron can only generate between configured minimum and maximum world heights.

Noise and probability are used together to create ore deposits instead of distributing individual ore blocks completely randomly.

This encourages the player to explore underground areas and mine deeper into the world.

---

# Cave Generation

The terrain generator supports procedural cave creation.

Caves are created by evaluating three-dimensional noise values for underground positions.

If a position satisfies the cave-generation conditions, the normal terrain block is not instantiated, leaving an empty space.

As a result, connected underground openings and tunnels can appear naturally inside the terrain.

Cave generation is restricted to underground areas so that it does not unnecessarily destroy the surface.

---

# Trees and Vegetation

Vegetation is generated after the terrain surface for a position has been determined.

Trees consist of multiple blocks:

* trunk blocks;
* leaf blocks.

The generator checks the biome before creating vegetation.

For example:

```text
Forest -> trees + bushes
Desert -> cacti
Snow -> biome-specific vegetation
```

This prevents logically incorrect combinations such as normal trees growing directly from desert sand.

Vegetation spawn probability can be configured through the Unity Inspector.

---

# Desert Cacti

Cacti replace normal bushes in desert regions.

A cactus is generated as a vertical structure consisting of several cactus blocks.

The generator randomly determines its height within a configurable range.

Cacti are only generated when:

* the current biome is Desert;
* the position is suitable;
* no conflicting object occupies the position;
* the random spawn probability succeeds.

They are not generated inside lava areas.

---

# Lava Areas

Desert regions can contain occasional lava formations.

Lava placement uses procedural noise rather than placing isolated lava blocks completely randomly. Nearby coordinates therefore tend to produce similar results, creating groups of lava blocks that resemble small lakes or pools.

Lava generation can be configured using parameters such as:

* frequency;
* threshold;
* maximum terrain height;
* lake depth.

This makes desert exploration more dangerous and gives the biome additional environmental variety.

---

# Player Controller

The game uses a first-person player controller.

The player can:

* walk;
* look around using the mouse;
* run;
* jump;
* interact with the environment;
* collect resources;
* break blocks;
* place blocks;
* select items from the hotbar.

The first-person perspective is designed to make resource gathering, building, exploration, and combat feel direct and intuitive.

---

# Player Survival System

The player has several survival-related statistics.

## Health

Health represents the player's current physical condition.

Health can be reduced by gameplay events such as enemy attacks or other hazards.

When health reaches zero, the player is considered dead and appropriate game-over or respawn logic can be executed.

## Stamina

Stamina represents the player's available physical energy.

Actions such as sprinting consume stamina.

When the player stops performing stamina-consuming actions, stamina gradually regenerates.

The current stamina value is displayed through the HUD.

## Hunger

Hunger represents the player's need for food.

The value can gradually change during gameplay and can later be connected to health regeneration, stamina, consumable items, or other survival mechanics.

Together, these systems create the basic survival component of the game.

---

# HUD and User Interface

The game contains an in-game HUD that provides important information without requiring the player to open additional menus.

The HUD includes elements such as:

* health bar;
* stamina bar;
* crosshair;
* hotbar;
* current time;
* current day;
* map control;
* interaction messages.

The UI updates dynamically according to the current player state.

---

# Interactive Messages

Context-sensitive messages are displayed when the player can interact with an object.

For example:

```text
Press E to gather
```

The message is only shown when the player is looking at or standing close enough to an interactable object.

After the interaction, the UI can display additional feedback indicating that the resource or item was collected.

This system helps the player understand available actions without permanently filling the screen with instructions.

---

# Resource Gathering

Resources can be collected directly from the game world.

The basic interaction process is:

1. The player approaches a resource.
2. The player aims at the resource.
3. The game detects the object using a raycast or interaction check.
4. An interaction message is displayed.
5. The player presses the interaction key.
6. The resource is added to the inventory.
7. The world object disappears or changes its state.

Gathered resources can later be used for crafting and building.

---

# Block Interaction

Voxel blocks can be interacted with directly.

The block interaction system supports two primary actions:

### Breaking Blocks

The player can damage and eventually destroy blocks.

Each destructible block can contain a `BlockHealth` component defining properties such as:

* block ID;
* maximum health;
* destruction effects;
* sound;
* dropped item.

After receiving enough damage, the block is destroyed and can produce loot.

### Placing Blocks

Placeable blocks from the player's inventory can be placed back into the world.

Placement uses the position of the targeted block to determine where the new block should appear.

This allows the player to modify the procedurally generated environment.

---

# Inventory System

The inventory stores resources, blocks, tools, and other items collected by the player.

Each inventory entry contains information such as:

```text
Item ID
Quantity
```

Items are connected to the game's block/item database through their unique IDs.

The inventory system supports:

* adding items;
* removing items;
* changing quantities;
* selecting items;
* displaying item icons;
* saving inventory data;
* loading previously saved inventory data.

---

# Hotbar

Frequently used items are displayed through the hotbar.

The player can switch between hotbar slots and immediately select blocks or tools.

The selected slot is visually highlighted.

Item information is synchronized with the inventory and block database, allowing icons and quantities to update automatically.

---

# Item and Block Database

The project uses a centralized block/item database.

Each registered object has a unique identifier and references to the data required by other gameplay systems.

An entry can contain:

```text
ID
Prefab
Icon
Type
Craftable state
```

The type determines whether the object behaves primarily as a placeable block or as an item/tool.

Using unique IDs allows inventory, crafting, world generation, and saving systems to reference the same objects consistently.

---

# Crafting System

The crafting system allows collected resources to be converted into useful items.

Crafting recipes are represented using Unity `ScriptableObject` assets.

A recipe defines:

```text
Output item
Output amount
Required ingredients
Ingredient quantities
Icon
```

During crafting:

1. The player opens the crafting menu.
2. A recipe is selected.
3. The game checks the inventory.
4. Required ingredient quantities are validated.
5. If enough resources exist, they are removed.
6. The crafted item is added to the inventory.
7. The interface is updated.

This architecture makes new recipes easy to add without rewriting the main crafting system.

---

# Building System

The player can use collected or crafted resources to modify the environment and construct structures.

Placeable objects can be selected from the hotbar and positioned in the voxel world.

Building is integrated with:

* inventory;
* block database;
* block interaction;
* world saving.

When a block is placed, the corresponding resource is removed from the inventory and its position becomes part of the persistent world state.

---

# Combat

The project contains basic combat mechanics for interaction with hostile entities.

Combat can use different detection methods depending on the weapon.

For example:

```text
Ranged weapon -> Raycast
Melee weapon -> Collider / range check
```

When an attack successfully reaches an enemy, damage is applied to its health.

If enemy health reaches zero, the enemy is defeated and can trigger death behaviour, animation, or item drops.

---

# Enemy and Mob System

The game supports AI-controlled entities.

Different mob types can have different behaviour configurations, including:

* hostile enemies;
* neutral creatures.

AI parameters can control properties such as:

* movement speed;
* aggression;
* player detection distance;
* attack range;
* health;
* behaviour state.

The system is designed so additional enemies can be created by using existing mob prefabs as a base and changing their models and configuration.

---

# Day and Night Cycle

The game contains a dynamic day/night system.

World lighting changes gradually according to the current game time.

The system controls:

* directional light;
* ambient lighting;
* sunrise;
* sunset;
* nighttime lighting;
* environmental lights.

The current time and day are displayed in the user interface.

This system makes the environment change dynamically while the player explores the world.

---

# Weather

A weather manager provides environmental variation.

Weather conditions can change automatically during gameplay.

For example, the world can alternate between:

* clear weather;
* rain.

Weather effects can use Unity particle systems and are synchronized with other environmental systems.

The system can be expanded later with additional weather types such as snow, storms, fog, or sandstorms.

---

# Save and Load System

The world supports persistent data using JSON files.

World information is stored in the Unity persistent data directory.

Saved information can include:

* block IDs;
* block positions;
* player-placed blocks;
* destroyed blocks;
* inventory contents;
* world/map information.

For example:

```json
{
    "blockID": "stone",
    "position": {
        "x": 10,
        "y": 15,
        "z": -4
    }
}
```

The `SaveManager` is responsible for registering world changes and writing them to persistent storage.

When the game starts again, saved information can be loaded and reconstructed.

---

# Automatic Saving

The game periodically saves world information automatically.

An autosave routine prevents the player from having to manually save after every world modification.

World data can therefore survive between game sessions.

This is particularly important in a survival/building game because the environment itself can be modified by the player.

---

# Game Data Structure

The project conceptually separates data into several groups.

## PlayerData

Contains player state:

```text
health
hunger
stamina
position
skills
```

Possible skills include:

```text
strength
agility
crafting
```

## InventoryData

Stores the contents of inventory slots:

```text
itemId
count
```

## WorldData

Stores information related to the current generated world:

```text
seed
timeOfDay
placedBuildings
placedBlocks
destroyedBlocks
```

## RecipeData

Contains crafting recipe definitions.

Recipes are primarily represented using Unity `ScriptableObject` assets.

## ItemData

Contains information about available game objects:

```text
ID
name
type
statistics
prefab
icon
```

## EnemyData

Contains enemy configuration:

```text
enemy type
health
behaviour parameters
spawn information
```

Together, these structures form the logical data model used by the game.

---

# Main Game Architecture

The project follows a modular component-based architecture based on Unity's GameObject and MonoBehaviour systems.

A simplified architecture can be represented as:

```text
                    GAME
                     |
        +------------+------------+
        |            |            |
      Player        World         UI
        |            |            |
   Controller    ChunkManager     HUD
   Survival          |
   Combat      VoxelWorldGenerator
   Interaction       |
        |       Biomes / Terrain
        |       Caves / Ores
        |       Vegetation
        |
   Inventory
        |
   +----+----+
   |         |
Crafting   Hotbar
   |
Item / Block Database

        SaveManager
             |
        JSON Save Data
```

Individual systems communicate with each other while remaining separated enough to allow individual mechanics to be changed or expanded.

---

# Important Scripts

Several scripts form the core of the project.

### `VoxelWorldGenerator.cs`

Responsible for procedural terrain generation.

It handles:

* terrain height;
* block layers;
* biome selection;
* ores;
* caves;
* trees;
* bushes;
* cacti;
* special biome features.

### `ChunkManager.cs`

Controls dynamic chunk loading around the player.

It calculates the current player chunk and generates or activates required neighboring chunks.

### `SaveManager.cs`

Handles persistent world data.

It stores information about blocks and loads saved world states.

### `WorldInitializer.cs`

Initializes the world and coordinates loading existing world information.

It can also start the automatic saving process.

### `BlockDatabase.cs`

Provides centralized access to registered blocks and items using their IDs.

### `BlockHealth.cs`

Controls destructible block health and destruction behaviour.

### `BlockInteraction.cs`

Handles player interaction with voxel blocks, including breaking and placing blocks.

### `InventoryManager.cs`

Stores and manages player inventory.

### `CraftingManager.cs`

Controls crafting recipes, ingredient validation, and creation of crafted objects.

### `FirstPersonController.cs`

Handles first-person player movement.

### `HeroPlayerScript.cs`

Controls player state such as health and stamina.

### `MobAI.cs`

Controls AI behaviour of game entities.

### `DayNightManager.cs`

Controls the world lighting cycle.

### `WeatherManager.cs`

Controls dynamic weather.

### `GameCanvas.cs`

Controls important gameplay UI elements.

---

# Technologies

The project is primarily developed using:

* **Unity**
* **C#**
* Unity GameObject/Component architecture
* Unity UI
* Unity physics and raycasting
* ScriptableObjects
* procedural noise generation
* JSON serialization
* prefab-based game architecture
* Git version control

---

# Project Structure

A simplified project structure is:

```text
Assets/
│
├── Animations/
├── Audio/
├── Data/
├── Materials and Textures/
├── Models/
├── Prefabs/
│   ├── Blocks/
│   ├── Items/
│   ├── Mobs/
│   └── UI/
│
├── Resources/
├── Scenes/
├── Scripts/
│   ├── VoxelWorldGenerator.cs
│   ├── ChunkManager.cs
│   ├── Chunk.cs
│   ├── SaveManager.cs
│   ├── WorldInitializer.cs
│   ├── BlockDatabase.cs
│   ├── BlockHealth.cs
│   ├── BlockInteraction.cs
│   ├── InventoryManager.cs
│   ├── CraftingManager.cs
│   ├── FirstPersonController.cs
│   ├── HeroPlayerScript.cs
│   ├── MobAI.cs
│   ├── DayNightManager.cs
│   ├── WeatherManager.cs
│   └── ...
│
├── ScriptableObjects/
├── Shaders/
├── Sounds/
└── UIElements/
```

---

# Basic Controls

Typical PC controls include:

| Action             | Control               |
| ------------------ | --------------------- |
| Move               | W / A / S / D         |
| Look around        | Mouse                 |
| Jump               | Space                 |
| Sprint             | Movement + sprint key |
| Interact / Gather  | E                     |
| Break block        | Left Mouse Button     |
| Place block        | Right Mouse Button    |
| Change hotbar item | Mouse Wheel           |
| Open Crafting      | C                     |
| Map                | M                     |
| Pause              | Escape                |

Controls may be changed during further development.

---

# Typical Gameplay Flow

A normal game session follows approximately this sequence:

1. The game initializes its managers.
2. Saved world information is loaded if available.
3. `ChunkManager` determines the player's current position.
4. Required chunks are created around the player.
5. `VoxelWorldGenerator` generates terrain.
6. Biomes and environmental objects are generated.
7. The player is positioned in the world.
8. The player begins exploring.
9. Resources are collected.
10. Items and tools are crafted.
11. Blocks and structures can be built.
12. Enemies can be encountered and fought.
13. Health, stamina, and other survival values are managed.
14. The world changes over time through weather and the day/night cycle.
15. Player modifications are periodically saved.

This creates the main survival gameplay loop.

---

# Project Goals

The main goal of Survival Realms is not to reproduce the scale of commercial survival games, but to demonstrate how the major systems behind this type of game can work together inside a single Unity project.

The project demonstrates practical implementation of:

* procedural content generation;
* dynamic world loading;
* first-person movement;
* survival mechanics;
* inventory management;
* crafting;
* building;
* environmental interaction;
* enemy AI;
* persistent game data;
* modular game architecture;
* user interface design.

---

# Current State

The project currently provides the foundation of a playable survival sandbox.

The core systems are implemented around a modular voxel survival framework and are being customized and expanded for Survival Realms.

Particular attention has been given to improving the procedural generation system so that terrain follows logical rules rather than simply placing random blocks.

For example:

* grass/earth dominates normal surface terrain;
* sand belongs to desert areas;
* snow belongs to snow regions;
* stone is primarily underground;
* iron is restricted by depth;
* trees only appear in suitable environments;
* desert vegetation uses cacti;
* caves can form underground;
* terrain elevation varies;
* special environmental features can depend on the biome.

---

# Future Development

The current architecture leaves room for significant future expansion.

Potential additions include:

* more advanced combat;
* additional weapons and tools;
* larger crafting trees;
* advanced building mechanics;
* additional ores and resources;
* food and cooking;
* farming;
* player leveling;
* RPG skills;
* quests;
* bosses;
* villages and structures;
* procedural dungeons;
* more advanced enemy AI;
* additional biomes;
* rivers and oceans;
* improved cave generation;
* improved terrain optimization;
* object pooling;
* mesh-based voxel chunks;
* multiplayer;
* improved world-save system;
* configurable world creation;
* procedural points of interest.

---

# University Project Documentation

Survival Realms was developed as a university software-development project.

In addition to the software itself, the project includes technical documentation describing the requirements, architecture, implementation, and usage of the system.

The documentation includes:

* **Programmatūras prasību specifikācija (PPS)** — Software Requirements Specification;
* **Programmatūras projektējuma apraksts (PPA)** — Software Design Description;
* **Lietotāja dokumentācija** — User Documentation.

These documents describe both the technical structure of the project and the player's interaction with the finished application.

---

# Third-Party Assets

The project uses third-party Unity assets as a foundation for some gameplay systems.

The voxel survival framework provides reusable systems for areas such as:

* voxel blocks;
* inventory;
* crafting;
* chunk management;
* saving/loading;
* first-person controls;
* environmental systems;
* UI;
* mob functionality.

These systems are integrated and modified as part of Survival Realms rather than representing the complete project by themselves.

Third-party assets remain subject to their respective licenses and should not be redistributed separately outside the conditions of those licenses.

---

# Development Approach

The project was developed iteratively.

Instead of attempting to implement every survival mechanic simultaneously, development was divided into individual systems:

```text
First-person movement
        ↓
World interaction
        ↓
Resource gathering
        ↓
Player statistics
        ↓
Inventory
        ↓
Procedural world
        ↓
Biomes
        ↓
Crafting / Building
        ↓
Enemies / Combat
        ↓
Saving
        ↓
Polish and expansion
```

This approach makes individual features easier to implement, test, debug, and demonstrate.

---

# Conclusion

**Survival Realms** is an experimental 3D survival sandbox that combines voxel world generation with traditional survival and RPG mechanics.

The project demonstrates how multiple independent gameplay systems can be connected into a single functioning game: the world is generated procedurally, divided into dynamically managed chunks, populated according to biome rules, modified by the player, and persisted through a save system.

Players can explore the generated world, manage survival statistics, gather resources, use an inventory and hotbar, craft items, build with blocks, interact with the environment, and encounter AI-controlled entities.

Although the current project represents a university-scale implementation rather than a complete commercial game, its modular architecture provides a foundation that can be expanded into a substantially larger survival experience in the future.
