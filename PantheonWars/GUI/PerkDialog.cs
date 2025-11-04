using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using PantheonWars.GUI.UI;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using VSImGui;
using VSImGui.API;

namespace PantheonWars.GUI;

/// <summary>
///     Main ImGui-based Perk Dialog for viewing and unlocking perks
///     Follows XSkillsGilded pattern with VSImGui integration
/// </summary>
public class PerkDialog : ModSystem
{
    private const int CheckDataInterval = 1000; // Check for data every 1 second
    private const int WindowBaseWidth = 1400;
    private const int WindowBaseHeight = 900;

    private ICoreClientAPI? _capi;
    private long _checkDataId;
    private ImGuiModSystem? _imguiModSystem;

    private bool _isOpen;
    private bool _isReady;
    private PerkDialogManager? _manager;
    private Stopwatch? _stopwatch;
    private ImGuiViewportPtr _viewport;

    // Window position tracking
    private float _windowPosX;
    private float _windowPosY;

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Client;
    }

    public override double ExecuteOrder()
    {
        return 1.5; // Load after main PantheonWarsSystem (1.0)
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);

        _capi = api;
        _viewport = ImGui.GetMainViewport();
        _stopwatch = Stopwatch.StartNew();

        // Register keybind (P key to open)
        _capi.Input.RegisterHotKey("pantheonwarsperks", "Show/Hide Perk Dialog", GlKeys.P,
            HotkeyType.GUIOrOtherControls);
        _capi.Input.SetHotKeyHandler("pantheonwarsperks", OnToggleDialog);

        // Initialize manager
        _manager = new PerkDialogManager(_capi);

        // Get ImGui mod system
        _imguiModSystem = _capi.ModLoader.GetModSystem<ImGuiModSystem>();
        if (_imguiModSystem != null)
        {
            _imguiModSystem.Draw += OnDraw;
            _imguiModSystem.Closed += OnClose;
        }
        else
        {
            _capi.Logger.Error("[PantheonWars] VSImGui mod not found! Perk dialog will not work.");
        }

        // Register periodic check for data availability
        _checkDataId = _capi.Event.RegisterGameTickListener(OnCheckDataAvailability, CheckDataInterval);

        _capi.Logger.Notification("[PantheonWars] Perk Dialog initialized");
    }

    /// <summary>
    ///     Periodically check if player religion data is available
    /// </summary>
    private void OnCheckDataAvailability(float dt)
    {
        if (_isReady) return;

        // TODO: In Phase 6, connect to PlayerReligionDataManager to get current religion/deity
        // For now, we'll just mark as ready and load test data
        _isReady = true;
        LoadTestData(); // Phase 4: Load test perks for visual testing

        if (_isReady)
        {
            _capi!.Event.UnregisterGameTickListener(_checkDataId);
            _capi.Logger.Debug("[PantheonWars] Perk Dialog ready");
        }
    }

    /// <summary>
    ///     Load test perk data for visual testing (Phase 4)
    /// </summary>
    private void LoadTestData()
    {
        // Initialize test religion data
        _manager!.Initialize("test_religion_uid", DeityType.Khoras, "Test Religion of Khoras");

        // Create test perks
        var testPlayerPerks = new List<Perk>
        {
            new("khoras_warriors_resolve", "Warrior's Resolve", DeityType.Khoras)
            {
                Kind = PerkKind.Player,
                Category = PerkCategory.Combat,
                Description = "Increases melee damage and health in combat.",
                RequiredFavorRank = 0,
                StatModifiers = new Dictionary<string, float>
                {
                    { "meleeDamage", 0.1f },
                    { "maxhealthExtraMultiplier", 0.1f }
                }
            },
            new("khoras_bloodlust", "Bloodlust", DeityType.Khoras)
            {
                Kind = PerkKind.Player,
                Category = PerkCategory.Combat,
                Description = "Gain attack speed after killing an enemy.",
                RequiredFavorRank = 1,
                PrerequisitePerks = new List<string> { "khoras_warriors_resolve" }
            }
        };

        var testReligionPerks = new List<Perk>
        {
            new("khoras_war_banner", "War Banner", DeityType.Khoras)
            {
                Kind = PerkKind.Religion,
                Category = PerkCategory.Combat,
                Description = "All religion members gain bonus damage when fighting together.",
                RequiredPrestigeRank = 1
            }
        };

        _manager.LoadPerkStates(testPlayerPerks, testReligionPerks);

        // Mark first perk as unlockable for testing
        _manager.SetPerkUnlocked("khoras_warriors_resolve", false);
        var firstPerk = _manager.GetPerkState("khoras_warriors_resolve");
        if (firstPerk != null)
        {
            firstPerk.CanUnlock = true;
            firstPerk.GlowAlpha = 0.5f; // Test glow effect
        }
    }

    /// <summary>
    ///     Keybind handler - toggle dialog open/close
    /// </summary>
    private bool OnToggleDialog(KeyCombination keyCombination)
    {
        if (_isOpen)
            Close();
        else
            Open();
        return true;
    }

    /// <summary>
    ///     Open the perk dialog
    /// </summary>
    private void Open()
    {
        if (_isOpen) return;

        if (!_isReady)
        {
            OnCheckDataAvailability(0);
            if (!_isReady)
            {
                _capi!.ShowChatMessage("Perk system not ready yet. Please try again in a moment.");
                return;
            }
        }

        _isOpen = true;
        _imguiModSystem?.Show();

        // TODO: Add open sound in Phase 5
        // _capi.Gui.PlaySound(new AssetLocation("pantheonwars", "sounds/click.ogg"), false, 0.3f);

        _capi!.Logger.Debug("[PantheonWars] Perk Dialog opened");
    }

    /// <summary>
    ///     Close the perk dialog
    /// </summary>
    private void Close()
    {
        if (!_isOpen) return;

        _isOpen = false;

        // TODO: Add close sound in Phase 5
        // _capi.Gui.PlaySound(new AssetLocation("pantheonwars", "sounds/click.ogg"), false, 0.3f);

        _capi!.Logger.Debug("[PantheonWars] Perk Dialog closed");
    }

    /// <summary>
    ///     ImGui Draw callback - called every frame when dialog is open
    /// </summary>
    private CallbackGUIStatus OnDraw(float deltaSeconds)
    {
        if (!_isOpen) return CallbackGUIStatus.Closed;

        // Allow ESC to close
        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            Close();
            return CallbackGUIStatus.Closed;
        }

        DrawWindow();

        return CallbackGUIStatus.GrabMouse;
    }

    /// <summary>
    ///     ImGui Closed callback
    /// </summary>
    private void OnClose()
    {
        if (_isOpen) Close();
    }

    /// <summary>
    ///     Draw the main perk dialog window
    /// </summary>
    private void DrawWindow()
    {
        var window = _capi!.Gui.WindowBounds;
        var deltaTime = _stopwatch!.ElapsedMilliseconds / 1000f;
        _stopwatch.Restart();

        // Calculate window size (constrained to screen)
        var windowWidth = Math.Min(WindowBaseWidth, (int)window.OuterWidth - 128);
        var windowHeight = Math.Min(WindowBaseHeight, (int)window.OuterHeight - 128);

        // Set window style (no borders, no title bar, no padding, centered)
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);

        // Position window at center of screen
        ImGui.SetNextWindowSize(new Vector2(windowWidth, windowHeight));
        ImGui.SetNextWindowPos(new Vector2(
            _viewport.Pos.X + (_viewport.Size.X - windowWidth) / 2,
            _viewport.Pos.Y + (_viewport.Size.Y - windowHeight) / 2
        ));

        var flags = ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse;

        // Set window background color
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.16f, 0.12f, 0.09f, 1.0f)); // Dark brown

        ImGui.Begin("PantheonWars Perk Dialog", flags);

        // Track window position for drawing
        var windowPos = ImGui.GetWindowPos();
        _windowPosX = windowPos.X;
        _windowPosY = windowPos.Y;

        // Draw content
        DrawBackground(windowWidth, windowHeight);

        // Draw UI using PerkUIRenderer coordinator (Phase 4)
        PerkUIRenderer.Draw(
            _manager!,
            _capi,
            windowWidth,
            windowHeight,
            OnUnlockButtonClicked,
            OnCloseButtonClicked
        );

        ImGui.End();
        ImGui.PopStyleColor(); // Pop window background color
        ImGui.PopStyleVar(4); // Pop all 4 style vars
    }

    /// <summary>
    ///     Draw placeholder background (Phase 1)
    /// </summary>
    private void DrawBackground(int width, int height)
    {
        var drawList = ImGui.GetWindowDrawList();
        var pos = new Vector2(_windowPosX, _windowPosY);

        // Draw dark brown background rectangle
        var bgColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.16f, 0.12f, 0.09f, 1.0f)); // #2a1f16
        drawList.AddRectFilled(pos, new Vector2(pos.X + width, pos.Y + height), bgColor);

        // Draw lighter brown frame/border
        var frameColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.24f, 0.18f, 0.13f, 1.0f)); // #3d2e20
        drawList.AddRect(pos, new Vector2(pos.X + width, pos.Y + height), frameColor, 0, ImDrawFlags.None, 4);
    }

    /// <summary>
    ///     Handle unlock button click
    /// </summary>
    private void OnUnlockButtonClicked()
    {
        var selectedState = _manager!.GetSelectedPerkState();
        if (selectedState == null || !selectedState.CanUnlock || selectedState.IsUnlocked) return;

        // TODO: Phase 6 - Send unlock request to server via PerkCommands
        _capi!.Logger.Debug($"[PantheonWars] Unlock requested: {selectedState.Perk.Name}");
        _capi.ShowChatMessage($"Unlock perk: {selectedState.Perk.Name} (Phase 6: Connect to server)");
    }

    /// <summary>
    ///     Handle close button click
    /// </summary>
    private void OnCloseButtonClicked()
    {
        Close();
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_capi != null && _checkDataId != 0) _capi.Event.UnregisterGameTickListener(_checkDataId);

        _capi?.Logger.Notification("[PantheonWars] Perk Dialog disposed");
    }
}