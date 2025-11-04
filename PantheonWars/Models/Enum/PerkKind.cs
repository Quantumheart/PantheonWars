namespace PantheonWars.Models.Enum
{
    /// <summary>
    /// Type of perk - determines who benefits and unlock requirements
    /// </summary>
    public enum PerkKind
    {
        /// <summary>
        /// Personal perk - unlocked by player favor rank, benefits individual player
        /// </summary>
        Player,

        /// <summary>
        /// Religion perk - unlocked by religion prestige rank, benefits all congregation members
        /// </summary>
        Religion
    }
}
