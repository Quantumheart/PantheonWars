using System;
using System.Text;
using PantheonWars.Models;
using PantheonWars.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PantheonWars.Commands
{
    /// <summary>
    /// Chat commands for favor management and testing
    /// </summary>
    public class FavorCommands
    {
        private readonly ICoreServerAPI _sapi;
        private readonly DeityRegistry _deityRegistry;
        private readonly PlayerDataManager _playerDataManager;

        // ReSharper disable once ConvertToPrimaryConstructor
        public FavorCommands(
            ICoreServerAPI sapi,
            DeityRegistry deityRegistry,
            PlayerDataManager playerDataManager)
        {
            _sapi = sapi;
            _deityRegistry = deityRegistry;
            _playerDataManager = playerDataManager;
        }

        /// <summary>
        /// Registers all favor-related commands
        /// </summary>
        public void RegisterCommands()
        {
            // Main /favor command with subcommands
            _sapi.ChatCommands.Create("favor")
                .WithDescription("Manage and check your divine favor")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnCheckFavor) // Default behavior: show current favor
                .BeginSubCommand("get")
                    .WithDescription("Check your current divine favor")
                    .HandleWith(OnCheckFavor)
                .EndSubCommand()
                .BeginSubCommand("info")
                    .WithDescription("View detailed favor information and rank progression")
                    .HandleWith(OnFavorInfo)
                .EndSubCommand()
                .BeginSubCommand("stats")
                    .WithDescription("View comprehensive favor statistics")
                    .HandleWith(OnFavorStats)
                .EndSubCommand()
                .BeginSubCommand("ranks")
                    .WithDescription("List all devotion ranks and their requirements")
                    .RequiresPrivilege(Privilege.chat)
                    .HandleWith(OnListRanks)
                .EndSubCommand()
                .BeginSubCommand("set")
                    .WithDescription("Set favor to a specific amount (Admin only)")
                    .WithArgs(_sapi.ChatCommands.Parsers.Int("amount"))
                    .RequiresPrivilege(Privilege.root)
                    .HandleWith(OnSetFavor)
                .EndSubCommand()
                .BeginSubCommand("add")
                    .WithDescription("Add favor (Admin only)")
                    .WithArgs(_sapi.ChatCommands.Parsers.Int("amount"))
                    .RequiresPrivilege(Privilege.root)
                    .HandleWith(OnAddFavor)
                .EndSubCommand()
                .BeginSubCommand("remove")
                    .WithDescription("Remove favor (Admin only)")
                    .WithArgs(_sapi.ChatCommands.Parsers.Int("amount"))
                    .RequiresPrivilege(Privilege.root)
                    .HandleWith(OnRemoveFavor)
                .EndSubCommand()
                .BeginSubCommand("reset")
                    .WithDescription("Reset favor to 0 (Admin only)")
                    .RequiresPrivilege(Privilege.root)
                    .HandleWith(OnResetFavor)
                .EndSubCommand()
                .BeginSubCommand("max")
                    .WithDescription("Set favor to maximum (Admin only)")
                    .RequiresPrivilege(Privilege.root)
                    .HandleWith(OnMaxFavor)
                .EndSubCommand()
                .BeginSubCommand("settotal")
                    .WithDescription("Set total favor earned and update rank (Admin only)")
                    .WithArgs(_sapi.ChatCommands.Parsers.Int("amount"))
                    .RequiresPrivilege(Privilege.root)
                    .HandleWith(OnSetTotalFavor)
                .EndSubCommand();

            _sapi.Logger.Notification("[PantheonWars] Favor commands registered");
        }

        #region Information Commands (Privilege.chat)

        /// <summary>
        /// Shows current favor amount - default command and /favor get
        /// </summary>
        private TextCommandResult OnCheckFavor(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Success("You have not pledged to any deity yet.");
            }

            var deity = _deityRegistry.GetDeity(playerData.DeityType);
            string deityName = deity?.Name ?? playerData.DeityType.ToString();

            return TextCommandResult.Success(
                $"You have {playerData.DivineFavor} favor with {deityName} (Rank: {playerData.DevotionRank})"
            );
        }

        /// <summary>
        /// Shows detailed favor information and rank progression
        /// </summary>
        private TextCommandResult OnFavorInfo(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Success("You have not pledged to any deity yet.");
            }

            var deity = _deityRegistry.GetDeity(playerData.DeityType);
            string deityName = deity?.Name ?? playerData.DeityType.ToString();

            var sb = new StringBuilder();
            sb.AppendLine("=== Divine Favor ===");
            sb.AppendLine($"Deity: {deityName}");
            sb.AppendLine($"Current Favor: {playerData.DivineFavor:N0}");
            sb.AppendLine($"Total Favor Earned: {playerData.TotalFavorEarned:N0}");
            sb.AppendLine($"Current Rank: {playerData.DevotionRank}");

            // Calculate next rank
            var (nextRank, nextThreshold) = GetNextRank(playerData.DevotionRank);
            if (nextRank.HasValue)
            {
                sb.AppendLine($"Next Rank: {nextRank.Value} ({nextThreshold:N0} total favor required)");

                int remaining = nextThreshold - playerData.TotalFavorEarned;
                float progress = (float)playerData.TotalFavorEarned / nextThreshold * 100f;
                sb.AppendLine($"Progress: {progress:F1}% ({remaining:N0} favor needed)");
            }
            else
            {
                sb.AppendLine("Next Rank: None (Maximum rank achieved!)");
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// Shows comprehensive favor statistics
        /// </summary>
        private TextCommandResult OnFavorStats(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Success("You have not pledged to any deity yet.");
            }

            var deity = _deityRegistry.GetDeity(playerData.DeityType);
            string deityName = deity?.Name ?? playerData.DeityType.ToString();

            var sb = new StringBuilder();
            sb.AppendLine("=== Divine Statistics ===");
            sb.AppendLine($"Deity: {deityName}");
            sb.AppendLine($"Current Favor: {playerData.DivineFavor:N0}");
            sb.AppendLine($"Total Favor Earned: {playerData.TotalFavorEarned:N0}");
            sb.AppendLine($"Devotion Rank: {playerData.DevotionRank}");
            sb.AppendLine($"Kill Count: {playerData.KillCount}");

            if (playerData.PledgeDate != DateTime.MinValue)
            {
                var daysServed = (DateTime.UtcNow - playerData.PledgeDate).Days;
                sb.AppendLine($"Days Served: {daysServed}");
                sb.AppendLine($"Pledge Date: {playerData.PledgeDate:yyyy-MM-dd}");
            }

            // Calculate next rank
            var (nextRank, nextThreshold) = GetNextRank(playerData.DevotionRank);
            if (nextRank.HasValue)
            {
                int remaining = nextThreshold - playerData.TotalFavorEarned;
                sb.AppendLine();
                sb.AppendLine($"Next Rank: {nextRank.Value}");
                sb.AppendLine($"Favor Needed: {remaining:N0}");
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// Lists all devotion ranks and their requirements
        /// Does NOT require deity pledge - informational only
        /// </summary>
        private TextCommandResult OnListRanks(TextCommandCallingArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Devotion Ranks ===");
            sb.AppendLine("Initiate: 0 total favor");
            sb.AppendLine("Disciple: 500 total favor");
            sb.AppendLine("Zealot: 2,000 total favor");
            sb.AppendLine("Champion: 5,000 total favor");
            sb.AppendLine("Avatar: 10,000 total favor");
            sb.AppendLine();
            sb.AppendLine("Higher ranks unlock more powerful abilities.");

            return TextCommandResult.Success(sb.ToString());
        }

        #endregion

        #region Admin Mutation Commands (Privilege.root)

        /// <summary>
        /// Sets favor to a specific amount (Admin only)
        /// </summary>
        private TextCommandResult OnSetFavor(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Error("You have not pledged to any deity yet.");
            }

            int amount = (int)args[0];

            // Validate amount
            if (amount < 0)
            {
                return TextCommandResult.Error("Favor amount cannot be negative.");
            }

            if (amount > 999999)
            {
                return TextCommandResult.Error("Favor amount cannot exceed 999,999.");
            }

            playerData.DivineFavor = amount;

            return TextCommandResult.Success($"Favor set to {amount:N0}");
        }

        /// <summary>
        /// Adds favor (Admin only)
        /// </summary>
        private TextCommandResult OnAddFavor(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Error("You have not pledged to any deity yet.");
            }

            int amount = (int)args[0];

            // Validate amount
            if (amount <= 0)
            {
                return TextCommandResult.Error("Amount must be greater than 0.");
            }

            if (amount > 999999)
            {
                return TextCommandResult.Error("Amount cannot exceed 999,999.");
            }

            int oldFavor = playerData.DivineFavor;
            playerData.DivineFavor += amount;

            return TextCommandResult.Success($"Added {amount:N0} favor ({oldFavor:N0} → {playerData.DivineFavor:N0})");
        }

        /// <summary>
        /// Removes favor (Admin only)
        /// </summary>
        private TextCommandResult OnRemoveFavor(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Error("You have not pledged to any deity yet.");
            }

            int amount = (int)args[0];

            // Validate amount
            if (amount <= 0)
            {
                return TextCommandResult.Error("Amount must be greater than 0.");
            }

            if (amount > 999999)
            {
                return TextCommandResult.Error("Amount cannot exceed 999,999.");
            }

            int oldFavor = playerData.DivineFavor;
            playerData.DivineFavor = Math.Max(0, playerData.DivineFavor - amount);
            int actualRemoved = oldFavor - playerData.DivineFavor;

            return TextCommandResult.Success($"Removed {actualRemoved:N0} favor ({oldFavor:N0} → {playerData.DivineFavor:N0})");
        }

        /// <summary>
        /// Resets favor to 0 (Admin only)
        /// </summary>
        private TextCommandResult OnResetFavor(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Error("You have not pledged to any deity yet.");
            }

            int oldFavor = playerData.DivineFavor;
            playerData.DivineFavor = 0;

            return TextCommandResult.Success($"Favor reset to 0 (was {oldFavor:N0})");
        }

        /// <summary>
        /// Sets favor to maximum (Admin only)
        /// </summary>
        private TextCommandResult OnMaxFavor(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Error("You have not pledged to any deity yet.");
            }

            int oldFavor = playerData.DivineFavor;
            playerData.DivineFavor = 99999;

            return TextCommandResult.Success($"Favor set to maximum: 99,999 (was {oldFavor:N0})");
        }

        /// <summary>
        /// Sets total favor earned and updates devotion rank (Admin only)
        /// </summary>
        private TextCommandResult OnSetTotalFavor(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Error("You have not pledged to any deity yet.");
            }

            int amount = (int)args[0];

            // Validate amount
            if (amount < 0)
            {
                return TextCommandResult.Error("Total favor earned cannot be negative.");
            }

            if (amount > 999999)
            {
                return TextCommandResult.Error("Total favor earned cannot exceed 999,999.");
            }

            int oldTotal = playerData.TotalFavorEarned;
            var oldRank = playerData.DevotionRank;

            playerData.TotalFavorEarned = amount;
            playerData.UpdateDevotionRank();

            var newRank = playerData.DevotionRank;

            var sb = new StringBuilder();
            sb.AppendLine($"Total favor earned set to {amount:N0} (was {oldTotal:N0})");

            if (oldRank != newRank)
            {
                sb.AppendLine($"Rank updated: {oldRank} → {newRank}");
            }
            else
            {
                sb.AppendLine($"Rank unchanged: {newRank}");
            }

            return TextCommandResult.Success(sb.ToString());
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the next devotion rank and its requirement
        /// </summary>
        private (DevotionRank? nextRank, int threshold) GetNextRank(DevotionRank currentRank)
        {
            return currentRank switch
            {
                DevotionRank.Initiate => (DevotionRank.Disciple, 500),
                DevotionRank.Disciple => (DevotionRank.Zealot, 2000),
                DevotionRank.Zealot => (DevotionRank.Champion, 5000),
                DevotionRank.Champion => (DevotionRank.Avatar, 10000),
                DevotionRank.Avatar => (null, 0), // Max rank
                _ => (null, 0)
            };
        }

        #endregion
    }
}
