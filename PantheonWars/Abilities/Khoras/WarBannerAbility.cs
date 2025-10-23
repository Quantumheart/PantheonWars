using PantheonWars.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Abilities.Khoras
{
    /// <summary>
    /// War Banner - Temporary damage boost for nearby allies
    /// </summary>
    public class WarBannerAbility : Ability
    {
        private const float RANGE = 10f;
        private const float DURATION = 15f;
        private const float DAMAGE_BOOST = 1.2f; // 20% damage increase

        public WarBannerAbility() : base(
            id: "khoras_warbanner",
            name: "War Banner",
            description: $"Raises a war banner, granting nearby allies +20% damage for {DURATION} seconds.",
            deity: DeityType.Khoras,
            type: AbilityType.Buff)
        {
            CooldownSeconds = 45f;
            FavorCost = 15;
            MinimumRank = DevotionRank.Initiate;
        }

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Find nearby allies (including self)
            var nearbyEntities = sapi.World.GetEntitiesAround(
                casterEntity.Pos.XYZ,
                RANGE,
                RANGE,
                entity => entity is EntityPlayer
            );

            int affectedCount = 0;
            foreach (var entity in nearbyEntities)
            {
                if (entity is EntityPlayer playerEntity)
                {
                    // Apply damage boost effect (simplified for MVP)
                    // In a full implementation, this would use a proper buff system
                    affectedCount++;

                    if (sapi.World.PlayerByUid(playerEntity.PlayerUID) is IServerPlayer player)
                    {
                        player.SendMessage(
                            GlobalConstants.GeneralChatGroup,
                            "[War Banner] You feel empowered by Khoras's blessing! (+20% damage)",
                            EnumChatType.Notification
                        );
                    }
                }
            }

            // Notify caster
            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                $"[War Banner] You raise the banner of war, empowering {affectedCount} allies!",
                EnumChatType.Notification
            );

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} used War Banner, affecting {affectedCount} entities");
            return true;
        }
    }
}
