# Pantheon Wars

A religion-based PvP mod for Vintage Story featuring custom religions, competing deities, and passive perk trees with dual progression systems.

## Overview

Pantheon Wars introduces a comprehensive religion and deity worship system where players create or join custom religions dedicated to different gods. Each religion unlocks unique passive perk trees that enhance all members. Players earn individual Divine Favor and collective Religion Prestige through PvP combat, unlocking powerful perks and progressing through dual ranking systems.

## Features

### Religion System ✅
- **Custom Player-Created Religions**: Create and name your own religions dedicated to any deity
- **Public & Private Religions**: Control who can join your congregation
- **Invitation System**: Invite specific players to join private religions
- **Founder Privileges**: Religion creators manage members and settings
- **Religion Switching**: Change religions with a 7-day cooldown (losing favor and perks)
- **Single Religion Membership**: Players can only be in one religion at a time

### Deity System ✅
- **8 Unique Deities**: Khoras (War), Lysa (Hunt), Morthen (Death), Aethra (Light), Umbros (Shadows), Tharos (Storms), Gaia (Earth), Vex (Madness)
- **Religion-Based Deity Assignment**: Your deity is determined by your religion
- **Deity Relationships**: Allied and rival deity dynamics affect favor and prestige gain
- **Deity-Specific Perk Trees**: Each deity has unique passive perks

### Dual Ranking System ✅
- **Player Favor Ranks**: Individual progression (Initiate → Disciple → Zealot → Champion → Avatar)
- **Religion Prestige Ranks**: Collective progression (Fledgling → Established → Renowned → Legendary → Mythic)
- **Divine Favor Currency**: Earned through PvP combat with deity relationship multipliers
- **Religion Prestige**: Earned collectively by all religion members through PvP

### Perk System ⚠️ (60% Complete)
- **160 Passive Perks**: 20 perks per deity (10 player perks + 10 religion perks)
- **Player Perks**: Unlock based on your individual Favor Rank
- **Religion Perks**: Unlock based on your religion's Prestige Rank, benefit all members
- **Stat Modifiers**: Perks provide passive bonuses (damage, defense, speed, etc.)
- **Special Effects**: Unique deity-themed abilities (lifesteal, poison, critical hits, etc.)
- **Perk Trees**: Visual progression with prerequisites and tiers

### PvP Features ⚠️ (Planned - Phase 4)
- **Divine Duels**: Formal 1v1 challenges with favor stakes
- **Crusade Events**: Server-wide deity war events
- **Relic System**: Powerful artifacts that grant temporary dominance
- **Apostate Mechanics**: Penalties and consequences for deity betrayal

## Development Setup

### Prerequisites
- .NET 8 SDK or later
- Vintage Story 1.21.0 or later
- Visual Studio 2022, VS Code, or JetBrains Rider

### Environment Variable
Set the `VINTAGE_STORY` environment variable to your Vintage Story installation directory:

**Windows:**
```powershell
$env:VINTAGE_STORY = "C:\Path\To\Vintage Story"
```

**Linux/Mac:**
```bash
export VINTAGE_STORY="/path/to/vintagestory"
```

### Building

**Windows:**
```powershell
./build.ps1
```

**Linux/Mac:**
```bash
./build.sh
```

This will:
1. Validate all JSON files
2. Build the mod
3. Create a release package in `Release/pantheonwars_x.x.x.zip`

### Debugging

Open `PantheonWars.sln` in your IDE and select either:
- **Vintage Story Client** - Launch client with mod loaded
- **Vintage Story Server** - Launch dedicated server with mod loaded

### Controls

- **K** - Open deity selection dialog

## Documentation

For detailed information about the mod's systems and mechanics, see:

**Phase 3 Documentation:**
- **[Implementation Guide](docs/implementation_guide.md)** - Development roadmap and phase breakdown (updated for Phase 3)
- **[Phase 3 Task Breakdown](docs/phase3_task_breakdown.md)** - Detailed task list and progress tracking
- **[Phase 3 Design Guide](docs/phase3_group_deity_perks_guide.md)** - Religion and perk system design specifications

**Legacy Documentation (Phase 1-2):**
- **[Deity Reference](docs/deity_reference.md)** - Complete deity information, relationships, and lore
- **[Favor System Guide](docs/favor_reference.md)** - How favor works, earning methods, and devotion ranks
- **[Ability Reference](docs/ability_reference.md)** - Old ability system (deprecated)

## Project Structure

```
PantheonWars/
├── CakeBuild/              # Build system
│   ├── Program.cs          # Build tasks and packaging
│   └── CakeBuild.csproj
├── docs/                   # Documentation
│   ├── implementation_guide.md          # Development roadmap
│   ├── phase3_task_breakdown.md         # Phase 3 detailed tasks
│   ├── phase3_group_deity_perks_guide.md # Phase 3 design doc
│   ├── deity_reference.md
│   ├── favor_reference.md
│   └── ability_reference.md (legacy)
├── PantheonWars/           # Main mod project
│   ├── Abilities/ (legacy) # Old ability system (Phase 1-2)
│   │   ├── Khoras/         # To be removed in Phase 3.5
│   │   └── Lysa/
│   ├── Commands/           # Chat commands
│   │   ├── DeityCommands.cs (legacy)
│   │   ├── AbilityCommands.cs (legacy)
│   │   ├── ReligionCommands.cs ✅ NEW
│   │   └── PerkCommands.cs ✅ NEW
│   ├── Data/               # Data models for persistence
│   │   ├── PlayerDeityData.cs (legacy)
│   │   ├── ReligionData.cs ✅ NEW
│   │   └── PlayerReligionData.cs ✅ NEW
│   ├── GUI/                # User interface
│   │   ├── DeitySelectionDialog.cs (legacy)
│   │   └── FavorHudElement.cs (updated for Phase 3)
│   ├── Models/             # Core data models
│   │   ├── Deity.cs
│   │   ├── Perk.cs ✅ NEW
│   │   ├── PrestigeRank.cs ✅ NEW
│   │   ├── FavorRank.cs ✅ NEW
│   │   ├── PerkType.cs ✅ NEW
│   │   ├── PerkCategory.cs ✅ NEW
│   │   └── Enums (DeityType, etc.)
│   ├── Network/            # Client-server networking
│   │   ├── PlayerDataPacket.cs (legacy)
│   │   └── PlayerReligionDataPacket.cs ✅ NEW
│   ├── Systems/            # Core game systems
│   │   ├── DeityRegistry.cs
│   │   ├── ReligionManager.cs ✅ NEW
│   │   ├── PlayerReligionDataManager.cs ✅ NEW
│   │   ├── ReligionPrestigeManager.cs ✅ NEW
│   │   ├── PerkRegistry.cs ✅ NEW
│   │   ├── PerkEffectSystem.cs ✅ NEW
│   │   ├── PerkDefinitions/ ✅ NEW
│   │   │   ├── KhorasPerks.cs (War - complete)
│   │   │   ├── LysaPerks.cs (Hunt - complete)
│   │   │   ├── MorthenPerks.cs (Death - complete)
│   │   │   ├── AethraPerks.cs (Light - stub)
│   │   │   ├── UmbrosPerks.cs (Shadows - stub)
│   │   │   ├── TharosPerks.cs (Storms - stub)
│   │   │   ├── GaiaPerks.cs (Earth - stub)
│   │   │   └── VexPerks.cs (Madness - stub)
│   │   ├── BuffSystem/ ✅ (Phase 2)
│   │   │   ├── BuffManager.cs
│   │   │   ├── ActiveEffect.cs
│   │   │   └── EntityBehaviorBuffTracker.cs
│   │   ├── PlayerDataManager.cs (legacy)
│   │   ├── AbilityCooldownManager.cs (legacy)
│   │   ├── FavorSystem.cs (updated for Phase 3)
│   │   └── AbilitySystem.cs (legacy - to be removed)
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── assets/
│   │   └── modinfo.json    # Mod metadata
│   ├── PantheonWars.csproj
│   └── PantheonWarsSystem.cs
├── Release/                # Build output
├── .gitignore
├── build.ps1               # Windows build script
├── build.sh                # Linux/Mac build script
├── PantheonWars.sln
└── README.md
```

## Current Status (v0.3.5 - Phase 3.3 Complete)

The mod is currently **60% complete** with core religion and perk systems functional. Phase 3 is in active development.

### Implemented Systems ✅

**Religion Management:**
- ✅ Create custom religions with any deity
- ✅ Public/private religion system with invitations
- ✅ Join, leave, and manage religions
- ✅ Founder privileges (kick members, disband, set description)
- ✅ 7-day switching cooldown with penalties
- ✅ Full persistence and save/load

**Dual Ranking System:**
- ✅ Player Favor Ranks (Initiate → Avatar) - Individual progression
- ✅ Religion Prestige Ranks (Fledgling → Mythic) - Collective progression
- ✅ PvP favor/prestige earning with deity relationship multipliers
- ✅ Rank-up notifications for both systems
- ✅ Network synchronization

**Perk System:**
- ✅ PerkRegistry with 160 perks registered
- ✅ Perk unlock validation (rank requirements, prerequisites)
- ✅ Stat modifier calculation and application (using VS Stats API)
- ✅ Perk persistence across sessions
- ✅ Combined player + religion perk effects
- ⚠️ 60/160 perks fully designed (3/8 deities complete)

**Available Deity Perk Trees (3/8):**
- ✅ **Khoras (War)** - 20 perks (combat, damage, defense)
- ✅ **Lysa (Hunt)** - 20 perks (tracking, ranged, mobility)
- ✅ **Morthen (Death)** - 20 perks (life drain, DoT, survivability)
- ⚠️ **Aethra (Light)** - Empty stub (20 perks needed)
- ⚠️ **Umbros (Shadows)** - Empty stub (20 perks needed)
- ⚠️ **Tharos (Storms)** - Empty stub (20 perks needed)
- ⚠️ **Gaia (Earth)** - Empty stub (20 perks needed)
- ⚠️ **Vex (Madness)** - Empty stub (20 perks needed)

**User Interface:**
- ✅ Enhanced HUD showing religion, deity, both ranks, favor/prestige
- ✅ All commands functional (17 commands total)
- ❌ Perk Tree Viewer GUI - Not yet implemented (Phase 3.5)
- ❌ Religion Management GUI - Not yet implemented (Phase 3.5)

## Development Roadmap

**Current Status:** Phase 3 In Progress (60% Complete - Phase 3.3 Complete)

- ✅ **Phase 1:** Foundation (MVP) - Complete
- ✅ **Phase 2:** Combat Integration - Complete
- ⚠️ **Phase 3:** Religion-Based Deity System with Perk Trees - 60% Complete
  - ✅ Phase 3.1: Foundation (Religion system, commands, persistence)
  - ✅ Phase 3.2: Ranking Systems (Dual progression, PvP integration)
  - ✅ Phase 3.3: Perk System Core (Registry, stat application, commands)
  - ⚠️ Phase 3.4: Deity Perk Trees (3/8 deities complete, 60/160 perks designed)
  - 🔲 Phase 3.5: Integration & Polish (UI, migration, testing)
- 🔲 **Phase 4:** Advanced Features - Planned (Divine duels, crusades, relics, apostates)

For detailed phase breakdowns, tasks, and timeline, see the **[Implementation Guide](docs/implementation_guide.md)**.

### Available Commands

**Religion Management (10 commands):**
- `/religion create <name> <deity> [public/private]` - Create a new religion
- `/religion join <religionname>` - Join an existing religion
- `/religion leave` - Leave your current religion
- `/religion list [deity]` - List all religions (optionally filter by deity)
- `/religion info [name]` - View religion details (defaults to your religion)
- `/religion members` - View members of your religion with ranks
- `/religion invite <playername>` - Invite a player to your religion
- `/religion kick <playername>` - Kick a member from your religion (founder only)
- `/religion disband` - Disband your religion (founder only)
- `/religion description <text>` - Set religion description (founder only)

**Perk Management (7 commands):**
- `/perks list` - Show all available perks for your deity
- `/perks player` - Show your unlocked player perks
- `/perks religion` - Show your religion's unlocked perks
- `/perks info <perkid>` - Get detailed perk information
- `/perks tree [player/religion]` - Display perk tree in text format
- `/perks unlock <perkid>` - Unlock a perk (if requirements met)
- `/perks active` - Show all active perks affecting you

**Legacy Commands (Phase 1-2 - Will be removed in Phase 3.5):**
- `/deity list` - Show all available deities
- `/deity info <deity>` - Get detailed deity information
- `/deity status` - View your current deity status
- `/favor` - Check your current divine favor
- `/ability list` - Show available abilities (deprecated)

## Contributing

This mod is currently in early development. Contributions, suggestions, and feedback are welcome!

## License

This project is licensed under the [Creative Commons Attribution 4.0 International License](LICENSE) (CC BY 4.0).

You are free to:
- **Share** — copy and redistribute the material in any medium or format
- **Adapt** — remix, transform, and build upon the material for any purpose, even commercially

Under the following terms:
- **Attribution** — You must give appropriate credit, provide a link to the license, and indicate if changes were made

See the [LICENSE](LICENSE) file for full details.

## Credits

- Built using the official [Vintage Story Mod Template](https://github.com/anegostudios/vsmodtemplate)
- Inspired by the [Karma System mod](https://mods.vintagestory.at/show/mod/28955)
