using System.Collections.Generic;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;

namespace PantheonWars.Systems.Interfaces;

public interface IPerkRegistry
{
    /// <summary>
    ///     Initializes the perk registry and registers all perks
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Registers a perk in the system
    /// </summary>
    void RegisterPerk(Perk perk);

    /// <summary>
    ///     Gets a perk by its ID
    /// </summary>
    Perk? GetPerk(string perkId);

    /// <summary>
    ///     Gets all perks for a specific deity and type
    /// </summary>
    List<Perk> GetPerksForDeity(DeityType deity, PerkKind? type = null);

    /// <summary>
    ///     Gets all perks in the registry
    /// </summary>
    List<Perk> GetAllPerks();

    /// <summary>
    ///     Checks if a perk can be unlocked by a player/religion
    /// </summary>
    (bool canUnlock, string reason) CanUnlockPerk(
        PlayerReligionData playerData,
        ReligionData? religionData,
        Perk? perk);
}