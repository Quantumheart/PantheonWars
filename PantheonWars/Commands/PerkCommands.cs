using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PantheonWars.Models;
using PantheonWars.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Commands
{
    /// <summary>
    /// Commands for managing perks (Phase 3.3)
    /// </summary>
    public class PerkCommands
    {
        private readonly ICoreServerAPI _sapi;
        private readonly PerkRegistry _perkRegistry;
        private readonly PlayerReligionDataManager _playerReligionDataManager;
        private readonly ReligionManager _religionManager;
        private readonly PerkEffectSystem _perkEffectSystem;

        public PerkCommands(
            ICoreServerAPI sapi,
            PerkRegistry perkRegistry,
            PlayerReligionDataManager playerReligionDataManager,
            ReligionManager religionManager,
            PerkEffectSystem perkEffectSystem)
        {
            _sapi = sapi;
            _perkRegistry = perkRegistry;
            _playerReligionDataManager = playerReligionDataManager;
            _religionManager = religionManager;
            _perkEffectSystem = perkEffectSystem;
        }

        /// <summary>
        /// Registers all perk commands
        /// </summary>
        public void RegisterCommands()
        {
            _sapi.ChatCommands.Create("perks")
                .WithDescription("Manage perks and view perk information")
                .RequiresPlayer()
                .BeginSubCommand("list")
                    .WithDescription("List all available perks for your deity")
                    .HandleWith(OnPerksList)
                .EndSubCommand()
                .BeginSubCommand("player")
                    .WithDescription("Show your unlocked player perks")
                    .HandleWith(OnPerksPlayer)
                .EndSubCommand()
                .BeginSubCommand("religion")
                    .WithDescription("Show your religion's unlocked perks")
                    .HandleWith(OnPerksReligion)
                .EndSubCommand()
                .BeginSubCommand("info")
                    .WithDescription("Show detailed information about a perk")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("perkid"))
                    .HandleWith(OnPerksInfo)
                .EndSubCommand()
                .BeginSubCommand("tree")
                    .WithDescription("Display perk tree (player or religion)")
                    .WithArgs(_sapi.ChatCommands.Parsers.OptionalWord("type"))
                    .HandleWith(OnPerksTree)
                .EndSubCommand()
                .BeginSubCommand("unlock")
                    .WithDescription("Unlock a perk")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("perkid"))
                    .HandleWith(OnPerksUnlock)
                .EndSubCommand()
                .BeginSubCommand("active")
                    .WithDescription("Show all active perks and stat modifiers")
                    .HandleWith(OnPerksActive)
                .EndSubCommand();

            _sapi.Logger.Notification("[PantheonWars] Perk commands registered");
        }

        /// <summary>
        /// /perks list - Lists all available perks for player's deity
        /// </summary>
        private TextCommandResult OnPerksList(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Player not found");

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);

            if (playerData.ActiveDeity == DeityType.None)
            {
                return TextCommandResult.Error("You must join a religion to view perks. Use /religion to get started.");
            }

            var playerPerks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, PerkType.Player);
            var religionPerks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, PerkType.Religion);

            var sb = new StringBuilder();
            sb.AppendLine($"=== Perks for {playerData.ActiveDeity} ===");
            sb.AppendLine();

            sb.AppendLine("--- Player Perks ---");
            foreach (var perk in playerPerks)
            {
                string status = playerData.IsPerkUnlocked(perk.PerkId) ? "[UNLOCKED]" : "";
                FavorRank requiredRank = (FavorRank)perk.RequiredFavorRank;
                sb.AppendLine($"{perk.Name} {status}");
                sb.AppendLine($"  ID: {perk.PerkId}");
                sb.AppendLine($"  Required Rank: {requiredRank}");
                sb.AppendLine($"  {perk.Description}");
                sb.AppendLine();
            }

            sb.AppendLine("--- Religion Perks ---");
            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;
            foreach (var perk in religionPerks)
            {
                bool unlocked = religion?.UnlockedPerks.TryGetValue(perk.PerkId, out bool u) == true && u;
                string status = unlocked ? "[UNLOCKED]" : "";
                PrestigeRank requiredRank = (PrestigeRank)perk.RequiredPrestigeRank;
                sb.AppendLine($"{perk.Name} {status}");
                sb.AppendLine($"  ID: {perk.PerkId}");
                sb.AppendLine($"  Required Rank: {requiredRank}");
                sb.AppendLine($"  {perk.Description}");
                sb.AppendLine();
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks player - Shows unlocked player perks
        /// </summary>
        private TextCommandResult OnPerksPlayer(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Player not found");

            var (playerPerks, _) = _perkEffectSystem.GetActivePerks(player.PlayerUID);

            if (playerPerks.Count == 0)
            {
                return TextCommandResult.Success("You have no unlocked player perks yet.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== Your Unlocked Player Perks ({playerPerks.Count}) ===");
            sb.AppendLine();

            foreach (var perk in playerPerks)
            {
                sb.AppendLine($"{perk.Name} ({perk.Category})");
                sb.AppendLine($"  {perk.Description}");

                if (perk.StatModifiers.Count > 0)
                {
                    sb.AppendLine("  Effects:");
                    foreach (var mod in perk.StatModifiers)
                    {
                        sb.AppendLine($"    {mod.Key}: +{mod.Value * 100:F1}%");
                    }
                }
                sb.AppendLine();
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks religion - Shows religion's unlocked perks
        /// </summary>
        private TextCommandResult OnPerksReligion(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Player not found");

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.ReligionUID == null)
            {
                return TextCommandResult.Error("You are not in a religion.");
            }

            var (_, religionPerks) = _perkEffectSystem.GetActivePerks(player.PlayerUID);

            if (religionPerks.Count == 0)
            {
                return TextCommandResult.Success("Your religion has no unlocked perks yet.");
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID);
            var sb = new StringBuilder();
            sb.AppendLine($"=== {religion?.ReligionName} Perks ({religionPerks.Count}) ===");
            sb.AppendLine();

            foreach (var perk in religionPerks)
            {
                sb.AppendLine($"{perk.Name} ({perk.Category})");
                sb.AppendLine($"  {perk.Description}");

                if (perk.StatModifiers.Count > 0)
                {
                    sb.AppendLine("  Effects (for all members):");
                    foreach (var mod in perk.StatModifiers)
                    {
                        sb.AppendLine($"    {mod.Key}: +{mod.Value * 100:F1}%");
                    }
                }
                sb.AppendLine();
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks info <perkid> - Shows detailed perk information
        /// </summary>
        private TextCommandResult OnPerksInfo(TextCommandCallingArgs args)
        {
            var perkId = args[0] as string;
            if (string.IsNullOrEmpty(perkId))
            {
                return TextCommandResult.Error("Usage: /perks info <perkid>");
            }

            var perk = _perkRegistry.GetPerk(perkId);
            if (perk == null)
            {
                return TextCommandResult.Error($"Perk not found: {perkId}");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== {perk.Name} ===");
            sb.AppendLine($"ID: {perk.PerkId}");
            sb.AppendLine($"Deity: {perk.Deity}");
            sb.AppendLine($"Type: {perk.Type}");
            sb.AppendLine($"Category: {perk.Category}");
            sb.AppendLine();
            sb.AppendLine($"Description: {perk.Description}");
            sb.AppendLine();

            if (perk.Type == PerkType.Player)
            {
                FavorRank requiredRank = (FavorRank)perk.RequiredFavorRank;
                sb.AppendLine($"Required Favor Rank: {requiredRank}");
            }
            else
            {
                PrestigeRank requiredRank = (PrestigeRank)perk.RequiredPrestigeRank;
                sb.AppendLine($"Required Prestige Rank: {requiredRank}");
            }

            if (perk.PrerequisitePerks.Count > 0)
            {
                sb.AppendLine("Prerequisites:");
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    var prereq = _perkRegistry.GetPerk(prereqId);
                    string prereqName = prereq?.Name ?? prereqId;
                    sb.AppendLine($"  - {prereqName}");
                }
            }

            if (perk.StatModifiers.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Stat Modifiers:");
                foreach (var mod in perk.StatModifiers)
                {
                    sb.AppendLine($"  {mod.Key}: +{mod.Value * 100:F1}%");
                }
            }

            if (perk.SpecialEffects.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Special Effects:");
                foreach (var effect in perk.SpecialEffects)
                {
                    sb.AppendLine($"  - {effect}");
                }
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks tree [player/religion] - Displays perk tree
        /// </summary>
        private TextCommandResult OnPerksTree(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Player not found");

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.ActiveDeity == DeityType.None)
            {
                return TextCommandResult.Error("You must join a religion to view perk trees.");
            }

            string type = args[0] as string ?? "player";
            type = type.ToLower();

            PerkType perkType = type == "religion" ? PerkType.Religion : PerkType.Player;
            var perks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, perkType);

            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;

            var sb = new StringBuilder();
            sb.AppendLine($"=== {playerData.ActiveDeity} {perkType} Perk Tree ===");
            sb.AppendLine();

            // Group by rank
            if (perkType == PerkType.Player)
            {
                foreach (FavorRank rank in Enum.GetValues(typeof(FavorRank)))
                {
                    var rankPerks = perks.Where(p => p.RequiredFavorRank == (int)rank).ToList();
                    if (rankPerks.Count == 0) continue;

                    sb.AppendLine($"--- {rank} Rank ---");
                    foreach (var perk in rankPerks)
                    {
                        bool unlocked = playerData.IsPerkUnlocked(perk.PerkId);
                        string status = unlocked ? "[✓]" : "[ ]";
                        sb.AppendLine($"{status} {perk.Name}");

                        if (perk.PrerequisitePerks.Count > 0)
                        {
                            sb.Append("    Requires: ");
                            var prereqNames = perk.PrerequisitePerks.Select(id =>
                            {
                                var p = _perkRegistry.GetPerk(id);
                                return p?.Name ?? id;
                            });
                            sb.AppendLine(string.Join(", ", prereqNames));
                        }
                    }
                    sb.AppendLine();
                }
            }
            else // Religion
            {
                foreach (PrestigeRank rank in Enum.GetValues(typeof(PrestigeRank)))
                {
                    var rankPerks = perks.Where(p => p.RequiredPrestigeRank == (int)rank).ToList();
                    if (rankPerks.Count == 0) continue;

                    sb.AppendLine($"--- {rank} Rank ---");
                    foreach (var perk in rankPerks)
                    {
                        bool unlocked = religion?.UnlockedPerks.TryGetValue(perk.PerkId, out bool u) == true && u;
                        string status = unlocked ? "[✓]" : "[ ]";
                        sb.AppendLine($"{status} {perk.Name}");

                        if (perk.PrerequisitePerks.Count > 0)
                        {
                            sb.Append("    Requires: ");
                            var prereqNames = perk.PrerequisitePerks.Select(id =>
                            {
                                var p = _perkRegistry.GetPerk(id);
                                return p?.Name ?? id;
                            });
                            sb.AppendLine(string.Join(", ", prereqNames));
                        }
                    }
                    sb.AppendLine();
                }
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks unlock <perkid> - Unlocks a perk
        /// </summary>
        private TextCommandResult OnPerksUnlock(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Player not found");

            var perkId = args[0] as string;
            if (string.IsNullOrEmpty(perkId))
            {
                return TextCommandResult.Error("Usage: /perks unlock <perkid>");
            }

            var perk = _perkRegistry.GetPerk(perkId);
            if (perk == null)
            {
                return TextCommandResult.Error($"Perk not found: {perkId}");
            }

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;

            // Check if can unlock
            var (canUnlock, reason) = _perkRegistry.CanUnlockPerk(playerData, religion, perk);
            if (!canUnlock)
            {
                return TextCommandResult.Error($"Cannot unlock perk: {reason}");
            }

            // Unlock the perk
            if (perk.Type == PerkType.Player)
            {
                bool success = _playerReligionDataManager.UnlockPlayerPerk(player.PlayerUID, perkId);
                if (!success)
                {
                    return TextCommandResult.Error("Failed to unlock perk (may already be unlocked)");
                }

                _perkEffectSystem.RefreshPlayerPerks(player.PlayerUID);
                return TextCommandResult.Success($"Unlocked player perk: {perk.Name}!");
            }
            else // Religion perk
            {
                if (religion == null)
                {
                    return TextCommandResult.Error("You must be in a religion to unlock religion perks.");
                }

                // Only founder can unlock religion perks (optional restriction)
                if (!religion.IsFounder(player.PlayerUID))
                {
                    return TextCommandResult.Error("Only the religion founder can unlock religion perks.");
                }

                religion.UnlockedPerks[perkId] = true;
                _perkEffectSystem.RefreshReligionPerks(religion.ReligionUID);

                // Notify all members
                foreach (var memberUID in religion.MemberUIDs)
                {
                    var member = _sapi.World.PlayerByUid(memberUID) as IServerPlayer;
                    member?.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        $"Your religion unlocked: {perk.Name}!",
                        EnumChatType.Notification
                    );
                }

                return TextCommandResult.Success($"Unlocked religion perk: {perk.Name} for all members!");
            }
        }

        /// <summary>
        /// /perks active - Shows all active perks and combined modifiers
        /// </summary>
        private TextCommandResult OnPerksActive(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Player not found");

            var (playerPerks, religionPerks) = _perkEffectSystem.GetActivePerks(player.PlayerUID);
            var combinedModifiers = _perkEffectSystem.GetCombinedStatModifiers(player.PlayerUID);

            var sb = new StringBuilder();
            sb.AppendLine("=== Active Perks & Stat Modifiers ===");
            sb.AppendLine();

            sb.AppendLine($"Player Perks ({playerPerks.Count}):");
            if (playerPerks.Count == 0)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var perk in playerPerks)
                {
                    sb.AppendLine($"  - {perk.Name}");
                }
            }
            sb.AppendLine();

            sb.AppendLine($"Religion Perks ({religionPerks.Count}):");
            if (religionPerks.Count == 0)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var perk in religionPerks)
                {
                    sb.AppendLine($"  - {perk.Name}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("Combined Stat Modifiers:");
            if (combinedModifiers.Count == 0)
            {
                sb.AppendLine("  No active modifiers");
            }
            else
            {
                sb.AppendLine(_perkEffectSystem.FormatStatModifiers(combinedModifiers));
            }

            return TextCommandResult.Success(sb.ToString());
        }
    }
}
