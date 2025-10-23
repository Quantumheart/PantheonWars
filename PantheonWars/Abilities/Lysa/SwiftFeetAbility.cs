using PantheonWars.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Abilities.Lysa
{
    /// <summary>
    /// Swift Feet - Temporary movement speed boost
    /// </summary>
    public class SwiftFeetAbility : Ability
    {
        private const float DURATION = 8f;
        private const float SPEED_MULTIPLIER = 1.2f; // 20% faster movement

        public SwiftFeetAbility() : base(
            id: "lysa_swift_feet",
            name: "Swift Feet",
            description: $"Channel Lysa's grace, increasing your movement speed by 50% for {DURATION} seconds.",
            deity: DeityType.Lysa,
            type: AbilityType.Utility)
        {
            CooldownSeconds = 20f;
            FavorCost = 8;
            MinimumRank = DevotionRank.Initiate;
        }

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Apply speed boost (simplified for MVP)
            // In a full implementation, this would modify actual movement speed stats
            var playerStats = casterEntity.Stats;
            if (playerStats == null) return false;
            playerStats.Set("walkspeed", "lysa_swift_feet", SPEED_MULTIPLIER, false);
            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                "[Swift Feet] Lysa blesses you with incredible agility! (+20% movement speed)",
                EnumChatType.Notification
            );

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} used Swift Feet");
            return true;
        }
    }
}
