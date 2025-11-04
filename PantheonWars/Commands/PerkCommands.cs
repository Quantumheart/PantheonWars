using System;
using System.Linq;
using System.Text;
using PantheonWars.Constants;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
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
            _sapi.ChatCommands.Create(PerkCommandConstants.CommandName)

            .WithDescription(PerkDescriptionConstants.CommandDescription)
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand(PerkCommandConstants.SubCommandList)
                .WithDescription(PerkDescriptionConstants.DescriptionList)
                .HandleWith(OnPerksList)
                .EndSubCommand()

                .BeginSubCommand(PerkCommandConstants.SubCommandPlayer)
                .WithDescription(PerkDescriptionConstants.DescriptionPlayer)
                .HandleWith(OnPerksPlayer)
                .EndSubCommand()

                .BeginSubCommand(PerkCommandConstants.SubCommandReligion)
                .WithDescription(PerkDescriptionConstants.DescriptionReligion)
                .HandleWith(OnPerksReligion)
                .EndSubCommand()

            .BeginSubCommand(PerkCommandConstants.SubCommandInfo)
                .WithDescription(PerkDescriptionConstants.DescriptionInfo)
                .WithArgs(_sapi.ChatCommands.Parsers.OptionalWord(ParameterConstants.ParamPerkId))
                .HandleWith(OnPerksInfo)
                .EndSubCommand()

            .BeginSubCommand(PerkCommandConstants.SubCommandTree)
                .WithDescription(PerkDescriptionConstants.DescriptionTree)
                .WithArgs(_sapi.ChatCommands.Parsers.Word(ParameterConstants.ParamType))
                .HandleWith(OnPerksTree)
                .EndSubCommand()

            .BeginSubCommand(PerkCommandConstants.SubCommandUnlock)
                .WithDescription(PerkDescriptionConstants.DescriptionUnlock)
                .WithArgs(_sapi.ChatCommands.Parsers.Word(ParameterConstants.ParamPerkId))
                .HandleWith(OnPerksUnlock)
                .EndSubCommand()

            .BeginSubCommand(PerkCommandConstants.SubCommandActive)
                .WithDescription(PerkDescriptionConstants.DescriptionActive)
                .HandleWith(OnPerksActive)
                .EndSubCommand();

            _sapi.Logger.Notification(LogMessageConstants.LogPerkCommandsRegistered);
        }

        /// <summary>
        /// /perks list - Lists all available perks for player's deity
        /// </summary>
        internal TextCommandResult OnPerksList(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);

            if (playerData.ActiveDeity == DeityType.None)
            {
                return TextCommandResult.Error(ErrorMessageConstants.ErrorMustJoinReligion);
            }

            var playerPerks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, PerkKind.Player);
            var religionPerks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, PerkKind.Religion);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(FormatStringConstants.HeaderPerksForDeity, playerData.ActiveDeity));
            sb.AppendLine();

            sb.AppendLine(FormatStringConstants.HeaderPlayerPerks);
            foreach (var perk in playerPerks)
            {
                string status = playerData.IsPerkUnlocked(perk.PerkId) ? FormatStringConstants.LabelUnlocked : "";
                FavorRank requiredRank = (FavorRank)perk.RequiredFavorRank;
                sb.AppendLine($"{perk.Name} {status}");
                sb.AppendLine(string.Format(FormatStringConstants.FormatPerkId, perk.PerkId));
                sb.AppendLine(string.Format(FormatStringConstants.FormatRequiredRank, requiredRank));
                sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, perk.Description));
                sb.AppendLine();
            }

            sb.AppendLine(FormatStringConstants.HeaderReligionPerks);
            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;
            foreach (var perk in religionPerks)
            {
                bool unlocked = religion?.UnlockedPerks.TryGetValue(perk.PerkId, out bool u) == true && u;
                string status = unlocked ? FormatStringConstants.LabelUnlocked : "";
                PrestigeRank requiredRank = (PrestigeRank)perk.RequiredPrestigeRank;
                sb.AppendLine($"{perk.Name} {status}");
                sb.AppendLine(string.Format(FormatStringConstants.FormatPerkId, perk.PerkId));
                sb.AppendLine(string.Format(FormatStringConstants.FormatRequiredRank, requiredRank));
                sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, perk.Description));
                sb.AppendLine();
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks player - Shows unlocked player perks
        /// </summary>
        internal TextCommandResult OnPerksPlayer(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

            var (playerPerks, _) = _perkEffectSystem.GetActivePerks(player.PlayerUID);

            if (playerPerks.Count == 0)
            {
                return TextCommandResult.Success(InfoMessageConstants.InfoNoPlayerPerks);
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(FormatStringConstants.HeaderUnlockedPlayerPerks, playerPerks.Count));
            sb.AppendLine();

            foreach (var perk in playerPerks)
            {
                sb.AppendLine(string.Format(FormatStringConstants.FormatPerkNameCategory, perk.Name, perk.Category));
                sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, perk.Description));

                if (perk.StatModifiers.Count > 0)
                {
                    sb.AppendLine(FormatStringConstants.LabelEffects);
                    foreach (var mod in perk.StatModifiers)
                    {
                        sb.AppendLine(string.Format(FormatStringConstants.FormatStatModifier, mod.Key,
                            mod.Value * 100));
                    }
                }

                sb.AppendLine();
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks religion - Shows religion's unlocked perks
        /// </summary>
        internal TextCommandResult OnPerksReligion(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.ReligionUID == null)
            {
                return TextCommandResult.Error(ErrorMessageConstants.ErrorNoReligion);
            }

            var (_, religionPerks) = _perkEffectSystem.GetActivePerks(player.PlayerUID);

            if (religionPerks.Count == 0)
            {
                return TextCommandResult.Success(InfoMessageConstants.InfoNoReligionPerks);
            }

            var religion = _religionManager.GetReligion(playerData.ReligionUID);
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(FormatStringConstants.HeaderReligionPerksWithName, religion?.ReligionName,
                religionPerks.Count));
            sb.AppendLine();

            foreach (var perk in religionPerks)
            {
                sb.AppendLine(string.Format(FormatStringConstants.FormatPerkNameCategory, perk.Name, perk.Category));
                sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, perk.Description));

                if (perk.StatModifiers.Count > 0)
                {
                    sb.AppendLine(FormatStringConstants.LabelEffectsForAllMembers);
                    foreach (var mod in perk.StatModifiers)
                    {
                        sb.AppendLine(string.Format(FormatStringConstants.FormatStatModifier, mod.Key,
                            mod.Value * 100));
                    }
                }

                sb.AppendLine();
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks info <perkid/> - Shows detailed perk information
        /// </summary>
        internal TextCommandResult OnPerksInfo(TextCommandCallingArgs args)
        {
            var perkId = args[0] as string;
            if (string.IsNullOrEmpty(perkId))
            {
                return TextCommandResult.Error(UsageMessageConstants.UsagePerksInfo);
            }

            var perk = _perkRegistry.GetPerk(perkId);
            if (perk == null)
            {
                return TextCommandResult.Error(string.Format(ErrorMessageConstants.ErrorPerkNotFound,
                    perkId));
            }

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(FormatStringConstants.HeaderPerkInfo, perk.Name));
            sb.AppendLine(string.Format(FormatStringConstants.LabelId, perk.PerkId));
            sb.AppendLine(string.Format(FormatStringConstants.LabelDeity, perk.Deity));
            sb.AppendLine(string.Format(FormatStringConstants.LabelType, perk.Kind));
            sb.AppendLine(string.Format(FormatStringConstants.LabelCategory, perk.Category));
            sb.AppendLine();
            sb.AppendLine(string.Format(FormatStringConstants.LabelDescriptionStandalone,
                perk.Description));
            sb.AppendLine();

            if (perk.Kind == PerkKind.Player)
            {
                FavorRank requiredRank = (FavorRank)perk.RequiredFavorRank;
                sb.AppendLine(
                    string.Format(FormatStringConstants.LabelRequiredFavorRank, requiredRank));
            }
            else
            {
                PrestigeRank requiredRank = (PrestigeRank)perk.RequiredPrestigeRank;
                sb.AppendLine(string.Format(FormatStringConstants.LabelRequiredPrestigeRank,
                    requiredRank));
            }

            if (perk.PrerequisitePerks.Count > 0)
            {
                sb.AppendLine(FormatStringConstants.LabelPrerequisites);
                foreach (var prereqId in perk.PrerequisitePerks)
                {
                    var prereq = _perkRegistry.GetPerk(prereqId);
                    string prereqName = prereq?.Name ?? prereqId;
                    sb.AppendLine(string.Format(FormatStringConstants.LabelPrerequisiteItem,
                        prereqName));
                }
            }

            if (perk.StatModifiers.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(FormatStringConstants.LabelStatModifiers);
                foreach (var mod in perk.StatModifiers)
                {
                    sb.AppendLine(string.Format(FormatStringConstants.FormatStatModifierPercent,
                        mod.Key, mod.Value * 100));
                }
            }

            if (perk.SpecialEffects.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(FormatStringConstants.LabelSpecialEffects);
                foreach (var effect in perk.SpecialEffects)
                {
                    sb.AppendLine(string.Format(FormatStringConstants.LabelSpecialEffectItem, effect));
                }
            }

            return TextCommandResult.Success(sb.ToString());
        }

        /// <summary>
        /// /perks tree [player/religion] - Displays perk tree
        /// </summary>
        internal TextCommandResult OnPerksTree(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null)
            {
                return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);
            }

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            if (playerData.ActiveDeity == DeityType.None)
            {
                return TextCommandResult.Error(ErrorMessageConstants.ErrorMustJoinReligionForTree);
            }

            string type = args[0] as string ?? FormatStringConstants.TypePlayer;
            type = type.ToLower();

            PerkKind perkKind = type == FormatStringConstants.TypeReligion
                ? PerkKind.Religion
                : PerkKind.Player;

            var perks = _perkRegistry.GetPerksForDeity(playerData.ActiveDeity, perkKind);

            var religion = playerData.ReligionUID != null
                ? _religionManager.GetReligion(playerData.ReligionUID)
                : null;

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(FormatStringConstants.HeaderPerkTree, playerData.ActiveDeity, perkKind));
            sb.AppendLine();

            // Group by rank
            if (perkKind == PerkKind.Player)
            {
                foreach (FavorRank rank in Enum.GetValues(typeof(FavorRank)))
                {
                    var rankPerks = perks
                        .Where(p => p.RequiredFavorRank == (int)rank)
                        .ToList();

                    if (rankPerks.Count == 0)
                        continue;

                    sb.AppendLine(string.Format(FormatStringConstants.HeaderRankSection, rank));
                    foreach (var perk in rankPerks)
                    {
                        bool unlocked = playerData.IsPerkUnlocked(perk.PerkId);
                        string status = unlocked
                            ? FormatStringConstants.LabelChecked
                            : FormatStringConstants.LabelUnchecked;
                        sb.AppendLine($"{status} {perk.Name}");

                        if (perk.PrerequisitePerks.Count > 0)
                        {
                            sb.Append(FormatStringConstants.LabelRequires);
                            var prereqNames = perk.PrerequisitePerks
                                .Select(id =>
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
                    var rankPerks = perks
                        .Where(p => p.RequiredPrestigeRank == (int)rank)
                        .ToList();

                    if (rankPerks.Count == 0)
                        continue;

                    sb.AppendLine(string.Format(FormatStringConstants.HeaderRankSection, rank));
                    foreach (var perk in rankPerks)
                    {
                        bool unlocked = religion?.UnlockedPerks.TryGetValue(perk.PerkId, out bool u) == true && u;
                        string status = unlocked
                            ? FormatStringConstants.LabelChecked
                            : FormatStringConstants.LabelUnchecked;
                        sb.AppendLine($"{status} {perk.Name}");

                        if (perk.PrerequisitePerks.Count > 0)
                        {
                            sb.Append(FormatStringConstants.LabelRequires);
                            var prereqNames = perk.PrerequisitePerks
                                .Select(id =>
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
        /// /perks unlock <perkid/> - Unlocks a perk
        /// </summary>
        internal TextCommandResult OnPerksUnlock(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);
            var perkId = args[0] as string;
            if (string.IsNullOrEmpty(perkId))
            {
                return TextCommandResult.Error(UsageMessageConstants.UsagePerksUnlock);
            }

            var perk = _perkRegistry.GetPerk(perkId);
            if (perk == null)
            {
                return TextCommandResult.Error(string.Format(ErrorMessageConstants.ErrorPerkNotFound, perkId));
            }

            var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
            var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;
            
            var (canUnlock, reason) = _perkRegistry.CanUnlockPerk(playerData, religion, perk);
            if (!canUnlock)
            {
                return TextCommandResult.Error(string.Format(ErrorMessageConstants.ErrorCannotUnlockPerk, reason));
            }

            // Unlock the perk
            if (perk.Kind == PerkKind.Player)
            {
                if (religion == null)
                {
                    return TextCommandResult.Error(ErrorMessageConstants.ErrorMustBeInReligionToUnlock);
                }

                bool success = _playerReligionDataManager.UnlockPlayerPerk(player.PlayerUID, perkId);
                if (!success)
                {
                    return TextCommandResult.Error(ErrorMessageConstants.ErrorFailedToUnlock);
                }

                _perkEffectSystem.RefreshPlayerPerks(player.PlayerUID);
                return TextCommandResult.Success(string.Format(SuccessMessageConstants.SuccessUnlockedPlayerPerk,
                    perk.Name));
            }
            else // Religion perk
            {
                if (religion == null)
                {
                    return TextCommandResult.Error(ErrorMessageConstants.ErrorMustBeInReligionToUnlock);
                }

                // Only founder can unlock religion perks (optional restriction)
                if (!religion.IsFounder(player.PlayerUID))
                {
                    return TextCommandResult.Error(ErrorMessageConstants.ErrorOnlyFounderCanUnlock);
                }

                religion.UnlockedPerks[perkId] = true;
                _perkEffectSystem.RefreshReligionPerks(religion.ReligionUID);
                // Notify all members
                foreach (var memberUid in religion.MemberUIDs)
                {
                    var member = _sapi.World.PlayerByUid(memberUid) as IServerPlayer;
                    member?.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        string.Format(SuccessMessageConstants.NotificationPerkUnlocked, perk.Name),
                        EnumChatType.Notification
                    );
                }

                return TextCommandResult.Success(string.Format(SuccessMessageConstants.SuccessUnlockedReligionPerk,
                    perk.Name));
            }
        }

        /// <summary>
        /// /perks active - Shows all active perks and combined modifiers
        /// </summary>
        internal TextCommandResult OnPerksActive(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;
            if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

            var (playerPerks, religionPerks) = _perkEffectSystem.GetActivePerks(player.PlayerUID);
            var combinedModifiers = _perkEffectSystem.GetCombinedStatModifiers(player.PlayerUID);

            var sb = new StringBuilder();
            sb.AppendLine(FormatStringConstants.HeaderActivePerks);
            sb.AppendLine();

            sb.AppendLine(string.Format(FormatStringConstants.LabelPlayerPerksSection, playerPerks.Count));
            if (playerPerks.Count == 0)
            {
                sb.AppendLine(FormatStringConstants.LabelNone);
            }
            else
            {
                foreach (var perk in playerPerks)
                {
                    sb.AppendLine(string.Format(FormatStringConstants.LabelPerkItem, perk.Name));
                }
            }

            sb.AppendLine();

            sb.AppendLine(string.Format(FormatStringConstants.LabelReligionPerksSection, religionPerks.Count));
            if (religionPerks.Count == 0)
            {
                sb.AppendLine(FormatStringConstants.LabelNone);
            }
            else
            {
                foreach (var perk in religionPerks)
                {
                    sb.AppendLine(string.Format(FormatStringConstants.LabelPerkItem, perk.Name));
                }
            }

            sb.AppendLine();

            sb.AppendLine(FormatStringConstants.LabelCombinedStatModifiers);
            if (combinedModifiers.Count == 0)
            {
                sb.AppendLine(FormatStringConstants.LabelNoActiveModifiers);
            }
            else
            {
                sb.AppendLine(_perkEffectSystem.FormatStatModifiers(combinedModifiers));
            }

            return TextCommandResult.Success(sb.ToString());
        }
    }
}
