using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Systems.BuffSystem;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Abilities.Lysa
{
    /// <summary>
    /// Predator Instinct - Enhanced perception and critical hit chance
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PredatorInstinctAbility : Ability
    {
        private const float DURATION = 15f;
        private const float CRIT_CHANCE_INCREASE = 0.25f; // 25% increased crit chance
        private const float DETECTION_RANGE = 30f;

        public PredatorInstinctAbility() : base(
            id: "lysa_predator_instinct",
            name: "Predator Instinct",
            description: $"Sharpen your senses like a predator. Gain enhanced perception and 25% increased critical hit chance for {DURATION} seconds.",
            deity: DeityType.Lysa,
            type: AbilityType.Buff)
        {
            CooldownSeconds = 40f;
            FavorCost = 12;
            MinimumRank = DevotionRank.Disciple;
        }

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi, BuffManager buffManager = null)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Apply predator buff
            if (buffManager != null)
            {
                Dictionary<string, float> statModifiers = new Dictionary<string, float>
                {
                    // Add crit chance bonus (may not be natively supported by VS, but won't break anything)
                    { "critChance", CRIT_CHANCE_INCREASE },
                    // Add a small damage boost as well to ensure benefit even without crit support
                    { "meleeDamageMultiplier", 0.1f },
                    { "rangedDamageMultiplier", 0.1f }
                };

                buffManager.ApplyEffect(
                    casterEntity,
                    "predator_instinct_buff",
                    DURATION,
                    Id,
                    caster.PlayerUID,
                    statModifiers,
                    true
                );
            }

            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                $"[Predator Instinct] Your senses sharpen to razor focus for {DURATION} seconds! You can see your prey's every weakness. (+25% crit chance, +10% damage)",
                EnumChatType.Notification
            );

            // Show nearby entities to the player (enhanced perception)
            var nearbyEntities = sapi.World.GetEntitiesAround(
                casterEntity.Pos.XYZ,
                DETECTION_RANGE,
                DETECTION_RANGE,
                entity => entity != casterEntity && entity.Alive
            );

            int entityCount = 0;
            int playerCount = 0;
            int creatureCount = 0;

            foreach (var entity in nearbyEntities)
            {
                entityCount++;
                if (entity is EntityPlayer)
                {
                    playerCount++;
                }
                else
                {
                    creatureCount++;
                }
            }

            if (entityCount > 0)
            {
                string detectionMsg = $"[Predator Instinct] You sense {entityCount} living beings nearby";
                if (playerCount > 0 && creatureCount > 0)
                {
                    detectionMsg += $" ({playerCount} players, {creatureCount} creatures)";
                }
                else if (playerCount > 0)
                {
                    detectionMsg += $" ({playerCount} players)";
                }
                else
                {
                    detectionMsg += $" ({creatureCount} creatures)";
                }
                detectionMsg += "...";

                caster.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    detectionMsg,
                    EnumChatType.Notification
                );
            }
            else
            {
                caster.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    "[Predator Instinct] The area around you is quiet... no prey detected.",
                    EnumChatType.Notification
                );
            }

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} used Predator Instinct, detected {entityCount} entities");
            return true;
        }
    }
}
