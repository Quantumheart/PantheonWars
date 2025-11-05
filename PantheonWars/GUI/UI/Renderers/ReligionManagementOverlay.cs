using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using PantheonWars.Network;
using Vintagestory.API.Client;

namespace PantheonWars.GUI.UI.Renderers;

/// <summary>
///     Overlay for managing religion (leader only)
///     Displays member list, kick, invite, edit description, disband
/// </summary>
internal static class ReligionManagementOverlay
{
    // Color constants
    private static readonly Vector4 ColorGold = new(0.996f, 0.682f, 0.204f, 1.0f);
    private static readonly Vector4 ColorWhite = new(0.9f, 0.9f, 0.9f, 1.0f);
    private static readonly Vector4 ColorGrey = new(0.573f, 0.502f, 0.416f, 1.0f);
    private static readonly Vector4 ColorDarkBrown = new(0.24f, 0.18f, 0.13f, 1.0f);
    private static readonly Vector4 ColorLightBrown = new(0.35f, 0.26f, 0.19f, 1.0f);
    private static readonly Vector4 ColorBackground = new(0.16f, 0.12f, 0.09f, 0.95f);
    private static readonly Vector4 ColorRed = new(0.8f, 0.2f, 0.2f, 1.0f);

    // State
    private static PlayerReligionInfoResponsePacket? _religionInfo;
    private static string _invitePlayerName = "";
    private static string _description = "";
    private static float _memberScrollY;
    private static string? _errorMessage;
    private static bool _showDisbandConfirm;

    /// <summary>
    ///     Initialize/reset overlay state
    /// </summary>
    public static void Initialize()
    {
        _religionInfo = null;
        _invitePlayerName = "";
        _description = "";
        _memberScrollY = 0f;
        _errorMessage = null;
        _showDisbandConfirm = false;
    }

    /// <summary>
    ///     Update religion info from server
    /// </summary>
    public static void UpdateReligionInfo(PlayerReligionInfoResponsePacket info)
    {
        _religionInfo = info;
        _description = info.Description ?? "";
    }

    /// <summary>
    ///     Draw the religion management overlay
    /// </summary>
    /// <param name="api">Client API</param>
    /// <param name="windowWidth">Parent window width</param>
    /// <param name="windowHeight">Parent window height</param>
    /// <param name="onClose">Callback when close clicked</param>
    /// <param name="onKickMember">Callback when kick clicked (memberUID)</param>
    /// <param name="onInvitePlayer">Callback when invite clicked (playerName)</param>
    /// <param name="onEditDescription">Callback when save description clicked (description)</param>
    /// <param name="onDisband">Callback when disband confirmed</param>
    /// <param name="onRequestRefresh">Callback to request fresh data</param>
    /// <returns>True if overlay should remain open</returns>
    public static bool Draw(
        ICoreClientAPI api,
        int windowWidth,
        int windowHeight,
        Action onClose,
        Action<string> onKickMember,
        Action<string> onInvitePlayer,
        Action<string> onEditDescription,
        Action onDisband,
        Action onRequestRefresh)
    {
        const float overlayWidth = 700f;
        const float overlayHeight = 650f;
        const float padding = 20f;

        var windowPos = ImGui.GetWindowPos();
        var overlayX = windowPos.X + (windowWidth - overlayWidth) / 2;
        var overlayY = windowPos.Y + (windowHeight - overlayHeight) / 2;

        var drawList = ImGui.GetForegroundDrawList();

        // Draw semi-transparent background
        var bgStart = windowPos;
        var bgEnd = new Vector2(windowPos.X + windowWidth, windowPos.Y + windowHeight);
        var bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.8f));
        drawList.AddRectFilled(bgStart, bgEnd, bgColor);

        // Draw main panel
        var panelStart = new Vector2(overlayX, overlayY);
        var panelEnd = new Vector2(overlayX + overlayWidth, overlayY + overlayHeight);
        var panelColor = ImGui.ColorConvertFloat4ToU32(ColorBackground);
        drawList.AddRectFilled(panelStart, panelEnd, panelColor, 8f);

        // Draw border
        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGold * 0.7f);
        drawList.AddRect(panelStart, panelEnd, borderColor, 8f, ImDrawFlags.None, 2f);

        // Check if data loaded
        if (_religionInfo == null || !_religionInfo.HasReligion)
        {
            DrawLoadingOrError(drawList, overlayX, overlayY, overlayWidth, overlayHeight, onClose, onRequestRefresh, api);
            return true;
        }

        // Disband confirmation dialog
        if (_showDisbandConfirm)
        {
            return DrawDisbandConfirmation(drawList, api, overlayX, overlayY, overlayWidth, overlayHeight, onDisband);
        }

        var currentY = overlayY + padding;

        // === HEADER ===
        var headerText = $"Manage {_religionInfo.ReligionName}";
        var headerSize = ImGui.CalcTextSize(headerText);
        var headerPos = new Vector2(overlayX + padding, currentY);
        var headerColor = ImGui.ColorConvertFloat4ToU32(ColorGold);
        drawList.AddText(ImGui.GetFont(), 20f, headerPos, headerColor, headerText);

        // Close button (X)
        const float closeButtonSize = 24f;
        var closeButtonX = overlayX + overlayWidth - padding - closeButtonSize;
        var closeButtonY = currentY;
        if (DrawCloseButton(drawList, closeButtonX, closeButtonY, closeButtonSize))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            onClose.Invoke();
            return false;
        }

        currentY += headerSize.Y + padding;

        // === MEMBER LIST ===
        DrawSectionLabel(drawList, "Members", overlayX + padding, currentY);
        currentY += 25f;

        const float memberListHeight = 200f;
        DrawMemberList(drawList, api, overlayX + padding, currentY, overlayWidth - padding * 2, memberListHeight, onKickMember);
        currentY += memberListHeight + padding;

        // === INVITE PLAYER ===
        DrawSectionLabel(drawList, "Invite Player", overlayX + padding, currentY);
        currentY += 25f;

        var inviteInputWidth = overlayWidth - padding * 2 - 120f;
        _invitePlayerName = DrawTextInput(drawList, "##invite_input", _invitePlayerName, overlayX + padding, currentY, inviteInputWidth, 32f, "Player name...");

        // Invite button
        var inviteButtonX = overlayX + padding + inviteInputWidth + 10f;
        if (DrawButton(drawList, "Invite", inviteButtonX, currentY, 100f, 32f, false, !string.IsNullOrWhiteSpace(_invitePlayerName)))
        {
            if (!string.IsNullOrWhiteSpace(_invitePlayerName))
            {
                api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                    api.World.Player.Entity, null, false, 8f, 0.5f);
                onInvitePlayer.Invoke(_invitePlayerName.Trim());
                _invitePlayerName = "";
            }
        }

        currentY += 40f;

        // === DESCRIPTION ===
        DrawSectionLabel(drawList, "Description", overlayX + padding, currentY);
        currentY += 25f;

        const float descHeight = 80f;
        _description = DrawMultilineTextInput(drawList, "##description_input", _description, overlayX + padding, currentY, overlayWidth - padding * 2, descHeight);
        currentY += descHeight + 5f;

        // Save Description button
        var saveButtonWidth = 150f;
        var saveButtonX = overlayX + overlayWidth - padding - saveButtonWidth;
        if (DrawButton(drawList, "Save Description", saveButtonX, currentY, saveButtonWidth, 32f, false, true))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            onEditDescription.Invoke(_description);
        }

        currentY += 40f;

        // Error message
        if (!string.IsNullOrEmpty(_errorMessage))
        {
            DrawErrorText(drawList, _errorMessage, overlayX + padding, currentY, overlayWidth - padding * 2);
            currentY += 30f;
        }

        // === DISBAND BUTTON ===
        const float disbandButtonWidth = 150f;
        const float disbandButtonHeight = 36f;
        var disbandButtonX = overlayX + padding;
        var disbandButtonY = overlayY + overlayHeight - padding - disbandButtonHeight;

        if (DrawButton(drawList, "Disband Religion", disbandButtonX, disbandButtonY, disbandButtonWidth, disbandButtonHeight, false, true, ColorRed))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            _showDisbandConfirm = true;
        }

        return true;
    }

    /// <summary>
    ///     Draw loading or error state
    /// </summary>
    private static void DrawLoadingOrError(ImDrawListPtr drawList, float x, float y, float width, float height, Action onClose, Action onRequestRefresh, ICoreClientAPI api)
    {
        var currentY = y + 20f;

        // Header with close button
        var headerText = "Manage Religion";
        var headerPos = new Vector2(x + 20f, currentY);
        var headerColor = ImGui.ColorConvertFloat4ToU32(ColorGold);
        drawList.AddText(ImGui.GetFont(), 20f, headerPos, headerColor, headerText);

        const float closeButtonSize = 24f;
        var closeButtonX = x + width - 20f - closeButtonSize;
        if (DrawCloseButton(drawList, closeButtonX, currentY, closeButtonSize))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            onClose.Invoke();
        }

        currentY += 60f;

        // Loading/Error message
        var message = _religionInfo == null ? "Loading religion data..." : "You are not in a religion.";
        var messageSize = ImGui.CalcTextSize(message);
        var messagePos = new Vector2(x + (width - messageSize.X) / 2, y + (height - messageSize.Y) / 2);
        var messageColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);
        drawList.AddText(messagePos, messageColor, message);

        // Refresh button if not loading
        if (_religionInfo != null)
        {
            const float buttonWidth = 120f;
            const float buttonHeight = 36f;
            var buttonX = x + (width - buttonWidth) / 2;
            var buttonY = messagePos.Y + 40f;

            if (DrawButton(drawList, "Refresh", buttonX, buttonY, buttonWidth, buttonHeight, false, true))
            {
                api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                    api.World.Player.Entity, null, false, 8f, 0.5f);
                onRequestRefresh.Invoke();
            }
        }
    }

    /// <summary>
    ///     Draw disband confirmation dialog
    /// </summary>
    private static bool DrawDisbandConfirmation(ImDrawListPtr drawList, ICoreClientAPI api, float x, float y, float width, float height, Action onDisband)
    {
        const float confirmWidth = 400f;
        const float confirmHeight = 200f;
        var confirmX = x + (width - confirmWidth) / 2;
        var confirmY = y + (height - confirmHeight) / 2;

        // Draw confirm background
        var confirmStart = new Vector2(confirmX, confirmY);
        var confirmEnd = new Vector2(confirmX + confirmWidth, confirmY + confirmHeight);
        var confirmBg = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown);
        drawList.AddRectFilled(confirmStart, confirmEnd, confirmBg, 8f);

        // Draw border
        var confirmBorder = ImGui.ColorConvertFloat4ToU32(ColorRed);
        drawList.AddRect(confirmStart, confirmEnd, confirmBorder, 8f, ImDrawFlags.None, 3f);

        var currentY = confirmY + 20f;

        // Warning text
        var warningText = "Disband Religion?";
        var warningSize = ImGui.CalcTextSize(warningText);
        var warningPos = new Vector2(confirmX + (confirmWidth - warningSize.X) / 2, currentY);
        var warningColor = ImGui.ColorConvertFloat4ToU32(ColorRed);
        drawList.AddText(ImGui.GetFont(), 18f, warningPos, warningColor, warningText);
        currentY += warningSize.Y + 20f;

        // Description
        var descText = "This will permanently delete the religion\nand remove all members. This cannot be undone.";
        var lines = descText.Split('\n');
        foreach (var line in lines)
        {
            var lineSize = ImGui.CalcTextSize(line);
            var linePos = new Vector2(confirmX + (confirmWidth - lineSize.X) / 2, currentY);
            var lineColor = ImGui.ColorConvertFloat4ToU32(ColorWhite);
            drawList.AddText(linePos, lineColor, line);
            currentY += lineSize.Y + 5f;
        }

        currentY += 20f;

        // Buttons
        const float buttonWidth = 120f;
        const float buttonHeight = 36f;
        const float buttonSpacing = 20f;
        var buttonsStartX = confirmX + (confirmWidth - buttonWidth * 2 - buttonSpacing) / 2;
        var buttonY = confirmY + confirmHeight - 20f - buttonHeight;

        // Cancel button
        if (DrawButton(drawList, "Cancel", buttonsStartX, buttonY, buttonWidth, buttonHeight, false, true))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            _showDisbandConfirm = false;
        }

        // Confirm button
        var confirmButtonX = buttonsStartX + buttonWidth + buttonSpacing;
        if (DrawButton(drawList, "Disband", confirmButtonX, buttonY, buttonWidth, buttonHeight, false, true, ColorRed))
        {
            api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                api.World.Player.Entity, null, false, 8f, 0.5f);
            onDisband.Invoke();
            _showDisbandConfirm = false;
            return false; // Close overlay
        }

        return true;
    }

    /// <summary>
    ///     Draw member list with scrolling
    /// </summary>
    private static void DrawMemberList(ImDrawListPtr drawList, ICoreClientAPI api, float x, float y, float width, float height, Action<string> onKickMember)
    {
        const float itemHeight = 30f;
        const float itemSpacing = 4f;
        const float scrollbarWidth = 16f;

        // Draw background
        var listStart = new Vector2(x, y);
        var listEnd = new Vector2(x + width, y + height);
        var listBgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.5f);
        drawList.AddRectFilled(listStart, listEnd, listBgColor, 4f);

        var members = _religionInfo?.Members ?? new List<PlayerReligionInfoResponsePacket.MemberInfo>();
        if (members.Count == 0)
        {
            var noMembersText = "No members";
            var noMembersSize = ImGui.CalcTextSize(noMembersText);
            var noMembersPos = new Vector2(x + (width - noMembersSize.X) / 2, y + (height - noMembersSize.Y) / 2);
            var noMembersColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);
            drawList.AddText(noMembersPos, noMembersColor, noMembersText);
            return;
        }

        // Calculate scroll
        var contentHeight = members.Count * (itemHeight + itemSpacing);
        var maxScroll = Math.Max(0f, contentHeight - height);

        // Handle mouse wheel
        var mousePos = ImGui.GetMousePos();
        var isMouseOver = mousePos.X >= x && mousePos.X <= x + width &&
                          mousePos.Y >= y && mousePos.Y <= y + height;
        if (isMouseOver)
        {
            var wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0)
            {
                _memberScrollY = Math.Clamp(_memberScrollY - wheel * 30f, 0f, maxScroll);
            }
        }

        // Clip to bounds
        drawList.PushClipRect(listStart, listEnd, true);

        // Draw members
        var itemY = y - _memberScrollY;
        var currentPlayerUID = api.World.Player.PlayerUID;

        foreach (var member in members)
        {
            // Skip if not visible
            if (itemY + itemHeight < y || itemY > y + height)
            {
                itemY += itemHeight + itemSpacing;
                continue;
            }

            DrawMemberItem(drawList, api, member, x, itemY, width - scrollbarWidth - 4f, itemHeight,
                currentPlayerUID, onKickMember);
            itemY += itemHeight + itemSpacing;
        }

        drawList.PopClipRect();

        // Draw scrollbar if needed
        if (contentHeight > height)
        {
            DrawScrollbar(drawList, x + width - scrollbarWidth, y, scrollbarWidth, height, _memberScrollY, maxScroll);
        }
    }

    /// <summary>
    ///     Draw single member item
    /// </summary>
    private static void DrawMemberItem(ImDrawListPtr drawList, ICoreClientAPI api,
        PlayerReligionInfoResponsePacket.MemberInfo member, float x, float y, float width, float height,
        string currentPlayerUID, Action<string> onKickMember)
    {
        const float padding = 8f;
        const float kickButtonWidth = 60f;

        // Draw background
        var itemStart = new Vector2(x, y);
        var itemEnd = new Vector2(x + width, y + height);
        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.8f);
        drawList.AddRectFilled(itemStart, itemEnd, bgColor, 3f);

        // Player name
        var nameText = member.PlayerName + (member.IsFounder ? " [Founder]" : "");
        var namePos = new Vector2(x + padding, y + (height - 14f) / 2);
        var nameColor = ImGui.ColorConvertFloat4ToU32(member.IsFounder ? ColorGold : ColorWhite);
        drawList.AddText(namePos, nameColor, nameText);

        // Favor rank
        var rankText = $"{member.FavorRank} ({member.Favor})";
        var rankSize = ImGui.CalcTextSize(rankText);
        var rankPos = new Vector2(x + width - padding - kickButtonWidth - 10f - rankSize.X, y + (height - 14f) / 2);
        var rankColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);
        drawList.AddText(rankPos, rankColor, rankText);

        // Kick button (only if not founder and not self)
        if (!member.IsFounder && member.PlayerUID != currentPlayerUID)
        {
            var kickButtonX = x + width - kickButtonWidth - padding;
            var kickButtonY = y + (height - 22f) / 2;
            if (DrawSmallButton(drawList, "Kick", kickButtonX, kickButtonY, kickButtonWidth, 22f))
            {
                api.World.PlaySoundAt(new Vintagestory.API.Common.AssetLocation("pantheonwars:sounds/click"),
                    api.World.Player.Entity, null, false, 8f, 0.5f);
                onKickMember.Invoke(member.PlayerUID);
            }
        }
    }

    /// <summary>
    ///     Draw section label
    /// </summary>
    private static void DrawSectionLabel(ImDrawListPtr drawList, string text, float x, float y)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorGold * 0.9f);
        drawList.AddText(ImGui.GetFont(), 15f, new Vector2(x, y), textColor, text);
    }

    /// <summary>
    ///     Draw error text
    /// </summary>
    private static void DrawErrorText(ImDrawListPtr drawList, string text, float x, float y, float width)
    {
        var textColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.3f, 0.3f, 1f));
        drawList.AddText(ImGui.GetFont(), 13f, new Vector2(x, y), textColor, text);
    }

    /// <summary>
    ///     Draw text input (same as CreateReligionOverlay)
    /// </summary>
    private static string DrawTextInput(ImDrawListPtr drawList, string id, string currentValue, float x, float y, float width, float height, string placeholder)
    {
        var inputStart = new Vector2(x, y);
        var inputEnd = new Vector2(x + width, y + height);

        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.7f);
        drawList.AddRectFilled(inputStart, inputEnd, bgColor, 4f);

        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGrey * 0.5f);
        drawList.AddRect(inputStart, inputEnd, borderColor, 4f, ImDrawFlags.None, 1f);

        ImGui.SetCursorScreenPos(inputStart);
        ImGui.InvisibleButton(id, new Vector2(width, height));
        var isActive = ImGui.IsItemActive() || ImGui.IsItemFocused();
        var wasClicked = ImGui.IsItemClicked();

        if (isActive || ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
        }

        // Set keyboard focus when clicked
        if (wasClicked)
        {
            ImGui.SetKeyboardFocusHere(-1);
        }

        if (isActive)
        {
            var io = ImGui.GetIO();
            io.WantCaptureKeyboard = true; // Block game input while typing

            if (ImGui.IsKeyPressed(ImGuiKey.Backspace) && currentValue.Length > 0)
            {
                currentValue = currentValue.Substring(0, currentValue.Length - 1);
            }

            for (int i = 0; i < io.InputQueueCharacters.Size; i++)
            {
                var c = (char)io.InputQueueCharacters[i];
                if (c >= 32 && c < 127)
                {
                    currentValue += c;
                }
            }
        }

        var displayText = string.IsNullOrEmpty(currentValue) ? placeholder : currentValue;
        var textColor = string.IsNullOrEmpty(currentValue)
            ? ImGui.ColorConvertFloat4ToU32(ColorGrey * 0.7f)
            : ImGui.ColorConvertFloat4ToU32(ColorWhite);

        var textPos = new Vector2(x + 8f, y + (height - 16f) / 2);
        drawList.AddText(textPos, textColor, displayText);

        if (isActive && (int)(ImGui.GetTime() * 2) % 2 == 0)
        {
            var textWidth = string.IsNullOrEmpty(currentValue) ? 0f : ImGui.CalcTextSize(currentValue).X;
            var cursorX = x + 8f + textWidth;
            drawList.AddLine(new Vector2(cursorX, y + 6f), new Vector2(cursorX, y + height - 6f),
                ImGui.ColorConvertFloat4ToU32(ColorWhite), 2f);
        }

        return currentValue ?? "";
    }

    /// <summary>
    ///     Draw multiline text input
    /// </summary>
    private static string DrawMultilineTextInput(ImDrawListPtr drawList, string id, string currentValue, float x, float y, float width, float height)
    {
        var inputStart = new Vector2(x, y);
        var inputEnd = new Vector2(x + width, y + height);

        var bgColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.7f);
        drawList.AddRectFilled(inputStart, inputEnd, bgColor, 4f);

        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorGrey * 0.5f);
        drawList.AddRect(inputStart, inputEnd, borderColor, 4f, ImDrawFlags.None, 1f);

        ImGui.SetCursorScreenPos(inputStart);
        ImGui.InvisibleButton(id, new Vector2(width, height));
        var isActive = ImGui.IsItemActive() || ImGui.IsItemFocused();
        var wasClicked = ImGui.IsItemClicked();

        if (isActive || ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
        }

        // Set keyboard focus when clicked
        if (wasClicked)
        {
            ImGui.SetKeyboardFocusHere(-1);
        }

        if (isActive)
        {
            var io = ImGui.GetIO();
            io.WantCaptureKeyboard = true; // Block game input while typing

            if (ImGui.IsKeyPressed(ImGuiKey.Backspace) && currentValue.Length > 0)
            {
                currentValue = currentValue.Substring(0, currentValue.Length - 1);
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Enter))
            {
                currentValue += "\n";
            }

            for (int i = 0; i < io.InputQueueCharacters.Size; i++)
            {
                var c = (char)io.InputQueueCharacters[i];
                if (c >= 32 && c < 127 && currentValue.Length < 500)
                {
                    currentValue += c;
                }
            }
        }

        // Draw text (simple word wrap)
        var textPos = new Vector2(x + 8f, y + 8f);
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorWhite);
        if (!string.IsNullOrEmpty(currentValue))
        {
            drawList.AddText(textPos, textColor, currentValue);
        }

        return currentValue ?? "";
    }

    /// <summary>
    ///     Draw scrollbar
    /// </summary>
    private static void DrawScrollbar(ImDrawListPtr drawList, float x, float y, float width, float height, float scrollY, float maxScroll)
    {
        var trackStart = new Vector2(x, y);
        var trackEnd = new Vector2(x + width, y + height);
        var trackColor = ImGui.ColorConvertFloat4ToU32(ColorDarkBrown * 0.7f);
        drawList.AddRectFilled(trackStart, trackEnd, trackColor, 4f);

        var thumbHeight = Math.Max(20f, height * (height / (height + maxScroll)));
        var thumbY = y + (scrollY / maxScroll) * (height - thumbHeight);
        var thumbStart = new Vector2(x + 2f, thumbY);
        var thumbEnd = new Vector2(x + width - 2f, thumbY + thumbHeight);
        var thumbColor = ImGui.ColorConvertFloat4ToU32(ColorGrey);
        drawList.AddRectFilled(thumbStart, thumbEnd, thumbColor, 4f);
    }

    /// <summary>
    ///     Draw close button
    /// </summary>
    private static bool DrawCloseButton(ImDrawListPtr drawList, float x, float y, float size)
    {
        var buttonStart = new Vector2(x, y);
        var buttonEnd = new Vector2(x + size, y + size);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + size &&
                        mousePos.Y >= y && mousePos.Y <= y + size;

        var bgColor = isHovering ? ColorLightBrown : ColorDarkBrown;
        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(buttonStart, buttonEnd, bgColorU32, 4f);

        var xColor = ImGui.ColorConvertFloat4ToU32(isHovering ? ColorWhite : ColorGrey);
        var xPadding = size * 0.25f;
        drawList.AddLine(new Vector2(x + xPadding, y + xPadding),
            new Vector2(x + size - xPadding, y + size - xPadding), xColor, 2f);
        drawList.AddLine(new Vector2(x + size - xPadding, y + xPadding),
            new Vector2(x + xPadding, y + size - xPadding), xColor, 2f);

        if (isHovering)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left);
    }

    /// <summary>
    ///     Draw button
    /// </summary>
    private static bool DrawButton(ImDrawListPtr drawList, string text, float x, float y, float width, float height, bool isPrimary, bool enabled, Vector4? customColor = null)
    {
        var buttonStart = new Vector2(x, y);
        var buttonEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = enabled && mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;

        var baseColor = customColor ?? (isPrimary ? ColorGold : ColorDarkBrown);

        Vector4 bgColor;
        if (!enabled)
        {
            bgColor = ColorDarkBrown * 0.5f;
        }
        else if (isHovering && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            bgColor = baseColor * 0.7f;
        }
        else if (isHovering)
        {
            bgColor = baseColor * 1.2f;
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        else
        {
            bgColor = baseColor * 0.8f;
        }

        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(buttonStart, buttonEnd, bgColorU32, 4f);

        var borderColor = ImGui.ColorConvertFloat4ToU32(enabled ? ColorGold * 0.7f : ColorGrey * 0.3f);
        drawList.AddRect(buttonStart, buttonEnd, borderColor, 4f, ImDrawFlags.None, 1.5f);

        var textSize = ImGui.CalcTextSize(text);
        var textPos = new Vector2(x + (width - textSize.X) / 2, y + (height - textSize.Y) / 2);
        var textColor = ImGui.ColorConvertFloat4ToU32(enabled ? ColorWhite : ColorGrey * 0.7f);
        drawList.AddText(textPos, textColor, text);

        return enabled && isHovering && ImGui.IsMouseReleased(ImGuiMouseButton.Left);
    }

    /// <summary>
    ///     Draw small button (for list items)
    /// </summary>
    private static bool DrawSmallButton(ImDrawListPtr drawList, string text, float x, float y, float width, float height)
    {
        var buttonStart = new Vector2(x, y);
        var buttonEnd = new Vector2(x + width, y + height);

        var mousePos = ImGui.GetMousePos();
        var isHovering = mousePos.X >= x && mousePos.X <= x + width &&
                        mousePos.Y >= y && mousePos.Y <= y + height;

        var bgColor = isHovering ? ColorRed * 0.8f : ColorDarkBrown;
        var bgColorU32 = ImGui.ColorConvertFloat4ToU32(bgColor);
        drawList.AddRectFilled(buttonStart, buttonEnd, bgColorU32, 3f);

        var borderColor = ImGui.ColorConvertFloat4ToU32(ColorRed * 0.7f);
        drawList.AddRect(buttonStart, buttonEnd, borderColor, 3f, ImDrawFlags.None, 1f);

        var textSize = ImGui.CalcTextSize(text);
        var textPos = new Vector2(x + (width - textSize.X) / 2, y + (height - textSize.Y) / 2);
        var textColor = ImGui.ColorConvertFloat4ToU32(ColorWhite);
        drawList.AddText(ImGui.GetFont(), 11f, textPos, textColor, text);

        if (isHovering)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return isHovering && ImGui.IsMouseClicked(ImGuiMouseButton.Left);
    }
}
