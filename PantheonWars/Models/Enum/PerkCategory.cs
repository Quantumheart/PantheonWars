namespace PantheonWars.Models.Enum
{
    /// <summary>
    /// Category of perk - used for organization and filtering
    /// </summary>
    public enum PerkCategory
    {
        /// <summary>
        /// Combat-focused perks - damage, attack speed, critical hits
        /// </summary>
        Combat,

        /// <summary>
        /// Defensive perks - resistances, health, shields
        /// </summary>
        Defense,

        /// <summary>
        /// Mobility perks - movement speed, jumping, agility
        /// </summary>
        Mobility,

        /// <summary>
        /// Utility perks - crafting, gathering, special mechanics
        /// </summary>
        Utility,

        /// <summary>
        /// Economic perks - trade, resource generation, wealth
        /// </summary>
        Economic,

        /// <summary>
        /// Territory perks - land control, building bonuses, area effects
        /// </summary>
        Territory
    }
}
