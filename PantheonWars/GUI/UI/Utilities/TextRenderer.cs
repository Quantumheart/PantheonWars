using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ImGuiNET;

namespace PantheonWars.GUI.UI.Utilities;

/// <summary>
///     Utility class for rendering various text styles
///     Provides consistent text rendering for labels, info text, errors, etc.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TextRenderer
{
    /// <summary>
    ///     Draw a label (white text, 14pt)
    /// </summary>
    /// <param name="drawList">ImGui draw list</param>
    /// <param name="text">Text to display</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="fontSize">Font size (default 14f)</param>
    /// <param name="color">Text color (default white)</param>
    public static void DrawLabel(
        ImDrawListPtr drawList,
        string text,
        float x,
        float y,
        float fontSize = 14f,
        Vector4? color = null)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(color ?? ColorPalette.White);
        drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, y), textColor, text);
    }

    /// <summary>
    ///     Draw info text (grey text, 12pt, word-wrapped)
    /// </summary>
    /// <param name="drawList">ImGui draw list</param>
    /// <param name="text">Text to display</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="width">Maximum width for word wrapping</param>
    /// <param name="fontSize">Font size (default 12f)</param>
    public static void DrawInfoText(
        ImDrawListPtr drawList,
        string text,
        float x,
        float y,
        float width,
        float fontSize = 12f)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Grey);

        // Simple word wrap
        var words = text.Split(' ');
        var currentLine = "";
        var lineY = y;
        var lineHeight = fontSize + 6f; // Spacing between lines

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            var testSize = ImGui.CalcTextSize(testLine);

            if (testSize.X > width && !string.IsNullOrEmpty(currentLine))
            {
                drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, lineY), textColor, currentLine);
                lineY += lineHeight;
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, lineY), textColor, currentLine);
        }
    }

    /// <summary>
    ///     Draw error text (red text, 13pt)
    /// </summary>
    /// <param name="drawList">ImGui draw list</param>
    /// <param name="text">Error message to display</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="fontSize">Font size (default 13f)</param>
    public static void DrawErrorText(
        ImDrawListPtr drawList,
        string text,
        float x,
        float y,
        float fontSize = 13f)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Red);
        drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, y), textColor, text);
    }

    /// <summary>
    ///     Draw success text (green text, 13pt)
    /// </summary>
    /// <param name="drawList">ImGui draw list</param>
    /// <param name="text">Success message to display</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="fontSize">Font size (default 13f)</param>
    public static void DrawSuccessText(
        ImDrawListPtr drawList,
        string text,
        float x,
        float y,
        float fontSize = 13f)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Green);
        drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, y), textColor, text);
    }

    /// <summary>
    ///     Draw warning text (yellow text, 13pt)
    /// </summary>
    /// <param name="drawList">ImGui draw list</param>
    /// <param name="text">Warning message to display</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="fontSize">Font size (default 13f)</param>
    public static void DrawWarningText(
        ImDrawListPtr drawList,
        string text,
        float x,
        float y,
        float fontSize = 13f)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Yellow);
        drawList.AddText(ImGui.GetFont(), fontSize, new Vector2(x, y), textColor, text);
    }
}
