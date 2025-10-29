using System;
using System.Linq;
using System.Text;
using PantheonWars.Models;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Commands
{
    /// <summary>
    /// Commands for managing perks (Phase 3.3)
    /// </summary>
    public class PerkCommands(
        ICoreServerAPI? sapi,
        IPerkRegistry? perkRegistry,
        IPlayerReligionDataManager? playerReligionDataManager,
        IReligionManager? religionManager,
        IPerkEffectSystem? perkEffectSystem)
    {
        #region Constants

        // Command and subcommand names
        private const string CommandName = "perks";
        private const string SubCommandList = "list";
        private const string SubCommandPlayer = "player";
        private const string SubCommandReligion = "religion";
        private const string SubCommandInfo = "info";
        private const string SubCommandTree = "tree";
        private const string SubCommandUnlock = "unlock";
        private const string SubCommandActive = "active";

        // Parameter names
        private const string ParamPerkId = "perkid";
        private const string ParamType = "type";

        // Command descriptions
        private const string CommandDescription = "Manage perks and view perk information";
        private const string DescriptionList = "List all available perks for your deity";
        private const string DescriptionPlayer = "Show your unlocked player perks";
        private const string DescriptionReligion = "Show your religion's unlocked perks";
        private const string DescriptionInfo = "Show detailed information about a perk";
        private const string DescriptionTree = "Display perk tree (player or religion)";
        private const string DescriptionUnlock = "Unlock a perk";
        private const string DescriptionActive = "Show all active perks and stat modifiers";

        // Common messages
        private const string ErrorPlayerNotFound = "Player not found";
        private const string ErrorNoReligion = "You are not in a religion.";
        private const string ErrorMustJoinReligion = "You must join a religion to view perks. Use /religion to get started.";
        private const string ErrorMustJoinReligionForTree = "You must join a religion to view perk trees.";
        private const string ErrorMustBeInReligionToUnlock = "You must be in a religion to unlock player perks.";
        private const string ErrorOnlyFounderCanUnlock = "Only the religion founder can unlock religion perks.";
        private const string ErrorPerkNotFound = "Perk not found: {0}";
        private const string ErrorCannotUnlockPerk = "Cannot unlock perk: {0}";
        private const string ErrorFailedToUnlock = "Failed to unlock perk (may already be unlocked)";

        // Usage messages
        private const string UsagePerksInfo = "Usage: /perks info <perkid>";
        private const string UsagePerksUnlock = "Usage: /perks unlock <perkid>";

        // Success messages
        private const string SuccessUnlockedPlayerPerk = "Unlocked player perk: {0}!";
        private const string SuccessUnlockedReligionPerk = "Unlocked religion perk: {0} for all members!";
        private const string NotificationPerkUnlocked = "Your religion unlocked: {0}!";

        // Info messages
        private const string InfoNoPlayerPerks = "You have no unlocked player perks yet.";
        private const string InfoNoReligionPerks = "Your religion has no unlocked perks yet.";

        // Log messages
        private const string LogCommandsRegistered = "[PantheonWars] Perk commands registered";

        // Format strings
        private const string HeaderPerksForDeity = "=== Perks for {0} ===";
        private const string HeaderPlayerPerks = "--- Player Perks ---";
        private const string HeaderReligionPerks = "--- Religion Perks ---";
        private const string HeaderUnlockedPlayerPerks = "=== Your Unlocked Player Perks ({0}) ===";
        private const string HeaderReligionPerksWithName = "=== {0} Perks ({1}) ===";
        private const string HeaderPerkInfo = "=== {0} ===";
        private const string HeaderPerkTree = "=== {0} {1} Perk Tree ===";
        private const string HeaderActivePerks = "=== Active Perks & Stat Modifiers ===";
        private const string HeaderRankSection = "--- {0} Rank ---";

        private const string LabelUnlocked = "[UNLOCKED]";
        private const string LabelChecked = "[✓]";
        private const string LabelUnchecked = "[ ]";

        private const string FormatPerkId = "  ID: {0}";
        private const string FormatRequiredRank = "  Required Rank: {0}";
        private const string FormatDescription = "  {0}";
        private const string FormatPerkNameCategory = "{0} ({1})";
        private const string FormatStatModifier = "    {0}: +{1:F1}%";
        private const string FormatStatModifierPercent = "  {0}: +{1:F1}%";

        private const string LabelId = "ID: {0}";
        private const string LabelDeity = "Deity: {0}";
        private const string LabelType = "Type: {0}";
        private const string LabelCategory = "Category: {0}";
        private const string LabelDescriptionStandalone = "Description: {0}";
        private const string LabelRequiredFavorRank = "Required Favor Rank: {0}";
        private const string LabelRequiredPrestigeRank = "Required Prestige Rank: {0}";
        private const string LabelPrerequisites = "Prerequisites:";
        private const string LabelStatModifiers = "Stat Modifiers:";
        private const string LabelSpecialEffects = "Special Effects:";
        private const string LabelEffects = "  Effects:";
        private const string LabelEffectsForAllMembers = "  Effects (for all members):";
        private const string LabelRequires = "    Requires: ";
        private const string LabelPrerequisiteItem = "  - {0}";
        private const string LabelSpecialEffectItem = "  - {0}";

        private const string LabelPlayerPerksSection = "Player Perks ({0}):";
        private const string LabelReligionPerksSection = "Religion Perks ({0}):";
        private const string LabelCombinedStatModifiers = "Combined Stat Modifiers:";
        private const string LabelNoActiveModifiers = "  No active modifiers";
        private const string LabelNone = "  None";
        private const string LabelPerkItem = "  - {0}";

        private const string TypePlayer = "player";
        private const string TypeReligion = "religion";

        #endregion

        private readonly ICoreServerAPI _sapi = sapi ?? throw new ArgumentNullException($"{nameof(sapi)}");
        private readonly IPerkRegistry _perkRegistry = perkRegistry ?? throw new ArgumentNullException($"{nameof(sapi)}");
        private readonly IPlayerReligionDataManager _playerReligionDataManager = playerReligionDataManager ?? throw new ArgumentNullException($"{nameof(sapi)}");
        private readonly IReligionManager _religionManager = religionManager ?? throw new ArgumentNullException($"{nameof(sapi)}");
        private readonly IPerkEffectSystem _perkEffectSystem = perkEffectSystem ?? throw new ArgumentNullException($"{nameof(sapi)}");

        /// <summary>
        /// Registers all perk commands
        /// </summary>
        public void RegisterCommands()
        {
            _sapi.ChatCommands.Create(CommandName)
                .WithDescription(CommandDescription)
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand(SubCommandList)
                    .WithDescription(DescriptionList)
                    .HandleWith(OnPerksList)
                .EndSubCommand()
                .BeginSubCommand(SubCommandPlayer)
                    .WithDescription(DescriptionPlayer)
                    .HandleWith(OnPerksPlayer)
                .EndSubCommand()
                .BeginSubCommand(SubCommandReligion)
                    .WithDescription(DescriptionReligion)
                    .HandleWith(OnPerksReligion)
                .EndSubCommand()
                .BeginSubCommand(SubCommandInfo)
                    .WithDescription(DescriptionInfo)
                    .WithArgs(_sapi.ChatCommands.Parsers.Word(ParamPerkId))
                    .HandleWith(OnPerksInfo)
                .EndSubCommand()
                .BeginSubCommand(SubCommandTree)
                    .WithDescription(DescriptionTree)
                    .WithArgs(_sapi.ChatCommands.Parsers.OptionalWord(ParamType))
                    .HandleWith(OnPerksTree)
                .EndSubCommand()
                .BeginSubCommand(SubCommandUnlock)
                    .WithDescription(DescriptionUnlock)
                    .WithArgs(_sapi.ChatCommands.Parsers.Word(ParamPerkId))
                    .HandleWith(OnPerksUnlock)
                .EndSubCommand()
                .BeginSubCommand(SubCommandActive)
                    .WithDescription(DescriptionActive)
                    .HandleWith(OnPerksActive)
                .EndSubCommand();

            _sapi.Logger.Notification(LogCommandsRegistered);
        }

        /// <summary>
        /// /perks list - Lists all available perks for player's deity
        /// </summary>
        private TextCommandResult OnPerksList(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error(ErrorPlayerNotFound);

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);

            if (playerData.ActiveDeity == DeityType.None)
            {
                return TextCommandResult.Error(ErrorMustJoinReligion);
            }

            var playerPerks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, PerkKind.Player);
            var religionPerks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, PerkKind.Religion);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(HeaderPerksForDeity, playerData.ActiveDeity));
            sb.AppendLine();

            sb.AppendLine(HeaderPlayerPerks);
            foreach (var perk in playerPerks)
            {
                string status = playerData.IsPerkUnlocked(perk.PerkId) ? LabelUnlocked : "";
                FavorRank requiredRank = (FavorRank)perk.RequiredFavorRank;
                sb.AppendLine($"{perk.Name} {status}");
                sb.AppendLine(string.Format(FormatPerkId, perk.PerkId));
                sb.AppendLine(string.Format(FormatRequiredRank, requiredRank));
                sb.AppendLine(string.Format(FormatDescription, perk.Description));
                sb.AppendLine();
            }

            sb.AppendLine(HeaderReligionPerks);
            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;
            foreach (var perk in religionPerks)
            {
                bool unlocked = religion?.UnlockedPerks.TryGetValue(perk.PerkId, out bool u) == true && u;
                string status = unlocked ? LabelUnlocked : "";
                PrestigeRank requiredRank = (PrestigeRank)perk.RequiredPrestigeRank;
                sb.AppendLine($"{perk.Name} {status}");
                sb.AppendLine(string.Format(FormatPerkId, perk.PerkId));
                sb.AppendLine(string.Format(FormatRequiredRank, requiredRank));
                sb.AppendLine(string.Format(FormatDescription, perk.Description));
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
            if (player == null) return TextCommandResult.Error(ErrorPlayerNotFound);

            var (playerPerks, _) = _perkEffectSystem.GetActivePerks(player.PlayerUID);

            if (playerPerks.Count == 0)
            {
                return TextCommandResult.Success(InfoNoPlayerPerks);
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(HeaderUnlockedPlayerPerks, playerPerks.Count));
            sb.AppendLine();

            foreach (var perk in playerPerks)
            {
                sb.AppendLine(string.Format(FormatPerkNameCategory, perk.Name, perk.Category));
                sb.AppendLine(string.Format(FormatDescription, perk.Description));

                if (perk.StatModifiers.Count > 0)
                {
                    sb.AppendLine(LabelEffects);
                    foreach (var mod in perk.StatModifiers)
                    {
                        sb.AppendLine(string.Format(FormatStatModifier, mod.Key, mod.Value * 100));
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
            if (player == null) return TextCommandResult.Error(ErrorPlayerNotFound);

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.ReligionUID == null)
            {
                return TextCommandResult.Error(ErrorNoReligion);
            }

            var (_, religionPerks) = _perkEffectSystem.GetActivePerks(player.PlayerUID);

            if (religionPerks.Count == 0)
            {
                return TextCommandResult.Success(InfoNoReligionPerks);
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(HeaderReligionPerksWithName, religion?.ReligionName, religionPerks.Count));
            sb.AppendLine();

            foreach (var perk in religionPerks)
            {
                sb.AppendLine(string.Format(FormatPerkNameCategory, perk.Name, perk.Category));
                sb.AppendLine(string.Format(FormatDescription, perk.Description));

                if (perk.StatModifiers.Count > 0)
                {
                    sb.AppendLine(LabelEffectsForAllMembers);
                    foreach (var mod in perk.StatModifiers)
                    {
                        sb.AppendLine(string.Format(FormatStatModifier, mod.Key, mod.Value * 100));
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
                return TextCommandResult.Error(UsagePerksInfo);
            }

            var perk = _perkRegistry.GetPerk(perkId);
            if (perk == null)
            {
                return TextCommandResult.Error(string.Format(ErrorPerkNotFound, perkId));
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(HeaderPerkInfo, perk.Name));
            sb.AppendLine(string.Format(LabelId, perk.PerkId));
            sb.AppendLine(string.Format(LabelDeity, perk.Deity));
            sb.AppendLine(string.Format(LabelType, perk.Kind));
            sb.AppendLine(string.Format(LabelCategory, perk.Category));
            sb.AppendLine();
            sb.AppendLine(string.Format(LabelDescriptionStandalone, perk.Description));
            sb.AppendLine();

            if (perk.Kind == PerkKind.Player)
            {
                FavorRank requiredRank = (FavorRank)perk.RequiredFavorRank;
                sb.AppendLine(string.Format(LabelRequiredFavorRank, requiredRank));
            }
            else
            {
                PrestigeRank requiredRank = (PrestigeRank)perk.RequiredPrestigeRank;
                sb.AppendLine(string.Format(LabelRequiredPrestigeRank, requiredRank));
            }

            if (perk.PrerequisitePerks.Count > 0)
            {
                sb.AppendLine(LabelPrerequisites);
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    var prereq = _perkRegistry.GetPerk(prereqId);
                    string prereqName = prereq?.Name ?? prereqId;
                    sb.AppendLine(string.Format(LabelPrerequisiteItem, prereqName));
                }
            }

            if (perk.StatModifiers.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(LabelStatModifiers);
                foreach (var mod in perk.StatModifiers)
                {
                    sb.AppendLine(string.Format(FormatStatModifierPercent, mod.Key, mod.Value * 100));
                }
            }

            if (perk.SpecialEffects.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(LabelSpecialEffects);
                foreach (var effect in perk.SpecialEffects)
                {
                    sb.AppendLine(string.Format(LabelSpecialEffectItem, effect));
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
            if (player == null) return TextCommandResult.Error(ErrorPlayerNotFound);

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.ActiveDeity == DeityType.None)
            {
                return TextCommandResult.Error(ErrorMustJoinReligionForTree);
            }

            string type = args[0] as string ?? TypePlayer;
            type = type.ToLower();

            PerkKind perkKind = type == TypeReligion ? PerkKind.Religion : PerkKind.Player;
            var perks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, perkKind);

            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(HeaderPerkTree, playerData.ActiveDeity, perkKind));
            sb.AppendLine();

            // Group by rank
            if (perkKind == PerkKind.Player)
            {
                foreach (FavorRank rank in Enum.GetValues(typeof(FavorRank)))
                {
                    var rankPerks = perks.Where(p => p.RequiredFavorRank == (int)rank).ToList();
                    if (rankPerks.Count == 0) continue;

                    sb.AppendLine(string.Format(HeaderRankSection, rank));
                    foreach (var perk in rankPerks)
                    {
                        bool unlocked = playerData.IsPerkUnlocked(perk.PerkId);
                        string status = unlocked ? LabelChecked : LabelUnchecked;
                        sb.AppendLine($"{status} {perk.Name}");

                        if (perk.PrerequisitePerks.Count > 0)
                        {
                            sb.Append(LabelRequires);
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

                    sb.AppendLine(string.Format(HeaderRankSection, rank));
                    foreach (var perk in rankPerks)
                    {
                        bool unlocked = religion?.UnlockedPerks.TryGetValue(perk.PerkId, out bool u) == true && u;
                        string status = unlocked ? LabelChecked : LabelUnchecked;
                        sb.AppendLine($"{status} {perk.Name}");

                        if (perk.PrerequisitePerks.Count > 0)
                        {
                            sb.Append(LabelRequires);
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
            if (player == null) return TextCommandResult.Error(ErrorPlayerNotFound);

            var perkId = args[0] as string;
            if (string.IsNullOrEmpty(perkId))
            {
                return TextCommandResult.Error(UsagePerksUnlock);
            }

            var perk = _perkRegistry.GetPerk(perkId);
            if (perk == null)
            {
                return TextCommandResult.Error(string.Format(ErrorPerkNotFound, perkId));
            }

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;

            // Check if can unlock
            var (canUnlock, reason) = _perkRegistry.CanUnlockPerk(playerData, religion, perk);
            if (!canUnlock)
            {
                return TextCommandResult.Error(string.Format(ErrorCannotUnlockPerk, reason));
            }

            // Unlock the perk
            if (perk.Kind == PerkKind.Player)
            {
                if (religion == null)
                {
                    return TextCommandResult.Error(ErrorMustBeInReligionToUnlock);
                }

                bool success = _playerReligionDataManager.UnlockPlayerPerk(player.PlayerUID, perkId);
                if (!success)
                {
                    return TextCommandResult.Error(ErrorFailedToUnlock);
                }

                _perkEffectSystem.RefreshPlayerPerks(player.PlayerUID);
                return TextCommandResult.Success(string.Format(SuccessUnlockedPlayerPerk, perk.Name));
            }
            else // Religion perk
            {
                if (religion == null)
                {
                    return TextCommandResult.Error(ErrorMustBeInReligionToUnlock);
                }

                // Only founder can unlock religion perks (optional restriction)
                if (!religion.IsFounder(player.PlayerUID))
                {
                    return TextCommandResult.Error(ErrorOnlyFounderCanUnlock);
                }

                religion.UnlockedPerks[perkId] = true;
                _perkEffectSystem.RefreshReligionPerks(religion.ReligionUID);

                // Notify all members
                foreach (var memberUID in religion.MemberUIDs)
                {
                    var member = _sapi.World.PlayerByUid(memberUID) as IServerPlayer;
                    member?.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        string.Format(NotificationPerkUnlocked, perk.Name),
                        EnumChatType.Notification
                    );
                }

                return TextCommandResult.Success(string.Format(SuccessUnlockedReligionPerk, perk.Name));
            }
        }

        /// <summary>
        /// /perks active - Shows all active perks and combined modifiers
        /// </summary>
        private TextCommandResult OnPerksActive(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error(ErrorPlayerNotFound);

            var (playerPerks, religionPerks) = _perkEffectSystem.GetActivePerks(player.PlayerUID);
            var combinedModifiers = _perkEffectSystem.GetCombinedStatModifiers(player.PlayerUID);

            var sb = new StringBuilder();
            sb.AppendLine(HeaderActivePerks);
            sb.AppendLine();

            sb.AppendLine(string.Format(LabelPlayerPerksSection, playerPerks.Count));
            if (playerPerks.Count == 0)
            {
                sb.AppendLine(LabelNone);
            }
            else
            {
                foreach (var perk in playerPerks)
                {
                    sb.AppendLine(string.Format(LabelPerkItem, perk.Name));
                }
            }
            sb.AppendLine();

            sb.AppendLine(string.Format(LabelReligionPerksSection, religionPerks.Count));
            if (religionPerks.Count == 0)
            {
                sb.AppendLine(LabelNone);
            }
            else
            {
                foreach (var perk in religionPerks)
                {
                    sb.AppendLine(string.Format(LabelPerkItem, perk.Name));
                }
            }
            sb.AppendLine();

            sb.AppendLine(LabelCombinedStatModifiers);
            if (combinedModifiers.Count == 0)
            {
                sb.AppendLine(LabelNoActiveModifiers);
            }
            else
            {
                sb.AppendLine(_perkEffectSystem.FormatStatModifiers(combinedModifiers));
            }

            return TextCommandResult.Success(sb.ToString());
        }
    }
}
