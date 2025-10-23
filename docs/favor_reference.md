# Divine Favor Reference

**Version:** 0.1.0

Complete guide to the Divine Favor system, how to earn and spend it, and the devotion rank progression system.

---

## What is Divine Favor?

Divine Favor is the primary currency in Pantheon Wars. It represents your deity's blessing and approval of your actions. Favor is:

- **Earned** through PvP combat and deity-aligned actions
- **Spent** on using deity abilities
- **Tracked** both current and lifetime totals
- **Persistent** across sessions (automatically saved)

Think of favor as both a resource (for abilities) and experience (for devotion ranks).

---

## Earning Favor

### PvP Combat (Primary Source)

#### Base Rewards
- **Kill an enemy player:** 10 favor (base)

#### Deity Relationship Modifiers

Your favor rewards are modified based on the relationship between your deity and your victim's deity:

| Victim's Deity | Relationship | Favor Multiplier | Total Favor |
|---------------|--------------|------------------|-------------|
| Rival deity | Rival | 2.0x | 20 favor |
| Allied deity | Allied | 0.5x | 5 favor |
| Same deity | Same | 0.5x | 5 favor |
| Neutral deity | Neutral | 1.0x | 10 favor |
| No deity | None | 1.0x | 10 favor |

**Examples:**
- Khoras follower kills Morthen follower: **20 favor** (rival bonus)
- Khoras follower kills Lysa follower: **5 favor** (allied penalty)
- Khoras follower kills another Khoras follower: **5 favor** (same deity penalty)

#### Design Philosophy
- **Rival bonus (2x)** encourages fighting your deity's enemies
- **Allied penalty (0.5x)** discourages attacking allies
- **Same deity penalty (0.5x)** discourages infighting among followers

---

### Death Penalties

When you die (regardless of cause):
- **Lose 5 favor** (minimum 0 - cannot go negative)
- Penalty applies to current favor only
- Lifetime favor total is not affected

This creates risk/reward tension in PvP engagement.

---

### Future Favor Sources (Planned)

The following favor sources are planned for future phases:

#### Phase 3: Deity-Aligned Actions
- Completing deity-specific quests
- Performing deity-aligned rituals
- Achieving deity-specific milestones

#### Phase 4: Territory Control
- Controlling shrines
- Holding sacred ground
- Completing temple objectives

#### Phase 5: Events
- Participating in crusades
- Winning divine duels
- Discovering relics

---

## Spending Favor

### Ability Costs

Each ability has a fixed favor cost. Using an ability consumes favor immediately upon activation.

#### Khoras Abilities
| Ability | Favor Cost | Power Level |
|---------|-----------|-------------|
| Battle Cry | 10 | Basic buff |
| Blade Storm | 12 | Damage dealer |
| War Banner | 15 | Team buff |
| Last Stand | 20 | Powerful defensive |

#### Lysa Abilities
| Ability | Favor Cost | Power Level |
|---------|-----------|-------------|
| Swift Feet | 8 | Basic mobility |
| Hunter's Mark | 10 | Target debuff |
| Predator Instinct | 12 | Self buff |
| Arrow Rain | 15 | AoE damage |

**Cost Design Philosophy:**
- More powerful abilities cost more favor
- AoE and team buffs are more expensive
- Basic utility abilities are cheapest
- Ultimate-tier abilities cost 20+ favor

### Failed Activation

If an ability fails to activate (insufficient favor, on cooldown, etc.), **no favor is consumed**. You only pay for successful activations.

---

## Devotion Ranks

Your **total lifetime favor earned** determines your devotion rank. Ranks are permanent milestones that unlock new abilities and show your dedication.

### Rank Progression

| Rank | Total Favor Required | Benefits |
|------|---------------------|----------|
| **Initiate** | 0 | Starting rank, access to basic abilities |
| **Disciple** | 500 | Unlocks advanced abilities |
| **Zealot** | 2,000 | *Future: Enhanced ability effects* |
| **Champion** | 5,000 | *Future: Unique passive bonuses* |
| **Avatar** | 10,000 | *Future: Ultimate tier abilities* |

### Rank Benefits (Current)

**Initiate (Starting Rank):**
- Access to 2-3 basic abilities per deity
- Full favor earning capability
- Can participate in all PvP

**Disciple (500 total favor):**
- Unlocks advanced abilities (marked "Requires: Disciple")
- Currently unlocks:
  - **Khoras:** Last Stand
  - **Lysa:** Arrow Rain, Predator Instinct

### Rank Benefits (Planned - Phase 3)

**Zealot (2,000 total favor):**
- 10% increased favor gain
- Reduced ability cooldowns (-10%)
- Visual rank indicator

**Champion (5,000 total favor):**
- 20% increased favor gain
- Reduced ability cooldowns (-20%)
- Deity-specific passive ability
- Unique title display

**Avatar (10,000 total favor):**
- 30% increased favor gain
- Reduced ability cooldowns (-30%)
- Ultimate ability unlock
- Unique visual effects
- Server-wide announcement on rank achievement

---

## Favor Management Strategy

### Early Game (Initiate)

**Goal:** Reach Disciple rank (500 favor)

- Focus on PvP against rival deity followers (20 favor per kill)
- Need ~25 rival kills or ~50 neutral kills
- Avoid spending too much favor on abilities initially
- Learn ability timings and cooldowns

### Mid Game (Disciple - Zealot)

**Goal:** Build favor reserve and master abilities

- Balance favor spending with saving
- Maintain 50-100 favor reserve for key fights
- Use abilities strategically, not on cooldown
- Target rivals for efficient favor gain

### Late Game (Champion+)

**Goal:** Maximize devotion and dominate PvP

- Full ability access and enhanced effects
- Enough favor income to use abilities liberally
- Focus on maintaining killstreaks
- Prepare for end-game content (crusades, duels)

---

## Favor Tracking

### Check Your Favor

- **Command:** `/favor` or `/deity status`
- **HUD:** Always visible in top-right corner
- **Display:** Shows current favor and devotion rank

### What's Tracked

Your player data tracks:
- **Current Favor:** Available to spend
- **Total Favor Earned:** Lifetime total (for rank progression)
- **Kill Count:** Total kills while pledged to current deity
- **Devotion Rank:** Current rank
- **Pledge Date:** When you joined your deity

---

## Favor Economy Examples

### Scenario 1: Efficient Hunting
You're a Khoras follower hunting Morthen followers (rivals):
- Kill 1: +20 favor (rival bonus)
- Kill 2: +20 favor (40 total)
- Kill 3: +20 favor (60 total)
- Use War Banner: -15 favor (45 remaining)
- Kill 4 (with buff): +20 favor (65 total)
- **Net: 65 favor from 4 kills, 1 ability used**

### Scenario 2: Aggressive Play
You're a Lysa follower in mixed combat:
- Kill neutral: +10 favor
- Kill rival (Umbros - future): +20 favor (30 total)
- Use Swift Feet: -8 favor (22 remaining)
- Die: -5 favor (17 remaining)
- Kill ally: +5 favor (22 remaining)
- **Net: 22 favor from 3 kills, 1 death, 1 ability used**

### Scenario 3: Rank Push
You're at 450 total favor, need 50 more for Disciple:
- Current favor: 25
- Need: 5 more rival kills (5 × 20 = 100 favor earned)
- Can use abilities: Yes (25 favor available)
- After 5 kills: 125 current, 550 total → **Disciple rank achieved!**

---

## Advanced Tips

### Favor Maximization
1. **Target rivals** when possible (2x favor)
2. **Avoid allied/same deity** fights (0.5x favor)
3. **Stay alive** to avoid death penalties
4. **Save favor** for critical fights
5. **Use abilities** to secure kills worth more than their cost

### Ability Economy
- Basic buff (8-10 favor) worth it if securing 1+ kills
- Team buff (15 favor) worth it if team secures 2+ kills
- Defensive (20 favor) worth it to avoid death penalty (5 favor) + lost favor income

### Rank Rushing
- Can reach Disciple in ~1-2 hours of focused PvP
- Target rich environments (populated servers)
- Rival hunting is most efficient route
- Advanced abilities often worth the grind

---

## Favor System Commands

- `/favor` - Check current favor and rank
- `/deity status` - Detailed favor statistics
- `/ability cooldowns` - Check ability availability

See [Ability Reference](ability_reference.md) for ability costs and [Deity Reference](deity_reference.md) for relationship bonuses.

---

## Future Enhancements

The favor system will expand in future phases:

**Phase 3:**
- Enhanced rank benefits
- Favor multipliers from devotion rank
- Favor costs for deity switching

**Phase 4:**
- Territory control bonuses
- Shrine offering mechanics
- Sacred ground favor generation

**Phase 5:**
- Favor stakes in divine duels
- Crusade favor rewards
- Relic bonuses to favor gain

Stay tuned for updates!
