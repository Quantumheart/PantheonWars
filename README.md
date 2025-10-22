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
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── assets/
│   │   └── modinfo.json    # Mod metadata
│   ├── PantheonWars.csproj
│   └── PantheonWarsModSystem.cs
├── Release/                # Build output
├── .gitignore
├── build.ps1               # Windows build script
├── build.sh                # Linux/Mac build script
├── PantheonWars.sln
└── README.md
```

## Implementation Phases

### Phase 1: Foundation (MVP) - In Progress
- [x] Project structure setup
- [ ] Deity registration system
- [ ] Player deity selection GUI
- [ ] Basic favor tracking
- [ ] Data persistence
- [ ] 1-2 deities with basic abilities

### Phase 2: Combat Integration
- [ ] Damage system hooks
- [ ] Death penalties/favor loss
- [ ] Rival deity bonuses
- [ ] Ability execution framework

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
