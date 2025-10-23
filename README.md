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
- .NET 7 SDK or later
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

## Project Structure

```
PantheonWars/
├── CakeBuild/              # Build system
│   ├── Program.cs          # Build tasks and packaging
│   └── CakeBuild.csproj
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

## Implementation Phases

### Phase 1: Foundation (MVP) - ✅ COMPLETE
- [x] Project structure setup
- [x] Deity registration system
- [x] Player deity selection GUI
- [x] Basic favor tracking
- [x] Data persistence
- [x] 1-2 deities with basic abilities

**Delivered:** Complete deity worship system with 2 fully playable deities (Khoras and Lysa), each with 4 unique abilities. Includes favor tracking, cooldown management, data persistence, GUI, HUD, and comprehensive command system.

**Commands Available:**
- `/deity list`, `/deity info <deity>`, `/deity select <deity>`, `/deity status`, `/favor`
- `/ability list`, `/ability info <ability>`, `/ability use <ability>`, `/ability cooldowns`

### Phase 2: Combat Integration - ✅ COMPLETE
- [ ] Damage system hooks *(deferred to Phase 3)*
- [x] Death penalties/favor loss
- [x] Rival deity bonuses
- [x] Ability execution framework

**Note:** Phase 2 core features were implemented as part of Phase 1 MVP. Damage system hooks will be enhanced in Phase 3.

### Phase 3: Full Ability System
- [ ] All 8 deities implemented
- [ ] Complete ability sets
- [ ] Visual effects
- [ ] Devotion progression

### Phase 4: World Integration
- [ ] Shrine blocks
- [ ] Temple world generation
- [ ] Sacred ground system
- [ ] Temple capture mechanics

### Phase 5: Advanced Features
- [ ] Divine duels
- [ ] Crusade events
- [ ] Relic system
- [ ] Apostate mechanics

## Contributing

This mod is currently in early development. Contributions, suggestions, and feedback are welcome!

## License

[Choose appropriate license]

## Credits

- Built using the official [Vintage Story Mod Template](https://github.com/anegostudios/vsmodtemplate)
- Inspired by the [Karma System mod](https://mods.vintagestory.at/show/mod/28955)
