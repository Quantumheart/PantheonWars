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
    /// Swift Feet - Temporary movement speed boost
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SwiftFeetAbility : Ability
    {
        private const float DURATION = 8f;
        private const float SPEED_BOOST = 0.5f; // 50% faster movement

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

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi, BuffManager buffManager = null)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Apply movement speed buff
            if (buffManager != null)
            {
                Dictionary<string, float> statModifiers = new Dictionary<string, float>
                {
                    { "walkspeed", SPEED_BOOST }
                };

                buffManager.ApplyEffect(
                    casterEntity,
                    "swift_feet_buff",
                    DURATION,
                    Id,
                    caster.PlayerUID,
                    statModifiers,
                    true
                );
            }

            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                $"[Swift Feet] Lysa blesses you with incredible agility for {DURATION} seconds! (+50% movement speed)",
                EnumChatType.Notification
            );

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} used Swift Feet");
            return true;
        }
    }
}
