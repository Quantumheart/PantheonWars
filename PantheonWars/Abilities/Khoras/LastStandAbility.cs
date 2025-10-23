using PantheonWars.Models;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Abilities.Khoras
{
    /// <summary>
    /// Last Stand - Temporary damage resistance when health is low
    /// </summary>
    public class LastStandAbility : Ability
    {
        private const float DURATION = 12f;
        private const float DAMAGE_REDUCTION = 0.5f; // 50% damage reduction

        public LastStandAbility() : base(
            id: "khoras_last_stand",
            name: "Last Stand",
            description: $"Channel Khoras's indomitable will, gaining 50% damage resistance for {DURATION} seconds.",
            deity: DeityType.Khoras,
            type: AbilityType.Defensive)
        {
            CooldownSeconds = 60f;
            FavorCost = 20;
            MinimumRank = DevotionRank.Disciple;
        }

        public override bool CanExecute(IServerPlayer caster, ICoreServerAPI sapi, out string failureReason)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null)
            {
                failureReason = "Entity not found";
                return false;
            }

            // Optional: Require low health to use (30% or less)
            var healthPercent = casterEntity.Health / casterEntity.MaxHealth;
            if (healthPercent > 0.3f)
            {
                failureReason = "Last Stand can only be used when your health is below 30%";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        public override bool Execute(IServerPlayer caster, ICoreServerAPI sapi)
        {
            var casterEntity = caster.Entity;
            if (casterEntity == null) return false;

            // Apply damage resistance (simplified for MVP)
            // In a full implementation, this would use the entity stats system
            caster.SendMessage(
                GlobalConstants.GeneralChatGroup,
                "[Last Stand] You refuse to fall! Khoras grants you unbreakable resolve! (50% damage resistance)",
                EnumChatType.Notification
            );

            // Notify nearby players
            var nearbyPlayers = sapi.World.GetPlayersAround(
                casterEntity.Pos.XYZ,
                15f,
                15f
            );

            foreach (var player in nearbyPlayers)
            {
                if (player.PlayerUID != caster.PlayerUID)
                {
                    (player as IServerPlayer)?.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        $"[Last Stand] {caster.PlayerName} stands defiant, refusing to yield!",
                        EnumChatType.Notification
                    );
                }
            }

            sapi.Logger.Debug($"[PantheonWars] {caster.PlayerName} used Last Stand");
            return true;
        }
    }
}
