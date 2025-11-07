# Pantheon Wars

**Version:** 0.2.0
**Status:** In Active Development

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

### Perk System ⚠️ (In Development)
- **80 Passive Perks**: 10 perks per deity (6 player perks + 4 religion perks)
- **Player Perks**: Unlock based on your individual Favor Rank
- **Religion Perks**: Unlock based on your religion's Prestige Rank, benefit all members
- **Stat Modifiers**: Perks provide passive bonuses (damage, defense, speed, health, armor, etc.)
- **Special Effects**: Unique deity-themed abilities (lifesteal, poison, critical hits, etc.)
- **Perk Trees**: Command-based tree viewer and GUI perk browser

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

**Development Documentation:**
- **[Implementation Guide](docs/implementation_guide.md)** - Development roadmap and phase breakdown
- **[UI Refactoring Plan](docs/ui-refactoring-plan.md)** - UI architecture and refactoring strategy
- **[UI Refactoring Progress](docs/ui-refactoring-progress.md)** - Current UI development progress

**System Documentation:**
- **[Deity Reference](docs/deity_reference.md)** - Complete deity information, relationships, and lore
- **[Perk Reference](docs/perk_reference.md)** - Complete perk trees for all deities
- **[Favor System Guide](docs/favor_reference.md)** - How favor works, earning methods, and devotion ranks
- **[Balance Testing Guide](docs/balance_testing_guide.md)** - Testing procedures and balance considerations

**Technical Documentation:**
- **[Buff Implementation Guide](docs/BUFF_IMPLEMENTATION_GUIDE.md)** - Buff/debuff system implementation
- **[Perk Stat Application](docs/PERK_STAT_APPLICATION_IMPLEMENTATION.md)** - Stat modifier system
- **[Special Effects Guide](docs/special_effects_implementation_guide.md)** - Special effect implementations

## Project Structure

```
PantheonWars/
├── CakeBuild/              # Build system
│   ├── Program.cs          # Build tasks and packaging
│   └── CakeBuild.csproj
├── docs/                   # Documentation
│   ├── implementation_guide.md          # Development roadmap
│   ├── ui-refactoring-plan.md           # UI architecture guide
│   ├── ui-refactoring-progress.md       # UI development progress
│   ├── deity_reference.md               # Deity information
│   ├── perk_reference.md                # Perk trees
│   ├── favor_reference.md               # Favor system
│   ├── ability_reference.md             # Legacy ability system
│   ├── balance_testing_guide.md         # Testing procedures
│   ├── BUFF_IMPLEMENTATION_GUIDE.md     # Buff/debuff system
│   ├── PERK_STAT_APPLICATION_IMPLEMENTATION.md # Stat modifiers
│   └── special_effects_implementation_guide.md # Special effects
├── PantheonWars/           # Main mod project
│   ├── Abilities/          # Legacy ability system (Phase 1-2)
│   │   ├── Khoras/
│   │   └── Lysa/
│   ├── Commands/           # Chat commands
│   │   ├── DeityCommands.cs
│   │   ├── AbilityCommands.cs
│   │   ├── FavorCommands.cs
│   │   ├── ReligionCommands.cs
│   │   └── PerkCommands.cs
│   ├── Constants/          # Game constants
│   ├── Data/               # Data models for persistence
│   │   ├── PlayerDeityData.cs
│   │   ├── ReligionData.cs
│   │   └── PlayerReligionData.cs
│   ├── GUI/                # User interface
│   │   ├── DeitySelectionDialog.cs
│   │   ├── FavorHudElement.cs
│   │   ├── CreateReligionDialog.cs
│   │   ├── EditDescriptionDialog.cs
│   │   ├── InvitePlayerDialog.cs
│   │   ├── OverlayCoordinator.cs
│   │   ├── PerkDialog.cs
│   │   ├── PerkDialogEventHandlers.cs
│   │   ├── PerkDialogManager.cs
│   │   ├── PerkTreeLayout.cs
│   │   ├── ReligionManagementDialog.cs
│   │   ├── State/          # Dialog state management
│   │   └── UI/             # Reusable UI components
│   │       ├── Components/ # Shared UI components
│   │       │   ├── Buttons/
│   │       │   ├── Inputs/
│   │       │   └── Lists/
│   │       ├── Renderers/  # Rendering components
│   │       │   └── Components/
│   │       ├── State/      # UI state management
│   │       └── Utilities/  # UI utilities (colors, helpers)
│   ├── Models/             # Core data models
│   │   ├── Deity.cs
│   │   ├── Perk.cs
│   │   ├── PrestigeRank.cs
│   │   ├── FavorRank.cs
│   │   ├── PerkType.cs
│   │   ├── PerkCategory.cs
│   │   └── Enums (DeityType, etc.)
│   ├── Network/            # Client-server networking
│   │   ├── PlayerDataPacket.cs
│   │   └── PlayerReligionDataPacket.cs
│   ├── Systems/            # Core game systems
│   │   ├── DeityRegistry.cs
│   │   ├── ReligionManager.cs
│   │   ├── PlayerReligionDataManager.cs
│   │   ├── ReligionPrestigeManager.cs
│   │   ├── PerkRegistry.cs
│   │   ├── PerkEffectSystem.cs
│   │   ├── PerkDefinitions.cs (all 80 perks)
│   │   │   ├── Khoras (War - 10 perks)
│   │   │   ├── Lysa (Hunt - 10 perks)
│   │   │   ├── Morthen (Death - 10 perks)
│   │   │   ├── Aethra (Light - 10 perks)
│   │   │   ├── Umbros (Shadows - 10 perks)
│   │   │   ├── Tharos (Storms - 10 perks)
│   │   │   ├── Gaia (Earth - 10 perks)
│   │   │   └── Vex (Madness - 10 perks)
│   │   ├── BuffSystem/ (Phase 2)
│   │   │   ├── BuffManager.cs
│   │   │   ├── ActiveEffect.cs
│   │   │   └── EntityBehaviorBuffTracker.cs
│   │   ├── PlayerDataManager.cs
│   │   ├── AbilityCooldownManager.cs
│   │   ├── FavorSystem.cs
│   │   └── AbilitySystem.cs
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

## Current Status (v0.2.0 - In Active Development)

The mod has completed Phase 1 (Foundation) and Phase 2 (Combat Integration) and is actively working through Phase 3 (Religion-Based Deity System with Perk Trees). Recent focus has been on UI refactoring and creating a modern, component-based architecture.

### Recent Progress

**Completed Systems:**
- ✅ **Core Religion System** - Create, join, manage religions
- ✅ **Dual Ranking System** - Player Favor and Religion Prestige
- ✅ **8 Deities Defined** - 80 perks designed across all deities
- ✅ **Buff/Debuff System** - Phase 2 combat integration complete
- ✅ **Perk Registry** - All perks defined and registered
- ✅ **UI Component Library** - Reusable UI components (buttons, inputs, scrollbars, dropdowns)

**In Progress:**
- ⚠️ **UI Refactoring** - Modernizing GUI architecture with component-based design
- ⚠️ **Perk GUI Browser** - Visual perk tree interface
- ⚠️ **Perk Stat Application** - Connecting perks to gameplay effects
- ⚠️ **Special Effects** - Implementing advanced perk mechanics

### Implemented Systems

**Religion Management:**
- ✅ Create custom religions with any deity
- ✅ Public/private religion system with invitations
- ✅ Join, leave, and manage religions
- ✅ Founder privileges (kick members, disband, set description)
- ✅ 7-day switching cooldown with penalties
- ✅ Full persistence and save/load
- ✅ Religion Management GUI

**Dual Ranking System:**
- ✅ Player Favor Ranks (Initiate → Avatar) - Individual progression
- ✅ Religion Prestige Ranks (Fledgling → Mythic) - Collective progression
- ✅ PvP favor/prestige earning with deity relationship multipliers
- ✅ Rank-up notifications for both systems
- ✅ Network synchronization

**Perk System:**
- ✅ PerkRegistry with 80 perks defined
- ✅ Perk unlock validation (rank requirements, prerequisites)
- ⚠️ Stat modifier application (in development)
- ✅ Perk persistence across sessions
- ⚠️ Special effect handlers (planned)

**Deity Perk Trees (8 deities defined):**
- ✅ **Khoras (War)** - 10 perks (combat, damage, defense)
- ✅ **Lysa (Hunt)** - 10 perks (tracking, precision, ranged combat)
- ✅ **Morthen (Death)** - 10 perks (life drain, DoT, survivability)
- ✅ **Aethra (Light)** - 10 perks (healing, shields, buffs)
- ✅ **Umbros (Shadows)** - 10 perks (stealth, backstab, evasion)
- ✅ **Tharos (Storms)** - 10 perks (AoE, lightning, mobility)
- ✅ **Gaia (Earth)** - 10 perks (defense, regeneration, durability)
- ✅ **Vex (Madness)** - 10 perks (chaos, confusion, unpredictability)

**User Interface:**
- ✅ HUD showing religion, deity, ranks, favor/prestige
- ✅ Religion Management GUI - Create, browse, and manage religions
- ⚠️ Visual Perk Tree Viewer (in development)
- ✅ Command-based interfaces for all systems

## Development Roadmap

**Current Status:** Phase 3 In Progress (Religion & Perk Systems)

- ✅ **Phase 1:** Foundation (MVP) - Complete
  - Core deity system, basic abilities, favor tracking, persistence
- ✅ **Phase 2:** Combat Integration - Complete
  - Buff/debuff system, entity behavior tracking, stat modifiers
- ⚠️ **Phase 3:** Religion-Based Deity System with Perk Trees - In Progress
  - ✅ Phase 3.1: Foundation (Religion system, commands, persistence)
  - ✅ Phase 3.2: Ranking Systems (Dual progression, PvP integration)
  - ⚠️ Phase 3.3: Perk System Core (Registry complete, stat application in progress)
  - ✅ Phase 3.4: Deity Perk Trees (8 deities, 80 perks defined)
  - ⚠️ Phase 3.5: Integration & Polish (UI refactoring, perk GUI, special effects)
- 🔲 **Phase 4:** Advanced Features - Planned
  - Divine duels, crusade events, relic system, apostate mechanics

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

**Legacy Commands (Phase 1-2):**
- `/deity list` - Show all available deities
- `/deity info <deity>` - Get detailed deity information
- `/deity status` - View your current deity status
- `/favor` - Check your current divine favor
- `/ability list` - Show available abilities (from Phase 1-2 ability system)

## Contributing

This project is in active development. Contributions, suggestions, and feedback are welcome! Please open an issue or discussion on the repository.

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
