# Ability Reference Guide

**Version:** 0.1.0

Complete reference for all deity abilities in Pantheon Wars, including mechanics, costs, cooldowns, and usage tips.

---

## How Abilities Work

### Activation
- **Command:** `/ability use <ability_id>`
- **Future:** Hotkey bindings (planned for Phase 3)

### Requirements
- Must be pledged to the ability's deity
- Must have sufficient favor
- Ability must not be on cooldown
- Must meet minimum devotion rank requirement

### Costs & Cooldowns
- **Favor Cost:** 8-20 favor per ability
- **Cooldown:** 20-60 seconds per ability
- **Rank Requirements:** Some abilities require Disciple or higher

### Implementation Status

| Status | Symbol | Meaning |
|--------|--------|---------|
| MVP | ⚠️ | Framework works, placeholder effects (chat notifications) |
| Full | ✅ | Complete implementation with actual game effects |
| Planned | 🔲 | Not yet implemented |

**Current Status:** All 8 implemented abilities are in **MVP** status. Phase 2 will upgrade them to full implementation with actual damage/stat modifications.

---

## Khoras Abilities (War God)

Theme: Aggressive melee combat with team support

### 1. War Banner (khoras_warbanner)

**Type:** Buff (AoE Team)
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 15
- **Cooldown:** 45 seconds
- **Rank Required:** Initiate
- **Range:** 10 blocks (radius)
- **Duration:** 15 seconds
- **Effect:** +20% damage to all allies

#### Current Implementation
- Finds all player entities within 10 blocks
- Sends notification to affected players
- **Missing:** Actual damage buff application

#### Planned Implementation (Phase 2)
- Apply damage modifier to entity stats
- Visual banner effect at caster location
- Particle effects around buffed players
- Audio cue for allies

#### Usage Tips
- Use before engaging in team fights
- Position centrally to maximize allies affected
- Combo with Battle Cry for maximum offense
- Worth the cost if team secures 2+ kills

---

### 2. Battle Cry (khoras_battlecry)

**Type:** Buff (Self)
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 10
- **Cooldown:** 30 seconds
- **Rank Required:** Initiate
- **Range:** Self
- **Duration:** 10 seconds
- **Effect:** +30% attack speed

#### Current Implementation
- Activates on caster
- Sends notification to nearby players (20 block radius)
- **Missing:** Actual attack speed modification

#### Planned Implementation (Phase 2)
- Modify entity attack speed stat
- Visual glow effect on caster
- Sound effect audible to nearby players
- Animation speed increase

#### Usage Tips
- Use during 1v1 duels for DPS burst
- Combine with War Banner for team fights
- Good for finishing low-health enemies
- Cheap enough to use liberally

---

### 3. Blade Storm (khoras_blade_storm)

**Type:** Damage (AoE)
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 12
- **Cooldown:** 20 seconds
- **Rank Required:** Initiate
- **Range:** 4 blocks (radius)
- **Damage:** 5 HP
- **Effect:** Spin attack hitting all nearby enemies

#### Current Implementation
- Finds entities within 4 blocks
- Calls ReceiveDamage() directly on enemies
- Sends notifications
- **Missing:** Animation, particle effects

#### Planned Implementation (Phase 2)
- Spinning animation for caster
- Slash particle effects in radius
- Knockback effect on hit entities
- Audio swoosh/impact sounds

#### Usage Tips
- Use when surrounded by enemies
- Follow up with Battle Cry to finish targets
- Effective for clearing PvE mobs
- Short cooldown allows frequent use

---

### 4. Last Stand (khoras_last_stand)

**Type:** Defensive
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 20
- **Cooldown:** 60 seconds
- **Rank Required:** Disciple (500 total favor)
- **Range:** Self
- **Duration:** 12 seconds
- **Effect:** 50% damage resistance
- **Special:** Requires <30% health to activate

#### Current Implementation
- Validates health requirement (<30%)
- Sends notifications
- **Missing:** Actual damage reduction

#### Planned Implementation (Phase 2)
- Apply damage reduction modifier
- Visual shield/glow effect
- Enraged visual state
- Pulse animation on damage blocked

#### Usage Tips
- Emergency defensive ability
- Use when low health and outnumbered
- Expensive but can save your life
- Plan engagement around this cooldown

---

## Lysa Abilities (Hunt Goddess)

Theme: Mobile ranged combat with tracking and positioning

### 5. Hunter's Mark (lysa_hunters_mark)

**Type:** Debuff (Single Target)
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 10
- **Cooldown:** 25 seconds
- **Rank Required:** Initiate
- **Range:** 20 blocks
- **Duration:** 20 seconds
- **Effect:** Target takes +25% damage

#### Current Implementation
- Raycasts to find target player is looking at
- Validates target within 30-degree cone
- Sends notification
- **Missing:** Damage amplification tracking

#### Planned Implementation (Phase 2)
- Track marked entity
- Apply damage multiplier on incoming damage
- Visual marker above target
- Outline effect visible through walls
- Duration timer display

#### Usage Tips
- Mark high-value targets before engagement
- Focus fire marked targets
- Works well with Arrow Rain combo
- Coordinate with team for maximum value

---

### 6. Swift Feet (lysa_swift_feet)

**Type:** Utility (Self Buff)
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 8
- **Cooldown:** 20 seconds
- **Rank Required:** Initiate
- **Range:** Self
- **Duration:** 8 seconds
- **Effect:** +50% movement speed

#### Current Implementation
- Activates on caster
- Sends notification
- **Missing:** Actual movement speed modification

#### Planned Implementation (Phase 2)
- Modify entity movement speed stat
- Trail particle effect
- Footstep audio increase
- Animation speed increase

#### Usage Tips
- Cheap mobility tool
- Use for kiting melee fighters
- Escape ability when outnumbered
- Chase down fleeing enemies
- Reposition during team fights

---

### 7. Arrow Rain (lysa_arrow_rain)

**Type:** Damage (AoE)
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 15
- **Cooldown:** 35 seconds
- **Rank Required:** Disciple (500 total favor)
- **Range:** 25 blocks (cast range)
- **AoE Radius:** 5 blocks
- **Damage:** 4 HP per target
- **Effect:** Rain of arrows at target location

#### Current Implementation
- Calculates target position from look vector
- Finds entities in 5-block radius at target
- Applies damage via ReceiveDamage()
- **Missing:** Projectile animation, particles

#### Planned Implementation (Phase 2)
- Arrow projectile effects falling from sky
- Impact particles
- Audio (arrows whistling, impacts)
- Ground targeting indicator
- Staggered damage application

#### Usage Tips
- Area denial tool
- Hit grouped enemies
- Use at max range for safety
- Combo with Hunter's Mark for focus fire
- Predict enemy movement for better hits

---

### 8. Predator Instinct (lysa_predator_instinct)

**Type:** Buff (Self)
**Status:** ⚠️ MVP Implementation

#### Stats
- **Favor Cost:** 12
- **Cooldown:** 40 seconds
- **Rank Required:** Disciple (500 total favor)
- **Range:** Self + 30 block detection
- **Duration:** 15 seconds
- **Effect:** +25% crit chance, entity detection

#### Current Implementation
- Counts nearby entities (30 block radius)
- Reports entity count to player
- **Missing:** Crit chance modification, entity highlighting

#### Planned Implementation (Phase 2)
- Modify crit chance stat
- Highlight nearby entities through walls
- Enhanced nameplate visibility
- Visual "hunter vision" effect
- Audio heartbeat for nearby enemies

#### Usage Tips
- Pre-engagement scouting
- Find hidden enemies
- Increased damage output
- Combo with Hunter's Mark for burst
- Use before entering dangerous areas

---

## Ability Combos

### Khoras Combinations

**Offensive Combo (Team Fight):**
1. War Banner (+20% team damage, 15 favor)
2. Battle Cry (+30% attack speed, 10 favor)
3. Blade Storm (AoE damage, 12 favor)
- **Total:** 37 favor, devastating team fight presence

**Defensive Combo (Outnumbered):**
1. Last Stand (50% damage reduction, 20 favor)
2. Battle Cry (increase DPS while tanking, 10 favor)
3. Blade Storm (clear nearby enemies, 12 favor)
- **Total:** 42 favor, survive and counter-attack

---

### Lysa Combinations

**Hunter Combo (Single Target):**
1. Predator Instinct (crit chance + vision, 12 favor)
2. Hunter's Mark (25% damage amp, 10 favor)
3. Arrow Rain (AoE burst, 15 favor)
- **Total:** 37 favor, extreme burst damage

**Kiting Combo (Escape/Chase):**
1. Swift Feet (50% movement speed, 8 favor)
2. Hunter's Mark (track target, 10 favor)
- **Total:** 18 favor, maintain range advantage

**Scout Combo (Information):**
1. Predator Instinct (detect enemies, 12 favor)
2. Swift Feet (mobility to reposition, 8 favor)
- **Total:** 20 favor, safe scouting

---

## Ability Commands Reference

### Primary Commands
- `/ability list` - Show your available abilities
- `/ability info <ability_id>` - Detailed ability information
- `/ability use <ability_id>` - Activate an ability
- `/ability cooldowns` - Check all ability cooldowns

### Ability IDs Quick Reference

**Khoras:**
- `khoras_warbanner`
- `khoras_battlecry`
- `khoras_blade_storm`
- `khoras_last_stand`

**Lysa:**
- `lysa_hunters_mark`
- `lysa_swift_feet`
- `lysa_arrow_rain`
- `lysa_predator_instinct`

---

## Ability Balance Philosophy

### Design Principles

1. **Power vs Cost**
   - More impactful abilities cost more favor
   - AoE effects more expensive than single target
   - Team buffs more expensive than self buffs

2. **Cooldown vs Impact**
   - Basic utility: 20-30 seconds
   - Moderate impact: 30-45 seconds
   - High impact: 45-60 seconds

3. **Rank Gating**
   - Initiate: Basic abilities, learn the deity
   - Disciple: Advanced abilities, mastery begins
   - Higher ranks (future): Enhanced versions

4. **Counterplay**
   - All abilities have weaknesses
   - Timing and positioning matter
   - No "I win" buttons

---

## Future Ability Plans

### Phase 2: Full Implementation
- Replace all MVP placeholders with real effects
- Add damage/stat modification hooks
- Implement buff/debuff tracking
- Add visual and audio feedback

### Phase 3: More Deities
- 24 additional abilities (6 deities × 4 abilities)
- Morthen, Aethra, Umbros, Tharos, Gaia, Vex
- 32 abilities total across 8 deities

### Phase 3: Enhanced Mechanics
- Particle effects for all abilities
- Animation hooks
- Advanced targeting systems
- Ability upgrade system (rank-based enhancements)

### Phase 5: Ultimate Abilities
- Avatar-rank exclusive abilities
- High cost, high impact
- Long cooldowns (2-5 minutes)
- Game-changing effects

---

## Troubleshooting

### "Ability not found"
- Check spelling of ability ID
- Use `/ability list` to see available abilities
- Ensure you're pledged to the correct deity

### "Insufficient favor"
- Check `/favor` for current favor
- Earn more through PvP kills
- Target rivals for 2x favor gain

### "On cooldown"
- Use `/ability cooldowns` to check remaining time
- Wait for cooldown to expire
- Plan ability usage more carefully

### "Rank too low"
- Some abilities require Disciple rank (500 total favor)
- Check `/deity status` for current rank
- Earn more favor to reach next rank

---

## Additional Resources

- [Deity Reference](deity_reference.md) - Deity information and relationships
- [Favor Reference](favor_reference.md) - Favor earning and devotion ranks
- [Implementation Guide](implementation_guide.md) - Development roadmap

See the main [README.md](../README.md) for general information.
