using System.Collections.Generic;
using PantheonWars.Models;
using PantheonWars.Systems.BuffSystem;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Abilities.Khoras
{
    /// <summary>
    /// Battle Cry - Short duration attack speed increase
    /// </summary>
    public class BattleCryAbility : Ability
    {
        private const float DURATION = 10f;
        private const float SPEED_BOOST = 0.3f; // 30% faster attacks

        public BattleCryAbility() : base(
            id: "khoras_battlecry",
            name: "Battle Cry",
            description: $"Release a mighty battle cry, increasing your attack speed by 30% for {DURATION} seconds.",
            deity: DeityType.Khoras,
            type: AbilityType.Buff)
        {
            CooldownSeconds = 30f;
            FavorCost = 10;
            MinimumRank = DevotionRank.Initiate;
        }

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi, BuffManager buffManager = null)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Apply attack speed buff
            if (buffManager != null)
            {
                Dictionary<string, float> statModifiers = new Dictionary<string, float>
                {
                    { "meleeWeaponsSpeed", SPEED_BOOST },
                    { "rangedWeaponsSpeed", SPEED_BOOST }
                };

                buffManager.ApplyEffect(
                    casterEntity,
                    "battle_cry_buff",
                    DURATION,
                    Id,
                    caster.PlayerUID,
                    statModifiers,
                    true
                );
            }

            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                $"[Battle Cry] You unleash a fierce war cry! Your attacks strike with furious speed for {DURATION} seconds! (+30% attack speed)",
                EnumChatType.Notification
            );

            // Visual/audio feedback: play sound to nearby players
            var nearbyPlayers = sapi.World.GetPlayersAround(
                casterEntity.Pos.XYZ,
                20f,
                20f
            );

            foreach (var player in nearbyPlayers)
            {
                if (player.PlayerUID != caster.PlayerUID)
                {
                    (player as IServerPlayer)?.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        $"[Battle Cry] {caster.PlayerName} releases a terrifying battle cry!",
                        EnumChatType.Notification
                    );
                }
            }

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} used Battle Cry");
            return true;
        }
    }
}
