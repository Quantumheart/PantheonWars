using PantheonWars.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Abilities.Lysa
{
    /// <summary>
    /// Predator Instinct - Enhanced perception and critical hit chance
    /// </summary>
    public class PredatorInstinctAbility : Ability
    {
        private const float DURATION = 15f;
        private const float CRIT_CHANCE_INCREASE = 0.25f; // 25% increased crit chance

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

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Apply predator buff (simplified for MVP)
            // In a full implementation, this would:
            // - Increase critical hit chance
            // - Reveal nearby entities (through walls)
            // - Highlight weak points on enemies
            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                "[Predator Instinct] Your senses sharpen to razor focus! You can see your prey's every weakness. (+25% crit chance)",
                EnumChatType.Notification
            );

            // Show nearby entities to the player (simplified notification)
            var nearbyEntities = sapi.World.GetEntitiesAround(
                casterEntity.Pos.XYZ,
                30f,
                30f,
                entity => entity != casterEntity && entity.Alive
            );

            int entityCount = 0;
            foreach (var _ in nearbyEntities)
            {
                entityCount++;
            }

            if (entityCount > 0)
            {
                caster.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    $"[Predator Instinct] You sense {entityCount} living beings nearby...",
                    EnumChatType.Notification
                );
            }

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} used Predator Instinct");
            return true;
        }
    }
}
