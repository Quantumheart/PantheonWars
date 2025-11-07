using System;
using System.Linq;
using System.Text;
using PantheonWars.Constants;
using PantheonWars.Models.Enum;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace PantheonWars.Commands;

/// <summary>
///     Commands for managing blessings (Phase 3.3)
/// </summary>
public class BlessingCommands(
    ICoreServerAPI? sapi,
    IBlessingRegistry? blessingRegistry,
    IPlayerReligionDataManager? playerReligionDataManager,
    IReligionManager? religionManager,
    IBlessingEffectSystem? blessingEffectSystem)
{
    private readonly IBlessingEffectSystem _blessingEffectSystem =
        blessingEffectSystem ?? throw new ArgumentNullException($"{nameof(sapi)}");

    private readonly IBlessingRegistry _blessingRegistry = blessingRegistry ?? throw new ArgumentNullException($"{nameof(sapi)}");

    private readonly IPlayerReligionDataManager _playerReligionDataManager =
        playerReligionDataManager ?? throw new ArgumentNullException($"{nameof(sapi)}");

    private readonly IReligionManager _religionManager =
        religionManager ?? throw new ArgumentNullException($"{nameof(sapi)}");

    private readonly ICoreServerAPI _sapi = sapi ?? throw new ArgumentNullException($"{nameof(sapi)}");

    /// <summary>
    ///     Registers all blessing commands
    /// </summary>
    public void RegisterCommands()
    {
        _sapi.ChatCommands.Create(BlessingCommandConstants.CommandName)
            .WithDescription(BlessingDescriptionConstants.CommandDescription)
            .RequiresPlayer()
            .RequiresPrivilege(Privilege.chat)
            .BeginSubCommand(BlessingCommandConstants.SubCommandList)
            .WithDescription(BlessingDescriptionConstants.DescriptionList)
            .HandleWith(OnBlessingsList)
            .EndSubCommand()
            .BeginSubCommand(BlessingCommandConstants.SubCommandPlayer)
            .WithDescription(BlessingDescriptionConstants.DescriptionPlayer)
            .HandleWith(OnBlessingsPlayer)
            .EndSubCommand()
            .BeginSubCommand(BlessingCommandConstants.SubCommandReligion)
            .WithDescription(BlessingDescriptionConstants.DescriptionReligion)
            .HandleWith(OnBlessingsReligion)
            .EndSubCommand()
            .BeginSubCommand(BlessingCommandConstants.SubCommandInfo)
            .WithDescription(BlessingDescriptionConstants.DescriptionInfo)
            .WithArgs(_sapi.ChatCommands.Parsers.OptionalWord(ParameterConstants.ParamBlessingId))
            .HandleWith(OnBlessingsInfo)
            .EndSubCommand()
            .BeginSubCommand(BlessingCommandConstants.SubCommandTree)
            .WithDescription(BlessingDescriptionConstants.DescriptionTree)
            .WithArgs(_sapi.ChatCommands.Parsers.Word(ParameterConstants.ParamType))
            .HandleWith(OnBlessingsTree)
            .EndSubCommand()
            .BeginSubCommand(BlessingCommandConstants.SubCommandUnlock)
            .WithDescription(BlessingDescriptionConstants.DescriptionUnlock)
            .WithArgs(_sapi.ChatCommands.Parsers.Word(ParameterConstants.ParamBlessingId))
            .HandleWith(OnBlessingsUnlock)
            .EndSubCommand()
            .BeginSubCommand(BlessingCommandConstants.SubCommandActive)
            .WithDescription(BlessingDescriptionConstants.DescriptionActive)
            .HandleWith(OnBlessingsActive)
            .EndSubCommand();

        _sapi.Logger.Notification(LogMessageConstants.LogBlessingCommandsRegistered);
    }

    /// <summary>
    ///     /blessings list - Lists all available blessings for player's deity
    /// </summary>
    internal TextCommandResult OnBlessingsList(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player as IServerPlayer;
        if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

        var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);

        if (playerData.ActiveDeity == DeityType.None)
            return TextCommandResult.Error(ErrorMessageConstants.ErrorMustJoinReligion);

        var playerBlessings = _blessingRegistry.GetBlessingsForDeity(playerData.ActiveDeity, BlessingKind.Player);
        var religionBlessings = _blessingRegistry.GetBlessingsForDeity(playerData.ActiveDeity, BlessingKind.Religion);

        var sb = new StringBuilder();
        sb.AppendLine(string.Format(FormatStringConstants.HeaderBlessingsForDeity, playerData.ActiveDeity));
        sb.AppendLine();

        sb.AppendLine(FormatStringConstants.HeaderPlayerBlessings);
        foreach (var blessing in playerBlessings)
        {
            var status = playerData.IsBlessingUnlocked(blessing.BlessingId) ? FormatStringConstants.LabelUnlocked : "";
            var requiredRank = (FavorRank)blessing.RequiredFavorRank;
            sb.AppendLine($"{blessing.Name} {status}");
            sb.AppendLine(string.Format(FormatStringConstants.FormatBlessingId, blessing.BlessingId));
            sb.AppendLine(string.Format(FormatStringConstants.FormatRequiredRank, requiredRank));
            sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, blessing.Description));
            sb.AppendLine();
        }

        sb.AppendLine(FormatStringConstants.HeaderReligionBlessings);
        var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;
        foreach (var blessing in religionBlessings)
        {
            var unlocked = religion?.UnlockedBlessings.TryGetValue(blessing.BlessingId, out var u) == true && u;
            var status = unlocked ? FormatStringConstants.LabelUnlocked : "";
            var requiredRank = (PrestigeRank)blessing.RequiredPrestigeRank;
            sb.AppendLine($"{blessing.Name} {status}");
            sb.AppendLine(string.Format(FormatStringConstants.FormatBlessingId, blessing.BlessingId));
            sb.AppendLine(string.Format(FormatStringConstants.FormatRequiredRank, requiredRank));
            sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, blessing.Description));
            sb.AppendLine();
        }

        return TextCommandResult.Success(sb.ToString());
    }

    /// <summary>
    ///     /blessings player - Shows unlocked player blessings
    /// </summary>
    internal TextCommandResult OnBlessingsPlayer(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player as IServerPlayer;
        if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

        var (playerBlessings, _) = _blessingEffectSystem.GetActiveBlessings(player.PlayerUID);

        if (playerBlessings.Count == 0) return TextCommandResult.Success(InfoMessageConstants.InfoNoPlayerBlessings);

        var sb = new StringBuilder();
        sb.AppendLine(string.Format(FormatStringConstants.HeaderUnlockedPlayerBlessings, playerBlessings.Count));
        sb.AppendLine();

        foreach (var blessing in playerBlessings)
        {
            sb.AppendLine(string.Format(FormatStringConstants.FormatBlessingNameCategory, blessing.Name, blessing.Category));
            sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, blessing.Description));

            if (blessing.StatModifiers.Count > 0)
            {
                sb.AppendLine(FormatStringConstants.LabelEffects);
                foreach (var mod in blessing.StatModifiers)
                    sb.AppendLine(string.Format(FormatStringConstants.FormatStatModifier, mod.Key,
                        mod.Value * 100));
            }

            sb.AppendLine();
        }

        return TextCommandResult.Success(sb.ToString());
    }

    /// <summary>
    ///     /blessings religion - Shows religion's unlocked blessings
    /// </summary>
    internal TextCommandResult OnBlessingsReligion(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player as IServerPlayer;
        if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

        var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
        if (playerData.ReligionUID == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorNoReligion);

        var (_, religionBlessings) = _blessingEffectSystem.GetActiveBlessings(player.PlayerUID);

        if (religionBlessings.Count == 0) return TextCommandResult.Success(InfoMessageConstants.InfoNoReligionBlessings);

        var religion = _religionManager.GetReligion(playerData.ReligionUID);
        var sb = new StringBuilder();
        sb.AppendLine(string.Format(FormatStringConstants.HeaderReligionBlessingsWithName, religion?.ReligionName,
            religionBlessings.Count));
        sb.AppendLine();

        foreach (var blessing in religionBlessings)
        {
            sb.AppendLine(string.Format(FormatStringConstants.FormatBlessingNameCategory, blessing.Name, blessing.Category));
            sb.AppendLine(string.Format(FormatStringConstants.FormatDescription, blessing.Description));

            if (blessing.StatModifiers.Count > 0)
            {
                sb.AppendLine(FormatStringConstants.LabelEffectsForAllMembers);
                foreach (var mod in blessing.StatModifiers)
                    sb.AppendLine(string.Format(FormatStringConstants.FormatStatModifier, mod.Key,
                        mod.Value * 100));
            }

            sb.AppendLine();
        }

        return TextCommandResult.Success(sb.ToString());
    }

    /// <summary>
    ///     /blessings info <blessingid /> - Shows detailed blessing information
    /// </summary>
    internal TextCommandResult OnBlessingsInfo(TextCommandCallingArgs args)
    {
        var blessingId = args[0] as string;
        if (string.IsNullOrEmpty(blessingId)) return TextCommandResult.Error(UsageMessageConstants.UsageBlessingsInfo);

        var blessing = _blessingRegistry.GetBlessing(blessingId);
        if (blessing == null)
            return TextCommandResult.Error(string.Format(ErrorMessageConstants.ErrorBlessingNotFound,
                blessingId));

        var sb = new StringBuilder();
        sb.AppendLine(string.Format(FormatStringConstants.HeaderBlessingInfo, blessing.Name));
        sb.AppendLine(string.Format(FormatStringConstants.LabelId, blessing.BlessingId));
        sb.AppendLine(string.Format(FormatStringConstants.LabelDeity, blessing.Deity));
        sb.AppendLine(string.Format(FormatStringConstants.LabelType, blessing.Kind));
        sb.AppendLine(string.Format(FormatStringConstants.LabelCategory, blessing.Category));
        sb.AppendLine();
        sb.AppendLine(string.Format(FormatStringConstants.LabelDescriptionStandalone,
            blessing.Description));
        sb.AppendLine();

        if (blessing.Kind == BlessingKind.Player)
        {
            var requiredRank = (FavorRank)blessing.RequiredFavorRank;
            sb.AppendLine(
                string.Format(FormatStringConstants.LabelRequiredFavorRank, requiredRank));
        }
        else
        {
            var requiredRank = (PrestigeRank)blessing.RequiredPrestigeRank;
            sb.AppendLine(string.Format(FormatStringConstants.LabelRequiredPrestigeRank,
                requiredRank));
        }

        if (blessing.PrerequisiteBlessings.Count > 0)
        {
            sb.AppendLine(FormatStringConstants.LabelPrerequisites);
            foreach (var prereqId in blessing.PrerequisiteBlessings)
            {
                var prereq = _blessingRegistry.GetBlessing(prereqId);
                var prereqName = prereq?.Name ?? prereqId;
                sb.AppendLine(string.Format(FormatStringConstants.LabelPrerequisiteItem,
                    prereqName));
            }
        }

        if (blessing.StatModifiers.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(FormatStringConstants.LabelStatModifiers);
            foreach (var mod in blessing.StatModifiers)
                sb.AppendLine(string.Format(FormatStringConstants.FormatStatModifierPercent,
                    mod.Key, mod.Value * 100));
        }

        if (blessing.SpecialEffects.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(FormatStringConstants.LabelSpecialEffects);
            foreach (var effect in blessing.SpecialEffects)
                sb.AppendLine(string.Format(FormatStringConstants.LabelSpecialEffectItem, effect));
        }

        return TextCommandResult.Success(sb.ToString());
    }

    /// <summary>
    ///     /blessings tree [player/religion] - Displays blessing tree
    /// </summary>
    internal TextCommandResult OnBlessingsTree(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player as IServerPlayer;
        if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

        var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
        if (playerData.ActiveDeity == DeityType.None)
            return TextCommandResult.Error(ErrorMessageConstants.ErrorMustJoinReligionForTree);

        var type = args[0] as string ?? FormatStringConstants.TypePlayer;
        type = type.ToLower();

        var blessingKind = type == FormatStringConstants.TypeReligion
            ? BlessingKind.Religion
            : BlessingKind.Player;

        var blessings = _blessingRegistry.GetBlessingsForDeity(playerData.ActiveDeity, blessingKind);

        var religion = playerData.ReligionUID != null
            ? _religionManager.GetReligion(playerData.ReligionUID)
            : null;

        var sb = new StringBuilder();
        sb.AppendLine(string.Format(FormatStringConstants.HeaderBlessingTree, playerData.ActiveDeity, blessingKind));
        sb.AppendLine();

        // Group by rank
        if (blessingKind == BlessingKind.Player)
            foreach (FavorRank rank in Enum.GetValues(typeof(FavorRank)))
            {
                var rankBlessings = blessings
                    .Where(p => p.RequiredFavorRank == (int)rank)
                    .ToList();

                if (rankBlessings.Count == 0)
                    continue;

                sb.AppendLine(string.Format(FormatStringConstants.HeaderRankSection, rank));
                foreach (var blessing in rankBlessings)
                {
                    var unlocked = playerData.IsBlessingUnlocked(blessing.BlessingId);
                    var status = unlocked
                        ? FormatStringConstants.LabelChecked
                        : FormatStringConstants.LabelUnchecked;
                    sb.AppendLine($"{status} {blessing.Name}");

                    if (blessing.PrerequisiteBlessings.Count > 0)
                    {
                        sb.Append(FormatStringConstants.LabelRequires);
                        var prereqNames = blessing.PrerequisiteBlessings
                            .Select(id =>
                            {
                                var p = _blessingRegistry.GetBlessing(id);
                                return p?.Name ?? id;
                            });
                        sb.AppendLine(string.Join(", ", prereqNames));
                    }
                }

                sb.AppendLine();
            }
        else // Religion
            foreach (PrestigeRank rank in Enum.GetValues(typeof(PrestigeRank)))
            {
                var rankBlessings = blessings
                    .Where(p => p.RequiredPrestigeRank == (int)rank)
                    .ToList();

                if (rankBlessings.Count == 0)
                    continue;

                sb.AppendLine(string.Format(FormatStringConstants.HeaderRankSection, rank));
                foreach (var blessing in rankBlessings)
                {
                    var unlocked = religion?.UnlockedBlessings.TryGetValue(blessing.BlessingId, out var u) == true && u;
                    var status = unlocked
                        ? FormatStringConstants.LabelChecked
                        : FormatStringConstants.LabelUnchecked;
                    sb.AppendLine($"{status} {blessing.Name}");

                    if (blessing.PrerequisiteBlessings.Count > 0)
                    {
                        sb.Append(FormatStringConstants.LabelRequires);
                        var prereqNames = blessing.PrerequisiteBlessings
                            .Select(id =>
                            {
                                var p = _blessingRegistry.GetBlessing(id);
                                return p?.Name ?? id;
                            });
                        sb.AppendLine(string.Join(", ", prereqNames));
                    }
                }

                sb.AppendLine();
            }

        return TextCommandResult.Success(sb.ToString());
    }

    /// <summary>
    ///     /blessings unlock <blessingid /> - Unlocks a blessing
    /// </summary>
    internal TextCommandResult OnBlessingsUnlock(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player as IServerPlayer;
        if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);
        var blessingId = args[0] as string;
        if (string.IsNullOrEmpty(blessingId)) return TextCommandResult.Error(UsageMessageConstants.UsageBlessingsUnlock);

        var blessing = _blessingRegistry.GetBlessing(blessingId);
        if (blessing == null)
            return TextCommandResult.Error(string.Format(ErrorMessageConstants.ErrorBlessingNotFound, blessingId));

        var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
        var religion = playerData.ReligionUID != null ? _religionManager.GetReligion(playerData.ReligionUID) : null;

        var (canUnlock, reason) = _blessingRegistry.CanUnlockBlessing(playerData, religion, blessing);
        if (!canUnlock)
            return TextCommandResult.Error(string.Format(ErrorMessageConstants.ErrorCannotUnlockBlessing, reason));

        // Unlock the blessing
        if (blessing.Kind == BlessingKind.Player)
        {
            if (religion == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorMustBeInReligionToUnlock);

            var success = _playerReligionDataManager.UnlockPlayerBlessing(player.PlayerUID, blessingId);
            if (!success) return TextCommandResult.Error(ErrorMessageConstants.ErrorFailedToUnlock);

            _blessingEffectSystem.RefreshPlayerBlessings(player.PlayerUID);
            return TextCommandResult.Success(string.Format(SuccessMessageConstants.SuccessUnlockedPlayerBlessing,
                blessing.Name));
        }

        // Religion blessing
        if (religion == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorMustBeInReligionToUnlock);

        // Only founder can unlock religion blessings (optional restriction)
        if (!religion.IsFounder(player.PlayerUID))
            return TextCommandResult.Error(ErrorMessageConstants.ErrorOnlyFounderCanUnlock);

        religion.UnlockedBlessings[blessingId] = true;
        _blessingEffectSystem.RefreshReligionBlessings(religion.ReligionUID);
        // Notify all members
        foreach (var memberUid in religion.MemberUIDs)
        {
            var member = _sapi.World.PlayerByUid(memberUid) as IServerPlayer;
            member?.SendMessage(
                GlobalConstants.GeneralChatGroup,
                string.Format(SuccessMessageConstants.NotificationBlessingUnlocked, blessing.Name),
                EnumChatType.Notification
            );
        }

        return TextCommandResult.Success(string.Format(SuccessMessageConstants.SuccessUnlockedReligionBlessing,
            blessing.Name));
    }

    /// <summary>
    ///     /blessings active - Shows all active blessings and combined modifiers
    /// </summary>
    internal TextCommandResult OnBlessingsActive(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player as IServerPlayer;
        if (player == null) return TextCommandResult.Error(ErrorMessageConstants.ErrorPlayerNotFound);

        var (playerBlessings, religionBlessings) = _blessingEffectSystem.GetActiveBlessings(player.PlayerUID);
        var combinedModifiers = _blessingEffectSystem.GetCombinedStatModifiers(player.PlayerUID);

        var sb = new StringBuilder();
        sb.AppendLine(FormatStringConstants.HeaderActiveBlessings);
        sb.AppendLine();

        sb.AppendLine(string.Format(FormatStringConstants.LabelPlayerBlessingsSection, playerBlessings.Count));
        if (playerBlessings.Count == 0)
            sb.AppendLine(FormatStringConstants.LabelNone);
        else
            foreach (var blessing in playerBlessings)
                sb.AppendLine(string.Format(FormatStringConstants.LabelBlessingItem, blessing.Name));

        sb.AppendLine();

        sb.AppendLine(string.Format(FormatStringConstants.LabelReligionBlessingsSection, religionBlessings.Count));
        if (religionBlessings.Count == 0)
            sb.AppendLine(FormatStringConstants.LabelNone);
        else
            foreach (var blessing in religionBlessings)
                sb.AppendLine(string.Format(FormatStringConstants.LabelBlessingItem, blessing.Name));

        sb.AppendLine();

        sb.AppendLine(FormatStringConstants.LabelCombinedStatModifiers);
        if (combinedModifiers.Count == 0)
            sb.AppendLine(FormatStringConstants.LabelNoActiveModifiers);
        else
            sb.AppendLine(_blessingEffectSystem.FormatStatModifiers(combinedModifiers));

        return TextCommandResult.Success(sb.ToString());
    }
}