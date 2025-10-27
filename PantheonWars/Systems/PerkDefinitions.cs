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

        #region Khoras (War) - 20 Perks

        private static List<Perk> GetKhorasPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (10 total)

                // Tier 1 - Initiate (0-499 favor)
                new Perk("khoras_warriors_resolve", "Warrior's Resolve", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Type = EnumTraitType.Positive,
                    Category = PerkCategory.Combat,
                    Description = "Your devotion to war strengthens your strikes. +5% melee damage.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.05f } }
                },
                new Perk("khoras_battle_endurance", "Battle Endurance", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Type = EnumTraitType.Positive,
                    Category = PerkCategory.Defense,
                    Description = "Your body adapts to the rigors of combat. +10% max health.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MaxHealthExtraPoints, 1.10f } }
                },

                // Tier 2 - Disciple (500-1999 favor)
                new Perk("khoras_bloodlust", "Bloodlust", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Each strike fuels your rage. +10% melee damage. Requires Warrior's Resolve.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { "khoras_warriors_resolve" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.10f } }
                },
                new Perk("khoras_iron_skin", "Iron Skin", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Battle hardens your flesh. +15% armor rating. Requires Battle Endurance.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { "khoras_battle_endurance" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponArmor, 0.15f } }
                },

                // Tier 3 - Zealot (2000-4999 favor)
                new Perk("khoras_berserker_rage", "Berserker Rage", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Unleash devastating fury. +15% melee damage, +5% attack speed. Requires Bloodlust.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { "khoras_bloodlust" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.05f }
                    }
                },
                new Perk("khoras_war_veteran", "War Veteran", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Survive countless battles. +15% max health, +10% armor. Requires Iron Skin.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { "khoras_iron_skin" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.15f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.10f }
                    }
                },

                // Tier 4 - Champion (5000-9999 favor)
                new Perk("khoras_weapon_master", "Weapon Master", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Master all weapons of war. +20% melee damage, +10% attack speed. Requires Berserker Rage.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { "khoras_berserker_rage" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { "critical_strike_chance" }
                },
                new Perk("khoras_unbreakable", "Unbreakable", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Become nearly invincible. +25% max health, +15% armor. Requires War Veteran.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { "khoras_war_veteran" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.25f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f }
                    }
                },

                // Tier 5 - Avatar (10000+ favor)
                new Perk("khoras_avatar_of_war", "Avatar of War", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Embody war itself. +30% melee damage, +15% attack speed, +10% movement speed. Requires Weapon Master.",
                    RequiredFavorRank = (int)FavorRank.Avatar,
                    PrerequisitePerks = new List<string> { "khoras_weapon_master" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.30f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.15f },
                        { VintageStoryStats.WalkSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { "lifesteal", "aoe_cleave" }
                },
                new Perk("khoras_immortal_warrior", "Immortal Warrior", DeityType.Khoras)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Death fears you. +35% max health, +20% armor, health regeneration. Requires Unbreakable.",
                    RequiredFavorRank = (int)FavorRank.Avatar,
                    PrerequisitePerks = new List<string> { "khoras_unbreakable" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.35f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.HealingEffectiveness, 0.50f }
                    },
                    SpecialEffects = new List<string> { "last_stand" }
                },

                // RELIGION PERKS (10 total)

                // Tier 1 - Fledgling (0-499 prestige)
                new Perk("khoras_congregation_strength", "Congregation's Strength", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "All members gain strength together. +3% melee damage for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.03f } }
                },
                new Perk("khoras_war_banner", "War Banner", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Your banner inspires courage. +5% max health for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MaxHealthExtraPoints, 1.05f } }
                },

                // Tier 2 - Established (500-1999 prestige)
                new Perk("khoras_legion_tactics", "Legion Tactics", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Coordinate attacks. +5% melee damage for all. Requires Congregation's Strength.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { "khoras_congregation_strength" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.05f } }
                },
                new Perk("khoras_fortress_faith", "Fortress Faith", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Collective defense. +8% max health for all. Requires War Banner.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { "khoras_war_banner" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MaxHealthExtraPoints, 1.08f } }
                },

                // Tier 3 - Renowned (2000-4999 prestige)
                new Perk("khoras_army_of_one", "Army of One", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Elite fighting force. +8% melee damage, +5% attack speed for all. Requires Legion Tactics.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { "khoras_legion_tactics" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.08f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.05f }
                    }
                },
                new Perk("khoras_shield_wall", "Shield Wall", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Impenetrable defense. +12% max health, +10% armor for all. Requires Fortress Faith.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { "khoras_fortress_faith" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.12f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.10f }
                    }
                },

                // Tier 4 - Legendary (5000-9999 prestige)
                new Perk("khoras_warhost", "Warhost", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Legendary warriors. +12% melee damage, +8% attack speed for all. Requires Army of One.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { "khoras_army_of_one" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.08f }
                    }
                },
                new Perk("khoras_iron_legion", "Iron Legion", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Unstoppable force. +18% max health, +15% armor for all. Requires Shield Wall.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { "khoras_shield_wall" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.18f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f }
                    }
                },

                // Tier 5 - Mythic (10000+ prestige)
                new Perk("khoras_pantheon_of_war", "Pantheon of War", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Gods of battle. +20% melee damage, +12% attack speed, +5% movement speed for all. Requires Warhost.",
                    RequiredPrestigeRank = (int)PrestigeRank.Mythic,
                    PrerequisitePerks = new List<string> { "khoras_warhost" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.12f },
                        { VintageStoryStats.WalkSpeed, 0.05f }
                    },
                    SpecialEffects = new List<string> { "religion_war_cry" }
                },
                new Perk("khoras_eternal_empire", "Eternal Empire", DeityType.Khoras)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Unkillable empire. +25% max health, +20% armor, health regen for all. Requires Iron Legion.",
                    RequiredPrestigeRank = (int)PrestigeRank.Mythic,
                    PrerequisitePerks = new List<string> { "khoras_iron_legion" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.25f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.HealingEffectiveness, 0.30f }
                    },
                    SpecialEffects = new List<string> { "religion_battle_standard" }
                }
            };
        }

        #endregion

        #region Lysa (Hunt) - 20 Perks

        private static List<Perk> GetLysaPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (10 total)

                // Tier 1 - Initiate
                new Perk("lysa_keen_eye", "Keen Eye", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Sharpen your aim. +5% ranged damage.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.RangedWeaponsDamage, 0.05f } }
                },
                new Perk("lysa_tracker", "Tracker", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Move swiftly through the wilderness. +8% movement speed.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.WalkSpeed, 0.08f } }
                },

                // Tier 2 - Disciple
                new Perk("lysa_hunters_focus", "Hunter's Focus", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Perfect accuracy. +10% ranged damage, +5% critical chance. Requires Keen Eye.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { "lysa_keen_eye" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.RangedWeaponsDamage, 0.10f } },
                    SpecialEffects = new List<string> { "critical_chance_5" }
                },
                new Perk("lysa_silent_stalker", "Silent Stalker", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Move like a ghost. +12% movement speed, reduced detection. Requires Tracker.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { "lysa_tracker" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.WalkSpeed, 0.12f } },
                    SpecialEffects = new List<string> { "stealth_bonus" }
                },

                // Tier 3 - Zealot
                new Perk("lysa_deadly_precision", "Deadly Precision", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Never miss. +15% ranged damage, +10% critical chance. Requires Hunter's Focus.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { "lysa_hunters_focus" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.RangedWeaponsDamage, 0.15f } },
                    SpecialEffects = new List<string> { "critical_chance_10" }
                },
                new Perk("lysa_predator", "Predator", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Become the apex hunter. +18% movement speed, +5% melee damage. Requires Silent Stalker.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { "lysa_silent_stalker" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.18f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.05f }
                    }
                },

                // Tier 4 - Champion
                new Perk("lysa_master_huntress", "Master Huntress", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Legendary marksmanship. +20% ranged damage, +15% critical chance, +5% attack speed. Requires Deadly Precision.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { "lysa_deadly_precision" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.05f }
                    },
                    SpecialEffects = new List<string> { "critical_chance_15", "headshot_bonus" }
                },
                new Perk("lysa_untamed", "Untamed", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "Wild and free. +25% movement speed, +10% melee damage. Requires Predator.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { "lysa_predator" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.25f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.10f }
                    }
                },

                // Tier 5 - Avatar
                new Perk("lysa_avatar_of_hunt", "Avatar of the Hunt", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Perfect hunter. +30% ranged damage, +25% critical chance, +10% attack speed. Requires Master Huntress.",
                    RequiredFavorRank = (int)FavorRank.Avatar,
                    PrerequisitePerks = new List<string> { "lysa_master_huntress" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.30f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { "critical_chance_25", "multishot", "tracking_vision" }
                },
                new Perk("lysa_nature_incarnate", "Nature Incarnate", DeityType.Lysa)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Mobility,
                    Description = "One with the wild. +35% movement speed, +15% melee damage, +10% max health. Requires Untamed.",
                    RequiredFavorRank = (int)FavorRank.Avatar,
                    PrerequisitePerks = new List<string> { "lysa_untamed" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.35f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.15f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.10f }
                    },
                    SpecialEffects = new List<string> { "animal_companion" }
                },

                // RELIGION PERKS (10 total)

                // Tier 1 - Fledgling
                new Perk("lysa_pack_hunters", "Pack Hunters", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Hunt as one. +3% ranged damage for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.RangedWeaponsDamage, 0.03f } }
                },
                new Perk("lysa_swift_pack", "Swift Pack", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Mobility,
                    Description = "Run together. +5% movement speed for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.WalkSpeed, 0.05f } }
                },

                // Tier 2 - Established
                new Perk("lysa_coordinated_strike", "Coordinated Strike", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Strike together. +5% ranged damage for all. Requires Pack Hunters.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { "lysa_pack_hunters" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.RangedWeaponsDamage, 0.05f } }
                },
                new Perk("lysa_pack_agility", "Pack Agility", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Mobility,
                    Description = "Agile collective. +8% movement speed for all. Requires Swift Pack.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { "lysa_swift_pack" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.WalkSpeed, 0.08f } }
                },

                // Tier 3 - Renowned
                new Perk("lysa_hunting_party", "Hunting Party", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Elite hunters. +8% ranged damage, +5% melee damage for all. Requires Coordinated Strike.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { "lysa_coordinated_strike" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.08f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.05f }
                    }
                },
                new Perk("lysa_wild_sprint", "Wild Sprint", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Mobility,
                    Description = "Lightning fast. +12% movement speed, +5% attack speed for all. Requires Pack Agility.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { "lysa_pack_agility" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.12f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.05f }
                    }
                },

                // Tier 4 - Legendary
                new Perk("lysa_apex_pack", "Apex Pack", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Unstoppable hunters. +12% ranged damage, +8% melee damage for all. Requires Hunting Party.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { "lysa_hunting_party" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.08f }
                    }
                },
                new Perk("lysa_cheetah_stride", "Cheetah Stride", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Mobility,
                    Description = "Blinding speed. +18% movement speed, +8% attack speed for all. Requires Wild Sprint.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { "lysa_wild_sprint" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.18f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.08f }
                    }
                },

                // Tier 5 - Mythic
                new Perk("lysa_hunters_paradise", "Hunter's Paradise", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Perfect hunt. +20% ranged damage, +12% melee damage, +10% attack speed for all. Requires Apex Pack.",
                    RequiredPrestigeRank = (int)PrestigeRank.Mythic,
                    PrerequisitePerks = new List<string> { "lysa_apex_pack" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.RangedWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponsDamage, 0.12f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { "religion_hunters_mark" }
                },
                new Perk("lysa_wild_gods", "Wild Gods", DeityType.Lysa)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Mobility,
                    Description = "Untouchable. +25% movement speed, +12% attack speed, +10% max health for all. Requires Cheetah Stride.",
                    RequiredPrestigeRank = (int)PrestigeRank.Mythic,
                    PrerequisitePerks = new List<string> { "lysa_cheetah_stride" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.WalkSpeed, 0.25f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.12f },
                        { VintageStoryStats.MaxHealthExtraPoints, 1.10f }
                    },
                    SpecialEffects = new List<string> { "religion_pack_bond" }
                }
            };
        }

        #endregion

        #region Morthen (Death) - 20 Perks

        private static List<Perk> GetMorthenPerks()
        {
            return new List<Perk>
            {
                // PLAYER PERKS (10 total) - Focus: Life drain, DoT, survivability

                // Tier 1 - Initiate
                new Perk("morthen_dark_touch", "Dark Touch", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Your strikes drain life. +5% melee damage.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.05f } },
                    SpecialEffects = new List<string> { "lifesteal_2percent" }
                },
                new Perk("morthen_death_resilience", "Death's Resilience", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Death empowers you. +8% max health.",
                    RequiredFavorRank = (int)FavorRank.Initiate,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MaxHealthExtraPoints, 1.08f } }
                },

                // Tier 2 - Disciple
                new Perk("morthen_soul_reaper", "Soul Reaper", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Harvest souls. +10% melee damage, increased lifesteal. Requires Dark Touch.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { "morthen_dark_touch" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.10f } },
                    SpecialEffects = new List<string> { "lifesteal_5percent" }
                },
                new Perk("morthen_undying", "Undying", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Resist death itself. +12% max health, +5% armor. Requires Death's Resilience.",
                    RequiredFavorRank = (int)FavorRank.Disciple,
                    PrerequisitePerks = new List<string> { "morthen_death_resilience" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.12f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.05f }
                    }
                },

                // Tier 3 - Zealot
                new Perk("morthen_plague_bearer", "Plague Bearer", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Spread disease. +15% melee damage, attacks apply poison. Requires Soul Reaper.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { "morthen_soul_reaper" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.15f } },
                    SpecialEffects = new List<string> { "lifesteal_5percent", "poison_dot" }
                },
                new Perk("morthen_deathless", "Deathless", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Cheat death. +18% max health, +10% armor, health regen. Requires Undying.",
                    RequiredFavorRank = (int)FavorRank.Zealot,
                    PrerequisitePerks = new List<string> { "morthen_undying" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.18f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.10f },
                        { VintageStoryStats.HealingEffectiveness, 0.20f }
                    }
                },

                // Tier 4 - Champion
                new Perk("morthen_death_lord", "Death Lord", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Command death. +20% melee damage, strong lifesteal. Requires Plague Bearer.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { "morthen_plague_bearer" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.20f } },
                    SpecialEffects = new List<string> { "lifesteal_10percent", "plague_aura", "weaken_enemy" }
                },
                new Perk("morthen_beyond_grave", "Beyond the Grave", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "Transcend mortality. +25% max health, +15% armor, strong regen. Requires Deathless.",
                    RequiredFavorRank = (int)FavorRank.Champion,
                    PrerequisitePerks = new List<string> { "morthen_deathless" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.25f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.15f },
                        { VintageStoryStats.HealingEffectiveness, 0.35f }
                    }
                },

                // Tier 5 - Avatar
                new Perk("morthen_avatar_of_death", "Avatar of Death", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Combat,
                    Description = "Become death incarnate. +30% melee damage, massive lifesteal. Requires Death Lord.",
                    RequiredFavorRank = (int)FavorRank.Avatar,
                    PrerequisitePerks = new List<string> { "morthen_death_lord" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.30f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.10f }
                    },
                    SpecialEffects = new List<string> { "lifesteal_15percent", "death_aura", "execute_low_health" }
                },
                new Perk("morthen_immortal", "Immortal", DeityType.Morthen)
                {
                    Kind = PerkKind.Player,
                    Category = PerkCategory.Defense,
                    Description = "True immortality. +35% max health, +20% armor, massive regen. Requires Beyond the Grave.",
                    RequiredFavorRank = (int)FavorRank.Avatar,
                    PrerequisitePerks = new List<string> { "morthen_beyond_grave" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.35f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.20f },
                        { VintageStoryStats.HealingEffectiveness, 0.50f }
                    },
                    SpecialEffects = new List<string> { "cheat_death_once" }
                },

                // RELIGION PERKS (10 total)

                // Tier 1 - Fledgling
                new Perk("morthen_cult_of_death", "Cult of Death", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Shared darkness. +3% melee damage for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.03f } }
                },
                new Perk("morthen_death_pact", "Death Pact", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Bound by death. +5% max health for all members.",
                    RequiredPrestigeRank = (int)PrestigeRank.Fledgling,
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MaxHealthExtraPoints, 1.05f } }
                },

                // Tier 2 - Established
                new Perk("morthen_necromantic_bond", "Necromantic Bond", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Dark communion. +5% melee damage for all. Requires Cult of Death.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { "morthen_cult_of_death" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.05f } }
                },
                new Perk("morthen_collective_undeath", "Collective Undeath", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Shared resilience. +8% max health for all. Requires Death Pact.",
                    RequiredPrestigeRank = (int)PrestigeRank.Established,
                    PrerequisitePerks = new List<string> { "morthen_death_pact" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MaxHealthExtraPoints, 1.08f } }
                },

                // Tier 3 - Renowned
                new Perk("morthen_plague_congregation", "Plague Congregation", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Spread pestilence. +8% melee damage for all. Requires Necromantic Bond.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { "morthen_necromantic_bond" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.08f } }
                },
                new Perk("morthen_deathless_army", "Deathless Army", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Unstoppable horde. +12% max health, +5% regen for all. Requires Collective Undeath.",
                    RequiredPrestigeRank = (int)PrestigeRank.Renowned,
                    PrerequisitePerks = new List<string> { "morthen_collective_undeath" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.12f },
                        { VintageStoryStats.HealingEffectiveness, 0.15f }
                    }
                },

                // Tier 4 - Legendary
                new Perk("morthen_death_legion", "Death Legion", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Army of the dead. +12% melee damage for all. Requires Plague Congregation.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { "morthen_plague_congregation" },
                    StatModifiers = new Dictionary<string, float> { { VintageStoryStats.MeleeWeaponsDamage, 0.12f } }
                },
                new Perk("morthen_undying_horde", "Undying Horde", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Unkillable legion. +18% max health, +10% regen for all. Requires Deathless Army.",
                    RequiredPrestigeRank = (int)PrestigeRank.Legendary,
                    PrerequisitePerks = new List<string> { "morthen_deathless_army" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.18f },
                        { VintageStoryStats.HealingEffectiveness, 0.25f }
                    }
                },

                // Tier 5 - Mythic
                new Perk("morthen_empire_of_death", "Empire of Death", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Combat,
                    Description = "Rule over death. +20% melee damage, +5% attack speed for all. Requires Death Legion.",
                    RequiredPrestigeRank = (int)PrestigeRank.Mythic,
                    PrerequisitePerks = new List<string> { "morthen_death_legion" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MeleeWeaponsDamage, 0.20f },
                        { VintageStoryStats.MeleeWeaponsSpeed, 0.05f }
                    },
                    SpecialEffects = new List<string> { "religion_death_mark" }
                },
                new Perk("morthen_eternal_undeath", "Eternal Undeath", DeityType.Morthen)
                {
                    Kind = PerkKind.Religion,
                    Category = PerkCategory.Defense,
                    Description = "Never die. +25% max health, +10% armor, +15% regen for all. Requires Undying Horde.",
                    RequiredPrestigeRank = (int)PrestigeRank.Mythic,
                    PrerequisitePerks = new List<string> { "morthen_undying_horde" },
                    StatModifiers = new Dictionary<string, float>
                    {
                        { VintageStoryStats.MaxHealthExtraPoints, 1.25f },
                        { VintageStoryStats.MeleeWeaponArmor, 0.10f },
                        { VintageStoryStats.HealingEffectiveness, 0.35f }
                    }
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
