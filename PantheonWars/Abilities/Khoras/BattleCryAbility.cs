using PantheonWars.Models;
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
        private const float SPEED_MULTIPLIER = 1.3f; // 30% faster attacks

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

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Apply attack speed buff (simplified for MVP)
            // In a full implementation, this would modify actual attack speed stats
            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                "[Battle Cry] You unleash a fierce war cry! Your attacks strike with furious speed! (+30% attack speed)",
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
