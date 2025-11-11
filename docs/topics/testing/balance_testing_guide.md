# Balance Testing Guide - PantheonWars Blessing System

**Version:** 1.0
**Last Updated:** October 30, 2025
**Phase:** 3.4 - Deity Blessing Trees
**Total Blessings:** 80 (8 deities × 10 blessings each)

---

## Table of Contents

1. [Overview](#overview)
2. [Test Environment Setup](#test-environment-setup)
3. [Testing Methodology](#testing-methodology)
4. [Test Scenarios by Deity](#test-scenarios-by-deity)
5. [Balance Metrics](#balance-metrics)
6. [Known Issues & Limitations](#known-issues--limitations)
7. [Balance Adjustment Process](#balance-adjustment-process)
8. [Test Results Template](#test-results-template)

---

## Overview

### Purpose

This guide provides a systematic approach to balance testing all 80 blessings across 8 deities in PantheonWars. The goal is to ensure:

1. **Parity** - All deities are roughly equal in power at full progression
2. **Viability** - Each deity has at least one competitive playstyle
3. **Choice** - Path choices within each deity are meaningful
4. **Progression** - Power scales appropriately across tiers
5. **Fun** - Each deity feels unique and enjoyable to play

### Scope

**What's Being Tested:**
- Stat modifier values (damage, armor, health, speed, healing)
- Blessing prerequisite chains and unlock progression
- Player blessing vs religion blessing balance
- Cross-deity balance (relative power levels)
- Path viability (offense vs defense vs utility)

**What's NOT Being Tested (Yet):**
- Special effect handlers (lifesteal, poison, critical strikes, etc.)
- GUI elements (BlessingTreeDialog, ReligionManagementDialog)
- Network synchronization edge cases
- Performance with 50+ players

---

## Test Environment Setup

### Requirements

1. **Local Test Server**
   - Single-player or local multiplayer world
   - Creative mode for easy favor/prestige manipulation
   - PvP enabled for combat testing

2. **Admin Commands Available**
   ```
   /favor set <amount>        # Set favor for rank testing
   /favor settotal <amount>   # Set total favor for unlocking
   ```

3. **Testing Accounts**
   - At least 2 player accounts (for PvP and religion testing)
   - Optionally 3-4 for full religion testing

4. **Testing Tools**
   - Stopwatch or timer for DPS measurements
   - Spreadsheet for recording test results
   - Screen recording software (optional, for bug reports)

### Initial Setup

```bash
# 1. Start a new creative world
# 2. Create a religion for each deity you're testing
/religion create "WarTestReligion" Khoras public
/religion create "HuntTestReligion" Lysa public
# ... etc for all 8 deities

# 3. Set yourself to max favor/prestige for full blessing access
/favor settotal 15000
# (As founder, religion prestige will also be high)

# 4. Unlock all blessings for your deity
/blessings list
/blessings unlock <blessing_id>  # Repeat for all blessings
```

---

## Testing Methodology

### Phase 1: Individual Blessing Verification (2-3 hours)

**Goal:** Verify each blessing applies stats correctly

**Process:**
1. Create a new test character
2. Join a religion of one deity
3. Set favor to unlock Tier 1 blessing
4. Unlock Tier 1 blessing
5. Verify stat changes using `/blessings active`
6. Check actual in-game stats (health bar, damage output, movement speed)
7. Repeat for all tiers

**What to Check:**
- ✅ Blessing unlocks successfully
- ✅ Stats apply immediately
- ✅ Stat values match blessing description
- ✅ Blessings stack additively (player + religion)
- ✅ Stats persist after logout/login

**Red Flags:**
- ❌ Blessing unlocks but stats don't change
- ❌ Stat values don't match description
- ❌ Stats disappear after logout
- ❌ Multiple blessings of same stat don't stack correctly

---

### Phase 2: Path Viability Testing (3-4 hours)

**Goal:** Ensure both paths within each deity are viable

**Process:**

**Test A: Offense Path Only**
1. Create character, unlock only Tier 1 + Tier 2 Offense + Tier 3 Offense
2. Test combat effectiveness against neutral mobs
3. Test PvP against same-gear opponent
4. Record: Time to kill, survivability, enjoyment

**Test B: Defense/Utility Path Only**
1. Create character, unlock only Tier 1 + Tier 2 Defense + Tier 3 Defense
2. Test survival against multiple mobs
3. Test PvP against same-gear opponent
4. Record: Time to kill, survivability, enjoyment

**Test C: Balanced (Both Paths)**
1. Create character, unlock Tier 1-3 from both paths + Tier 4 capstone
2. Test combat effectiveness
3. Test PvP effectiveness
4. Record: Time to kill, survivability, enjoyment

**What to Check:**
- ✅ Both paths feel competitive
- ✅ Capstone (Tier 4) feels powerful and rewarding
- ✅ No "trap" choices that make character unplayable

**Red Flags:**
- ❌ One path drastically outperforms the other
- ❌ Character feels weak without capstone
- ❌ Offense path kills so fast that defense is unnecessary
- ❌ Defense path makes character unkillable but unable to kill

---

### Phase 3: Cross-Deity Balance (4-6 hours)

**Goal:** Ensure all 8 deities are roughly equal in power

**Process:**

1. **Create 8 Max-Blessing Characters** (one per deity)
   - Set favor to 15000
   - Set religion prestige to 15000
   - Unlock all 10 blessings (6 player + 4 religion)

2. **PvE Benchmark Test**
   - Location: Same area with consistent mob spawns
   - Test: Kill 10 hostile mobs, record time
   - Measure: Average time to kill, deaths, health remaining

3. **PvP Round-Robin Tournament**
   - Format: Each deity fights each other deity 3 times
   - Arena: Flat, open area with no environmental advantages
   - Gear: Identical gear for all participants
   - Record: Win rate, average fight duration, damage taken/dealt

4. **Survival Stress Test**
   - Test: Survive as long as possible against waves of mobs
   - Measure: Time survived, total kills, damage taken

**What to Check:**
- ✅ No deity has >65% win rate in PvP
- ✅ PvE clear times are within 25% of each other
- ✅ Each deity has clear identity/playstyle

**Red Flags:**
- ❌ One deity dominates all matchups
- ❌ One deity has no winning matchups
- ❌ Deities feel too similar in playstyle
- ❌ Extreme rock-paper-scissors dynamics (acceptable in moderation)

---

### Phase 4: Religion Blessing Impact (2-3 hours)

**Goal:** Verify religion blessings provide meaningful group benefits

**Process:**

1. **Solo vs Group Comparison**
   - Test A: Character with only player blessings (no religion)
   - Test B: Character with player + religion blessings
   - Measure: Stat difference, combat effectiveness

2. **Scaling Test**
   - Religion with 1 member (founder only)
   - Religion with 3 members
   - Religion with 5+ members
   - Verify: Religion blessings apply to all members equally

3. **Cross-Religion PvP**
   - 2v2 battles: Religion A vs Religion B
   - Different deity combinations
   - Measure: Which religion blessings provide best team fight advantage

**What to Check:**
- ✅ Religion blessings provide ~30-40% of total power budget
- ✅ Religion blessings encourage group play
- ✅ All members receive religion buffs equally

**Red Flags:**
- ❌ Religion blessings feel negligible
- ❌ Solo play is just as strong as group play
- ❌ Religion blessings don't apply to some members
- ❌ One religion's blessings vastly outperform others

---

## Test Scenarios by Deity

### Khoras (War) - Melee Tank/DPS

**Identity:** High damage, high survivability melee fighter

**Offense Path:** Bloodlust → Berserker Rage
**Defense Path:** Iron Skin → Unbreakable
**Capstone:** Avatar of War (both paths required)

**Test Scenarios:**

1. **Tank Test**
   - Take defensive path blessings only
   - Face tank 5 hostile mobs simultaneously
   - Expected: Survive with >40% health

2. **Berserker Test**
   - Take offensive path blessings only
   - Kill 3 hostile mobs in quick succession
   - Expected: Lifesteal keeps you alive through combat

3. **PvP Duel vs Lysa (Hunt)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Khoras advantage: Higher survivability
   - Lysa advantage: Kiting, range

4. **Religion Raid Boss**
   - 3-member Khoras religion vs tough boss mob
   - Expected: Combined melee damage + War Banner = fast kill

**Balance Targets:**
- Average mob TTK (time to kill): 5-8 seconds
- PvP survivability: 15-25 seconds under focus fire
- Capstone power spike: +40-50% combat effectiveness

---

### Lysa (Hunt) - Ranged DPS/Mobility

**Identity:** High mobility, high ranged damage, critical hits

**Offense Path:** Deadly Precision → Master Huntress
**Defense Path:** Silent Stalker → Apex Predator
**Capstone:** Avatar of the Hunt (both paths required)

**Test Scenarios:**

1. **Kiting Test**
   - Take mobility path blessings only
   - Fight melee enemy while maintaining distance
   - Expected: Never get hit, high movement advantage

2. **Sniper Test**
   - Take precision path blessings only
   - Kill targets from maximum bow range
   - Expected: High burst damage with critical hits

3. **PvP Duel vs Khoras (War)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Lysa advantage: Range, mobility, kiting
   - Khoras advantage: Survivability if closes distance

4. **Hunt Efficiency**
   - Time to gather 20 animal resources
   - Expected: Fastest resource gathering of all deities

**Balance Targets:**
- Movement speed bonus: 50-80% faster than base
- Critical hit frequency: 15-25% of attacks
- Ranged combat effectiveness: Equal to Khoras melee at range

---

### Morthen (Death) - Lifesteal/DoT Sustain Fighter

**Identity:** Sustained damage, lifesteal, poison effects, durability

**Offense Path:** Soul Reaper → Plague Bearer
**Defense Path:** Undying → Deathless
**Capstone:** Lord of Death (both paths required)

**Test Scenarios:**

1. **Sustain Test**
   - Take offensive path (lifesteal blessings)
   - Fight 10 mobs without healing items
   - Expected: Lifesteal sustains health throughout

2. **Tank Test**
   - Take defensive path (health/armor/regen)
   - Survive prolonged combat engagement
   - Expected: Highest effective HP pool

3. **PvP Duel vs Aethra (Light)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Morthen advantage: Lifesteal, poison DoT
   - Aethra advantage: Burst healing, armor

4. **War of Attrition**
   - Extended PvP fight (no healing items allowed)
   - Expected: Morthen wins via sustain and lifesteal

**Balance Targets:**
- Lifesteal healing: 10-20% of damage dealt
- Poison DoT: 2-5% max HP per second
- Effective HP (health + armor + regen): Highest of all deities

---

### Aethra (Light) - Healer/Support/Paladin

**Identity:** Healing, divine protection, support, balanced damage

**Offense Path:** Radiant Strike → Purifying Light
**Defense Path:** Blessed Shield → Aegis of Light
**Capstone:** Avatar of Light (both paths required)

**Test Scenarios:**

1. **Healing Test**
   - Take all healing effectiveness blessings
   - Use healing items in combat
   - Expected: Healing efficiency 50-80% higher than base

2. **Support Test**
   - 2v2 PvP: Aethra + ally vs 2 enemies
   - Aethra focuses on healing/supporting ally
   - Expected: Support role is viable and effective

3. **PvP Duel vs Morthen (Death)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Aethra advantage: Burst healing, damage reduction
   - Morthen advantage: Sustain, lifesteal

4. **Raid Healing**
   - 4-member religion vs tough boss
   - Aethra heals/supports 3 DPS members
   - Expected: Significantly improved survival

**Balance Targets:**
- Healing effectiveness: +50-80% from blessings
- Damage output: 70-85% of pure DPS deities
- Survivability with healing: Equal to tank deities

---

### Umbros (Shadows) - Stealth Assassin

**Identity:** Stealth, high burst damage, evasion, critical hits

**Offense Path:** Assassinate → Deadly Ambush
**Defense Path:** Phantom Dodge → Vanish
**Capstone:** Avatar of Shadows (both paths required)

**Test Scenarios:**

1. **Burst Damage Test**
   - Take offensive path (critical + backstab)
   - Ambush unsuspecting target
   - Expected: Highest burst damage in game

2. **Evasion Test**
   - Take mobility path (speed + stealth)
   - Survive against 3 ranged attackers
   - Expected: Difficult to hit, high survival via evasion

3. **PvP Duel vs Tharos (Storms)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Umbros advantage: Stealth, burst, single-target
   - Tharos advantage: AoE, can't be avoided

4. **Assassination Mission**
   - Sneak past guards, eliminate target, escape
   - Expected: Best stealth/assassination performance

**Balance Targets:**
- Movement speed: 80-120% faster than base
- Critical hit chance: 20-30% with blessings
- Burst damage: 2-3x normal attack in optimal conditions

---

### Tharos (Storms) - AoE Elemental Caster

**Identity:** AoE damage, lightning, elemental power, mobility

**Offense Path:** Lightning Strike → Thunderlord
**Defense Path:** Storm Rider → Tempest
**Capstone:** Avatar of Storms (both paths required)

**Test Scenarios:**

1. **AoE Damage Test**
   - Take offensive path
   - Fight 5+ enemies grouped together
   - Expected: Highest AoE clear speed

2. **Kiting Test**
   - Take mobility path
   - Fight melee enemy while maintaining distance
   - Expected: High mobility, hard to catch

3. **PvP Duel vs Umbros (Shadows)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Tharos advantage: AoE, can't be stealthed against
   - Umbros advantage: Single-target burst, mobility

4. **Mob Clear Speed**
   - Clear area of 20+ hostile mobs
   - Expected: Fastest clear time due to AoE

**Balance Targets:**
- AoE damage: 60-80% of single-target per enemy hit
- Movement speed: 60-100% faster than base
- Multi-target effectiveness: Best in game

---

### Gaia (Earth) - Tank/Regeneration

**Identity:** Maximum durability, regeneration, slow but unstoppable

**Offense Path:** Stone Form → Mountain Guard
**Defense Path:** Nature's Blessing → Lifebloom
**Capstone:** Avatar of Earth (both paths required)

**Test Scenarios:**

1. **Tank Test**
   - Take defensive path
   - Face tank 10 hostile mobs simultaneously
   - Expected: Survive indefinitely with regeneration

2. **Siege Test**
   - Stand in one location defending against waves
   - No retreating allowed
   - Expected: Highest wave survival

3. **PvP Duel vs Vex (Madness)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Gaia advantage: Durability, regeneration, sustain
   - Vex advantage: Burst damage, chaos effects

4. **Immortal Test**
   - How long can you survive 5v1 PvP?
   - Expected: Longest survival time of all deities

**Balance Targets:**
- Effective HP: 200-300% of base character
- Regeneration: 1-3% max HP per second
- Damage output: 60-75% of pure DPS deities (trade-off for survivability)

---

### Vex (Madness) - Chaos/Hybrid

**Identity:** Unpredictable, balanced offense/defense, chaos effects

**Offense Path:** Chaotic Fury → Pandemonium
**Defense Path:** Delirium Shield → Mind Fortress
**Capstone:** Avatar of Madness (both paths required)

**Test Scenarios:**

1. **Chaos Test**
   - Take offensive path
   - Test damage variance and unpredictability
   - Expected: High damage but inconsistent

2. **Hybrid Test**
   - Take both paths for balanced build
   - Test versatility in various situations
   - Expected: Jack-of-all-trades, good at everything

3. **PvP Duel vs Gaia (Earth)**
   - Full blessings, identical gear
   - Expected: Close fight (45-55% win rate)
   - Vex advantage: Higher damage output, speed
   - Gaia advantage: Sustain, regeneration

4. **Unpredictability Test**
   - Multiple fights with same setup
   - Expected: Different outcomes due to chaos effects

**Balance Targets:**
- Average damage: Equal to focused DPS deities
- Survivability: 80-120% of base (balanced)
- Versatility: Viable in most situations

---

## Balance Metrics

### Quantitative Metrics

Track these numbers for each deity at max blessings:

**Combat Stats:**
- Average mob TTK (time to kill): 5-10 seconds (±30%)
- Average PvP TTK: 15-30 seconds (±40%)
- Effective HP (health × damage reduction): ±50% variance acceptable
- DPS (damage per second): ±35% variance acceptable
- Movement speed: ±60% variance acceptable (mobility vs tank trade-off)

**Progression Metrics:**
- Tier 1 power increase: +15-25%
- Tier 2 power increase: +30-45%
- Tier 3 power increase: +50-70%
- Tier 4 power increase: +80-120%

**Religion Metrics:**
- Solo vs religion buff difference: +30-50%
- Religion blessing contribution: 30-40% of total power

### Qualitative Metrics

**Player Experience:**
- Does each deity feel unique? (Yes/No)
- Are path choices meaningful? (Yes/No)
- Is progression satisfying? (1-10 scale)
- Would you play this deity? (Yes/No)

**Balance Feel:**
- Any deity feels OP? (Name it)
- Any deity feels UP? (Name it)
- Any specific blessing feels mandatory? (Name it)
- Any specific blessing feels useless? (Name it)

---

## Known Issues & Limitations

### Current Limitations (Oct 30, 2025)

1. **Special Effects Not Implemented**
   - Lifesteal (Khoras, Morthen, Aethra)
   - Poison DoT (Morthen)
   - Critical hits (Lysa, Umbros)
   - Stealth bonuses (Umbros)
   - AoE effects (Khoras, Tharos)
   - Damage reduction (multiple deities)
   - Many other special effects

   **Impact:** Stat testing is accurate, but gameplay testing is incomplete. Special effects may significantly shift balance when implemented.

2. **No GUI for Blessing Trees**
   - All interaction via commands
   - Difficult to visualize blessing dependencies
   - May miss UX issues

3. **Limited Multiplayer Testing**
   - Religion mechanics need 3+ players to fully test
   - PvP balance needs diverse player skill levels
   - Cross-religion raid testing requires coordination

### Testing Workarounds

**Special Effects:**
- Test stat modifiers independently
- Document expected behavior for special effects
- Re-test balance after special effect implementation

**GUI:**
- Use `/blessings tree` command for visualization
- Manual tracking of unlocked blessings
- Plan for GUI testing in Phase 3.5

**Multiplayer:**
- Recruit 2-3 friends for multiplayer tests
- Use alt accounts if necessary
- Post on community forums for volunteer testers

---

## Balance Adjustment Process

### When to Adjust

**Adjust if:**
- One deity has >65% win rate across all matchups
- One deity has <35% win rate across all matchups
- PvE clear times differ by >40%
- Players report specific blessing is "mandatory" or "useless"
- One path within a deity is clearly superior (>75% pick rate)

**Don't adjust if:**
- Win rates are 45-55% (acceptable variance)
- Deity has clear counter-play available
- Difference is <20% in any metric
- Issue is player skill rather than balance

### How to Adjust

**Stat Modifier Adjustments:**

1. **Identify the Problem**
   - Too strong? Reduce stat bonuses by 10-20%
   - Too weak? Increase stat bonuses by 10-20%
   - One path dominates? Buff weaker path by 15-25%

2. **Make Surgical Changes**
   - Don't buff/nerf entire deity at once
   - Target specific problematic blessings
   - Maintain relative power between tiers

3. **Example Adjustment**
   ```csharp
   // Before: Khoras Berserker Rage feels too strong
   StatModifiers = new Dictionary<string, float>
   {
       { VintageStoryStats.MeleeWeaponsDamage, 0.25f },  // 25% bonus
       { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
   }

   // After: Reduce damage bonus
   StatModifiers = new Dictionary<string, float>
   {
       { VintageStoryStats.MeleeWeaponsDamage, 0.20f },  // 20% bonus (-5%)
       { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }    // Keep same
   }
   ```

4. **Document Changes**
   - Track all balance changes in changelog
   - Note reasoning for each change
   - Schedule re-testing after changes

### Iteration Cycle

1. **Test** - Run balance tests
2. **Analyze** - Review metrics and feedback
3. **Adjust** - Make targeted changes
4. **Re-test** - Verify adjustments improved balance
5. **Repeat** - Continue until balanced

**Expected Iterations:** 2-4 passes to achieve good balance

---

## Test Results Template

Use this template to record your testing:

```markdown
# Balance Test Results - [Deity Name]

**Date:** [Date]
**Tester:** [Your Name]
**Version:** 0.3.x

## Configuration

- **Favor Rank:** Champion (10,000 favor)
- **Prestige Rank:** Legendary (8,000 prestige)
- **Blessings Unlocked:** [List]
- **Path Tested:** Offense / Defense / Balanced

## PvE Testing

| Metric | Result | Target | Status |
|--------|--------|--------|--------|
| Average Mob TTK | 7.2s | 5-10s | ✅ Pass |
| 10-Mob Clear Time | 1m 23s | 1-2m | ✅ Pass |
| Deaths during test | 0 | <2 | ✅ Pass |
| Health remaining | 65% | >40% | ✅ Pass |

## PvP Testing (vs [Opponent Deity])

| Match | Winner | Duration | Notes |
|-------|--------|----------|-------|
| 1 | Khoras | 22s | Close fight, lifesteal clutch |
| 2 | Lysa | 18s | Kiting worked well |
| 3 | Khoras | 31s | Long fight, both low HP |

**Win Rate:** 66% (2/3)
**Average Duration:** 23.7s

## Notes

**Strengths:**
- Lifesteal sustain is very strong
- Capstone feels impactful
- Fun to play

**Weaknesses:**
- Vulnerable to kiting
- Lacks ranged options
- Movement speed could be higher

**Balance Concerns:**
- Berserker Rage might be overtuned (+25% damage feels high)
- Iron Skin prerequisite chain feels clunky

**Recommendations:**
- Reduce Berserker Rage damage bonus to +20%
- Consider adding movement speed to Tier 1
```

---

## Quick Reference: Balance Testing Checklist

### Pre-Test Setup
- [ ] Test world created (creative mode, PvP enabled)
- [ ] Admin commands available
- [ ] 2+ test accounts ready
- [ ] Recording tools ready (spreadsheet, timer)

### Phase 1: Individual Blessings (2-3 hours)
- [ ] Test all 8 deities × 10 blessings = 80 blessing verifications
- [ ] Verify stats apply correctly
- [ ] Check persistence after logout

### Phase 2: Path Viability (3-4 hours)
- [ ] Test offense path for each deity
- [ ] Test defense/utility path for each deity
- [ ] Test balanced/capstone build for each deity
- [ ] Document path strengths/weaknesses

### Phase 3: Cross-Deity Balance (4-6 hours)
- [ ] Create 8 max-blessing characters (one per deity)
- [ ] Run PvE benchmarks for all 8
- [ ] Conduct round-robin PvP tournament (28 matchups minimum)
- [ ] Analyze win rates and TTK

### Phase 4: Religion Blessings (2-3 hours)
- [ ] Test solo vs religion buff difference
- [ ] Test religion blessing scaling (1, 3, 5+ members)
- [ ] Cross-religion PvP tests

### Post-Test Analysis
- [ ] Compile all test data
- [ ] Identify balance issues
- [ ] Propose adjustments
- [ ] Schedule re-testing

**Total Estimated Time:** 11-16 hours for comprehensive testing

---

## Conclusion

This balance testing guide provides a systematic approach to ensuring all 80 blessings across 8 deities are properly balanced. Follow the methodology, record your results, and iterate on adjustments until all deities feel competitive and fun.

**Remember:**
- Balance is iterative - don't expect perfection on the first pass
- Player feedback is valuable - listen to actual gameplay experiences
- Unique identity is more important than perfect numerical balance
- Fun gameplay trumps spreadsheet balance

Good luck with testing! 🎯

---

**Next Steps After Balance Testing:**
1. Document all balance changes in changelog
2. Update blessing descriptions if stat values changed
3. Move to Phase 3.5: Integration & Polish
4. Implement special effect handlers (will require re-balancing)
