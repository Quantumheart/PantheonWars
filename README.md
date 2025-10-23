# Pantheon Wars

A religious-themed PvP mod for Vintage Story featuring competing deities with unique abilities and a favor-based progression system.

## Overview

Pantheon Wars introduces a comprehensive deity worship system where players pledge themselves to different gods, each granting unique combat abilities and playstyles. Players earn Divine Favor through PvP combat and deity-aligned actions, unlocking powerful miracles and progressing through devotion ranks.

## Features (Planned)

### Deity System
- **8 Unique Deities**: Khoras (War), Lysa (Hunt), Morthen (Death), Aethra (Light), Umbros (Shadows), Tharos (Storms), Gaia (Earth), Vex (Madness)
- **Divine Favor Currency**: Earned through deity-aligned actions and PvP combat
- **Devotion Ranks**: Progress from Initiate to Avatar (5 ranks)
- **Deity Relationships**: Allied and rival deity dynamics affect favor gain

### Ability System
- **4 Abilities per Deity**: Each with unique mechanics and playstyles
- **Cooldown Management**: Balanced ability usage with strategic timing
- **Visual Effects**: Particle effects and animations for abilities

### PvP Features
- **Divine Duels**: Formal 1v1 challenges with favor stakes
- **Crusades**: Server-wide deity war events
- **Sacred Ground**: Territory control and blessings
- **Relic System**: Powerful artifacts that grant temporary dominance

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

- **[Implementation Guide](docs/implementation_guide.md)** - Development roadmap and phase breakdown
- **[Deity Reference](docs/deity_reference.md)** - Complete deity information, relationships, and lore
- **[Favor System Guide](docs/favor_reference.md)** - How favor works, earning methods, and devotion ranks
- **[Ability Reference](docs/ability_reference.md)** - Detailed ability mechanics, costs, and usage tips

## Project Structure

```
PantheonWars/
├── CakeBuild/              # Build system
│   ├── Program.cs          # Build tasks and packaging
│   └── CakeBuild.csproj
├── docs/                   # Documentation
│   ├── implementation_guide.md
│   ├── deity_reference.md
│   ├── favor_reference.md
│   └── ability_reference.md
├── PantheonWars/           # Main mod project
│   ├── Abilities/          # Deity abilities
│   │   ├── Khoras/         # War god abilities
│   │   └── Lysa/           # Hunt goddess abilities
│   ├── Commands/           # Chat commands
│   │   ├── DeityCommands.cs
│   │   └── AbilityCommands.cs
│   ├── Data/               # Data models for persistence
│   │   ├── PlayerDeityData.cs
│   │   └── PlayerAbilityData.cs
│   ├── GUI/                # User interface
│   │   ├── DeitySelectionDialog.cs
│   │   └── FavorHudElement.cs
│   ├── Models/             # Core data models
│   │   ├── Deity.cs
│   │   ├── Ability.cs
│   │   └── Enums (DeityType, AbilityType, etc.)
│   ├── Network/            # Client-server networking
│   │   └── PlayerDataPacket.cs
│   ├── Systems/            # Core game systems
│   │   ├── DeityRegistry.cs
│   │   ├── AbilityRegistry.cs
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

## Current Features (v0.1.0)

The mod is currently **fully playable** with the following implemented features:

### Playable Deities (2/8)
- **Khoras (God of War)** - Aggressive melee combat deity
  - War Banner: AoE damage buff for allies
  - Battle Cry: Attack speed increase
  - Blade Storm: Spin attack dealing damage to nearby enemies
  - Last Stand: Damage resistance when health is low

- **Lysa (Goddess of the Hunt)** - Mobile ranged combat deity
  - Hunter's Mark: Mark target to take extra damage
  - Swift Feet: Movement speed boost
  - Arrow Rain: AoE ranged damage
  - Predator Instinct: Enhanced perception and critical hit chance

### Core Systems
- ✅ **Deity Selection:** GUI dialog (hotkey: K) and commands
- ✅ **Divine Favor:** Earned through PvP kills, spent on abilities
- ✅ **Devotion Ranks:** Progress from Initiate to Avatar based on total favor earned
- ✅ **Ability System:** 8 unique abilities with cooldowns (20-60s) and favor costs (8-20 favor)
- ✅ **Deity Relationships:** Allied deities give 0.5x favor, rivals give 2x favor from kills
- ✅ **Data Persistence:** All progress saves automatically
- ✅ **HUD Display:** Always-visible deity, favor, and rank display
- ✅ **PvP Integration:** Favor rewards on kills, penalties on death

## Development Roadmap

**Current Status:** Phase 1 Complete, Phase 2 In Progress (3/4)

- ✅ **Phase 1:** Foundation (MVP) - Complete
- 🟡 **Phase 2:** Combat Integration - In Progress (75% complete)
- 🔲 **Phase 3:** Full Ability System - Planned
- 🔲 **Phase 4:** World Integration - Planned
- 🔲 **Phase 5:** Advanced Features - Planned

For detailed phase breakdowns, tasks, and timeline, see the **[Implementation Guide](docs/implementation_guide.md)**.

### Available Commands

**Deity Management:**
- `/deity list` - Show all available deities
- `/deity info <deity>` - Get detailed deity information
- `/deity select <deity>` - Pledge to a deity
- `/deity status` - View your current deity status and stats

**Favor & Abilities:**
- `/favor` - Check your current divine favor
- `/ability list` - Show your available abilities
- `/ability info <ability>` - Get detailed ability information
- `/ability use <ability>` - Activate an ability
- `/ability cooldowns` - Check all ability cooldown status

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
