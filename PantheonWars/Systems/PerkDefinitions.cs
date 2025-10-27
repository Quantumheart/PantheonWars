using System.Collections.Generic;
using PantheonWars.Constants;
using PantheonWars.Models;
using Vintagestory.GameContent;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Contains all perk definitions for all deities (Phase 3.4)
    /// Total: 160 perks (8 deities × 20 perks each)
    /// </summary>
    public static class PerkDefinitions
    {
        /// <summary>
        /// Gets all perk definitions for registration
        /// </summary>
        public static List<Perk> GetAllPerks()
        {
            var perks = new List<Perk>();

            perks.AddRange(GetKhorasPerks());
            perks.AddRange(GetLysaPerks());
            perks.AddRange(GetMorthenPerks());
            perks.AddRange(GetAethraPerks());
            perks.AddRange(GetUmbrosPerks());
            perks.AddRange(GetTharosPerks());
            perks.AddRange(GetGaiaPerks());
            perks.AddRange(GetVexPerks());

            return perks;
        }

        #region Khoras (War) - 10 Perks (Refactored)

        private static List<Perk> GetKhorasPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Streamlined for meaningful choices

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.KhorasWarriorsResolve, "Warrior's Resolve", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Type = EnumTraitType.Positive,
                    Category = PerkCategory.Combat,
                    Description = "Your devotion to war strengthens body and blade. +10% melee damage, +10% max health.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.10f }
                    }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.KhorasBloodlust, "Bloodlust", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Embrace the rage of battle. +15% melee damage, +10% attack speed. Offense path. Requires Warrior's Resolve.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasWarriorsResolve },
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    }
                },
                new Perk(PerkIds.KhorasIronSkin, "Iron Skin", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Battle hardens your body. +20% armor, +15% max health. Defense path. Requires Warrior's Resolve.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasWarriorsResolve },
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f }
                    }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.KhorasBerserkerRage, "Berserker Rage", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Unleash devastating fury with lifesteal. +25% melee damage, +15% attack speed, heal 10% of damage dealt. Requires Bloodlust.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasBloodlust },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.25f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.Lifesteal10 }
                },
                new Perk(PerkIds.KhorasUnbreakable, "Unbreakable", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Become nearly invincible. +30% armor, +25% max health, 10% damage reduction. Requires Iron Skin.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasIronSkin },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.30f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.25f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DamageReduction10 }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.KhorasAvatarOfWar, "Avatar of War", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Embody war itself. +15% to all combat stats, +10% movement speed, AoE cleave attacks. Requires both Berserker Rage and Unbreakable.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasBerserkerRage, PerkIds.KhorasUnbreakable },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.WalkSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.AoeCleave }
                },

                // RELIGION PERKS (4 total) - Unified group buffs

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.KhorasWarBanner, "War Banner", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your congregation's banner inspires strength and courage. +8% melee damage, +8% max health for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponsDamage, 0.08f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.08f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.KhorasLegionTactics, "Legion Tactics", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Coordinated warfare. +12% melee damage, +10% armor, +5% attack speed for all. Requires War Banner.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasWarBanner },
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.10f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.05f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Force
                new Perk(PerkIds.KhorasWarhost, "Warhost", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Elite fighting force. +18% melee damage, +15% armor, +15% max health, +10% attack speed for all. Requires Legion Tactics.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasLegionTactics },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - Unstoppable Army
                new Perk(PerkIds.KhorasPantheonOfWar, "Pantheon of War", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your religion becomes legendary. +25% melee damage, +20% armor, +20% max health, +15% attack speed, +8% movement speed for all. Group war cry ability. Requires Warhost.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.KhorasWarhost },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.25f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f },
                        { VintageStoryStats.WalkSpeed, 0.08f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.ReligionWarCry }
                }
            };
        }

        #endregion

        #region Lysa (Hunt) - 10 Perks (Refactored)

        private static List<Perk> GetLysaPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Streamlined for meaningful choices

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.LysaKeenEye, "Keen Eye", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "The hunt sharpens your senses. +10% ranged damage, +10% movement speed.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.RangedWeaponsDamage, 0.10f },
                        { VintageStoryStats.WalkSpeed, 0.10f }
                    }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.LysaDeadlyPrecision, "Deadly Precision", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Perfect your aim. +15% ranged damage, +10% critical chance. Precision path. Requires Keen Eye.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.LysaKeenEye },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.RangedWeaponsDamage, 0.15f } },
                    SpecialEffects = new List<string> { SpecialEffects.CriticalChance10 }
                },
                new Perk(PerkIds.LysaSilentStalker, "Silent Stalker", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Move like a shadow. +18% movement speed, +10% melee damage. Mobility path. Requires Keen Eye.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.LysaKeenEye },
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.WalkSpeed, 0.18f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.StealthBonus }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.LysaMasterHuntress, "Master Huntress", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Legendary marksmanship. +25% ranged damage, +20% critical chance, headshot bonus. Requires Deadly Precision.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.LysaDeadlyPrecision },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.RangedWeaponsDamage, 0.25f } },
                    SpecialEffects = new List<string> { SpecialEffects.CriticalChance20, SpecialEffects.HeadshotBonus }
                },
                new Perk(PerkIds.LysaApexPredator, "Apex Predator", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Untouchable hunter. +28% movement speed, +18% melee damage, +15% attack speed. Requires Silent Stalker.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.LysaSilentStalker },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.28f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.TrackingVision }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.LysaAvatarOfHunt, "Avatar of the Hunt", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Embody the perfect hunter. +15% all damage, +20% movement speed, +10% attack speed, multishot ability. Requires both Master Huntress and Apex Predator.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.LysaMasterHuntress, PerkIds.LysaApexPredator },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.WalkSpeed, 0.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.Multishot, SpecialEffects.AnimalCompanion }
                },

                // RELIGION PERKS (4 total) - Unified pack buffs

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.LysaPackHunters, "Pack Hunters", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your pack hunts as one. +8% ranged damage, +8% movement speed for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.RangedWeaponsDamage, 0.08f },
                        { VintageStoryStats.WalkSpeed, 0.08f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.LysaCoordinatedStrike, "Coordinated Strike", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Coordinated hunting. +12% ranged damage, +10% melee damage, +10% movement speed for all. Requires Pack Hunters.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.LysaPackHunters },
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.RangedWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f },
                        { VintageStoryStats.WalkSpeed, 0.10f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Pack
                new Perk(PerkIds.LysaApexPack, "Apex Pack", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Elite hunting force. +18% ranged damage, +15% melee damage, +15% movement speed, +10% attack speed for all. Requires Coordinated Strike.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.LysaCoordinatedStrike },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.WalkSpeed, 0.15f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - Perfect Pack
                new Perk(PerkIds.LysaHuntersParadise, "Hunter's Paradise", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your congregation becomes unstoppable predators. +25% ranged damage, +20% melee damage, +22% movement speed, +15% attack speed for all. Pack tracking ability. Requires Apex Pack.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.LysaApexPack },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.25f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.WalkSpeed, 0.22f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.ReligionPackTracking }
                }
            };
        }

        #endregion

        #region Morthen (Death) - 10 Perks (Refactored)

        private static List<Perk> GetMorthenPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Streamlined for meaningful choices

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.MorthenDeathsEmbrace, "Death's Embrace", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Death empowers your strikes and body. +10% melee damage, +10% max health, minor lifesteal.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.Lifesteal3 }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.MorthenSoulReaper, "Soul Reaper", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Harvest souls with dark magic. +15% melee damage, +10% lifesteal, attacks apply poison. Offense path. Requires Death's Embrace.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenDeathsEmbrace },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.15f } },
                    SpecialEffects = new List<string> { SpecialEffects.Lifesteal10, SpecialEffects.PoisonDot }
                },
                new Perk(PerkIds.MorthenUndying, "Undying", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Resist death itself. +20% max health, +15% armor, +10% health regeneration. Defense path. Requires Death's Embrace.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenDeathsEmbrace },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.20f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.HealingEffectiveness, 0.10f }
                    }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.MorthenPlagueBearer, "Plague Bearer", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Spread pestilence and decay. +25% melee damage, +15% lifesteal, plague aura weakens enemies. Requires Soul Reaper.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenSoulReaper },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.25f } },
                    SpecialEffects = new List<string> { SpecialEffects.Lifesteal15, SpecialEffects.PoisonDotStrong, SpecialEffects.PlagueAura }
                },
                new Perk(PerkIds.MorthenDeathless, "Deathless", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Transcend mortality. +30% max health, +25% armor, +20% health regen, death resistance. Requires Undying.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenUndying },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.30f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.25f },
                        { VintageStoryStats.HealingEffectiveness, 0.20f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DamageReduction10 }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.MorthenLordOfDeath, "Lord of Death", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Command death itself. +15% all stats, +10% attack speed, death aura, execute low health enemies. Requires both Plague Bearer and Deathless.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenPlagueBearer, PerkIds.MorthenDeathless },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f },
                        { VintageStoryStats.HealingEffectiveness, 0.15f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DeathAura, SpecialEffects.ExecuteThreshold, SpecialEffects.Lifesteal20 }
                },

                // RELIGION PERKS (4 total) - Unified death cult progression

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.MorthenDeathCult, "Death Cult", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your congregation embraces the darkness. +8% melee damage, +8% max health for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponsDamage, 0.08f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.08f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.MorthenNecromanticCovenant, "Necromantic Covenant", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Dark pact strengthens all. +12% melee damage, +10% armor, +8% health regen for all. Requires Death Cult.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenDeathCult },
                    StatModifiers = new Dictionary<string, float> 
                    { 
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.10f },
                        { VintageStoryStats.HealingEffectiveness, 0.08f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Force
                new Perk(PerkIds.MorthenDeathlessLegion, "Deathless Legion", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Unkillable army of the dead. +18% melee damage, +15% armor, +15% max health, +12% regen for all. Requires Necromantic Covenant.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenNecromanticCovenant },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.HealingEffectiveness, 0.12f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - Death's Empire
                new Perk(PerkIds.MorthenEmpireOfDeath, "Empire of Death", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your religion rules over death itself. +25% melee damage, +20% armor, +20% max health, +18% regen, +10% attack speed for all. Death mark ability. Requires Deathless Legion.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.MorthenDeathlessLegion },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.25f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.20f },
                        { VintageStoryStats.HealingEffectiveness, 0.18f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.ReligionDeathMark }
                }
            };
        }

        #endregion

        #region Aethra (Light) - 20 Perks

        private static List<Perk> GetAethraPerks()
        {
            // Implementing 20 perks for Aethra - Light deity
            return new List<Perk> { };
        }

        #endregion

        #region Umbros (Shadows) - 20 Perks

        private static List<Perk> GetUmbrosPerks()
        {
            // Implementing 20 perks for Umbros - Shadows deity
            return new List<Perk> { };
        }

        #endregion

        #region Tharos (Storms) - 20 Perks

        private static List<Perk> GetTharosPerks()
        {
            // Implementing 20 perks for Tharos - Storms deity
            return new List<Perk> { };
        }

        #endregion

        #region Gaia (Earth) - 20 Perks

        private static List<Perk> GetGaiaPerks()
        {
            // Implementing 20 perks for Gaia - Earth deity
            return new List<Perk> { };
        }

        #endregion

        #region Vex (Madness) - 20 Perks

        private static List<Perk> GetVexPerks()
        {
            // Implementing 20 perks for Vex - Madness deity
            return new List<Perk> { };
        }

        #endregion
    }
}
