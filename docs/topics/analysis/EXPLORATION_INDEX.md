# PantheonWars XSkills Mod Analysis - Documentation Index

## Overview

This directory contains a comprehensive analysis of the XSkills mod's implementation patterns, specifically examining how they handle stat modifications, entity extensions, and buff/debuff systems. This analysis is the foundation for implementing PantheonWars' buff system.

**Analysis Date**: October 23, 2024
**Scope**: Very thorough - 7 key architectural areas covered
**Total Documentation**: 2,033 lines across 4 files

---

## Documentation Files

### 1. XSKILLS_EXPLORATION_SUMMARY.md (RECOMMENDED START HERE)
**Length**: 315 lines | **Size**: 8.8KB

High-level executive summary of the entire analysis. Start here for:
- Key findings overview
- Recommended architecture for PantheonWars
- Simplified 4-layer design
- Critical implementation notes
- Next steps roadmap

**Best for**: Quick understanding of architecture and immediate action items

---

### 2. XSKILLS_ANALYSIS.md (DETAILED TECHNICAL REFERENCE)
**Length**: 1,022 lines | **Size**: 32KB

Complete technical deep-dive with actual code examples from XSkills. Covers:

**Section 1: Entity Extension Patterns**
- XSkillsEntityBehavior hierarchy
- AffectedEntityBehavior as effect container
- Custom data storage in WatchedAttributes
- Player/Animal specific extensions

**Section 2: Stat Modification Patterns**
- Direct Stats.Set/Remove API usage
- Effect-based stat modifications
- Compound effects via Condition class
- Namespaced modifier IDs

**Section 3: Update Cycles Integration**
- OnGameTick patterns with time thresholds
- Effect system update cycles
- Deferred updates vs immediate
- Server-client update separation

**Section 4: Damage Calculation Hooks**
- Outgoing damage (OnDamage)
- Incoming damage (OnEntityReceiveDamage)
- Weapon/tool type detection
- Ability-based damage modifications

**Section 5: Client-Server Synchronization**
- WatchedAttributes automatic sync
- Effect tree serialization/deserialization
- Harmony patches for interception
- Transpiler usage examples

**Section 6: Buff/Debuff Tracking**
- ExpireState enum (Endless, Death, Time, Intensity, Accumulates)
- Effect lifecycle (OnStart, OnEnd, OnInterval, OnExpires)
- Condition compound effects
- Stacking and merging logic

**Section 7: Key Patterns for Adaptation**
- Two-layer behavior system
- Deferred stat updates
- Effect-driven stat management
- Hierarchical synchronization

**Best for**: Detailed technical understanding, implementation debugging

---

### 3. BUFF_IMPLEMENTATION_GUIDE.md (READY-TO-IMPLEMENT)
**Length**: 486 lines | **Size**: 14KB

PantheonWars-specific implementation guide with code templates. Includes:

**Layer 1: Buff Definition & Registry**
- BuffType class definition
- BuffRegistry singleton pattern
- Buff type registration

**Layer 2: Buff Entity Behavior**
- PantheonBuffBehavior complete implementation
- ApplyBuff/RemoveBuff methods
- Stat modification application/removal
- OnGameTick update cycle
- WatchedAttributes synchronization

**Layer 3: Stat Integration**
- WarBanner example with stat multipliers
- Multiple stat modification patterns
- Buff group conflicts handling

**Layer 4: Damage Calculation Integration**
- Harmony patch example
- Damage event hooks
- Buff-based damage modification

**Implementation Checklist**
- Phase 1: Core Infrastructure
- Phase 2: Integration
- Phase 3: Ability Integration
- Phase 4: UI & Polish

**Critical Implementation Notes**
- Server-only updates pattern
- Deferred stat updates requirement
- Modifier ID conventions
- Update frequency guidelines
- WatchedAttributes listener registration

**Best for**: Direct copy-paste implementation, phase-by-phase development

---

### 4. README.md (ORIGINAL PROJECT DOCUMENTATION)
**Length**: 210 lines | **Size**: 7.8KB

Original PantheonWars project documentation. Contains:
- Project overview
- Phase descriptions
- Current progress (Phase 2: 3/4 complete)
- Architecture overview

---

## File Structure

```
/home/quantumheart/RiderProjects/PantheonWars/
├── EXPLORATION_INDEX.md               (THIS FILE)
├── XSKILLS_EXPLORATION_SUMMARY.md     (START HERE - 8.8KB)
├── XSKILLS_ANALYSIS.md                (REFERENCE - 32KB)
├── BUFF_IMPLEMENTATION_GUIDE.md       (IMPLEMENT - 14KB)
├── README.md                          (PROJECT INFO - 7.8KB)
├── PantheonWars/                      (PROJECT SOURCE)
│   ├── Abilities/
│   ├── Models/
│   ├── Systems/
│   ├── Data/
│   └── Commands/
```

---

## Quick Start Guide

### Step 1: Understanding (30 minutes)
1. Read `XSKILLS_EXPLORATION_SUMMARY.md`
2. Focus on "Key Findings" section
3. Review "Recommended PantheonWars Implementation"

### Step 2: Detailed Study (1-2 hours)
1. Read relevant sections from `XSKILLS_ANALYSIS.md`
2. Start with "Entity Extension Patterns"
3. Continue with "Stat Modification Patterns"
4. Review "Client-Server Synchronization"

### Step 3: Implementation (ongoing)
1. Use `BUFF_IMPLEMENTATION_GUIDE.md` as primary reference
2. Follow the "Implementation Checklist"
3. Adapt code examples for PantheonWars needs
4. Reference `XSKILLS_ANALYSIS.md` for detailed patterns

---

## Key Concepts Reference

### Entity Extension
- Extend entities via EntityBehavior classes
- Store custom data in WatchedAttributes
- Cache references to other behaviors
- Load definitions from central system

### Stat Modification
```csharp
entity.Stats.Set(statName, modifierId, value, false);  // Apply
entity.Stats.Remove(statName, modifierId);              // Remove
entity.Stats.GetBlended(statName);                      // Query
```

### Update Cycle
```csharp
public override void OnGameTick(float deltaTime)
{
    updateTimer += deltaTime;
    if (updateTimer >= 0.1f)  // Update every 100ms
    {
        // Process buffs, check expiration, sync
        updateTimer = 0f;
    }
}
```

### Synchronization
```csharp
// Server: Serialize and mark dirty
entity.WatchedAttributes.SetAttribute("pantheonBuffs", buffTree);
entity.WatchedAttributes.MarkPathDirty("pantheonBuffs");

// Client: Listen for changes
entity.WatchedAttributes.RegisterModifiedListener("pantheonBuffs", 
    this.OnBuffsChanged);
```

### Buff Application
```csharp
buffBehavior.ApplyBuff("warbanner", intensity: 1.0f);
entity.Stats.Set("damage", "buff-warbanner", 0.25f, false);
```

---

## Reference Code Locations

All code examples are from the actual XSkills mod:

| File | Location | Key Content |
|------|----------|-------------|
| XSkillsPlayerBehavior.cs | src/EntityBehaviors/ | OnGameTick, Stat.Set usage |
| XSkillsEntityBehavior.cs | src/EntityBehaviors/ | Damage hooks, entity extension |
| AffectedEntityBehavior.cs | xlib/xeffects/ | Effect management, synchronization |
| Effect.cs | xlib/xeffects/Effect/ | Effect lifecycle, expiration |
| StatEffect.cs | xlib/xeffects/Effect/ | Stat modification pattern |
| Condition.cs | xlib/xeffects/Effect/ | Compound effects |
| ModSystemWearableStatsPatch.cs | xskills/Patches/Combat/ | Harmony patches |

Full paths in `/home/quantumheart/RiderProjects/VSMods/mods/`

---

## Implementation Status

### Completed
- Very thorough XSkills exploration
- Complete code example extraction
- Architecture pattern analysis
- PantheonWars-specific adaptation guide

### In Progress
- PantheonWars buff system design (Phase 2)

### Planned (Next Phases)
1. **Phase 3**: Complete ability system + buff integration
2. **Phase 4**: UI/UX and polish

---

## Important Notes

### Critical Patterns
1. **Always use deferred updates** (`false` parameter in Stats.Set)
2. **Respect server-client separation** (check entity.Api.Side)
3. **Use proper update frequency** (100ms for buff checks)
4. **Namespace modifier IDs** (`"buff-{id}"` format)
5. **Handle buff groups** (prevent conflicting buffs)

### Common Mistakes to Avoid
- Not checking `entity.Api.Side` before modifying stats
- Using `true` for immediate updates (causes performance issues)
- Updating buffs every frame instead of every 100ms
- Not removing stat modifiers when buff expires
- Forgetting to mark WatchedAttributes dirty for synchronization

### Testing Checklist
- [ ] Buffs apply correctly on server
- [ ] Client receives buff updates
- [ ] Buff expiration works properly
- [ ] Stat modifications blend correctly
- [ ] Buff groups prevent conflicts
- [ ] Serialization/deserialization works
- [ ] No memory leaks from event subscriptions

---

## Document Maintenance

- **Last Updated**: October 23, 2024
- **By**: Claude Code Analysis System
- **Scope**: Very thorough (all 7 key areas covered)
- **Confidence Level**: High (actual production code analysis)

---

## Questions & Clarifications

For detailed questions, refer to:
1. **Architecture questions**: XSKILLS_EXPLORATION_SUMMARY.md
2. **Implementation questions**: BUFF_IMPLEMENTATION_GUIDE.md
3. **Technical details**: XSKILLS_ANALYSIS.md sections 1-7

---

**Ready to implement? Start with BUFF_IMPLEMENTATION_GUIDE.md Phase 1 checklist!**
