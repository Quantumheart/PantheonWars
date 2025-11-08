# Blessing Reference - PantheonWars

**Version:** 0.3.5
**Last Updated:** October 30, 2025
**Total Blessings:** 80 (8 deities × 10 blessings each)

---

## Table of Contents

1. [Overview](#overview)
2. [How Blessings Work](#how-blessings-work)
3. [Blessing Commands](#blessing-commands)
4. [Deity Blessing Trees](#deity-blessing-trees)
   - [Khoras (War)](#khoras-war)
   - [Lysa (Hunt)](#lysa-hunt)
   - [Morthen (Death)](#morthen-death)
   - [Aethra (Light)](#aethra-light)
   - [Umbros (Shadows)](#umbros-shadows)
   - [Tharos (Storms)](#tharos-storms)
   - [Gaia (Earth)](#gaia-earth)
   - [Vex (Madness)](#vex-madness)
5. [Progression Guide](#progression-guide)
6. [Build Recommendations](#build-recommendations)

---

## Overview

Blessings are permanent passive bonuses that enhance your character and religion. Each of the 8 deities has a unique blessing tree with 10 blessings:

- **6 Player Blessings** - Personal bonuses that only affect you
- **4 Religion Blessings** - Congregation-wide bonuses that affect all members

### Blessing Structure

**Player Blessings (6 total per deity):**
- **Tier 1 (Initiate):** 1 foundational blessing - Available at 0 favor
- **Tier 2 (Disciple):** 2 path blessings - Choose your playstyle at 500 favor
- **Tier 3 (Zealot):** 2 specialization blessings - Double down at 2,000 favor
- **Tier 4 (Champion):** 1 capstone blessing - Ultimate power at 5,000 favor (requires both Tier 3)

**Religion Blessings (4 total per deity):**
- **Tier 1 (Fledgling):** Basic group buff - Available at 0 prestige
- **Tier 2 (Established):** Improved group buff - Available at 500 prestige
- **Tier 3 (Renowned):** Advanced group buff - Available at 2,000 prestige
- **Tier 4 (Legendary):** Capstone group buff - Available at 5,000 prestige

### Key Features

✨ **Meaningful Choices** - Tier 2 offers path selection (offense vs defense vs utility)
✨ **Synergistic Capstones** - Tier 4 requires both Tier 3 blessings, rewarding full investment
✨ **Unique Identities** - Each deity has distinct themes and playstyles
✨ **Additive Stacking** - Player and religion blessings stack together
✨ **Permanent Power** - Blessings persist forever once unlocked

---

## How Blessings Work

### Unlocking Blessings

**Player Blessings:**
1. Join a religion serving your chosen deity
2. Earn favor through PvP combat (10 favor per kill, modified by deity relationships)
3. Reach required favor rank (Initiate, Disciple, Zealot, Champion)
4. Unlock prerequisite blessings first (Tier 2+ have prerequisites)
5. Use `/blessings unlock <blessing_id>` to unlock

**Religion Blessings:**
1. Your religion must reach required prestige rank
2. Only the religion founder can unlock religion blessings
3. Prestige is earned collectively by all members through PvP
4. Use `/blessings unlock <blessing_id>` as founder

### Favor Ranks (Player Progression)

| Rank | Total Favor Required | Blessings Available |
|------|---------------------|-----------------|
| **Initiate** | 0 | Tier 1 |
| **Disciple** | 500 | Tier 2 |
| **Zealot** | 2,000 | Tier 3 |
| **Champion** | 5,000 | Tier 4 |
| **Avatar** | 10,000 | (Future expansion) |

### Prestige Ranks (Religion Progression)

| Rank | Total Prestige Required | Blessings Available |
|------|------------------------|-----------------|
| **Fledgling** | 0 | Tier 1 |
| **Established** | 500 | Tier 2 |
| **Renowned** | 2,000 | Tier 3 |
| **Legendary** | 5,000 | Tier 4 |
| **Mythic** | 10,000 | (Future expansion) |

### Stat Modifiers Explained

**Percentage Bonuses:**
- `+10% melee damage` = 10% more damage with melee weapons
- `+15% max health` = 15% higher health pool
- `+20% movement speed` = 20% faster walking/running

**Multiplicative Stacking:**
- Player blessings stack additively with each other
- Religion blessings stack additively with each other
- Player + religion blessings combine for total bonus
- Example: Player +15% damage + Religion +10% damage = +25% total damage

**Special Effects:**
- Some blessings grant special abilities (lifesteal, critical hits, stealth, etc.)
- Special effects are noted in blessing descriptions
- **Note:** Special effect handlers are planned for future implementation

---

## Blessing Commands

### Viewing Blessings

```bash
# List all available blessings for your deity
/blessings list

# View your unlocked player blessings
/blessings player

# View your religion's unlocked blessings
/blessings religion

# View details about a specific blessing
/blessings info <blessing_id>

# View your deity's blessing tree
/blessings tree player
/blessings tree religion

# View all active blessings affecting you
/blessings active
```

### Unlocking Blessings

```bash
# Unlock a player blessing (if you meet requirements)
/blessings unlock <blessing_id>

# Unlock a religion blessing (founder only, if religion meets requirements)
/blessings unlock <blessing_id>
```

### Requirements Check

Before unlocking, you must:
- ✅ Be in a religion of the correct deity
- ✅ Meet favor/prestige rank requirements
- ✅ Have unlocked all prerequisite blessings
- ✅ Not already have the blessing unlocked

---

## Deity Blessing Trees

---

## Khoras (War)

**Theme:** Brutal melee combat, high survivability, frontline warrior

**Playstyle:** Tank/DPS hybrid - Stand and fight, never retreat

### Player Blessings (6)

#### Tier 1: Initiate

**🗡️ Warrior's Resolve**
- **ID:** `khoras_warriors_resolve`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +10% melee damage
  - +10% max health
- **Description:** Your devotion to war strengthens body and blade. Foundation for all Khoras warriors.

---

#### Tier 2: Disciple - Choose Your Path

**⚔️ Bloodlust** (Offense Path)
- **ID:** `khoras_bloodlust`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Warrior's Resolve
- **Effects:**
  - +15% melee damage
  - +10% attack speed
- **Description:** Embrace the rage of battle. High damage output for aggressive playstyle.

**🛡️ Iron Skin** (Defense Path)
- **ID:** `khoras_iron_skin`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Warrior's Resolve
- **Effects:**
  - +20% armor
  - +15% max health
- **Description:** Battle hardens your body. Tank path for frontline defense.

---

#### Tier 3: Zealot - Specialization

**🔥 Berserker Rage** (Offense Specialization)
- **ID:** `khoras_berserker_rage`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Bloodlust
- **Effects:**
  - +25% melee damage
  - +15% attack speed
  - Lifesteal: Heal 10% of damage dealt
- **Description:** Unleash devastating fury with lifesteal. Sustained aggression with self-healing.

**🏔️ Unbreakable** (Defense Specialization)
- **ID:** `khoras_unbreakable`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Iron Skin
- **Effects:**
  - +30% armor
  - +25% max health
  - 10% damage reduction
- **Description:** Become nearly invincible. Maximum survivability for tanking.

---

#### Tier 4: Champion - Capstone

**👑 Avatar of War** (Ultimate Power)
- **ID:** `khoras_avatar_of_war`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Berserker Rage AND Unbreakable
- **Effects:**
  - +15% melee damage
  - +15% attack speed
  - +15% armor
  - +15% max health
  - +10% movement speed
  - AoE cleave attacks
- **Description:** Embody war itself. Requires full commitment to both paths. Devastating power.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**🚩 War Banner**
- **ID:** `khoras_war_banner`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +8% melee damage
  - +8% max health
- **Description:** Your congregation's banner inspires strength and courage.

#### Tier 2: Established

**⚔️ Legion Tactics**
- **ID:** `khoras_legion_tactics`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** War Banner
- **Effects (All Members):**
  - +12% melee damage
  - +10% armor
  - +5% attack speed
- **Description:** Coordinated warfare. Your religion fights as one.

#### Tier 3: Renowned

**🏰 Warhost**
- **ID:** `khoras_warhost`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Legion Tactics
- **Effects (All Members):**
  - +18% melee damage
  - +15% armor
  - +15% max health
  - +10% attack speed
- **Description:** Elite fighting force. Unstoppable army.

#### Tier 4: Legendary

**⚡ Pantheon of War**
- **ID:** `khoras_pantheon_of_war`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Warhost
- **Effects (All Members):**
  - +25% melee damage
  - +20% armor
  - +20% max health
  - +15% attack speed
  - +8% movement speed
  - Group war cry ability
- **Description:** Your religion becomes legendary. Maximum combat power.

---

## Lysa (Hunt)

**Theme:** Ranged precision, high mobility, critical strikes, tracking

**Playstyle:** Kiting archer/scout - Strike from range, stay mobile

### Player Blessings (6)

#### Tier 1: Initiate

**👁️ Keen Eye**
- **ID:** `lysa_keen_eye`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +10% ranged damage
  - +10% movement speed
- **Description:** The hunt sharpens your senses. Foundation for hunters.

---

#### Tier 2: Disciple - Choose Your Path

**🎯 Deadly Precision** (Precision Path)
- **ID:** `lysa_deadly_precision`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Keen Eye
- **Effects:**
  - +15% ranged damage
  - +10% critical chance
- **Description:** Perfect your aim. Burst damage through crits.

**🌙 Silent Stalker** (Mobility Path)
- **ID:** `lysa_silent_stalker`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Keen Eye
- **Effects:**
  - +18% movement speed
  - +10% melee damage
  - Stealth bonus
- **Description:** Move like a shadow. Hybrid mobility with stealth.

---

#### Tier 3: Zealot - Specialization

**🏹 Master Huntress** (Precision Specialization)
- **ID:** `lysa_master_huntress`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Deadly Precision
- **Effects:**
  - +25% ranged damage
  - +20% critical chance
  - Headshot bonus
- **Description:** Legendary marksmanship. Maximum burst damage.

**🐆 Apex Predator** (Mobility Specialization)
- **ID:** `lysa_apex_predator`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Silent Stalker
- **Effects:**
  - +28% movement speed
  - +18% melee damage
  - +15% attack speed
  - Tracking vision
- **Description:** Untouchable hunter. Chase down any prey.

---

#### Tier 4: Champion - Capstone

**👑 Avatar of the Hunt** (Ultimate Power)
- **ID:** `lysa_avatar_of_hunt`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Master Huntress AND Apex Predator
- **Effects:**
  - +15% ranged damage
  - +15% melee damage
  - +20% movement speed
  - +10% attack speed
  - Multishot ability
  - Animal companion
- **Description:** Embody the perfect hunter. Master of pursuit and precision.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**🐺 Pack Hunters**
- **ID:** `lysa_pack_hunters`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +8% ranged damage
  - +8% movement speed
- **Description:** Your pack hunts as one.

#### Tier 2: Established

**⚡ Coordinated Strike**
- **ID:** `lysa_coordinated_strike`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** Pack Hunters
- **Effects (All Members):**
  - +12% ranged damage
  - +10% melee damage
  - +10% movement speed
- **Description:** Coordinated hunting. Perfect teamwork.

#### Tier 3: Renowned

**🦅 Apex Pack**
- **ID:** `lysa_apex_pack`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Coordinated Strike
- **Effects (All Members):**
  - +18% ranged damage
  - +15% melee damage
  - +15% movement speed
  - +10% attack speed
- **Description:** Elite hunting force. Unstoppable predators.

#### Tier 4: Legendary

**🌲 Hunter's Paradise**
- **ID:** `lysa_hunters_paradise`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Apex Pack
- **Effects (All Members):**
  - +25% ranged damage
  - +20% melee damage
  - +22% movement speed
  - +15% attack speed
  - Pack tracking ability
- **Description:** Your congregation becomes unstoppable predators.

---

## Morthen (Death)

**Theme:** Lifesteal, poison, durability, sustained damage over time

**Playstyle:** Sustain fighter - Outlast enemies with lifesteal and DoT

### Player Blessings (6)

#### Tier 1: Initiate

**💀 Death's Embrace**
- **ID:** `morthen_deaths_embrace`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +10% melee damage
  - +10% max health
  - Lifesteal: Heal 3% of damage dealt
- **Description:** Death empowers your strikes and body.

---

#### Tier 2: Disciple - Choose Your Path

**⚔️ Soul Reaper** (Offense Path)
- **ID:** `morthen_soul_reaper`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Death's Embrace
- **Effects:**
  - +15% melee damage
  - Lifesteal: Heal 10% of damage dealt
  - Poison: Attacks apply poison DoT
- **Description:** Harvest souls with dark magic. Lifesteal and poison.

**🛡️ Undying** (Defense Path)
- **ID:** `morthen_undying`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Death's Embrace
- **Effects:**
  - +20% max health
  - +15% armor
  - +10% healing effectiveness
- **Description:** Resist death itself. Tank with regeneration.

---

#### Tier 3: Zealot - Specialization

**☠️ Plague Bearer** (Offense Specialization)
- **ID:** `morthen_plague_bearer`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Soul Reaper
- **Effects:**
  - +25% melee damage
  - Lifesteal: Heal 15% of damage dealt
  - Poison: Strong DoT
  - Plague aura weakens enemies
- **Description:** Spread pestilence and decay. Death incarnate.

**⚰️ Deathless** (Defense Specialization)
- **ID:** `morthen_deathless`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Undying
- **Effects:**
  - +30% max health
  - +25% armor
  - +20% healing effectiveness
  - 10% damage reduction
- **Description:** Transcend mortality. Extreme durability.

---

#### Tier 4: Champion - Capstone

**👑 Lord of Death** (Ultimate Power)
- **ID:** `morthen_lord_of_death`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Plague Bearer AND Deathless
- **Effects:**
  - +15% melee damage
  - +15% armor
  - +15% max health
  - +10% attack speed
  - +15% healing effectiveness
  - Lifesteal: Heal 20% of damage dealt
  - Death aura
  - Execute low health enemies
- **Description:** Command death itself. Unkillable sustain fighter.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**🕯️ Death Cult**
- **ID:** `morthen_death_cult`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +8% melee damage
  - +8% max health
- **Description:** Your congregation embraces the darkness.

#### Tier 2: Established

**📜 Necromantic Covenant**
- **ID:** `morthen_necromantic_covenant`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** Death Cult
- **Effects (All Members):**
  - +12% melee damage
  - +10% armor
  - +8% healing effectiveness
- **Description:** Dark pact strengthens all.

#### Tier 3: Renowned

**⚔️ Deathless Legion**
- **ID:** `morthen_deathless_legion`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Necromantic Covenant
- **Effects (All Members):**
  - +18% melee damage
  - +15% armor
  - +15% max health
  - +12% healing effectiveness
- **Description:** Unkillable army of the dead.

#### Tier 4: Legendary

**👁️ Empire of Death**
- **ID:** `morthen_empire_of_death`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Deathless Legion
- **Effects (All Members):**
  - +25% melee damage
  - +20% armor
  - +20% max health
  - +18% healing effectiveness
  - +10% attack speed
  - Death mark ability
- **Description:** Your religion rules over death itself.

---

## Aethra (Light)

**Theme:** Healing, divine protection, support, balanced holy warrior

**Playstyle:** Paladin/Cleric - Support allies, tank with healing

### Player Blessings (6)

#### Tier 1: Initiate

**✨ Divine Grace**
- **ID:** `aethra_divine_grace`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +10% max health
  - +12% healing effectiveness
- **Description:** The light blesses you with divine vitality.

---

#### Tier 2: Disciple - Choose Your Path

**⚔️ Radiant Strike** (Offense Path)
- **ID:** `aethra_radiant_strike`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Divine Grace
- **Effects:**
  - +12% melee damage
  - +10% ranged damage
  - Lifesteal: Heal 5% of damage dealt
- **Description:** Your attacks radiate holy energy. Hybrid offense with healing.

**🛡️ Blessed Shield** (Defense Path)
- **ID:** `aethra_blessed_shield`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Divine Grace
- **Effects:**
  - +18% armor
  - +15% max health
  - 8% damage reduction
- **Description:** Light shields you from harm. Divine tank.

---

#### Tier 3: Zealot - Specialization

**☀️ Purifying Light** (Offense Specialization)
- **ID:** `aethra_purifying_light`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Radiant Strike
- **Effects:**
  - +22% melee damage
  - +18% ranged damage
  - Lifesteal: Heal 12% of damage dealt
  - AoE healing pulse
- **Description:** Unleash devastating holy power. Smite with healing.

**🌟 Aegis of Light** (Defense Specialization)
- **ID:** `aethra_aegis_of_light`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Blessed Shield
- **Effects:**
  - +28% armor
  - +25% max health
  - +18% healing effectiveness
  - 15% damage reduction
- **Description:** Become nearly invincible with divine protection.

---

#### Tier 4: Champion - Capstone

**👑 Avatar of Light** (Ultimate Power)
- **ID:** `aethra_avatar_of_light`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Purifying Light AND Aegis of Light
- **Effects:**
  - +15% melee damage
  - +15% ranged damage
  - +15% armor
  - +15% max health
  - +20% healing effectiveness
  - Lifesteal: Heal 15% of damage dealt
  - Radiant aura heals allies
  - Smite enemies
- **Description:** Embody divine radiance. Beacon of hope and destruction.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**🕊️ Blessing of Light**
- **ID:** `aethra_blessing_of_light`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +8% max health
  - +10% healing effectiveness
- **Description:** Your congregation is blessed by divine light.

#### Tier 2: Established

**⛪ Divine Sanctuary**
- **ID:** `aethra_divine_sanctuary`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** Blessing of Light
- **Effects (All Members):**
  - +12% armor
  - +10% max health
  - +12% healing effectiveness
- **Description:** Sacred protection shields all.

#### Tier 3: Renowned

**🤝 Sacred Bond**
- **ID:** `aethra_sacred_bond`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Divine Sanctuary
- **Effects (All Members):**
  - +15% armor
  - +15% max health
  - +15% healing effectiveness
  - +10% melee damage
  - +10% ranged damage
- **Description:** Divine unity empowers the congregation.

#### Tier 4: Legendary

**🏛️ Cathedral of Light**
- **ID:** `aethra_cathedral_of_light`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Sacred Bond
- **Effects (All Members):**
  - +20% armor
  - +20% max health
  - +20% healing effectiveness
  - +15% melee damage
  - +15% ranged damage
  - +8% movement speed
  - Divine sanctuary ability
- **Description:** Your religion becomes a beacon of divine power.

---

## Umbros (Shadows)

**Theme:** Stealth, assassination, high burst damage, evasion

**Playstyle:** Rogue/Assassin - Strike from shadows, high mobility

### Player Blessings (6)

#### Tier 1: Initiate

**🌑 Shadow Blend**
- **ID:** `umbros_shadow_blend`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +15% movement speed
  - +10% melee damage
  - Stealth bonus
- **Description:** Merge with shadows for speed and stealth.

---

#### Tier 2: Disciple - Choose Your Path

**🗡️ Assassinate** (Offense Path)
- **ID:** `umbros_assassinate`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Shadow Blend
- **Effects:**
  - +18% melee damage
  - +10% attack speed
  - +15% critical chance
  - Backstab bonus
- **Description:** Strike from darkness with lethal precision.

**💨 Phantom Dodge** (Mobility Path)
- **ID:** `umbros_phantom_dodge`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Shadow Blend
- **Effects:**
  - +25% movement speed
  - +12% attack speed
  - Enhanced evasion
  - Stealth bonus
- **Description:** Become untouchable through shadows.

---

#### Tier 3: Zealot - Specialization

**☠️ Deadly Ambush** (Offense Specialization)
- **ID:** `umbros_deadly_ambush`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Assassinate
- **Effects:**
  - +28% melee damage
  - +15% attack speed
  - +20% critical chance
  - Execute low health enemies
- **Description:** Master the art of assassination. Devastating burst.

**👻 Vanish** (Mobility Specialization)
- **ID:** `umbros_vanish`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Phantom Dodge
- **Effects:**
  - +35% movement speed
  - +18% attack speed
  - +12% melee damage
  - Near-invisibility
  - Stealth bonus
- **Description:** Disappear into shadows at will.

---

#### Tier 4: Champion - Capstone

**👑 Avatar of Shadows** (Ultimate Power)
- **ID:** `umbros_avatar_of_shadows`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Deadly Ambush AND Vanish
- **Effects:**
  - +20% melee damage
  - +30% movement speed
  - +20% attack speed
  - +20% critical chance
  - Shadow clones
  - Perfect stealth
  - Stealth bonus
- **Description:** Become one with darkness. Ultimate assassin.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**🌙 Shadow Cult**
- **ID:** `umbros_shadow_cult`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +10% movement speed
  - +8% melee damage
- **Description:** Your congregation moves through darkness.

#### Tier 2: Established

**🌫️ Cloak of Shadows**
- **ID:** `umbros_cloak`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** Shadow Cult
- **Effects (All Members):**
  - +15% movement speed
  - +12% melee damage
  - +10% attack speed
- **Description:** Shadows shroud all members.

#### Tier 3: Renowned

**🗡️ Night Assassins**
- **ID:** `umbros_night_assassins`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Cloak of Shadows
- **Effects (All Members):**
  - +20% movement speed
  - +18% melee damage
  - +15% attack speed
- **Description:** Elite shadow assassins.

#### Tier 4: Legendary

**🌑 Eternal Darkness**
- **ID:** `umbros_eternal_darkness`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Night Assassins
- **Effects (All Members):**
  - +28% movement speed
  - +25% melee damage
  - +20% attack speed
  - Shadow strike ability
- **Description:** Your religion commands the darkness.

---

## Tharos (Storms)

**Theme:** AoE lightning damage, elemental power, high mobility

**Playstyle:** Battlemage - AoE burst, kiting, elemental destruction

### Player Blessings (6)

#### Tier 1: Initiate

**⚡ Stormborn**
- **ID:** `tharos_stormborn`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +10% ranged damage
  - +12% movement speed
  - Shocking touch
- **Description:** Born of thunder and lightning.

---

#### Tier 2: Disciple - Choose Your Path

**🌩️ Lightning Strike** (Offense Path)
- **ID:** `tharos_lightning_strike`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Stormborn
- **Effects:**
  - +18% ranged damage
  - +15% melee damage
  - Chain lightning
- **Description:** Channel devastating lightning bolts. AoE damage.

**🌪️ Storm Rider** (Mobility Path)
- **ID:** `tharos_storm_rider`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Stormborn
- **Effects:**
  - +22% movement speed
  - +12% attack speed
  - +10% ranged damage
  - +10% melee damage
- **Description:** Ride the winds of the storm. Ultimate mobility.

---

#### Tier 3: Zealot - Specialization

**⚡ Thunderlord** (Offense Specialization)
- **ID:** `tharos_thunderlord`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Lightning Strike
- **Effects:**
  - +28% ranged damage
  - +22% melee damage
  - +15% attack speed
  - AoE lightning strikes
- **Description:** Command the fury of thunder. Maximum AoE damage.

**🌀 Tempest** (Mobility Specialization)
- **ID:** `tharos_tempest`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Storm Rider
- **Effects:**
  - +32% movement speed
  - +18% attack speed
  - +15% ranged damage
  - +15% melee damage
  - Whirlwind mobility
- **Description:** Become the eye of the storm.

---

#### Tier 4: Champion - Capstone

**👑 Avatar of Storms** (Ultimate Power)
- **ID:** `tharos_avatar_of_storms`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Thunderlord AND Tempest
- **Effects:**
  - +20% ranged damage
  - +20% melee damage
  - +25% movement speed
  - +20% attack speed
  - Permanent lightning aura
  - Thunderbolt strike
- **Description:** Embody the storm itself. Elemental devastation.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**⛈️ Storm Callers**
- **ID:** `tharos_storm_callers`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +8% ranged damage
  - +10% movement speed
- **Description:** Your congregation calls the storm.

#### Tier 2: Established

**⚡ Lightning Chain**
- **ID:** `tharos_lightning_chain`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** Storm Callers
- **Effects (All Members):**
  - +12% ranged damage
  - +10% melee damage
  - +12% movement speed
- **Description:** Lightning chains between allies.

#### Tier 3: Renowned

**🌩️ Thunderstorm**
- **ID:** `tharos_thunderstorm`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Lightning Chain
- **Effects (All Members):**
  - +18% ranged damage
  - +15% melee damage
  - +18% movement speed
  - +10% attack speed
- **Description:** Unleash devastating thunderstorms.

#### Tier 4: Legendary

**🌪️ Eye of the Storm**
- **ID:** `tharos_eye_of_the_storm`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Thunderstorm
- **Effects (All Members):**
  - +25% ranged damage
  - +20% melee damage
  - +25% movement speed
  - +15% attack speed
  - Massive AoE lightning storm
- **Description:** Your religion commands the heavens.

---

## Gaia (Earth)

**Theme:** Maximum durability, regeneration, defense, nature magic

**Playstyle:** Tank - Immovable object, outlast all enemies

### Player Blessings (6)

#### Tier 1: Initiate

**🌍 Earthen Resilience**
- **ID:** `gaia_earthen_resilience`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +15% max health
  - +10% armor
  - +8% healing effectiveness
- **Description:** Earth's strength flows through you.

---

#### Tier 2: Disciple - Choose Your Path

**🗿 Stone Form** (Defense Path)
- **ID:** `gaia_stone_form`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Earthen Resilience
- **Effects:**
  - +22% armor
  - +18% max health
  - 10% damage reduction
- **Description:** Become as unyielding as stone. Ultimate tank.

**🌿 Nature's Blessing** (Regeneration Path)
- **ID:** `gaia_natures_blessing`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Earthen Resilience
- **Effects:**
  - +20% max health
  - +18% healing effectiveness
  - Slow passive regeneration
- **Description:** Nature restores you constantly. Sustain tank.

---

#### Tier 3: Zealot - Specialization

**⛰️ Mountain Guard** (Defense Specialization)
- **ID:** `gaia_mountain_guard`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Stone Form
- **Effects:**
  - +32% armor
  - +28% max health
  - +10% melee damage
  - 15% damage reduction
- **Description:** Stand immovable like a mountain. Maximum defense.

**🌸 Lifebloom** (Regeneration Specialization)
- **ID:** `gaia_lifebloom`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Nature's Blessing
- **Effects:**
  - +30% max health
  - +28% healing effectiveness
  - Strong passive regeneration
  - Heal nearby allies
- **Description:** Life flourishes around you. Ultimate sustain.

---

#### Tier 4: Champion - Capstone

**👑 Avatar of Earth** (Ultimate Power)
- **ID:** `gaia_avatar_of_earth`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Mountain Guard AND Lifebloom
- **Effects:**
  - +25% armor
  - +35% max health
  - +30% healing effectiveness
  - +15% melee damage
  - 15% damage reduction
  - Earthen aura protects and heals
- **Description:** Embody the eternal earth. Immortal guardian.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**🌱 Earthwardens**
- **ID:** `gaia_earthwardens`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +10% max health
  - +8% armor
- **Description:** Your congregation stands as guardians of the earth.

#### Tier 2: Established

**🏰 Living Fortress**
- **ID:** `gaia_living_fortress`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** Earthwardens
- **Effects (All Members):**
  - +15% max health
  - +12% armor
  - +10% healing effectiveness
- **Description:** United, you become an impenetrable fortress.

#### Tier 3: Renowned

**🌳 Nature's Wrath**
- **ID:** `gaia_natures_wrath`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Living Fortress
- **Effects (All Members):**
  - +20% max health
  - +18% armor
  - +15% healing effectiveness
  - +12% melee damage
- **Description:** Nature defends its own with fury.

#### Tier 4: Legendary

**🌲 World Tree**
- **ID:** `gaia_world_tree`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Nature's Wrath
- **Effects (All Members):**
  - +30% max health
  - +25% armor
  - +22% healing effectiveness
  - +18% melee damage
  - Massive regeneration aura
- **Description:** Your religion becomes the eternal world tree.

---

## Vex (Madness)

**Theme:** Chaos, unpredictability, hybrid offense/defense, random effects

**Playstyle:** Wildcard - Balanced stats, chaotic effects

### Player Blessings (6)

#### Tier 1: Initiate

**🌀 Maddening Whispers**
- **ID:** `vex_maddening_whispers`
- **Rank Required:** Initiate (0 favor)
- **Prerequisites:** None
- **Effects:**
  - +12% melee damage
  - +12% ranged damage
  - +10% attack speed
  - Chance to confuse enemies
- **Description:** Madness whispers through your strikes.

---

#### Tier 2: Disciple - Choose Your Path

**💥 Chaotic Fury** (Offense Path)
- **ID:** `vex_chaotic_fury`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Maddening Whispers
- **Effects:**
  - +18% melee damage
  - +18% ranged damage
  - +15% attack speed
  - Random damage spikes
- **Description:** Unleash unpredictable chaos. High burst variance.

**🛡️ Delirium Shield** (Defense Path)
- **ID:** `vex_delirium_shield`
- **Rank Required:** Disciple (500 favor)
- **Prerequisites:** Maddening Whispers
- **Effects:**
  - +18% max health
  - +15% armor
  - Chance to dodge attacks
- **Description:** Madness protects the insane.

---

#### Tier 3: Zealot - Specialization

**🔥 Pandemonium** (Offense Specialization)
- **ID:** `vex_pandemonium`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Chaotic Fury
- **Effects:**
  - +28% melee damage
  - +28% ranged damage
  - +22% attack speed
  - Attacks cause confusion and fear
- **Description:** Spread chaos with every strike.

**🧠 Mind Fortress** (Defense Specialization)
- **ID:** `vex_mind_fortress`
- **Rank Required:** Zealot (2,000 favor)
- **Prerequisites:** Delirium Shield
- **Effects:**
  - +28% max health
  - +25% armor
  - +15% healing effectiveness
  - Immune to confusion
- **Description:** Only the mad are truly sane. Perfect mental defense.

---

#### Tier 4: Champion - Capstone

**👑 Avatar of Madness** (Ultimate Power)
- **ID:** `vex_avatar_of_madness`
- **Rank Required:** Champion (5,000 favor)
- **Prerequisites:** Pandemonium AND Mind Fortress
- **Effects:**
  - +20% melee damage
  - +20% ranged damage
  - +20% armor
  - +20% max health
  - +18% attack speed
  - +15% movement speed
  - Chaos aura disrupts enemies
  - Random devastating effects
- **Description:** Embody pure insanity. Reality warps around you.

---

### Religion Blessings (4)

#### Tier 1: Fledgling

**😵 Cult of Chaos**
- **ID:** `vex_cult_of_chaos`
- **Rank Required:** Fledgling (0 prestige)
- **Prerequisites:** None
- **Effects (All Members):**
  - +10% melee damage
  - +10% ranged damage
  - +8% attack speed
- **Description:** Your congregation embraces beautiful madness.

#### Tier 2: Established

**🌀 Shared Madness**
- **ID:** `vex_shared_madness`
- **Rank Required:** Established (500 prestige)
- **Prerequisites:** Cult of Chaos
- **Effects (All Members):**
  - +15% melee damage
  - +15% ranged damage
  - +12% attack speed
  - +10% movement speed
- **Description:** Madness spreads through the congregation.

#### Tier 3: Renowned

**💫 Insanity Aura**
- **ID:** `vex_insanity_aura`
- **Rank Required:** Renowned (2,000 prestige)
- **Prerequisites:** Shared Madness
- **Effects (All Members):**
  - +20% melee damage
  - +20% ranged damage
  - +18% attack speed
  - +15% movement speed
  - +12% armor
- **Description:** Your presence spreads chaos.

#### Tier 4: Legendary

**🌪️ Realm of Madness**
- **ID:** `vex_realm_of_madness`
- **Rank Required:** Legendary (5,000 prestige)
- **Prerequisites:** Insanity Aura
- **Effects (All Members):**
  - +28% melee damage
  - +28% ranged damage
  - +25% attack speed
  - +22% movement speed
  - +18% armor
  - +15% max health
  - Chaos reigns
- **Description:** Your religion warps reality itself.

---

## Progression Guide

### Typical Blessing Unlock Path

**Early Game (0-500 favor):**
1. Join a religion serving your chosen deity
2. Unlock Tier 1 player blessing (foundational bonus)
3. Help religion unlock Tier 1 religion blessing (group bonus)
4. Earn 500 favor through PvP combat

**Mid Game (500-2,000 favor):**
1. Decide on your path (offense vs defense vs utility)
2. Unlock Tier 2 player blessing matching your playstyle
3. Help religion reach 500 prestige for Tier 2 religion blessing
4. Earn 2,000 favor for Tier 3 access

**Late Game (2,000-5,000 favor):**
1. Unlock Tier 3 specialization blessing (doubling down on your path)
2. Consider unlocking the OTHER Tier 2 path as well (for capstone later)
3. Unlock other Tier 3 specialization blessing
4. Help religion reach 5,000 prestige for Tier 4 religion blessing
5. Earn 5,000 favor for Champion rank

**End Game (5,000+ favor):**
1. Unlock Tier 4 capstone blessing (requires BOTH Tier 3 blessings)
2. Enjoy maximum power with all 6 player + 4 religion blessings
3. Continue earning favor for future content (Avatar tier)

### Favor Farming Tips

**Efficient Favor Gain:**
- Kill enemy players (10 favor base, modified by deity relationship)
- Target rival deity followers for 2x favor (20 instead of 10)
- Avoid same-deity or allied-deity for better rewards
- Death penalty is only 5 favor - take risks!

**PvP Strategies:**
- Group with religion members for safety and teamwork
- Use terrain and positioning to your advantage
- Learn your deity's strengths and exploit enemy weaknesses
- Practice kiting if ranged, practice gap-closing if melee

---

## Build Recommendations

### Khoras (War)

**Pure Tank Build:**
- Iron Skin → Unbreakable → (unlock Bloodlust/Berserker for capstone) → Avatar of War
- Max survivability, wade into combat fearlessly

**Berserker Build:**
- Bloodlust → Berserker Rage → (unlock Iron Skin/Unbreakable for capstone) → Avatar of War
- High damage with lifesteal sustain, glass cannon who heals

**Balanced Warrior:**
- Both paths equally, rush to Avatar of War
- Best overall performance, well-rounded

---

### Lysa (Hunt)

**Sniper Build:**
- Deadly Precision → Master Huntress → (unlock mobility blessings for capstone) → Avatar of Hunt
- Maximum burst damage with crits, headshot hunter

**Speed Demon Build:**
- Silent Stalker → Apex Predator → (unlock precision blessings for capstone) → Avatar of Hunt
- Untouchable mobility, chase down fleeing enemies

**Hybrid Hunter:**
- Both paths equally, balance damage and mobility
- Versatile hunter, good at kiting and bursting

---

### Morthen (Death)

**Lifesteal Vampire:**
- Soul Reaper → Plague Bearer → (unlock tank blessings for capstone) → Lord of Death
- Sustain through lifesteal, spread poison, outlast everyone

**Unkillable Tank:**
- Undying → Deathless → (unlock lifesteal blessings for capstone) → Lord of Death
- Maximum HP/armor/regen, immortal warrior

**Death Knight:**
- Both paths, ultimate sustain fighter
- Cannot be killed, heals through everything

---

### Aethra (Light)

**Holy Damage:**
- Radiant Strike → Purifying Light → (unlock defense for capstone) → Avatar of Light
- Smite enemies while healing, offensive paladin

**Divine Tank:**
- Blessed Shield → Aegis of Light → (unlock offense for capstone) → Avatar of Light
- Invincible with healing, guardian paladin

**Support Cleric:**
- Both paths, focus on healing effectiveness
- Best healer in game, support allies

---

### Umbros (Shadows)

**Assassin Build:**
- Assassinate → Deadly Ambush → (unlock mobility for capstone) → Avatar of Shadows
- Maximum burst, delete enemies from stealth

**Ninja Build:**
- Phantom Dodge → Vanish → (unlock offense for capstone) → Avatar of Shadows
- Never get hit, perfect evasion

**Shadow Master:**
- Both paths, ultimate stealth killer
- Best assassin, untouchable and deadly

---

### Tharos (Storms)

**Lightning Mage:**
- Lightning Strike → Thunderlord → (unlock mobility for capstone) → Avatar of Storms
- AoE destruction, clear packs instantly

**Storm Chaser:**
- Storm Rider → Tempest → (unlock offense for capstone) → Avatar of Storms
- Impossible to catch, kite forever

**Battlemage:**
- Both paths, mobile AoE damage
- Best at group fights, devastating storms

---

### Gaia (Earth)

**Stone Wall:**
- Stone Form → Mountain Guard → (unlock regen for capstone) → Avatar of Earth
- Immovable defense, never die

**Life Tank:**
- Nature's Blessing → Lifebloom → (unlock defense for capstone) → Avatar of Earth
- Regenerate everything, outlast all

**Earth Guardian:**
- Both paths, immortal tank
- Highest effective HP in game, unkillable

---

### Vex (Madness)

**Chaos Damage:**
- Chaotic Fury → Pandemonium → (unlock defense for capstone) → Avatar of Madness
- Unpredictable burst, high variance

**Mad Tank:**
- Delirium Shield → Mind Fortress → (unlock offense for capstone) → Avatar of Madness
- Tanky with chaos effects

**Pure Insanity:**
- Both paths, embrace the chaos
- Jack of all trades, master of chaos

---

## Frequently Asked Questions

**Q: Can I respec blessings?**
A: No, blessings are permanent once unlocked. Choose carefully!

**Q: What happens if I switch religions?**
A: You lose all blessings and start over with a 7-day cooldown on switching.

**Q: Can I unlock blessings for multiple deities?**
A: No, you can only unlock blessings for your current religion's deity.

**Q: Do religion blessings affect offline members?**
A: Yes, when they log in they get the buffs immediately.

**Q: Can I unlock all blessings?**
A: Yes! You can unlock all 6 player + 4 religion blessings (10 total).

**Q: What if my religion founder leaves?**
A: Founder transfers to next oldest member automatically.

**Q: Do blessings stack with equipment bonuses?**
A: Yes, blessings apply before equipment calculations.

**Q: Are special effects implemented?**
A: Not yet. Stat modifiers work, special effects coming in future update.

---

**For more information, see:**
- [Implementation Guide](implementation_guide.md) - Development roadmap
- [Balance Testing Guide](balance_testing_guide.md) - How to test balance
- [Deity Reference](deity_reference.md) - Deity lore and relationships
- [Favor Reference](favor_reference.md) - Favor system details

---

**May your chosen deity guide you to victory!** ⚔️
