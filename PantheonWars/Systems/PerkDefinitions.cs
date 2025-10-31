using System.Collections.Generic;
using PantheonWars.Constants;
using PantheonWars.Models;
using Vintagestory.GameContent;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Contains all perk definitions for all deities (Phase 3.4)
    /// Total: 80 perks (8 deities × 10 perks each)
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

        #region Aethra (Light) - 10 Perks (Refactored)

        private static List<Perk> GetAethraPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Divine protection and healing

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.AethraDivineGrace, "Divine Grace", DeityType.Aethra)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Utility,
                    Description = "The light blesses you with divine vitality. +10% max health, +12% healing effectiveness.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.10f },
                        { VintageStoryStats.HealingEffectiveness, 0.12f }
                    }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.AethraRadiantStrike, "Radiant Strike", DeityType.Aethra)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Your attacks radiate holy energy. +12% melee damage, +10% ranged damage, heal 5% on hit. Offense path. Requires Divine Grace.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.AethraDivineGrace },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.Lifesteal3 }
                },
                new Perk(PerkIds.AethraBlessedShield, "Blessed Shield", DeityType.Aethra)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Light shields you from harm. +18% armor, +15% max health, 8% damage reduction. Defense path. Requires Divine Grace.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.AethraDivineGrace },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.18f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DamageReduction10 }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.AethraPurifyingLight, "Purifying Light", DeityType.Aethra)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Unleash devastating holy power. +22% melee damage, +18% ranged damage, heal 12% on hit, AoE healing pulse. Requires Radiant Strike.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.AethraRadiantStrike },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.22f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.18f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.Lifesteal10 }
                },
                new Perk(PerkIds.AethraAegisOfLight, "Aegis of Light", DeityType.Aethra)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Become nearly invincible with divine protection. +28% armor, +25% max health, 15% damage reduction, +18% healing. Requires Blessed Shield.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.AethraBlessedShield },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.28f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.25f },
                        { VintageStoryStats.HealingEffectiveness, 0.18f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DamageReduction10 }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.AethraAvatarOfLight, "Avatar of Light", DeityType.Aethra)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Embody divine radiance. +15% all stats, +20% healing, radiant aura heals allies, smite enemies. Requires both Purifying Light and Aegis of Light.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.AethraPurifyingLight, PerkIds.AethraAegisOfLight },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.HealingEffectiveness, 0.20f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.Lifesteal15 }
                },

                // RELIGION PERKS (4 total) - Divine congregation

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.AethraBlessingOfLight, "Blessing of Light", DeityType.Aethra)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Utility,
                    Description = "Your congregation is blessed by divine light. +8% max health, +10% healing effectiveness for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.08f },
                        { VintageStoryStats.HealingEffectiveness, 0.10f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.AethraDivineSanctuary, "Divine Sanctuary", DeityType.Aethra)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Sacred protection shields all. +12% armor, +10% max health, +12% healing for all. Requires Blessing of Light.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.AethraBlessingOfLight },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.12f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.10f },
                        { VintageStoryStats.HealingEffectiveness, 0.12f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Force
                new Perk(PerkIds.AethraSacredBond, "Sacred Bond", DeityType.Aethra)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Divine unity empowers the congregation. +15% armor, +15% max health, +15% healing, +10% all damage for all. Requires Divine Sanctuary.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.AethraDivineSanctuary },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.HealingEffectiveness, 0.15f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.10f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - Divine Temple
                new Perk(PerkIds.AethraCathedralOfLight, "Cathedral of Light", DeityType.Aethra)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your religion becomes a beacon of divine power. +20% armor, +20% max health, +20% healing, +15% all damage, +8% movement for all. Divine sanctuary ability. Requires Sacred Bond.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.AethraSacredBond },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.20f },
                        { VintageStoryStats.HealingEffectiveness, 0.20f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.15f },
                        { VintageStoryStats.WalkSpeed, 0.08f }
                    }
                }
            };
        }

        #endregion

        #region Umbros (Shadows) - 10 Perks (Refactored)

        private static List<Perk> GetUmbrosPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Shadow assassin

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.UmbrosShadowBlend, "Shadow Blend", DeityType.Umbros)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Merge with shadows for speed and stealth. +15% movement speed, +10% melee damage, stealth bonus.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.15f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.StealthBonus }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.UmbrosAssassinate, "Assassinate", DeityType.Umbros)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Strike from darkness with lethal precision. +18% melee damage, +15% critical chance, backstab bonus. Offense path. Requires Shadow Blend.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosShadowBlend },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.CriticalChance10 }
                },
                new Perk(PerkIds.UmbrosPhantomDodge, "Phantom Dodge", DeityType.Umbros)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Become untouchable through shadows. +25% movement speed, +12% attack speed, enhanced evasion. Mobility path. Requires Shadow Blend.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosShadowBlend },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.25f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.12f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.StealthBonus }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.UmbrosDeadlyAmbush, "Deadly Ambush", DeityType.Umbros)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Master the art of assassination. +28% melee damage, +20% critical chance, +15% attack speed, execute low health enemies. Requires Assassinate.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosAssassinate },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.28f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.CriticalChance20, SpecialEffects.ExecuteThreshold }
                },
                new Perk(PerkIds.UmbrosVanish, "Vanish", DeityType.Umbros)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Disappear into shadows at will. +35% movement speed, +18% attack speed, +12% melee damage, near-invisibility. Requires Phantom Dodge.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosPhantomDodge },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.35f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.18f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.StealthBonus }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.UmbrosAvatarOfShadows, "Avatar of Shadows", DeityType.Umbros)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Become one with darkness. +20% melee damage, +30% movement speed, +20% attack speed, shadow clones, perfect stealth. Requires both Deadly Ambush and Vanish.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosDeadlyAmbush, PerkIds.UmbrosVanish },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.WalkSpeed, 0.30f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.20f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.StealthBonus, SpecialEffects.CriticalChance20 }
                },

                // RELIGION PERKS (4 total) - Shadow cult

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.UmbrosShadowCult, "Shadow Cult", DeityType.Umbros)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Mobility,
                    Description = "Your congregation moves through darkness. +10% movement speed, +8% melee damage for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.10f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.08f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.UmbrosCloak, "Cloak of Shadows", DeityType.Umbros)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Shadows shroud all members. +15% movement speed, +12% melee damage, +10% attack speed for all. Requires Shadow Cult.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosShadowCult },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.15f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Assassins
                new Perk(PerkIds.UmbrosNightAssassins, "Night Assassins", DeityType.Umbros)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Elite shadow assassins. +20% movement speed, +18% melee damage, +15% attack speed for all. Requires Cloak of Shadows.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosCloak },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.20f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - Shadow Empire
                new Perk(PerkIds.UmbrosEternalDarkness, "Eternal Darkness", DeityType.Umbros)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your religion commands the darkness. +28% movement speed, +25% melee damage, +20% attack speed for all. Shadow strike ability. Requires Night Assassins.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.UmbrosNightAssassins },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.28f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.25f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.20f }
                    }
                }
            };
        }

        #endregion

        #region Tharos (Storms) - 10 Perks (Refactored)

        private static List<Perk> GetTharosPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Storm master

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.TharosStormborn, "Stormborn", DeityType.Tharos)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Born of thunder and lightning. +10% ranged damage, +12% movement speed, shocking touch.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.10f },
                        { VintageStoryStats.WalkSpeed, 0.12f }
                    }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.TharosLightningStrike, "Lightning Strike", DeityType.Tharos)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Channel devastating lightning bolts. +18% ranged damage, +15% melee damage, chain lightning. Offense path. Requires Stormborn.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.TharosStormborn },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f }
                    }
                },
                new Perk(PerkIds.TharosStormRider, "Storm Rider", DeityType.Tharos)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Ride the winds of the storm. +22% movement speed, +12% attack speed, +10% all damage. Mobility path. Requires Stormborn.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.TharosStormborn },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.22f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.12f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.10f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f }
                    }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.TharosThunderlord, "Thunderlord", DeityType.Tharos)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Command the fury of thunder. +28% ranged damage, +22% melee damage, +15% attack speed, AoE lightning strikes. Requires Lightning Strike.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.TharosLightningStrike },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.28f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.22f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    }
                },
                new Perk(PerkIds.TharosTempest, "Tempest", DeityType.Tharos)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Become the eye of the storm. +32% movement speed, +18% attack speed, +15% all damage, whirlwind mobility. Requires Storm Rider.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.TharosStormRider },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.32f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.18f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f }
                    }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.TharosAvatarOfStorms, "Avatar of Storms", DeityType.Tharos)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Embody the storm itself. +20% all damage, +25% movement speed, +20% attack speed, permanent lightning aura, thunderbolt strike. Requires both Thunderlord and Tempest.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.TharosThunderlord, PerkIds.TharosTempest },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.WalkSpeed, 0.25f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.20f }
                    }
                },

                // RELIGION PERKS (4 total) - Storm callers

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.TharosStormCallers, "Storm Callers", DeityType.Tharos)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your congregation calls the storm. +8% ranged damage, +10% movement speed for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.08f },
                        { VintageStoryStats.WalkSpeed, 0.10f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.TharosLightningChain, "Lightning Chain", DeityType.Tharos)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Lightning chains between allies. +12% ranged damage, +10% melee damage, +12% movement speed for all. Requires Storm Callers.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.TharosStormCallers },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f },
                        { VintageStoryStats.WalkSpeed, 0.12f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Force
                new Perk(PerkIds.TharosThunderstorm, "Thunderstorm", DeityType.Tharos)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Unleash devastating thunderstorms. +18% ranged damage, +15% melee damage, +18% movement speed, +10% attack speed for all. Requires Lightning Chain.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.TharosLightningChain },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.WalkSpeed, 0.18f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - Storm's Wrath
                new Perk(PerkIds.TharosEyeOfTheStorm, "Eye of the Storm", DeityType.Tharos)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your religion commands the heavens. +25% ranged damage, +20% melee damage, +25% movement speed, +15% attack speed for all. Massive AoE lightning storm. Requires Thunderstorm.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.TharosThunderstorm },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.25f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.WalkSpeed, 0.25f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    }
                }
            };
        }

        #endregion

        #region Gaia (Earth) - 10 Perks (Refactored)

        private static List<Perk> GetGaiaPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Earth defender

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.GaiaEarthenResilience, "Earthen Resilience", DeityType.Gaia)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Earth's strength flows through you. +15% max health, +10% armor, +8% healing effectiveness.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.10f },
                        { VintageStoryStats.HealingEffectiveness, 0.08f }
                    }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.GaiaStoneForm, "Stone Form", DeityType.Gaia)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Become as unyielding as stone. +22% armor, +18% max health, 10% damage reduction. Defense path. Requires Earthen Resilience.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaEarthenResilience },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.22f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.18f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DamageReduction10 }
                },
                new Perk(PerkIds.GaiaNaturesBlessing, "Nature's Blessing", DeityType.Gaia)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Utility,
                    Description = "Nature restores you constantly. +20% max health, +18% healing effectiveness, slow passive regeneration. Regeneration path. Requires Earthen Resilience.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaEarthenResilience },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.20f },
                        { VintageStoryStats.HealingEffectiveness, 0.18f }
                    }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.GaiaMountainGuard, "Mountain Guard", DeityType.Gaia)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Stand immovable like a mountain. +32% armor, +28% max health, 15% damage reduction, +10% melee damage. Requires Stone Form.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaStoneForm },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.32f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.28f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DamageReduction10 }
                },
                new Perk(PerkIds.GaiaLifebloom, "Lifebloom", DeityType.Gaia)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Utility,
                    Description = "Life flourishes around you. +30% max health, +28% healing effectiveness, strong passive regeneration, heal nearby allies. Requires Nature's Blessing.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaNaturesBlessing },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.30f },
                        { VintageStoryStats.HealingEffectiveness, 0.28f }
                    }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.GaiaAvatarOfEarth, "Avatar of Earth", DeityType.Gaia)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Embody the eternal earth. +25% armor, +35% max health, +30% healing, 15% damage reduction, earthen aura protects and heals. Requires both Mountain Guard and Lifebloom.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaMountainGuard, PerkIds.GaiaLifebloom },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponArmor, 0.25f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.35f },
                        { VintageStoryStats.HealingEffectiveness, 0.30f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f }
                    },
                    SpecialEffects = new List<string> { SpecialEffects.DamageReduction10 }
                },

                // RELIGION PERKS (4 total) - Earth wardens

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.GaiaEarthwardens, "Earthwardens", DeityType.Gaia)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Your congregation stands as guardians of the earth. +10% max health, +8% armor for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.10f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.08f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.GaiaLivingFortress, "Living Fortress", DeityType.Gaia)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "United, you become an impenetrable fortress. +15% max health, +12% armor, +10% healing for all. Requires Earthwardens.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaEarthwardens },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.12f },
                        { VintageStoryStats.HealingEffectiveness, 0.10f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Force
                new Perk(PerkIds.GaiaNaturesWrath, "Nature's Wrath", DeityType.Gaia)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Nature defends its own with fury. +20% max health, +18% armor, +15% healing, +12% melee damage for all. Requires Living Fortress.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaLivingFortress },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.20f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.18f },
                        { VintageStoryStats.HealingEffectiveness, 0.15f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - World Tree
                new Perk(PerkIds.GaiaWorldTree, "World Tree", DeityType.Gaia)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Your religion becomes the eternal world tree. +30% max health, +25% armor, +22% healing, +18% melee damage for all. Massive regeneration aura. Requires Nature's Wrath.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.GaiaNaturesWrath },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.30f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.25f },
                        { VintageStoryStats.HealingEffectiveness, 0.22f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.18f }
                    }
                }
            };
        }

        #endregion

        #region Vex (Madness) - 10 Perks (Refactored)

        private static List<Perk> GetVexPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (6 total) - Chaos incarnate

                // Tier 1 - Initiate (0-499 favor) - Foundation
                new Perk(PerkIds.VexMaddeningWhispers, "Maddening Whispers", DeityType.Vex)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Madness whispers through your strikes. +12% all damage, +10% attack speed, chance to confuse enemies.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    }
                },

                // Tier 2 - Disciple (500-1999 favor) - Choose Your Path
                new Perk(PerkIds.VexChaoticFury, "Chaotic Fury", DeityType.Vex)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Unleash unpredictable chaos. +18% all damage, +15% attack speed, random damage spikes. Offense path. Requires Maddening Whispers.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.VexMaddeningWhispers },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.18f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.18f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f }
                    }
                },
                new Perk(PerkIds.VexDeliriumShield, "Delirium Shield", DeityType.Vex)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Madness protects the insane. +18% max health, +15% armor, chance to dodge attacks. Defense path. Requires Maddening Whispers.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { PerkIds.VexMaddeningWhispers },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.18f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f }
                    }
                },

                // Tier 3 - Zealot (2000-4999 favor) - Specialization
                new Perk(PerkIds.VexPandemonium, "Pandemonium", DeityType.Vex)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Spread chaos with every strike. +28% all damage, +22% attack speed, attacks cause confusion and fear. Requires Chaotic Fury.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.VexChaoticFury },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.28f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.28f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.22f }
                    }
                },
                new Perk(PerkIds.VexMindFortress, "Mind Fortress", DeityType.Vex)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Only the mad are truly sane. +28% max health, +25% armor, +15% healing, immune to confusion. Requires Delirium Shield.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { PerkIds.VexDeliriumShield },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.28f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.25f },
                        { VintageStoryStats.HealingEffectiveness, 0.15f }
                    }
                },

                // Tier 4 - Champion (5000-9999 favor) - Capstone (requires both paths)
                new Perk(PerkIds.VexAvatarOfMadness, "Avatar of Madness", DeityType.Vex)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Embody pure insanity. +20% all stats, +18% attack speed, chaos aura disrupts enemies, random devastating effects. Requires both Pandemonium and Mind Fortress.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { PerkIds.VexPandemonium, PerkIds.VexMindFortress },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.18f },
                        { VintageStoryStats.WalkSpeed, 0.15f }
                    }
                },

                // RELIGION PERKS (4 total) - Madness cult

                // Tier 1 - Fledgling (0-499 prestige) - Foundation
                new Perk(PerkIds.VexCultOfChaos, "Cult of Chaos", DeityType.Vex)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your congregation embraces beautiful madness. +10% all damage, +8% attack speed for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.10f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.08f }
                    }
                },

                // Tier 2 - Established (500-1999 prestige) - Coordination
                new Perk(PerkIds.VexSharedMadness, "Shared Madness", DeityType.Vex)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Madness spreads through the congregation. +15% all damage, +12% attack speed, +10% movement for all. Requires Cult of Chaos.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { PerkIds.VexCultOfChaos },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.12f },
                        { VintageStoryStats.WalkSpeed, 0.10f }
                    }
                },

                // Tier 3 - Renowned (2000-4999 prestige) - Elite Force
                new Perk(PerkIds.VexInsanityAura, "Insanity Aura", DeityType.Vex)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your presence spreads chaos. +20% all damage, +18% attack speed, +15% movement, +12% armor for all. Requires Shared Madness.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { PerkIds.VexSharedMadness },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.18f },
                        { VintageStoryStats.WalkSpeed, 0.15f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.12f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige) - Reality Breaks
                new Perk(PerkIds.VexRealmOfMadness, "Realm of Madness", DeityType.Vex)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Your religion warps reality itself. +28% all damage, +25% attack speed, +22% movement, +18% armor, +15% max health for all. Chaos reigns. Requires Insanity Aura.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { PerkIds.VexInsanityAura },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.28f },
                        { VintageStoryStats.RangedWeaponsDamage, 0.28f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.25f },
                        { VintageStoryStats.WalkSpeed, 0.22f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.18f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f }
                    }
                }
            };
        }

        #endregion
    }
}
