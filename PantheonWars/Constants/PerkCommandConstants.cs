namespace PantheonWars.Constants;

// Command and subcommand names
public static class PerkCommandConstants
{
    public const string CommandName = "perks";
    public const string SubCommandList = "list";
    public const string SubCommandPlayer = "player";
    public const string SubCommandReligion = "religion";
    public const string SubCommandInfo = "info";
    public const string SubCommandTree = "tree";
    public const string SubCommandUnlock = "unlock";
    public const string SubCommandActive = "active";
}

// Parameter names
public static class ParameterConstants
{
    public const string ParamPerkId = "perkid";
    public const string ParamType = "type";
}

// Command descriptions
public static class PerkDescriptionConstants
{
    public const string CommandDescription = "Manage perks and view perk information";
    public const string DescriptionList = "List all available perks for your deity";
    public const string DescriptionPlayer = "Show your unlocked player perks";
    public const string DescriptionReligion = "Show your religion's unlocked perks";
    public const string DescriptionInfo = "Show detailed information about a perk";
    public const string DescriptionTree = "Display perk tree (player or religion)";
    public const string DescriptionUnlock = "Unlock a perk";
    public const string DescriptionActive = "Show all active perks and stat modifiers";
}

// Common messages
public static class ErrorMessageConstants
{
    public const string ErrorPlayerNotFound = "Player not found";
    public const string ErrorNoReligion = "You are not in a religion.";
    public const string ErrorMustJoinReligion = "You must join a religion to view perks. Use /religion to get started.";
    public const string ErrorMustJoinReligionForTree = "You must join a religion to view perk trees.";
    public const string ErrorMustBeInReligionToUnlock = "You must be in a religion to unlock player perks.";
    public const string ErrorOnlyFounderCanUnlock = "Only the religion founder can unlock religion perks.";
    public const string ErrorPerkNotFound = "Perk not found: {0}";
    public const string ErrorCannotUnlockPerk = "Cannot unlock perk: {0}";
    public const string ErrorFailedToUnlock = "Failed to unlock perk (may already be unlocked)";
}

// Usage messages
public static class UsageMessageConstants
{
    public const string UsagePerksInfo = "Usage: /perks info <perkid>";
    public const string UsagePerksUnlock = "Usage: /perks unlock <perkid>";
}

// Success messages
public static class SuccessMessageConstants
{
    public const string SuccessUnlockedPlayerPerk = "Unlocked player perk: {0}!";
    public const string SuccessUnlockedReligionPerk = "Unlocked religion perk: {0} for all members!";
    public const string NotificationPerkUnlocked = "Your religion unlocked: {0}!";
}

// Info messages
public static class InfoMessageConstants
{
    public const string InfoNoPlayerPerks = "You have no unlocked player perks yet.";
    public const string InfoNoReligionPerks = "Your religion has no unlocked perks yet.";
}

// Log messages
public static class LogMessageConstants
{
    public const string LogPerkCommandsRegistered = "[PantheonWars] Perk commands registered";
}

// Format strings
public static class FormatStringConstants
{
    public const string HeaderPerksForDeity = "=== Perks for {0} ===";
    public const string HeaderPlayerPerks = "--- Player Perks ---";
    public const string HeaderReligionPerks = "--- Religion Perks ---";
    public const string HeaderUnlockedPlayerPerks = "=== Your Unlocked Player Perks ({0}) ===";
    public const string HeaderReligionPerksWithName = "=== {0} Perks ({1}) ===";
    public const string HeaderPerkInfo = "=== {0} ===";
    public const string HeaderPerkTree = "=== {0} {1} Perk Tree ===";
    public const string HeaderActivePerks = "=== Active Perks & Stat Modifiers ===";
    public const string HeaderRankSection = "--- {0} Rank ---";
    public const string LabelUnlocked = "[UNLOCKED]";
    public const string LabelChecked = "[✓]";
    public const string LabelUnchecked = "[ ]";
    public const string FormatPerkId = "  ID: {0}";
    public const string FormatRequiredRank = "  Required Rank: {0}";
    public const string FormatDescription = "  {0}";
    public const string FormatPerkNameCategory = "{0} ({1})";
    public const string FormatStatModifier = "    {0}: +{1:F1}%";
    public const string FormatStatModifierPercent = "  {0}: +{1:F1}%";
    public const string LabelId = "ID: {0}";
    public const string LabelDeity = "Deity: {0}";
    public const string LabelType = "Type: {0}";
    public const string LabelCategory = "Category: {0}";
    public const string LabelDescriptionStandalone = "Description: {0}";
    public const string LabelRequiredFavorRank = "Required Favor Rank: {0}";
    public const string LabelRequiredPrestigeRank = "Required Prestige Rank: {0}";
    public const string LabelPrerequisites = "Prerequisites:";
    public const string LabelStatModifiers = "Stat Modifiers:";
    public const string LabelSpecialEffects = "Special Effects:";
    public const string LabelEffects = "  Effects:";
    public const string LabelEffectsForAllMembers = "  Effects (for all members):";
    public const string LabelRequires = "    Requires: ";
    public const string LabelPrerequisiteItem = "  - {0}";
    public const string LabelSpecialEffectItem = "  - {0}";
    public const string LabelPlayerPerksSection = "Player Perks ({0}):";
    public const string LabelReligionPerksSection = "Religion Perks ({0}):";
    public const string LabelCombinedStatModifiers = "Combined Stat Modifiers:";
    public const string LabelNoActiveModifiers = "  No active modifiers";
    public const string LabelNone = "  None";
    public const string LabelPerkItem = "  - {0}";
    public const string TypePlayer = "player";
    public const string TypeReligion = "religion";
}