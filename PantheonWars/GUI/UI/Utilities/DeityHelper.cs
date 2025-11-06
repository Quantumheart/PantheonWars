using System.Numerics;
using PantheonWars.Models.Enum;

namespace PantheonWars.GUI.UI.Utilities;

/// <summary>
///     Centralized helper for deity-related UI operations
///     Provides deity colors, titles, and lists for consistent UI presentation
/// </summary>
internal static class DeityHelper
{
    /// <summary>
    ///     All deity names in order
    /// </summary>
    public static readonly string[] DeityNames =
    {
        "Khoras", "Lysa", "Morthen", "Aethra", "Umbros", "Tharos", "Gaia", "Vex"
    };

    /// <summary>
    ///     Get the thematic color for a deity (by name)
    /// </summary>
    public static Vector4 GetDeityColor(string deity)
    {
        return deity switch
        {
            "Khoras" => new Vector4(0.8f, 0.2f, 0.2f, 1.0f),      // Red - War
            "Lysa" => new Vector4(0.4f, 0.8f, 0.3f, 1.0f),        // Green - Hunt
            "Morthen" => new Vector4(0.3f, 0.1f, 0.4f, 1.0f),     // Purple - Death
            "Aethra" => new Vector4(0.9f, 0.9f, 0.6f, 1.0f),      // Light yellow - Light
            "Umbros" => new Vector4(0.2f, 0.2f, 0.3f, 1.0f),      // Dark grey - Shadows
            "Tharos" => new Vector4(0.3f, 0.6f, 0.9f, 1.0f),      // Blue - Storms
            "Gaia" => new Vector4(0.5f, 0.4f, 0.2f, 1.0f),        // Brown - Earth
            "Vex" => new Vector4(0.7f, 0.2f, 0.7f, 1.0f),         // Magenta - Madness
            _ => new Vector4(0.5f, 0.5f, 0.5f, 1.0f)              // Grey - Unknown
        };
    }

    /// <summary>
    ///     Get the thematic color for a deity (by enum)
    /// </summary>
    public static Vector4 GetDeityColor(DeityType deity)
    {
        return deity switch
        {
            DeityType.Khoras => new Vector4(0.8f, 0.2f, 0.2f, 1.0f),      // Red - War
            DeityType.Lysa => new Vector4(0.4f, 0.8f, 0.3f, 1.0f),        // Green - Hunt
            DeityType.Morthen => new Vector4(0.3f, 0.1f, 0.4f, 1.0f),     // Purple - Death
            DeityType.Aethra => new Vector4(0.9f, 0.9f, 0.6f, 1.0f),      // Light yellow - Light
            DeityType.Umbros => new Vector4(0.2f, 0.2f, 0.3f, 1.0f),      // Dark grey - Shadows
            DeityType.Tharos => new Vector4(0.3f, 0.6f, 0.9f, 1.0f),      // Blue - Storms
            DeityType.Gaia => new Vector4(0.5f, 0.4f, 0.2f, 1.0f),        // Brown - Earth
            DeityType.Vex => new Vector4(0.7f, 0.2f, 0.7f, 1.0f),         // Magenta - Madness
            _ => new Vector4(0.5f, 0.5f, 0.5f, 1.0f)                      // Grey - Unknown
        };
    }

    /// <summary>
    ///     Get the full title/description for a deity
    /// </summary>
    public static string GetDeityTitle(string deity)
    {
        return deity switch
        {
            "Khoras" => "God of War",
            "Lysa" => "Goddess of the Hunt",
            "Morthen" => "God of Death",
            "Aethra" => "Goddess of Light",
            "Umbros" => "God of Shadows",
            "Tharos" => "God of Storms",
            "Gaia" => "Goddess of Earth",
            "Vex" => "God of Madness",
            _ => "Unknown Deity"
        };
    }

    /// <summary>
    ///     Get the full title/description for a deity (by enum)
    /// </summary>
    public static string GetDeityTitle(DeityType deity)
    {
        return deity switch
        {
            DeityType.Khoras => "God of War",
            DeityType.Lysa => "Goddess of the Hunt",
            DeityType.Morthen => "God of Death",
            DeityType.Aethra => "Goddess of Light",
            DeityType.Umbros => "God of Shadows",
            DeityType.Tharos => "God of Storms",
            DeityType.Gaia => "Goddess of Earth",
            DeityType.Vex => "God of Madness",
            _ => "Unknown Deity"
        };
    }

    /// <summary>
    ///     Convert deity name string to DeityType enum
    /// </summary>
    public static DeityType ParseDeityType(string deityName)
    {
        return deityName switch
        {
            "Khoras" => DeityType.Khoras,
            "Lysa" => DeityType.Lysa,
            "Morthen" => DeityType.Morthen,
            "Aethra" => DeityType.Aethra,
            "Umbros" => DeityType.Umbros,
            "Tharos" => DeityType.Tharos,
            "Gaia" => DeityType.Gaia,
            "Vex" => DeityType.Vex,
            _ => DeityType.None
        };
    }

    /// <summary>
    ///     Get formatted display text for a deity (e.g., "Khoras - God of War")
    /// </summary>
    public static string GetDeityDisplayText(string deity)
    {
        return $"{deity} - {GetDeityTitle(deity)}";
    }

    /// <summary>
    ///     Get formatted display text for a deity (e.g., "Khoras - God of War")
    /// </summary>
    public static string GetDeityDisplayText(DeityType deity)
    {
        return $"{deity} - {GetDeityTitle(deity)}";
    }
}
