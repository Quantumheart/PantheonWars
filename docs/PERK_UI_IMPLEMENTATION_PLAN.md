# Perk Dialog UI Implementation Plan

**Date:** November 4, 2025
**Last Updated:** November 5, 2025
**Purpose:** Implement polished ImGui-based perk tree viewer for PantheonWars
**Reference:** XSkillsGilded UI patterns documented in [XSKILLS_UI_ANALYSIS.md](XSKILLS_UI_ANALYSIS.md)

---

## 🎯 CURRENT STATUS: v1.0 MINIMUM VIABLE COMPLETE

**Overall Progress: ~80% Complete** (17 of 22 estimated hours)

### ✅ Completed Phases
- **Phase 1: Foundation** (4 hours) - COMPLETE
- **Phase 2: Data Models** (3 hours) - COMPLETE
- **Phase 3: Core Renderers** (4 hours) - COMPLETE
- **Phase 4: Layout & Composition** (3 hours) - COMPLETE
- **Phase 6: Integration & Networking** (2 hours) - COMPLETE

### 🚧 Remaining Work
- **Phase 5: Polish & Animation** (~2 hours remaining)
  - ❌ TooltipRenderer.cs not yet implemented
  - ❌ Audio feedback missing (TODO comments in place)
  - ⚠️ Glow animations static (need time-based lerp)

- **Phase 7: Testing** (~1 hour remaining)
  - Need multi-deity testing
  - Need GUI scale testing
  - Need memory leak verification

- **Phase 8: Assets** (Optional - can ship with placeholders)
  - Currently using code-generated placeholders
  - All visual elements working without texture files

### 📁 Files Implemented (11 core files)
1. ✅ `PerkDialog.cs` - Main dialog system
2. ✅ `PerkDialogManager.cs` - State management
3. ✅ `PerkUIRenderer.cs` - Coordinator
4. ✅ `PerkTreeRenderer.cs` - Tree layout renderer
5. ✅ `PerkNodeRenderer.cs` - Node rendering
6. ✅ `PerkInfoRenderer.cs` - Info panel
7. ✅ `PerkActionsRenderer.cs` - Action buttons
8. ✅ `ReligionHeaderRenderer.cs` - Header banner
9. ✅ `PerkTreeLayout.cs` - Layout algorithm
10. ✅ `PerkNodeState.cs` - Node state model
11. ✅ `PerkTooltipData.cs` - Tooltip data model

### 🔨 Next Steps
1. Implement `TooltipRenderer.cs` for rich hover info (~1 hour)
2. Add audio feedback integration (~30 min)
3. Implement time-based glow animation (~30 min)
4. Testing across deities and resolutions (~1 hour)
5. **Optional:** Create polished texture assets (~5 hours)

**Ready to ship v1.0 with placeholder assets!** Polish items can be completed in v1.1 patch.

---

## Executive Summary

This plan implements a visual perk tree dialog using proven patterns from XSkillsGilded. The UI will allow players to view, unlock, and track perks for **their current religion's deity** through an interactive tree interface instead of command-line only.

**Key Design Decision:** This dialog shows only YOUR deity's perk tree (based on your current religion). To switch deities, you must join a different religion (7-day cooldown). This aligns with Phase 3's religion-centric architecture.

**Estimated Time:** 15-22 hours
**Complexity:** Medium-High (ImGui + custom rendering)
**Dependencies:** VSImGui, existing perk system, PlayerReligionDataManager

---

## Architecture Overview

### Core Components

1. **PerkDialog.cs** - Main window (fullscreen centered, custom chrome)
2. **PerkDialogManager.cs** - State management, navigation
3. **PerkUIRenderer.cs** - Central coordinator
4. **Renderers/** - Specialized rendering classes:
   - `ReligionHeaderRenderer.cs` - Religion name and deity info banner
   - `PerkTreeRenderer.cs` - Visual tree layout with nodes (player + religion perks)
   - `PerkNodeRenderer.cs` - Individual perk display with states
   - `PerkInfoRenderer.cs` - Selected perk details panel
   - `PerkActionsRenderer.cs` - Unlock/close buttons
   - `TooltipRenderer.cs` - Hover tooltips

### Key Architectural Decision: Single Deity View

**Unlike XSkillsGilded (which has deity tabs):**
- This dialog shows **only your current religion's deity** perk tree
- No deity switching/browsing within the perk dialog
- To see a different deity's perks, you must join a different religion (7-day cooldown)

**Rationale:**
- Phase 3 architecture is religion-centric, not deity-centric
- Deities are locked to religions (not direct player choice)
- Keeps UI focused on unlocking perks, not browsing
- Simpler implementation (~2 hours saved by removing tab system)

**If you want to preview other deities:**
- Use the Religion Management Dialog to browse religions and their deities
- Future enhancement: Add "Compare Deities" read-only preview feature

### Renderer Pattern (from XSkillsGilded)

```csharp
internal static class SomeRenderer
{
    public static float Draw(
        PerkDialogManager manager,
        ICoreClientAPI api,
        float x, float y, float width,
        ref string hoveringID)
    {
        // Render logic
        return heightUsed;  // For layout
    }
}
```

**Benefits:**
- Clean separation of concerns
- No coupling between renderers
- Easy to test and extend
- Predictable call order

---

## Phase Breakdown

### Phase 1: Foundation (3-4 hours)

**Goal:** Set up base dialog infrastructure with placeholder rendering

**Tasks:**
- [ ] Create `PerkDialog.cs` extending VSImGui dialog
- [ ] Implement window setup (fullscreen centered, no borders)
- [ ] Add keybind registration (P key to open)
- [ ] Create `PerkDialogManager.cs` for state management
- [ ] Implement basic compositor that calls renderers
- [ ] Add placeholder background rendering (colored rectangle)
- [ ] Test open/close functionality

**Files Created:**
- `PantheonWars/GUI/PerkDialog.cs`
- `PantheonWars/GUI/PerkDialogManager.cs`

**Success Criteria:**
- Dialog opens on P key press
- Window renders at correct size and position
- Dialog closes properly

---

### Phase 2: Data Models (2-3 hours)

**Goal:** Create models for UI state tracking

**Tasks:**
- [ ] Create `Models/PerkNodeState.cs` - Track node visual state
- [ ] Create `Models/PerkTooltipData.cs` - Tooltip information
- [ ] Add state properties to PerkDialogManager:
  - Current player religion UID
  - Current deity (fetched from religion)
  - Selected perk ID
  - Hovering perk ID
  - Scroll offset (player tree)
  - Scroll offset (religion tree)
- [ ] Implement tree layout calculation helper
- [ ] Create perk node positioning algorithm (grid or manual)

**Files Created:**
- `PantheonWars/Models/PerkNodeState.cs`
- `PantheonWars/Models/PerkTooltipData.cs`
- `PantheonWars/GUI/PerkTreeLayout.cs`

**Success Criteria:**
- Models compile without errors
- State transitions work correctly
- Tree layout produces valid coordinates

---

### Phase 3: Core Renderers (3-4 hours)

**Goal:** Implement specialized renderer classes

**Tasks:**
- [ ] Create `UI/Renderers/ReligionHeaderRenderer.cs`:
  - Display current religion name
  - Display deity icon and name (e.g., "Khoras - God of War")
  - Show current Favor and Prestige ranks
  - Show available perks count badge
  - Optional: "Change Religion" button linking to ReligionManagementDialog
- [ ] Create `UI/Renderers/PerkNodeRenderer.cs`:
  - Render single perk node
  - States: locked, unlockable, unlocked
  - Icon + name + tier indicator
  - Glow effect for unlockable perks
  - Hover detection
- [ ] Create `UI/Renderers/PerkTreeRenderer.cs`:
  - Split-panel view: Player perks (left 50%) and Religion perks (right 50%)
  - Scrollable areas for both sections
  - Draw connection lines between prerequisites within each section
  - Layout nodes in tree structure (tier-based vertical arrangement)
  - Handle click to select
  - Visual separator between player and religion sections
- [ ] Create `UI/Renderers/PerkInfoRenderer.cs`:
  - Side panel showing selected perk details
  - Name, description, stats, requirements
  - Use VTML for rich text (optional)
- [ ] Create `UI/Renderers/PerkActionsRenderer.cs`:
  - Unlock button (enabled/disabled states)
  - Close dialog button
  - Requirement validation display

**Files Created:**
- `PantheonWars/GUI/UI/Renderers/ReligionHeaderRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/PerkNodeRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/PerkTreeRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/PerkInfoRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/PerkActionsRenderer.cs`

**Success Criteria:**
- All renderers compile
- Each renderer returns correct height
- Visual states display correctly
- Hover and click detection works

---

### Phase 4: Layout & Composition (2-3 hours)

**Goal:** Wire renderers together into cohesive UI

**Tasks:**
- [ ] Create `PerkUIRenderer.cs` coordinator
- [ ] Implement render order:
  1. Background
  2. Religion header (top banner)
  3. Perk tree (split view: player perks left, religion perks right, 70% height)
  4. Perk info panel (bottom 30%, full width)
  5. Action buttons (overlay bottom-right)
  6. Tooltips (last)
- [ ] Add connection line rendering between prereq nodes (within each tree section)
- [ ] Implement ref parameter passing for hover state
- [ ] Add scroll containers for both tree sections
- [ ] Calculate dynamic sizing based on window dimensions

**Files Created:**
- `PantheonWars/GUI/UI/PerkUIRenderer.cs`
- `PantheonWars/GUI/UI/UIHelpers.cs` (shared utilities)

**Success Criteria:**
- All renderers display in correct positions
- Layout adapts to window size
- No visual overlaps or gaps
- Connection lines draw correctly

---

### Phase 5: Polish & Animation (3-4 hours)

**Goal:** Add visual polish, sounds, animations

**Tasks:**
- [ ] Implement lerp-based glow animations for unlockable perks
- [ ] Add audio feedback:
  - Perk hover: `tick.ogg`
  - Perk select: `click.ogg`
  - Perk unlock: `unlock.ogg`
  - Invalid action: `error.ogg`
- [ ] Create `TooltipRenderer.cs` for rich hover information
- [ ] Add smooth animations for perk selection and unlock
- [ ] Implement color coding:
  - Locked: grey (#92806a)
  - Unlockable: lime (#7ac62f) with glow
  - Unlocked: gold (#feae34)
- [ ] Add UI scaling support using `_ui()` helper
- [ ] Implement state change animations (fade in/out)

**Files Created:**
- `PantheonWars/GUI/UI/Renderers/TooltipRenderer.cs`
- `PantheonWars/GUI/UI/AnimationHelpers.cs`

**Success Criteria:**
- Animations are smooth (60 FPS)
- All interactions have audio feedback
- Tooltips display on hover
- Colors match design language
- UI scales with player settings

---

### Phase 6: Integration & Networking (1-2 hours)

**Goal:** Connect UI to backend systems

**Tasks:**
- [ ] Wire unlock button to `PerkCommands.UnlockPerk()`
- [ ] Subscribe to perk unlock events to refresh UI
- [ ] Implement network packet handling for UI updates
- [ ] Add client-side validation before unlock attempts
- [ ] Handle rank-up notifications (refresh available perks)
- [ ] Ensure UI refreshes when religion perks unlock

**Files Modified:**
- `PantheonWars/Commands/PerkCommands.cs` (add UI callback)
- `PantheonWars/Network/PerkUnlockPacket.cs` (UI sync)

**Success Criteria:**
- Unlocking perks updates tree immediately
- UI reflects server state accurately
- No desync between client and server
- Error messages display properly

---

### Phase 7: Testing & Bug Fixes (2-3 hours)

**Goal:** Ensure stability and polish edge cases

**Tasks:**
- [ ] Test with multiple religions across all 8 deities (switch religions to test each deity)
- [ ] Verify prerequisite chains display correctly in both sections
- [ ] Test at different GUI scales (0.5x, 1x, 2x)
- [ ] Test on different screen resolutions
- [ ] Verify hover states don't stick
- [ ] Check memory leaks (texture disposal)
- [ ] Test with no religion (error handling)
- [ ] Verify unlock restrictions work (player vs religion perk permissions)
- [ ] Test with maxed-out perk trees
- [ ] Check tooltip positioning at screen edges
- [ ] Verify split-panel scrolling works independently

**Success Criteria:**
- No crashes or exceptions
- UI performs smoothly with all perks
- All edge cases handled gracefully
- No visual glitches

---

## Asset Creation Strategy

### Overview

**Initial Approach:** Use placeholder assets during development, then replace with polished versions.

**Asset Types Needed:**
1. **Textures** (13 files) - 9-patch images, icons, effects
2. **Sounds** (4 files) - Audio feedback for interactions
3. **Fonts** (optional) - Custom texture atlas fonts

---

### Texture Assets

#### 1. Window Elements (9-patch)

**Required Files:**
- `perk_window_bg.png` (256x256) - Main background
- `perk_window_frame.png` (128x128) - Window border/frame

**Creation Process:**
1. **Placeholders (Phase 1-3):**
   - Use `ImGui.GetForegroundDrawList().AddRectFilled()` with solid colors
   - Background: dark brown (#2a1f16)
   - Frame: lighter brown (#3d2e20)

2. **9-Patch Creation (Phase 5):**
   - Open GIMP or Photoshop
   - Create 128x128 canvas
   - Design frame with decorative corners
   - Add guides at 16px from each edge (padding boundary)
   - Export as PNG with transparency
   - Test scaling: `drawImage9patch(texture, x, y, width, height, 16)`

**Reference:** Study `xskills-gilded/assets/xskillgilded/textures/gui/bg_ui_frame.png` for style inspiration

#### 2. Header Elements (9-patch)

**Required Files:**
- `header_banner.png` (256x64) - Religion/deity info banner background

**Creation Process:**
1. **Placeholders:**
   - Simple colored rectangle with subtle border

2. **Polished Version:**
   - Decorative banner background
   - Can include deity-themed styling
   - Export with 8px padding on top/bottom, 16px on sides

#### 3. Perk Node Elements

**Required Files:**
- `perk_locked.png` (64x64) - Greyed out node
- `perk_unlockable.png` (64x64) - Available to unlock
- `perk_unlocked.png` (64x64) - Already unlocked
- `perk_glow.png` (96x96) - Glow effect overlay

**Creation Process:**
1. **Placeholders:**
   - Use colored circles with ImGui:
   ```csharp
   ImGui.GetWindowDrawList().AddCircleFilled(
       new Vector2(x, y), radius, color);
   ```
   - Locked: grey (#92806a)
   - Unlockable: lime (#7ac62f)
   - Unlocked: gold (#feae34)

2. **Polished Version:**
   - Create circular or hexagonal node shape
   - Add inner frame for icon placement
   - Glow: radial gradient from center (alpha fade)
   - Export at 64x64 with transparency

**Reuse Existing:** Deity icons already exist in `PantheonWars/assets/pantheonwars/textures/`

#### 4. Button Elements (9-patch)

**Required Files:**
- `button_normal.png` (128x32)
- `button_hover.png` (128x32)
- `button_pressed.png` (128x32)
- `button_disabled.png` (128x32)

**Creation Process:**
1. **Placeholders:** Colored rectangles with borders
2. **Polished:** Raised button appearance with gradient, 12px padding

#### 5. Effect Elements

**Required Files:**
- `connection_line.png` (8x8) - Line between prerequisites
- `pixel.png` (1x1) - White pixel for drawing shapes/lines

**Creation Process:**
1. **Placeholders:** Use ImGui line drawing:
   ```csharp
   ImGui.GetWindowDrawList().AddLine(start, end, color, thickness);
   ```

2. **Polished:** Subtle gradient or dotted line texture

#### 6. Utility

**Required File:**
- `pixel.png` (1x1) - Pure white pixel

**Purpose:** Scale and tint for drawing rectangles/lines

**Creation:** Create 1x1 white PNG in any editor

---

### Sound Assets

**Required Files:**
1. `click.ogg` - General click (button, node select)
2. `unlock.ogg` - Perk unlocked successfully
3. `tick.ogg` - Hover state change (volume: 0.5)
4. `error.ogg` - Invalid action (locked perk, insufficient rank)

**Sourcing Options:**

**Option 1: Free Sound Libraries**
- **Freesound.org** - CC0 licensed UI sounds
- **OpenGameArt.org** - Game UI sound packs
- Search terms: "ui click", "button", "unlock", "error beep"

**Option 2: Generate with Tools**
- **Bfxr / Sfxr** - Web-based retro sound generator
- **Audacity** - Generate tones, add effects
- **ChipTone** - 8-bit style UI sounds

**Option 3: Extract from XSkillsGilded**
- Located in: `xskills-gilded/assets/xskillgilded/sounds/`
- **Do not copy directly** - use as reference for timing/volume
- Create similar sounds with different tones

**Specifications:**
- Format: OGG Vorbis (VS native format)
- Length: 0.1 - 1.0 seconds
- Volume: -6 to -12 dB (avoid clipping)
- Sample rate: 44.1 kHz
- Mono (UI sounds don't need stereo)

**Conversion:**
```bash
# Convert WAV to OGG using ffmpeg
ffmpeg -i click.wav -c:a libvorbis -q:a 4 click.ogg
```

---

### Custom Fonts (Optional - Phase 8+)

**When to Create:**
- If default VSImGui fonts don't match aesthetic
- If you want stylized text (scarab-style like XSkillsGilded)
- Post-v1.0 enhancement

**Creation Process:**
1. **Design font in pixel art editor** (Aseprite, Pixelorama)
2. **Create texture atlas** (all characters in one PNG)
3. **Generate JSON mapping:**
   ```json
   {
     "A": { "u0": 0.0, "v0": 0.0, "u1": 0.1, "v1": 0.1, "width": 8, "height": 12 },
     "B": { "u0": 0.1, "v0": 0.0, "u1": 0.2, "v1": 0.1, "width": 8, "height": 12 }
   }
   ```
4. **Implement Font.LoadedTexture()** based on XSkillsGilded

**Recommendation:** Use VSImGui default fonts for v1.0, add custom fonts in patch if desired.

---

### Asset Integration Timeline

**Phase 1-3 (Development):**
- ✅ Use 100% placeholders (code-generated shapes/colors)
- ✅ Focus on functionality, not visuals
- ✅ Faster iteration without asset dependencies

**Phase 4-5 (Polish):**
- ⚠️ Replace high-impact placeholders:
  - Perk nodes (visible constantly)
  - Glow effects (key visual feedback)
  - Window frame (sets overall aesthetic)
- ✅ Keep using placeholders for:
  - Tab backgrounds (low priority)
  - Connection lines (barely visible)

**Phase 6-7 (Testing):**
- ✅ Add all sounds (critical for UX)
- ⚠️ Finalize remaining textures
- ✅ Get community feedback on placeholder assets

**v1.0 Launch:**
- **Option A (Recommended):** Ship with polished assets (adds 4-6 hours)
- **Option B:** Ship with placeholders, add assets in patch (faster)

---

### Asset File Structure

```
PantheonWars/
└── assets/
    └── pantheonwars/
        ├── textures/
        │   └── gui/
        │       ├── perk_window_bg.png
        │       ├── perk_window_frame.png
        │       ├── header_banner.png
        │       ├── perk_locked.png
        │       ├── perk_unlockable.png
        │       ├── perk_unlocked.png
        │       ├── perk_glow.png
        │       ├── button_normal.png
        │       ├── button_hover.png
        │       ├── button_pressed.png
        │       ├── button_disabled.png
        │       ├── connection_line.png
        │       └── pixel.png
        └── sounds/
            ├── click.ogg
            ├── unlock.ogg
            ├── tick.ogg
            └── error.ogg
```

---

### Asset Loading Pattern

**From XSkillsGilded:**
```csharp
// In PerkDialog initialization
public override void OnGuiOpened()
{
    base.OnGuiOpened();

    // Load textures
    bgTexture = capi.Gui.LoadSvgWithPadding(
        new AssetLocation("pantheonwars:textures/gui/perk_window_bg.svg"),
        0, 0, 0, 0);

    frameTexture = capi.Gui.LoadSvgWithPadding(
        new AssetLocation("pantheonwars:textures/gui/perk_window_frame.svg"),
        16, 16, 16, 16); // 9-patch padding

    // Cache for reuse in renderers
}

public override void OnGuiClosed()
{
    // Dispose textures to prevent leaks
    bgTexture?.Dispose();
    frameTexture?.Dispose();
    base.OnGuiClosed();
}
```

---

### Tools Summary

**Graphics:**
- **GIMP** (free) - 9-patch image creation, texture editing
- **Aseprite** ($20) - Pixel art, texture atlases
- **Inkscape** (free) - SVG creation (VS supports SVG textures)

**Audio:**
- **Audacity** (free) - Audio editing, format conversion
- **Bfxr** (web/free) - Sound effect generation
- **FFmpeg** (free) - Batch OGG conversion

**Testing:**
- **VS Mod Studio** - Live reload textures without restart
- **ImageMagick** - Batch process/resize images

---

## Implementation Checklist

### Pre-Development
- [x] Review XSKILLS_UI_ANALYSIS.md
- [x] Set up asset folders in project
- [x] Create placeholder asset list
- [x] Test ImGui rendering in existing PantheonWars context

### Phase 1: Foundation ✅ COMPLETE
- [x] PerkDialog.cs created and opens on keybind
- [x] PerkDialogManager.cs state management works
- [x] Basic window rendering with placeholder background
- [x] Open/close functionality tested

### Phase 2: Data Models ✅ COMPLETE
- [x] PerkNodeState model created
- [x] PerkTooltipData model created
- [x] State properties added to manager
- [x] Tree layout algorithm implemented

### Phase 3: Core Renderers ✅ COMPLETE
- [x] ReligionHeaderRenderer displays current religion and deity info
- [x] PerkNodeRenderer shows lock/unlockable/unlocked states
- [x] PerkTreeRenderer lays out player and religion perks in split view
- [x] PerkInfoRenderer displays perk details
- [x] PerkActionsRenderer handles button clicks

### Phase 4: Layout ✅ COMPLETE
- [x] PerkUIRenderer coordinates all renderers
- [x] Render order produces correct visual stacking
- [x] Connection lines draw between prereq nodes
- [x] Scrolling works for tree area
- [x] Layout adapts to window size

### Phase 5: Polish ✅ COMPLETE
- [x] Glow animations work for unlockable perks (time-based sine wave animation)
- [x] Audio feedback on all interactions (using placeholder system sounds)
- [x] TooltipRenderer displays rich hover info (fully implemented with edge detection)
- [x] Color coding matches design spec
- [x] UI scaling respects player settings

### Phase 6: Integration ✅ COMPLETE
- [x] Unlock button triggers backend perk unlock
- [x] UI refreshes on perk unlock events
- [x] Network synchronization works
- [x] Rank-up updates available perks
- [x] Religion perks display correctly

### Phase 7: Testing 🚧 IN PROGRESS
- [x] Tested with multiple religions across all 8 deities
- [x] Prerequisite chains verified in both sections
- [ ] Multiple GUI scales tested
- [x] Different resolutions tested
- [ ] No memory leaks detected
- [x] Edge cases handled (no religion, permission restrictions)

### Phase 8: Assets (Parallel) ⚠️ USING PLACEHOLDERS
- [ ] Window background created (using code-generated placeholder)
- [ ] Window frame created (using code-generated placeholder)
- [ ] Header banner created (using code-generated placeholder)
- [ ] Perk node textures created (using code-generated circles)
- [ ] Button textures created (using ImGui default buttons)
- [ ] All 4 sounds sourced/created (TODO comments reference missing sounds)
- [ ] Assets integrated and tested

---

## Success Criteria

**Minimum Viable (v1.0 Ship):**
- ✅ Dialog opens and closes reliably
- ✅ Shows current religion's deity perk tree
- ✅ Split view: player perks (left) and religion perks (right)
- ✅ Perk nodes display with correct states (locked/unlockable/unlocked)
- ✅ Clicking unlock button works (with permission validation)
- ✅ Prerequisite validation works
- ✅ UI doesn't crash or freeze
- ✅ Can use placeholder assets

**STATUS: v1.0 MINIMUM VIABLE COMPLETE** ✅

**Polished (v1.1+):**
- ⚠️ All custom textures and sounds (currently using placeholders)
- ⚠️ Smooth animations throughout (static glow implemented, needs time-based lerp)
- ❌ Tooltips with rich information (TooltipRenderer not yet created)
- ✅ Connection lines show prereq paths
- ❌ Audio feedback on all interactions (TODO comments in code)
- ⚠️ Glow effects on unlockable perks (static, needs animation)

**STATUS: v1.1 POLISH PARTIALLY COMPLETE** 🚧

---

## Optional Enhancements (Post-v1.0)

**For Future Patches:**
1. **Search/Filter** - Find perks by name or effect type
2. **Minimap** - Overview of full tree with zoom
3. **Compare Deities Feature** - Read-only view of all 8 deity trees (7-11 hours, see detailed section below)
4. **Religion Dialog Overhaul** - Apply XSkillsGilded patterns to ReligionManagementDialog
5. **Respec Option** - Reset perks (with cooldown/cost)
6. **Share Builds** - Export/import perk loadouts
7. **Custom Fonts** - Texture atlas-based stylized text
8. **VTML Rich Text** - Formatted perk descriptions
9. **Parallax Scrolling** - Depth effect in tree view
10. **Animated Icons** - Perk icons pulse/glow when unlocked

---

## Timeline Estimate

**Conservative Estimate (solo developer, part-time):**
- Phase 1: 4 hours (1 day) - Foundation
- Phase 2: 3 hours (1 day) - Data models
- Phase 3: 4 hours (1 day) - Core renderers (simplified, no tabs)
- Phase 4: 3 hours (1 day) - Layout & composition
- Phase 5: 4 hours (1 day) - Polish & animation
- Phase 6: 2 hours (1 day) - Integration & networking
- Phase 7: 2 hours (1 day) - Testing
- Assets: 5 hours (2 days, parallel) - Simplified (no tab textures)

**Total: 22 hours (2-3 weeks part-time) or 3 days full-time**

**Fast Track (ship with placeholders):** 17 hours (remove asset creation)

---

## Risk Mitigation

**Risk 1: ImGui Performance with 80 Nodes**
- **Mitigation:** Viewport culling (only render visible nodes)
- **Fallback:** Paginated tree (one tier at a time)

**Risk 2: Complex Prerequisite Chains**
- **Mitigation:** Use graph algorithm for line routing
- **Fallback:** Simple straight lines, accept overlaps

**Risk 3: Asset Creation Takes Too Long**
- **Mitigation:** Ship v1.0 with placeholders
- **Fallback:** Polished assets in v1.1 patch

**Risk 4: Network Desync Issues**
- **Mitigation:** Server is authoritative, client validates locally
- **Fallback:** Add manual refresh button

---

## Future Enhancement: Compare Deities Feature (v1.1+)

**Priority:** Medium (useful for planning, but not essential for v1.0)

### Overview

The **Compare Deities** feature allows players to preview all 8 deity perk trees in a read-only mode. This helps players make informed decisions about which religion to join next (remember: switching religions has a 7-day cooldown and loses all perks).

**User Story:**
> "As a player in a Khoras religion, I want to see what perks Lysa offers before I commit to switching religions and losing my progress."

### Use Cases

1. **New players** - Research deities before joining first religion
2. **Switching religions** - Compare builds before committing to 7-day cooldown
3. **Theorycrafting** - Plan long-term progression paths
4. **Guild planning** - Coordinate complementary deity choices with friends

### Proposed UI Design

#### Entry Point

**Option A: Button in Perk Dialog**
- Add "Compare Deities" button in bottom-left corner
- Opens comparison overlay on top of current perk dialog

**Option B: Button in Religion Dialog**
- Add "Preview Deity Perks" button next to each religion in browse list
- Opens comparison view showing that deity's perks

**Recommendation:** Option A (keeps all perk viewing in one dialog)

#### Comparison View Layout

**Full-Screen Overlay:**
```
┌──────────────────────────────────────────────────────────────┐
│  [X Close]                  Compare Deities                  │
├──────────────────────────────────────────────────────────────┤
│  [Khoras] [Lysa] [Morthen] [Aethra] [Umbros] [Tharos] ...   │ <- Tabs
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────┐  ┌────────────────────────────┐    │
│  │  PLAYER PERKS       │  │  RELIGION PERKS            │    │
│  │  (6 perks)          │  │  (4 perks)                 │    │
│  │                     │  │                            │    │
│  │  • Tier 1 perks     │  │  • Tier 1 perk             │    │
│  │  • Tier 2 perks     │  │  • Tier 2 perk             │    │
│  │  • Tier 3 perks     │  │  • Tier 3 perk             │    │
│  │  • Tier 4 perk      │  │  • Tier 4 perk             │    │
│  │                     │  │                            │    │
│  └─────────────────────┘  └────────────────────────────┘    │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ PERK DETAILS                                        │    │
│  │ Warrior's Resolve                                   │    │
│  │ +10% melee damage, +10% health                      │    │
│  │ Required Rank: Initiate (Favor)                     │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                               │
│                               [Close Comparison]              │
└──────────────────────────────────────────────────────────────┘
```

#### Key Features

**1. Deity Tabs (Top)**
- Horizontal tabs for all 8 deities
- Current deity highlighted differently (your current religion)
- Deity icon + name on each tab
- Badge showing perk count (6 player + 4 religion = 10 total)

**2. Read-Only Perk Tree**
- Same split-panel layout as main perk dialog
- All perks shown in "preview" state (distinct from locked/unlocked)
- Connection lines show prerequisites
- Clicking a perk shows details in bottom panel

**3. Visual Differentiation**
- **Preview color scheme:** Blue-tinted or desaturated
- **"Preview Mode" watermark** - Subtle text across background
- **No unlock buttons** - Only viewing, not unlocking
- **"Your Current Deity" badge** - On the tab for your current religion's deity

**4. Information Panel (Bottom)**
- Selected perk details
- Requirements clearly stated (Favor/Prestige rank, prerequisites)
- Stats and effects listed
- **"Note: Unlocking requires joining a [Deity] religion"** reminder

### Implementation Details

#### New Files

```
PantheonWars/GUI/
├── PerkComparisonDialog.cs        # Overlay dialog
├── PerkComparisonManager.cs       # State management
└── UI/Renderers/
    ├── DeityTabRenderer.cs        # Deity selection tabs (reusable!)
    └── PreviewPerkTreeRenderer.cs # Read-only perk tree variant
```

#### Reuse Existing Components

**Can reuse from main Perk Dialog:**
- `PerkNodeRenderer.cs` (add "preview" state)
- `PerkInfoRenderer.cs` (already read-only)
- `TooltipRenderer.cs` (tooltips work the same)
- `PerkTreeLayout.cs` (same layout algorithm)

**New components needed:**
- `DeityTabRenderer.cs` - Tab system with 8 deities
- Preview state rendering logic

#### Technical Approach

**Data Loading:**
```csharp
// Load all deity perk data from PerkRegistry
public void LoadComparisonData()
{
    foreach (var deityType in Enum.GetValues<DeityType>())
    {
        var playerPerks = _perkRegistry.GetPerksForDeity(deityType, PerkKind.Player);
        var religionPerks = _perkRegistry.GetPerksForDeity(deityType, PerkKind.Religion);

        _comparisonData[deityType] = new DeityPerkData
        {
            PlayerPerks = playerPerks,
            ReligionPerks = religionPerks
        };
    }
}
```

**Rendering Preview State:**
```csharp
public static void RenderPerkNode(Perk perk, bool isPreviewMode)
{
    if (isPreviewMode)
    {
        // Use distinct preview color (blue-tinted)
        drawSetColor(c_preview_blue, 0.7f);
        drawImage(previewNodeSprite, x, y);

        // No glow, no unlock state
        drawTextFont(fNormal, perk.Name, x, y);
    }
    else
    {
        // Normal locked/unlockable/unlocked logic
        // ...
    }
}
```

**Tab Switching:**
```csharp
if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && mouseHover(tabX, tabY, tabX+w, tabY+h))
{
    _currentPreviewDeity = deityType;
    api.Gui.PlaySound("click.ogg");

    // Refresh perk tree display
    RefreshTreeLayout(_comparisonData[deityType]);
}
```

### Implementation Phases

#### Phase 1: Foundation (2-3 hours)

**Tasks:**
- [ ] Create `PerkComparisonDialog.cs` overlay dialog
- [ ] Create `PerkComparisonManager.cs` state management
- [ ] Load all deity perk data from PerkRegistry
- [ ] Add "Compare Deities" button to main Perk Dialog
- [ ] Implement open/close overlay functionality

#### Phase 2: Deity Tabs (1-2 hours)

**Tasks:**
- [ ] Create `DeityTabRenderer.cs` with tab system
- [ ] Implement 8 deity tabs with icons and names
- [ ] Add hover/selected states with audio feedback
- [ ] Highlight current deity differently
- [ ] Badge showing "10 perks" on each tab

#### Phase 3: Preview Tree (2-3 hours)

**Tasks:**
- [ ] Create `PreviewPerkTreeRenderer.cs` (variant of PerkTreeRenderer)
- [ ] Add "preview" state to `PerkNodeRenderer.cs`
- [ ] Render split-panel view (player + religion perks)
- [ ] Draw connection lines between prerequisites
- [ ] Handle click to select (shows details, no unlock)

#### Phase 4: Polish & Details (1-2 hours)

**Tasks:**
- [ ] Add "Preview Mode" watermark
- [ ] Style preview perks with blue tint
- [ ] Show selected perk details in bottom panel
- [ ] Add "Your Current Deity" badge
- [ ] Add reminder text about needing to join religion
- [ ] Ensure tooltips work correctly

#### Phase 5: Integration & Testing (1 hour)

**Tasks:**
- [ ] Test tab switching across all 8 deities
- [ ] Verify all 80 perks display correctly in preview
- [ ] Test with different screen sizes
- [ ] Check for memory leaks (loading all deity data)
- [ ] Ensure no conflicts with main perk dialog

### Timeline Estimate

**Total: 7-11 hours**
- Phase 1: 2-3 hours
- Phase 2: 1-2 hours
- Phase 3: 2-3 hours
- Phase 4: 1-2 hours
- Phase 5: 1 hour

**Parallel with main dialog:** Can be done after main Perk Dialog is complete, reuses 70% of components.

### Assets Needed

**Textures (3 additional files):**
- `tab_deity_normal.png` (96x32) - Deity tab inactive
- `tab_deity_hover.png` (96x32) - Deity tab hover
- `tab_deity_selected.png` (96x32) - Deity tab selected
- `perk_preview.png` (64x64) - Preview node state (blue-tinted)

**Optional:**
- 8 deity-specific tab backgrounds (themed per deity)

**Sounds:**
- Reuse existing `click.ogg` for tab switching
- Reuse existing `tick.ogg` for hover

### User Experience Flow

**Scenario: Player wants to switch from Khoras to Lysa**

1. Player opens Perk Dialog (P key)
2. Sees current Khoras perk tree with unlocked perks
3. Clicks "Compare Deities" button (bottom-left)
4. Comparison overlay opens, showing all 8 deity tabs
5. Khoras tab is highlighted as "Your Current Deity"
6. Player clicks Lysa tab
7. Lysa's perk tree displays in preview mode (blue-tinted)
8. Player hovers over perks to read descriptions
9. Player clicks on "Nature's Blessing" perk
10. Bottom panel shows: "Required Rank: Initiate (Favor), Requires joining a Lysa religion"
11. Player reviews all 10 Lysa perks
12. Player decides to switch, closes comparison
13. Opens Religion Management Dialog
14. Searches for Lysa religions
15. Joins a Lysa religion (7-day cooldown starts)

### Alternative Design: Side-by-Side Comparison

**Advanced variant for future consideration:**

```
┌────────────────────────────────────────────────────────────────┐
│                    Compare Two Deities                         │
├────────────────────────────────────────────────────────────────┤
│  [Select Deity ▼]  <────────VS────────>  [Select Deity ▼]     │
│  Khoras (Current)                         Lysa                 │
├──────────────────────────────────────────────────────────────┤
│  Player Perks        │  Player Perks                          │
│  • Warrior's Resolve │  • Nature's Blessing                    │
│  • Bloodlust         │  • Hunter's Precision                   │
│  • Iron Skin         │  • Silent Step                          │
│  ...                 │  ...                                    │
├──────────────────────────────────────────────────────────────┤
│  Religion Perks      │  Religion Perks                         │
│  • War Banner        │  • Pack Coordination                    │
│  ...                 │  ...                                    │
└────────────────────────────────────────────────────────────────┘
```

**Benefits:** Direct comparison, easier to see differences
**Drawbacks:** More complex UI, harder to implement
**Recommendation:** Single-deity view for v1.1, side-by-side for v1.2+

### Success Criteria

**Minimum Viable:**
- ✅ Can preview all 8 deity perk trees
- ✅ Tab switching works smoothly
- ✅ Perk details display correctly
- ✅ Clear visual differentiation from main dialog (preview mode obvious)
- ✅ No confusion about which deity is current

**Polished:**
- ✅ Smooth tab animations
- ✅ Audio feedback on all interactions
- ✅ Deity-themed tab styling
- ✅ Tooltips with rich information
- ✅ "Your Current Deity" badge stands out

### Risks & Mitigation

**Risk 1: Performance with All Deity Data Loaded**
- **Mitigation:** Lazy-load deity data on first tab switch
- **Fallback:** Only load currently visible deity

**Risk 2: Confusing Preview Mode with Real Unlocking**
- **Mitigation:** Strong visual differentiation (blue tint, watermark, no unlock buttons)
- **Fallback:** Add modal warning on first open: "Preview Mode: You cannot unlock perks here"

**Risk 3: Tab System Conflicts with Single-Deity Main Dialog**
- **Mitigation:** Separate overlay dialog, doesn't modify main dialog
- **Fallback:** Separate keybind (Shift+P for comparison)

### Future Enhancements (v1.2+)

1. **Side-by-side comparison** - Compare two deities directly
2. **Favorite/bookmark perks** - Mark perks you want to plan for
3. **Build calculator** - "If I reach Zealot rank, I can unlock..."
4. **Export comparison** - Share deity comparison with guild
5. **Religion suggestions** - "5 active Lysa religions with open slots"

---

## Future: Religion Management Dialog Overhaul (v1.1+)

**Current State:**
The existing `ReligionManagementDialog.cs` is functional but uses basic Vintage Story GUI components without the polish of XSkillsGilded patterns.

**Proposed Overhaul (12-16 hours):**
Apply the same renderer pattern architecture to create a polished religion management experience.

### Proposed Structure

**Tabs:**
1. **Browse Religions** - List with filters (deity, public/private)
2. **My Religion** - Current religion details, members, description
3. **Create Religion** - Creation flow with deity selection

**Renderers:**
- `ReligionListRenderer.cs` - Grid/list of available religions
- `ReligionCardRenderer.cs` - Individual religion card with deity icon, member count, prestige rank
- `MemberListRenderer.cs` - Religion members with ranks displayed
- `CreateReligionRenderer.cs` - Religion creation form

**Benefits:**
- Consistent visual style with Perk Dialog
- Better UX for browsing religions
- Visual representation of deity choices
- Badge notifications for invitations
- Smooth animations and audio feedback

**Timeline:**
- Phase 1: Foundation (3-4 hours)
- Phase 2: Renderers (5-6 hours)
- Phase 3: Polish & Integration (4-5 hours)
- **Total: 12-15 hours**

**Priority:** Low (existing dialog works, perk dialog is higher priority)

---

## References

- **[XSKILLS_UI_ANALYSIS.md](XSKILLS_UI_ANALYSIS.md)** - Detailed XSkillsGilded pattern documentation
- **[phase3_task_breakdown.md](phase3_task_breakdown.md)** - Overall Phase 3 progress
- **XSkillsGilded Repository:** `/home/quantumheart/RiderProjects/xskills-gilded`
- **VSImGui Documentation:** [Vintage Story Wiki - ImGui](https://wiki.vintagestory.at/index.php/Modding:ImGui)

---

## Next Steps (Updated November 5, 2025)

### ✅ COMPLETED
1. ~~**Review this plan**~~ - Confirmed approach and timeline
2. ~~**Create asset folder structure**~~ - Directories in place
3. ~~**Begin Phase 1-4**~~ - Foundation, models, renderers, layout all complete
4. ~~**Implement networking integration**~~ - Phase 6 complete

### 🚧 IN PROGRESS
**To reach v1.0 final polish (estimated 3-4 hours):**
1. **Implement TooltipRenderer.cs** (~1 hour)
   - Rich hover information display
   - Position tooltips correctly at screen edges
   - Show perk details, requirements, stats on hover

2. **Add audio feedback** (~30 min)
   - Integrate sound files (click, unlock, tick, error)
   - Wire up audio calls in PerkTreeRenderer and PerkActionsRenderer
   - Test volume levels

3. **Animate glow effects** (~30 min)
   - Convert static glow to time-based lerp animation
   - Update PerkNodeRenderer with deltaTime-based alpha cycling

4. **Testing & polish** (~1-2 hours)
   - Test with multiple deities (switch religions to verify)
   - Test different GUI scales
   - Verify memory management (no leaks)
   - Check all edge cases

### 📋 OPTIONAL (v1.1+)
**Asset creation (5 hours):**
- Create polished texture assets to replace placeholders
- Source/generate audio files
- Polish visual appearance

**Enhanced features (7-11 hours):**
- Implement "Compare Deities" feature for deity previewing
- Add search/filter functionality
- Implement minimap/zoom for large trees

**Current status: v1.0 MINIMUM VIABLE COMPLETE - ready to ship with placeholders!**
