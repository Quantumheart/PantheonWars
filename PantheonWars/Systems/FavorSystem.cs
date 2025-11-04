using System;
using PantheonWars.Models.Enum;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using IPlayerDataManager = PantheonWars.Systems.Interfaces.IPlayerDataManager;

namespace PantheonWars.Systems;

/// <summary>
///     Manages divine favor rewards and penalties
/// </summary>
public class FavorSystem
{
    private const int BASE_KILL_FAVOR = 10;
    private const int DEATH_PENALTY_FAVOR = 5;
    private readonly DeityRegistry _deityRegistry;
    private readonly IPlayerDataManager _playerDataManager;

    private readonly ICoreServerAPI _sapi;

    public FavorSystem(ICoreServerAPI sapi, IPlayerDataManager playerDataManager, DeityRegistry deityRegistry)
    {
        _sapi = sapi;
        _playerDataManager = playerDataManager;
        _deityRegistry = deityRegistry;
    }

    /// <summary>
    ///     Initializes the favor system and hooks into game events
    /// </summary>
    public void Initialize()
    {
        _sapi.Logger.Notification("[PantheonWars] Initializing Favor System...");

        // Hook into player death event for PvP favor rewards
        _sapi.Event.PlayerDeath += OnPlayerDeath;

        _sapi.Logger.Notification("[PantheonWars] Favor System initialized");
    }

    /// <summary>
    ///     Handles player death and awards/penalizes favor
    /// </summary>
    internal void OnPlayerDeath(IServerPlayer deadPlayer, DamageSource damageSource)
    {
        // Check if death was caused by another player (PvP)
        if (damageSource?.SourceEntity is EntityPlayer attackerEntity)
            if (_sapi.World.PlayerByUid(attackerEntity.PlayerUID) is IServerPlayer attackerPlayer &&
                attackerPlayer != deadPlayer)
                ProcessPvPKill(attackerPlayer, deadPlayer);

        // Apply death penalty
        ProcessDeathPenalty(deadPlayer);
    }

    /// <summary>
    ///     Processes PvP kill and awards favor to the attacker
    /// </summary>
    internal void ProcessPvPKill(IServerPlayer attacker, IServerPlayer victim)
    {
        var attackerData = _playerDataManager.GetOrCreatePlayerData(attacker);
        var victimData = _playerDataManager.GetOrCreatePlayerData(victim);

        // Check if attacker has a deity
        if (!attackerData.HasDeity()) return;

        // Calculate favor reward
        var favorReward = CalculateFavorReward(attackerData.DeityType, victimData.DeityType);

        // Award favor
        _playerDataManager.AddFavor(attacker.PlayerUID, favorReward, $"PvP kill against {victim.PlayerName}");
        attackerData.KillCount++;

        // Get deity for display
        var deity = _deityRegistry.GetDeity(attackerData.DeityType);
        var deityName = deity?.Name ?? attackerData.DeityType.ToString();

        // Notify attacker
        attacker.SendMessage(
            GlobalConstants.GeneralChatGroup,
            $"[Divine Favor] {deityName} rewards you with {favorReward} favor for your victory!",
            EnumChatType.Notification
        );

        // Notify victim
        if (victimData.HasDeity())
        {
            var victimDeity = _deityRegistry.GetDeity(victimData.DeityType);
            var victimDeityName = victimDeity?.Name ?? victimData.DeityType.ToString();
            victim.SendMessage(
                GlobalConstants.GeneralChatGroup,
                $"[Divine Favor] {victimDeityName} is displeased by your defeat.",
                EnumChatType.Notification
            );
        }

        _sapi.Logger.Debug(
            $"[PantheonWars] {attacker.PlayerName} earned {favorReward} favor for killing {victim.PlayerName}");
    }

    /// <summary>
    ///     Applies death penalty to the player
    /// </summary>
    internal void ProcessDeathPenalty(IServerPlayer player)
    {
        var playerData = _playerDataManager.GetOrCreatePlayerData(player);

        if (!playerData.HasDeity()) return;

        // Remove favor as penalty (minimum 0)
        var penalty = Math.Min(DEATH_PENALTY_FAVOR, playerData.DivineFavor);
        if (penalty > 0)
        {
            _playerDataManager.RemoveFavor(player.PlayerUID, penalty, "Death penalty");

            player.SendMessage(
                GlobalConstants.GeneralChatGroup,
                $"[Divine Favor] You lost {penalty} favor upon death.",
                EnumChatType.Notification
            );
        }
    }

    /// <summary>
    ///     Calculates favor reward based on deity relationships
    /// </summary>
    internal int CalculateFavorReward(DeityType attackerDeity, DeityType victimDeity)
    {
        var baseFavor = BASE_KILL_FAVOR;

        // No victim deity = standard reward
        if (victimDeity == DeityType.None) return baseFavor;

        // Same deity = reduced favor (discourages infighting)
        if (attackerDeity == victimDeity) return baseFavor / 2;

        // Apply relationship multiplier
        var multiplier = _deityRegistry.GetFavorMultiplier(attackerDeity, victimDeity);
        return (int)(baseFavor * multiplier);
    }

    /// <summary>
    ///     Awards favor for deity-aligned actions (extensible for future features)
    /// </summary>
    public void AwardFavorForAction(IServerPlayer player, string actionType, int amount)
    {
        var playerData = _playerDataManager.GetOrCreatePlayerData(player);

        if (!playerData.HasDeity()) return;

        _playerDataManager.AddFavor(player.PlayerUID, amount, actionType);

        player.SendMessage(
            GlobalConstants.GeneralChatGroup,
            $"[Divine Favor] You gained {amount} favor for {actionType}",
            EnumChatType.Notification
        );
    }
}