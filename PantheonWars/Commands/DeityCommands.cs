using System;
using System.Linq;
using System.Text;
using PantheonWars.Models;
using PantheonWars.Systems;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PantheonWars.Commands
{
    /// <summary>
    /// Chat commands for deity management
    /// </summary>
    public class DeityCommands
    {
        private readonly ICoreServerAPI _sapi;
        private readonly DeityRegistry _deityRegistry;
        private readonly PlayerDataManager _playerDataManager;

        public DeityCommands(ICoreServerAPI sapi, DeityRegistry deityRegistry, PlayerDataManager playerDataManager)
        {
            _sapi = sapi;
            _deityRegistry = deityRegistry;
            _playerDataManager = playerDataManager;
        }

        /// <summary>
        /// Registers all deity-related commands
        /// </summary>
        public void RegisterCommands()
        {
            _sapi.ChatCommands.Create("deity")
                .WithDescription("Manage your deity and divine favor")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("list")
                    .WithDescription("List all available deities")
                    .HandleWith(OnListDeities)
                .EndSubCommand()
                .BeginSubCommand("info")
                    .WithDescription("Get information about a specific deity")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("deity"))
                    .HandleWith(OnDeityInfo)
                .EndSubCommand()
                .BeginSubCommand("select")
                    .WithDescription("Pledge to a deity")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("deity"))
                    .HandleWith(OnSelectDeity)
                .EndSubCommand()
                .BeginSubCommand("status")
                    .WithDescription("View your current deity status")
                    .HandleWith(OnDeityStatus)
                .EndSubCommand();

            _sapi.ChatCommands.Create("favor")
                .WithDescription("Check your current divine favor")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(OnCheckFavor);

            _sapi.Logger.Notification("[PantheonWars] Deity commands registered");
        }

        private TextCommandResult OnListDeities(TextCommandCallingArgs args)
        {
            var deities = _deityRegistry.GetAllDeities().ToList();
            var sb = new StringBuilder();
            sb.AppendLine("=== Available Deities ===");

            foreach (var deity in deities)
            {
                sb.AppendLine($"{deity.Name} ({deity.Domain}) - {deity.Alignment}");
                sb.AppendLine($"  {deity.Description}");
                sb.AppendLine();
            }

            return TextCommandResult.Success(sb.ToString());
        }

        private TextCommandResult OnDeityInfo(TextCommandCallingArgs args)
        {
            string deityName = args[0] as string ?? string.Empty;

            // Try to find deity by name
            var deity = _deityRegistry.GetAllDeities()
                .FirstOrDefault(d => d.Name.Equals(deityName, StringComparison.OrdinalIgnoreCase));

            if (deity == null)
            {
                return TextCommandResult.Error($"Deity '{deityName}' not found. Use /deity list to see available deities.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== {deity.Name} - God/Goddess of {deity.Domain} ===");
            sb.AppendLine($"Alignment: {deity.Alignment}");
            sb.AppendLine($"Description: {deity.Description}");
            sb.AppendLine($"Playstyle: {deity.Playstyle}");
            sb.AppendLine();

            // Show relationships
            if (deity.Relationships.Any())
            {
                sb.AppendLine("Relationships:");
                foreach (var relationship in deity.Relationships)
                {
                    var relatedDeity = _deityRegistry.GetDeity(relationship.Key);
                    string relatedName = relatedDeity?.Name ?? relationship.Key.ToString();
                    sb.AppendLine($"  {relationship.Value}: {relatedName}");
                }
            }

            return TextCommandResult.Success(sb.ToString());
        }

        private TextCommandResult OnSelectDeity(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            string deityName = args[0] as string ?? string.Empty;

            // Try to find deity by name
            var deity = _deityRegistry.GetAllDeities()
                .FirstOrDefault(d => d.Name.Equals(deityName, StringComparison.OrdinalIgnoreCase));

            if (deity == null)
            {
                return TextCommandResult.Error($"Deity '{deityName}' not found. Use /deity list to see available deities.");
            }

            // Get current player data
            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            // Check if already pledged to this deity
            if (playerData.DeityType == deity.Type)
            {
                return TextCommandResult.Error($"You are already pledged to {deity.Name}!");
            }

            // Warn if switching deities
            if (playerData.HasDeity())
            {
                var currentDeity = _deityRegistry.GetDeity(playerData.DeityType);
                string currentName = currentDeity?.Name ?? playerData.DeityType.ToString();
                return TextCommandResult.Error(
                    $"You are already pledged to {currentName}. " +
                    "Switching deities will reset your progress. " +
                    "This feature will be implemented in a future update."
                );
            }

            // Pledge to deity
            _playerDataManager.SetDeity(player.PlayerUID, deity.Type);

            return TextCommandResult.Success(
                $"You have pledged yourself to {deity.Name}, God/Goddess of {deity.Domain}!\n" +
                $"May your devotion be rewarded with divine favor."
            );
        }

        private TextCommandResult OnDeityStatus(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error("Command must be used by a player");

            var playerData = _playerDataManager.GetOrCreatePlayerData(player);

            if (!playerData.HasDeity())
            {
                return TextCommandResult.Success("You have not pledged to any deity. Use /deity list to see available deities.");
            }

            var deity = _deityRegistry.GetDeity(playerData.DeityType);
            string deityName = deity?.Name ?? playerData.DeityType.ToString();

            var sb = new StringBuilder();
            sb.AppendLine($"=== Your Divine Status ===");
            sb.AppendLine($"Deity: {deityName}");
            sb.AppendLine($"Divine Favor: {playerData.DivineFavor}");
            sb.AppendLine($"Devotion Rank: {playerData.DevotionRank}");
            sb.AppendLine($"Kills: {playerData.KillCount}");
            sb.AppendLine($"Total Favor Earned: {playerData.TotalFavorEarned}");

            if (playerData.PledgeDate != DateTime.MinValue)
            {
                var daysServed = (DateTime.UtcNow - playerData.PledgeDate).Days;
                sb.AppendLine($"Days Served: {daysServed}");
            }

            return TextCommandResult.Success(sb.ToString());
        }

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
    }
}
