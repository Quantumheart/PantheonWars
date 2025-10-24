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
    /// Handles all religion-related chat commands
    /// </summary>
    public class ReligionCommands
    {
        private readonly ICoreServerAPI _sapi;
        private readonly ReligionManager _religionManager;
        private readonly PlayerReligionDataManager _playerReligionDataManager;

        public ReligionCommands(ICoreServerAPI sapi, ReligionManager religionManager, PlayerReligionDataManager playerReligionDataManager)
        {
            _sapi = sapi;
            _religionManager = religionManager;
            _playerReligionDataManager = playerReligionDataManager;
        }

        /// <summary>
        /// Registers all religion commands
        /// </summary>
        public void RegisterCommands()
        {
            _sapi.ChatCommands.Create("religion")
                .WithDescription("Manage religions and congregation membership")
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("create")
                    .WithDescription("Create a new religion")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("name"),
                              _sapi.ChatCommands.Parsers.Word("deity"),
                              _sapi.ChatCommands.Parsers.OptionalWord("visibility"))
                    .HandleWith(OnCreateReligion)
                .EndSubCommand()
                .BeginSubCommand("join")
                    .WithDescription("Join a religion")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("name"))
                    .HandleWith(OnJoinReligion)
                .EndSubCommand()
                .BeginSubCommand("leave")
                    .WithDescription("Leave your current religion")
                    .HandleWith(OnLeaveReligion)
                .EndSubCommand()
                .BeginSubCommand("list")
                    .WithDescription("List all religions")
                    .WithArgs(_sapi.ChatCommands.Parsers.OptionalWord("deity"))
                    .HandleWith(OnListReligions)
                .EndSubCommand()
                .BeginSubCommand("info")
                    .WithDescription("Show religion information")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("name"))
                    .HandleWith(OnReligionInfo)
                .EndSubCommand()
                .BeginSubCommand("members")
                    .WithDescription("Show members of your religion")
                    .HandleWith(OnListMembers)
                .EndSubCommand()
                .BeginSubCommand("invite")
                    .WithDescription("Invite a player to your religion")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("playername"))
                    .HandleWith(OnInvitePlayer)
                .EndSubCommand()
                .BeginSubCommand("kick")
                    .WithDescription("Kick a player from your religion (founder only)")
                    .WithArgs(_sapi.ChatCommands.Parsers.Word("playername"))
                    .HandleWith(OnKickPlayer)
                .EndSubCommand()
                .BeginSubCommand("disband")
                    .WithDescription("Disband your religion (founder only)")
                    .HandleWith(OnDisbandReligion)
                .EndSubCommand()
                .BeginSubCommand("description")
                    .WithDescription("Set your religion's description (founder only)")
                    .WithArgs(_sapi.ChatCommands.Parsers.All("text"))
                    .HandleWith(OnSetDescription)
                .EndSubCommand();

            _sapi.Logger.Notification("[PantheonWars] Religion commands registered");
        }

        #region Command Handlers

        /// <summary>
        /// Handler for /religion create <name> <deity> [public/private]
        /// </summary>
        private TextCommandResult OnCreateReligion(TextCommandCallingArgs args)
        {
            string religionName = (string)args[0];
            string deityName = (string)args[1];
            string? visibility = args.Parsers.Count > 2 ? (string?)args[2] : "public";

            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            // Check if player already has a religion
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.HasReligion())
            {
                return TextCommandResult.Error("You are already in a religion. Use /religion leave first.");
            }

            // Parse deity type
            if (!Enum.TryParse<DeityType>(deityName, true, out DeityType deity) || deity == DeityType.None)
            {
                var validDeities = string.Join(", ", Enum.GetNames(typeof(DeityType)).Where(d => d != "None"));
                return TextCommandResult.Error($"Invalid deity. Valid options: {validDeities}");
            }

            // Parse visibility
            bool isPublic = visibility?.ToLower() != "private";

            // Check if religion name already exists
            if (_religionManager.GetReligionByName(religionName) != null)
            {
                return TextCommandResult.Error($"A religion named '{religionName}' already exists");
            }

            // Create the religion
            var religion = _religionManager.CreateReligion(religionName, deity, player.PlayerUID, isPublic);

            // Auto-join the founder
            _playerReligionDataManager.JoinReligion(player.PlayerUID, religion.ReligionUID);

            return TextCommandResult.Success($"Religion '{religionName}' created! You are now the founder serving {deity}.");
        }

        /// <summary>
        /// Handler for /religion join <name>
        /// </summary>
        private TextCommandResult OnJoinReligion(TextCommandCallingArgs args)
        {
            string religionName = (string)args[0];

            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            // Check if player can switch religions
            if (!_playerReligionDataManager.CanSwitchReligion(player.PlayerUID))
            {
                var cooldown = _playerReligionDataManager.GetSwitchCooldownRemaining(player.PlayerUID);
                return TextCommandResult.Error($"You must wait {cooldown?.Days} days, {cooldown?.Hours} hours before switching religions");
            }

            // Find the religion
            var religion = _religionManager.GetReligionByName(religionName);
            if (religion == null)
            {
                return TextCommandResult.Error($"Religion '{religionName}' not found");
            }

            // Check if player can join
            if (!_religionManager.CanJoinReligion(religion.ReligionUID, player.PlayerUID))
            {
                return TextCommandResult.Error("This religion is private and you have not been invited");
            }

            // Apply switching penalty if needed
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.HasReligion())
            {
                _playerReligionDataManager.HandleReligionSwitch(player.PlayerUID);
            }

            // Join the religion
            _playerReligionDataManager.JoinReligion(player.PlayerUID, religion.ReligionUID);

            // Remove invitation if exists
            _religionManager.RemoveInvitation(player.PlayerUID, religion.ReligionUID);

            return TextCommandResult.Success($"You have joined {religion.ReligionName}! May {religion.Deity} guide you.");
        }

        /// <summary>
        /// Handler for /religion leave
        /// </summary>
        private TextCommandResult OnLeaveReligion(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (!playerData.HasReligion())
            {
                return TextCommandResult.Error("You are not in any religion");
            }

            // Get religion info before leaving
            var religion = _religionManager.GetReligion(playerData.ReligionUID!);
            string religionName = religion?.ReligionName ?? "Unknown";

            // Leave the religion
            _playerReligionDataManager.LeaveReligion(player.PlayerUID);

            return TextCommandResult.Success($"You have left {religionName}");
        }

        /// <summary>
        /// Handler for /religion list [deity]
        /// </summary>
        private TextCommandResult OnListReligions(TextCommandCallingArgs args)
        {
            string? deityFilter = args.Parsers.Count > 0 ? (string?)args[0] : null;

            var religions = _religionManager.GetAllReligions();

            // Apply deity filter if specified
            if (!string.IsNullOrEmpty(deityFilter))
            {
                if (!Enum.TryParse<DeityType>(deityFilter, true, out DeityType deity))
                {
                    return TextCommandResult.Error($"Invalid deity: {deityFilter}");
                }
                religions = _religionManager.GetReligionsByDeity(deity);
            }

            if (religions.Count == 0)
            {
                return TextCommandResult.Success("No religions found");
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== Religions ===");
            foreach (var religion in religions.OrderByDescending(r => r.TotalPrestige))
            {
                string visibility = religion.IsPublic ? "Public" : "Private";
                sb.AppendLine($"- {religion.ReligionName} ({religion.Deity}) | {visibility} | {religion.GetMemberCount()} members | Rank: {religion.PrestigeRank}");
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// Handler for /religion info [name]
        /// </summary>
        private TextCommandResult OnReligionInfo(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            string? religionName = args.Parsers.Count > 0 ? (string?)args[0] : null;

            // Get the religion
            Data.ReligionData? religion;
            if (!string.IsNullOrEmpty(religionName))
            {
                religion = _religionManager.GetReligionByName(religionName);
                if (religion == null)
                {
                    return TextCommandResult.Error($"Religion '{religionName}' not found");
                }
            }
            else
            {
                // Show current religion
                var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
                if (!playerData.HasReligion())
                {
                    return TextCommandResult.Error("You are not in any religion. Specify a religion name to view.");
                }
                religion = _religionManager.GetReligion(playerData.ReligionUID!);
                if (religion == null)
                {
                    return TextCommandResult.Error("Could not find your religion data");
                }
            }

            // Build info display
            var sb = new StringBuilder();
            sb.AppendLine($"=== {religion.ReligionName} ===");
            sb.AppendLine($"Deity: {religion.Deity}");
            sb.AppendLine($"Visibility: {(religion.IsPublic ? "Public" : "Private")}");
            sb.AppendLine($"Members: {religion.GetMemberCount()}");
            sb.AppendLine($"Prestige Rank: {religion.PrestigeRank}");
            sb.AppendLine($"Prestige: {religion.Prestige} (Total: {religion.TotalPrestige})");
            sb.AppendLine($"Created: {religion.CreationDate:yyyy-MM-dd}");

            var founderPlayer = _sapi.World.PlayerByUid(religion.FounderUID);
            sb.AppendLine($"Founder: {founderPlayer?.PlayerName ?? "Unknown"}");

            if (!string.IsNullOrEmpty(religion.Description))
            {
                sb.AppendLine($"Description: {religion.Description}");
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// Handler for /religion members
        /// </summary>
        private TextCommandResult OnListMembers(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (!playerData.HasReligion())
            {
                return TextCommandResult.Error("You are not in any religion");
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID!);
            if (religion == null)
            {
                return TextCommandResult.Error("Could not find your religion data");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== {religion.ReligionName} Members ({religion.GetMemberCount()}) ===");

            foreach (var memberUID in religion.MemberUIDs)
            {
                var memberPlayer = _sapi.World.PlayerByUid(memberUID);
                string memberName = memberPlayer?.PlayerName ?? "Unknown";

                var memberData = _playerReligionDataManager.GetOrCreatePlayerData(memberUID);
                string role = religion.IsFounder(memberUID) ? "Founder" : "Member";

                sb.AppendLine($"- {memberName} ({role}) | Rank: {memberData.FavorRank} | Favor: {memberData.Favor}");
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// Handler for /religion invite <playername>
        /// </summary>
        private TextCommandResult OnInvitePlayer(TextCommandCallingArgs args)
        {
            string targetPlayerName = (string)args[0];

            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            // Check if player is in a religion
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (!playerData.HasReligion())
            {
                return TextCommandResult.Error("You are not in any religion");
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID!);
            if (religion == null)
            {
                return TextCommandResult.Error("Could not find your religion data");
            }

            // Find target player
            var targetPlayer = _sapi.World.AllOnlinePlayers
                .FirstOrDefault(p => p.PlayerName.Equals(targetPlayerName, StringComparison.OrdinalIgnoreCase)) as IServerPlayer;

            if (targetPlayer == null)
            {
                return TextCommandResult.Error($"Player '{targetPlayerName}' not found online");
            }

            // Check if target is already a member
            if (religion.IsMember(targetPlayer.PlayerUID))
            {
                return TextCommandResult.Error($"{targetPlayerName} is already a member of {religion.ReligionName}");
            }

            // Send invitation
            _religionManager.InvitePlayer(religion.ReligionUID, targetPlayer.PlayerUID, player.PlayerUID);

            // Notify target player
            targetPlayer.SendMessage(
                Vintagestory.API.Config.GlobalConstants.GeneralChatGroup,
                $"You have been invited to join {religion.ReligionName}! Use /religion join {religion.ReligionName} to accept.",
                Vintagestory.API.Common.EnumChatType.Notification
            );

            return TextCommandResult.Success($"Invitation sent to {targetPlayerName}");
        }

        /// <summary>
        /// Handler for /religion kick <playername>
        /// </summary>
        private TextCommandResult OnKickPlayer(TextCommandCallingArgs args)
        {
            string targetPlayerName = (string)args[0];

            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            // Check if player is in a religion
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (!playerData.HasReligion())
            {
                return TextCommandResult.Error("You are not in any religion");
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID!);
            if (religion == null)
            {
                return TextCommandResult.Error("Could not find your religion data");
            }

            // Check if player is founder
            if (!religion.IsFounder(player.PlayerUID))
            {
                return TextCommandResult.Error("Only the founder can kick members");
            }

            // Find target player by name
            var targetPlayer = _sapi.World.AllPlayers
                .FirstOrDefault(p => p.PlayerName.Equals(targetPlayerName, StringComparison.OrdinalIgnoreCase));

            if (targetPlayer == null)
            {
                return TextCommandResult.Error($"Player '{targetPlayerName}' not found");
            }

            // Check if target is a member
            if (!religion.IsMember(targetPlayer.PlayerUID))
            {
                return TextCommandResult.Error($"{targetPlayerName} is not a member of {religion.ReligionName}");
            }

            // Cannot kick yourself
            if (targetPlayer.PlayerUID == player.PlayerUID)
            {
                return TextCommandResult.Error("You cannot kick yourself. Use /religion disband or /religion leave instead.");
            }

            // Kick the player
            _playerReligionDataManager.LeaveReligion(targetPlayer.PlayerUID);

            // Notify target if online
            var targetServerPlayer = targetPlayer as IServerPlayer;
            if (targetServerPlayer != null)
            {
                targetServerPlayer.SendMessage(
                    Vintagestory.API.Config.GlobalConstants.GeneralChatGroup,
                    $"You have been removed from {religion.ReligionName}",
                    Vintagestory.API.Common.EnumChatType.Notification
                );
            }

            return TextCommandResult.Success($"{targetPlayerName} has been removed from {religion.ReligionName}");
        }

        /// <summary>
        /// Handler for /religion disband
        /// </summary>
        private TextCommandResult OnDisbandReligion(TextCommandCallingArgs args)
        {
            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            // Check if player is in a religion
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (!playerData.HasReligion())
            {
                return TextCommandResult.Error("You are not in any religion");
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID!);
            if (religion == null)
            {
                return TextCommandResult.Error("Could not find your religion data");
            }

            // Check if player is founder
            if (!religion.IsFounder(player.PlayerUID))
            {
                return TextCommandResult.Error("Only the founder can disband the religion");
            }

            string religionName = religion.ReligionName;

            // Remove all members
            var members = religion.MemberUIDs.ToList(); // Copy to avoid modification during iteration
            foreach (var memberUID in members)
            {
                _playerReligionDataManager.LeaveReligion(memberUID);

                // Notify member if online
                var memberPlayer = _sapi.World.PlayerByUid(memberUID) as IServerPlayer;
                if (memberPlayer != null && memberUID != player.PlayerUID)
                {
                    memberPlayer.SendMessage(
                        Vintagestory.API.Config.GlobalConstants.GeneralChatGroup,
                        $"{religionName} has been disbanded by its founder",
                        Vintagestory.API.Common.EnumChatType.Notification
                    );
                }
            }

            // Delete the religion
            _religionManager.DeleteReligion(religion.ReligionUID, player.PlayerUID);

            return TextCommandResult.Success($"{religionName} has been disbanded");
        }

        /// <summary>
        /// Handler for /religion description <text>
        /// </summary>
        private TextCommandResult OnSetDescription(TextCommandCallingArgs args)
        {
            string description = (string)args[0];

            IServerPlayer player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error("Command can only be used by players");
            }

            // Check if player is in a religion
            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (!playerData.HasReligion())
            {
                return TextCommandResult.Error("You are not in any religion");
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID!);
            if (religion == null)
            {
                return TextCommandResult.Error("Could not find your religion data");
            }

            // Check if player is founder
            if (!religion.IsFounder(player.PlayerUID))
            {
                return TextCommandResult.Error("Only the founder can set the religion description");
            }

            // Set description
            religion.Description = description;

            return TextCommandResult.Success($"Description set for {religion.ReligionName}");
        }

        #endregion
    }
}