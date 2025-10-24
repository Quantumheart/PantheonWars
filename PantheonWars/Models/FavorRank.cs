namespace PantheonWars.Models
{
    /// <summary>
    /// Player favor ranks - individual progression for each player
    /// </summary>
    public enum FavorRank
    {
        /// <summary>
        /// Starting rank - 0-499 favor
        /// </summary>
        Initiate = 0,

        /// <summary>
        /// Second rank - 500-1999 favor
        /// </summary>
        Disciple = 1,

        /// <summary>
        /// Third rank - 2000-4999 favor
        /// </summary>
        Zealot = 2,

        /// <summary>
        /// Fourth rank - 5000-9999 favor
        /// </summary>
        Champion = 3,

        /// <summary>
        /// Highest rank - 10000+ favor
        /// </summary>
        Avatar = 4
    }
}
