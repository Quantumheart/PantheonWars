using PantheonWars.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace PantheonWars.Abilities.Lysa
{
    /// <summary>
    /// Hunter's Mark - Mark target to take extra damage
    /// </summary>
    public class HuntersMarkAbility : Ability
    {
        private const float RANGE = 20f;
        private const float DURATION = 20f;
        private const float DAMAGE_MULTIPLIER = 1.25f; // 25% extra damage

        public HuntersMarkAbility() : base(
            id: "lysa_hunters_mark",
            name: "Hunter's Mark",
            description: $"Mark your prey. Marked targets take 25% extra damage for {DURATION} seconds.",
            deity: DeityType.Lysa,
            type: AbilityType.Debuff)
        {
            CooldownSeconds = 25f;
            FavorCost = 10;
            MinimumRank = DevotionRank.Initiate;
        }

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Find target the player is looking at
            EntityAgent target = null;
            var lookVec = casterEntity.SidedPos.GetViewVector();
            var startPos = casterEntity.SidedPos.XYZ.Add(0, casterEntity.LocalEyePos.Y, 0);

            // Raycast to find target
            var entities = sapi.World.GetEntitiesAround(
                startPos,
                RANGE,
                RANGE,
                entity => entity != casterEntity && entity is EntityAgent
            );

            double closestDistance = double.MaxValue;
            foreach (var entity in entities)
            {
                if (entity is EntityAgent agent && agent.Alive)
                {
                    var toEntity = entity.Pos.XYZ.Sub(startPos).Normalize();
                    var dot = lookVec.Dot(toEntity);

                    // Check if entity is roughly in the direction player is looking (within 30 degrees)
                    if (dot > 0.866) // cos(30°) ≈ 0.866
                    {
                        var distance = startPos.DistanceTo(entity.Pos.XYZ);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            target = agent;
                        }
                    }
                }
            }

            if (target == null)
            {
                caster.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    "[Hunter's Mark] No valid target in range.",
                    EnumChatType.Notification
                );
                return false;
            }

            // Apply mark (simplified for MVP - just a notification)
            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                $"[Hunter's Mark] You mark your prey! Target will take 25% extra damage for {DURATION} seconds.",
                EnumChatType.Notification
            );

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} marked target with Hunter's Mark");
            return true;
        }
    }
}
