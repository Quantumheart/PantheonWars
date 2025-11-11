# Pantheon Wars - Icons Directory

This directory contains all icon assets for the Pantheon Wars mod.

## Structure

```
icons/
├── deities/          # 8 deity symbols (32x32 PNG)
│   ├── khoras.png
│   ├── lysa.png
│   ├── morthen.png
│   ├── aethra.png
│   ├── umbros.png
│   ├── tharos.png
│   ├── gaia.png
│   └── vex.png
│
└── perks/            # 80 perk icons organized by deity
    ├── khoras/       # 10 icons for Khoras (War)
    ├── lysa/         # 10 icons for Lysa (Hunt)
    ├── morthen/      # 10 icons for Morthen (Death)
    ├── aethra/       # 10 icons for Aethra (Light)
    ├── umbros/       # 10 icons for Umbros (Shadows)
    ├── tharos/       # 10 icons for Tharos (Storms)
    ├── gaia/         # 10 icons for Gaia (Earth)
    └── vex/          # 10 icons for Vex (Madness)
```

## Icon Specifications

**Format:** PNG with transparency (RGBA)
**Size:** 32x32 pixels
**Style:** Pixel art / Low-res fantasy (Vintage Story aesthetic)

## Documentation

For detailed information about icon generation, see:

- **`/docs/topics/art-assets/icon_specifications.md`** - Detailed visual descriptions for each icon
- **`/docs/topics/art-assets/icon_manifest.json`** - JSON manifest for tracking generation progress
- **`/docs/topics/art-assets/icon_generation_guide.md`** - Practical guide for generating icons

## Usage in Code

Icons are referenced using the Vintage Story asset path format:

```csharp
// Deity symbols
string khorasIcon = "pantheonwars:textures/icons/deities/khoras.png";

// Perk icons
string perkIcon = "pantheonwars:textures/icons/perks/khoras/warriors_resolve.png";
```

## Status

Total icons needed: **88**
- Deity symbols: 8
- Perk icons: 80 (10 per deity × 8 deities)

Check `/docs/topics/art-assets/icon_manifest.json` for current generation progress.
