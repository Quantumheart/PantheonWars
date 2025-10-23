using System;
using PantheonWars.Models;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Manages ability execution and validation
    /// </summary>
    public class AbilitySystem
    {
        private readonly ICoreServerAPI _sapi;
        private readonly AbilityRegistry _abilityRegistry;
        private readonly PlayerDataManager _playerDataManager;
        private readonly AbilityCooldownManager _cooldownManager;

        public AbilitySystem(
            ICoreServerAPI sapi,
            AbilityRegistry abilityRegistry,
            PlayerDataManager playerDataManager,
            AbilityCooldownManager cooldownManager)
        {
            _sapi = sapi;
            _abilityRegistry = abilityRegistry;
            _playerDataManager = playerDataManager;
            _cooldownManager = cooldownManager;
        }

        /// <summary>
        /// Initializes the ability system
        /// </summary>
        public void Initialize()
        {
            _sapi.Logger.Notification("[PantheonWars] Initializing Ability System...");
            _sapi.Logger.Notification("[PantheonWars] Ability System initialized");
        }

        /// <summary>
        /// Attempts to execute an ability for a player
        /// </summary>
        /// <param name="player">The player using the ability</param>
        /// <param name="abilityId">The ability ID to execute</param>
        /// <returns>True if ability was successfully executed, false otherwise</returns>
        public bool ExecuteAbility(IServerPlayer player, string abilityId)
        {
            // Validate player
            if (player == null || player.Entity == null)
            {
                return false;
            }

            // Get ability
            var ability = _abilityRegistry.GetAbility(abilityId);
            if (ability == null)
            {
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    $"[Ability] Unknown ability: {abilityId}",
                    EnumChatType.CommandError
                );
                return false;
            }

            // Get player data
            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            // Validation: Check if player has pledged to a deity
            if (!playerData.HasDeity())
            {
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    "[Ability] You must pledge to a deity before using abilities.",
                    EnumChatType.CommandError
                );
                return false;
            }

            // Validation: Check if ability belongs to player's deity
            if (ability.Deity != playerData.DeityType)
            {
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    $"[Ability] {ability.Name} does not belong to your deity.",
                    EnumChatType.CommandError
                );
                return false;
            }

            // Validation: Check devotion rank requirement
            if (playerData.DevotionRank < ability.MinimumRank)
            {
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    $"[Ability] {ability.Name} requires {ability.MinimumRank} rank (you are {playerData.DevotionRank}).",
                    EnumChatType.CommandError
                );
                return false;
            }

            // Validation: Check favor cost
            if (playerData.DivineFavor < ability.FavorCost)
            {
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    $"[Ability] Insufficient favor. {ability.Name} costs {ability.FavorCost} favor (you have {playerData.DivineFavor}).",
                    EnumChatType.CommandError
                );
                return false;
            }

            // Validation: Check cooldown
            if (_cooldownManager.IsOnCooldown(player.PlayerUID, abilityId, ability.CooldownSeconds))
            {
                var remaining = _cooldownManager.GetRemainingCooldown(player.PlayerUID, abilityId, ability.CooldownSeconds);
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    $"[Ability] {ability.Name} is on cooldown ({remaining:F1} seconds remaining).",
                    EnumChatType.CommandError
                );
                return false;
            }

            // Custom validation from ability
            if (!ability.CanExecute(player, _sapi, out string failureReason))
            {
                if (!string.IsNullOrEmpty(failureReason))
                {
                    player.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        $"[Ability] {failureReason}",
                        EnumChatType.CommandError
                    );
                }
                return false;
            }

            // Execute ability
            bool success = false;
            try
            {
                success = ability.Execute(player, _sapi);
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Error executing ability {abilityId}: {ex.Message}");
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    $"[Ability] Error executing {ability.Name}. Check server logs.",
                    EnumChatType.CommandError
                );
                return false;
            }

            // If execution succeeded, apply costs and cooldown
            if (success)
            {
                _playerDataManager.RemoveFavor(player.PlayerUID, ability.FavorCost, $"Used ability: {ability.Name}");
                _cooldownManager.StartCooldown(player.PlayerUID, abilityId);
                _sapi.Logger.Debug($"[PantheonWars] {player.PlayerName} successfully used {ability.Name}");
            }

            return success;
        }

        /// <summary>
        /// Gets ability information for a player
        /// </summary>
        public Ability? GetAbility(string abilityId)
        {
            return _abilityRegistry.GetAbility(abilityId);
        }

        /// <summary>
        /// Gets all abilities for a player's deity
        /// </summary>
        public System.Collections.Generic.IEnumerable<Ability> GetPlayerAbilities(IServerPlayer player)
        {
            var playerData = _playerDataManager.GetOrCreatePlayerData(player);
            if (!playerData.HasDeity())
            {
                return System.Linq.Enumerable.Empty<Ability>();
            }

            return _abilityRegistry.GetAbilitiesForDeity(playerData.DeityType);
        }

        /// <summary>
        /// Gets cooldown status for an ability
        /// </summary>
        public float GetAbilityCooldown(IServerPlayer player, string abilityId)
        {
            var ability = _abilityRegistry.GetAbility(abilityId);
            if (ability == null) return 0f;

            return _cooldownManager.GetRemainingCooldown(player.PlayerUID, abilityId, ability.CooldownSeconds);
        }
    }
}
