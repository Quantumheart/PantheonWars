# Blessing Dialog UI Implementation Plan

**Date:** November 4, 2025
**Last Updated:** November 5, 2025
**Purpose:** Implement polished ImGui-based blessing tree viewer for PantheonWars
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
1. ✅ `BlessingDialog.cs` - Main dialog system
2. ✅ `BlessingDialogManager.cs` - State management
3. ✅ `BlessingUIRenderer.cs` - Coordinator
4. ✅ `BlessingTreeRenderer.cs` - Tree layout renderer
5. ✅ `BlessingNodeRenderer.cs` - Node rendering
6. ✅ `BlessingInfoRenderer.cs` - Info panel
7. ✅ `BlessingActionsRenderer.cs` - Action buttons
8. ✅ `ReligionHeaderRenderer.cs` - Header banner
9. ✅ `BlessingTreeLayout.cs` - Layout algorithm
10. ✅ `BlessingNodeState.cs` - Node state model
11. ✅ `BlessingTooltipData.cs` - Tooltip data model

### 🔨 Next Steps
1. Implement `TooltipRenderer.cs` for rich hover info (~1 hour)
2. Add audio feedback integration (~30 min)
3. Implement time-based glow animation (~30 min)
4. Testing across deities and resolutions (~1 hour)
5. **Optional:** Create polished texture assets (~5 hours)

**Ready to ship v1.0 with placeholder assets!** Polish items can be completed in v1.1 patch.

---

## Executive Summary

This plan implements a visual blessing tree dialog using proven patterns from XSkillsGilded. The UI will allow players to view, unlock, and track blessings for **their current religion's deity** through an interactive tree interface instead of command-line only.

**Key Design Decision:** This dialog shows only YOUR deity's blessing tree (based on your current religion). To switch deities, you must join a different religion (7-day cooldown). This aligns with Phase 3's religion-centric architecture.

**Estimated Time:** 15-22 hours
**Complexity:** Medium-High (ImGui + custom rendering)
**Dependencies:** VSImGui, existing blessing system, PlayerReligionDataManager

---

## Architecture Overview

### Core Components

1. **BlessingDialog.cs** - Main window (fullscreen centered, custom chrome)
2. **BlessingDialogManager.cs** - State management, navigation
3. **BlessingUIRenderer.cs** - Central coordinator
4. **Renderers/** - Specialized rendering classes:
   - `ReligionHeaderRenderer.cs` - Religion name and deity info banner
   - `BlessingTreeRenderer.cs` - Visual tree layout with nodes (player + religion blessings)
   - `BlessingNodeRenderer.cs` - Individual blessing display with states
   - `BlessingInfoRenderer.cs` - Selected blessing details panel
   - `BlessingActionsRenderer.cs` - Unlock/close buttons
   - `TooltipRenderer.cs` - Hover tooltips

### Key Architectural Decision: Single Deity View

**Unlike XSkillsGilded (which has deity tabs):**
- This dialog shows **only your current religion's deity** blessing tree
- No deity switching/browsing within the blessing dialog
- To see a different deity's blessings, you must join a different religion (7-day cooldown)

**Rationale:**
- Phase 3 architecture is religion-centric, not deity-centric
- Deities are locked to religions (not direct player choice)
- Keeps UI focused on unlocking blessings, not browsing
- Simpler implementation (~2 hours saved by removing tab system)

**If you want to preview other deities:**
- Use the Religion Management Dialog to browse religions and their deities
- Future enhancement: Add "Compare Deities" read-only preview feature

### Renderer Pattern (from XSkillsGilded)

```csharp
internal static class SomeRenderer
{
    public static float Draw(
        BlessingDialogManager manager,
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
- [ ] Create `BlessingDialog.cs` extending VSImGui dialog
- [ ] Implement window setup (fullscreen centered, no borders)
- [ ] Add keybind registration (P key to open)
- [ ] Create `BlessingDialogManager.cs` for state management
- [ ] Implement basic compositor that calls renderers
- [ ] Add placeholder background rendering (colored rectangle)
- [ ] Test open/close functionality

**Files Created:**
- `PantheonWars/GUI/BlessingDialog.cs`
- `PantheonWars/GUI/BlessingDialogManager.cs`

**Success Criteria:**
- Dialog opens on P key press
- Window renders at correct size and position
- Dialog closes properly

---

### Phase 2: Data Models (2-3 hours)

**Goal:** Create models for UI state tracking

**Tasks:**
- [ ] Create `Models/BlessingNodeState.cs` - Track node visual state
- [ ] Create `Models/BlessingTooltipData.cs` - Tooltip information
- [ ] Add state properties to BlessingDialogManager:
  - Current player religion UID
  - Current deity (fetched from religion)
  - Selected blessing ID
  - Hovering blessing ID
  - Scroll offset (player tree)
  - Scroll offset (religion tree)
- [ ] Implement tree layout calculation helper
- [ ] Create blessing node positioning algorithm (grid or manual)

**Files Created:**
- `PantheonWars/Models/BlessingNodeState.cs`
- `PantheonWars/Models/BlessingTooltipData.cs`
- `PantheonWars/GUI/BlessingTreeLayout.cs`

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
  - Show available blessings count badge
  - Optional: "Change Religion" button linking to ReligionManagementDialog
- [ ] Create `UI/Renderers/BlessingNodeRenderer.cs`:
  - Render single blessing node
  - States: locked, unlockable, unlocked
  - Icon + name + tier indicator
  - Glow effect for unlockable blessings
  - Hover detection
- [ ] Create `UI/Renderers/BlessingTreeRenderer.cs`:
  - Split-panel view: Player blessings (left 50%) and Religion blessings (right 50%)
  - Scrollable areas for both sections
  - Draw connection lines between prerequisites within each section
  - Layout nodes in tree structure (tier-based vertical arrangement)
  - Handle click to select
  - Visual separator between player and religion sections
- [ ] Create `UI/Renderers/BlessingInfoRenderer.cs`:
  - Side panel showing selected blessing details
  - Name, description, stats, requirements
  - Use VTML for rich text (optional)
- [ ] Create `UI/Renderers/BlessingActionsRenderer.cs`:
  - Unlock button (enabled/disabled states)
  - Close dialog button
  - Requirement validation display

**Files Created:**
- `PantheonWars/GUI/UI/Renderers/ReligionHeaderRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/BlessingNodeRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/BlessingTreeRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/BlessingInfoRenderer.cs`
- `PantheonWars/GUI/UI/Renderers/BlessingActionsRenderer.cs`

**Success Criteria:**
- All renderers compile
- Each renderer returns correct height
- Visual states display correctly
- Hover and click detection works

---

### Phase 4: Layout & Composition (2-3 hours)

**Goal:** Wire renderers together into cohesive UI

**Tasks:**
- [ ] Create `BlessingUIRenderer.cs` coordinator
- [ ] Implement render order:
  1. Background
  2. Religion header (top banner)
  3. Blessing tree (split view: player blessings left, religion blessings right, 70% height)
  4. Blessing info panel (bottom 30%, full width)
  5. Action buttons (overlay bottom-right)
  6. Tooltips (last)
- [ ] Add connection line rendering between prereq nodes (within each tree section)
- [ ] Implement ref parameter passing for hover state
- [ ] Add scroll containers for both tree sections
- [ ] Calculate dynamic sizing based on window dimensions

**Files Created:**
- `PantheonWars/GUI/UI/BlessingUIRenderer.cs`
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
- [ ] Implement lerp-based glow animations for unlockable blessings
- [ ] Add audio feedback:
  - Blessing hover: `tick.ogg`
  - Blessing select: `click.ogg`
  - Blessing unlock: `unlock.ogg`
  - Invalid action: `error.ogg`
- [ ] Create `TooltipRenderer.cs` for rich hover information
- [ ] Add smooth animations for blessing selection and unlock
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
- [ ] Wire unlock button to `BlessingCommands.UnlockBlessing()`
- [ ] Subscribe to blessing unlock events to refresh UI
- [ ] Implement network packet handling for UI updates
- [ ] Add client-side validation before unlock attempts
- [ ] Handle rank-up notifications (refresh available blessings)
- [ ] Ensure UI refreshes when religion blessings unlock

**Files Modified:**
- `PantheonWars/Commands/BlessingCommands.cs` (add UI callback)
- `PantheonWars/Network/BlessingUnlockPacket.cs` (UI sync)

**Success Criteria:**
- Unlocking blessings updates tree immediately
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
- [ ] Verify unlock restrictions work (player vs religion blessing permissions)
- [ ] Test with maxed-out blessing trees
- [ ] Check tooltip positioning at screen edges
- [ ] Verify split-panel scrolling works independently

**Success Criteria:**
- No crashes or exceptions
- UI performs smoothly with all blessings
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
- `blessing_window_bg.png` (256x256) - Main background
- `blessing_window_frame.png` (128x128) - Window border/frame

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

#### 3. Blessing Node Elements

**Required Files:**
- `blessing_locked.png` (64x64) - Greyed out node
- `blessing_unlockable.png` (64x64) - Available to unlock
- `blessing_unlocked.png` (64x64) - Already unlocked
- `blessing_glow.png` (96x96) - Glow effect overlay

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
2. `unlock.ogg` - Blessing unlocked successfully
3. `tick.ogg` - Hover state change (volume: 0.5)
4. `error.ogg` - Invalid action (locked blessing, insufficient rank)

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
  - Blessing nodes (visible constantly)
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
        │       ├── blessing_window_bg.png
        │       ├── blessing_window_frame.png
        │       ├── header_banner.png
        │       ├── blessing_locked.png
        │       ├── blessing_unlockable.png
        │       ├── blessing_unlocked.png
        │       ├── blessing_glow.png
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
// In BlessingDialog initialization
public override void OnGuiOpened()
{
    base.OnGuiOpened();

    // Load textures
    bgTexture = capi.Gui.LoadSvgWithPadding(
        new AssetLocation("pantheonwars:textures/gui/blessing_window_bg.svg"),
        0, 0, 0, 0);

    frameTexture = capi.Gui.LoadSvgWithPadding(
        new AssetLocation("pantheonwars:textures/gui/blessing_window_frame.svg"),
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
- [x] BlessingDialog.cs created and opens on keybind
- [x] BlessingDialogManager.cs state management works
- [x] Basic window rendering with placeholder background
- [x] Open/close functionality tested

### Phase 2: Data Models ✅ COMPLETE
- [x] BlessingNodeState model created
- [x] BlessingTooltipData model created
- [x] State properties added to manager
- [x] Tree layout algorithm implemented

### Phase 3: Core Renderers ✅ COMPLETE
- [x] ReligionHeaderRenderer displays current religion and deity info
- [x] BlessingNodeRenderer shows lock/unlockable/unlocked states
- [x] BlessingTreeRenderer lays out player and religion blessings in split view
- [x] BlessingInfoRenderer displays blessing details
- [x] BlessingActionsRenderer handles button clicks

### Phase 4: Layout ✅ COMPLETE
- [x] BlessingUIRenderer coordinates all renderers
- [x] Render order produces correct visual stacking
- [x] Connection lines draw between prereq nodes
- [x] Scrolling works for tree area
- [x] Layout adapts to window size

### Phase 5: Polish ✅ COMPLETE
- [x] Glow animations work for unlockable blessings (time-based sine wave animation)
- [x] Audio feedback on all interactions (using placeholder system sounds)
- [x] TooltipRenderer displays rich hover info (fully implemented with edge detection)
- [x] Color coding matches design spec
- [x] UI scaling respects player settings

### Phase 6: Integration ✅ COMPLETE
- [x] Unlock button triggers backend blessing unlock
- [x] UI refreshes on blessing unlock events
- [x] Network synchronization works
- [x] Rank-up updates available blessings
- [x] Religion blessings display correctly

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
- [ ] Blessing node textures created (using code-generated circles)
- [ ] Button textures created (using ImGui default buttons)
- [ ] All 4 sounds sourced/created (TODO comments reference missing sounds)
- [ ] Assets integrated and tested

---

## Success Criteria

**Minimum Viable (v1.0 Ship):**
- ✅ Dialog opens and closes reliably
- ✅ Shows current religion's deity blessing tree
- ✅ Split view: player blessings (left) and religion blessings (right)
- ✅ Blessing nodes display with correct states (locked/unlockable/unlocked)
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
- ⚠️ Glow effects on unlockable blessings (static, needs animation)

**STATUS: v1.1 POLISH PARTIALLY COMPLETE** 🚧

---

## Optional Enhancements (Post-v1.0)

**For Future Patches:**
1. **Search/Filter** - Find blessings by name or effect type
2. **Minimap** - Overview of full tree with zoom
3. **Compare Deities Feature** - Read-only view of all 8 deity trees (7-11 hours, see detailed section below)
4. **Religion Dialog Overhaul** - Apply XSkillsGilded patterns to ReligionManagementDialog
5. **Respec Option** - Reset blessings (with cooldown/cost)
6. **Share Builds** - Export/import blessing loadouts
7. **Custom Fonts** - Texture atlas-based stylized text
8. **VTML Rich Text** - Formatted blessing descriptions
9. **Parallax Scrolling** - Depth effect in tree view
10. **Animated Icons** - Blessing icons pulse/glow when unlocked

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

The **Compare Deities** feature allows players to preview all 8 deity blessing trees in a read-only mode. This helps players make informed decisions about which religion to join next (remember: switching religions has a 7-day cooldown and loses all blessings).

**User Story:**
> "As a player in a Khoras religion, I want to see what blessings Lysa offers before I commit to switching religions and losing my progress."

### Use Cases

1. **New players** - Research deities before joining first religion
2. **Switching religions** - Compare builds before committing to 7-day cooldown
3. **Theorycrafting** - Plan long-term progression paths
4. **Guild planning** - Coordinate complementary deity choices with friends

### Proposed UI Design

#### Entry Point

**Option A: Button in Blessing Dialog**
- Add "Compare Deities" button in bottom-left corner
- Opens comparison overlay on top of current blessing dialog

**Option B: Button in Religion Dialog**
- Add "Preview Deity Blessings" button next to each religion in browse list
- Opens comparison view showing that deity's blessings

**Recommendation:** Option A (keeps all blessing viewing in one dialog)

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
│  │  PLAYER BLESSINGS       │  │  RELIGION BLESSINGS            │    │
│  │  (6 blessings)          │  │  (4 blessings)                 │    │
│  │                     │  │                            │    │
│  │  • Tier 1 blessings     │  │  • Tier 1 blessing             │    │
│  │  • Tier 2 blessings     │  │  • Tier 2 blessing             │    │
│  │  • Tier 3 blessings     │  │  • Tier 3 blessing             │    │
│  │  • Tier 4 blessing      │  │  • Tier 4 blessing             │    │
│  │                     │  │                            │    │
│  └─────────────────────┘  └────────────────────────────┘    │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ BLESSING DETAILS                                        │    │
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
- Badge showing blessing count (6 player + 4 religion = 10 total)

**2. Read-Only Blessing Tree**
- Same split-panel layout as main blessing dialog
- All blessings shown in "preview" state (distinct from locked/unlocked)
- Connection lines show prerequisites
- Clicking a blessing shows details in bottom panel

**3. Visual Differentiation**
- **Preview color scheme:** Blue-tinted or desaturated
- **"Preview Mode" watermark** - Subtle text across background
- **No unlock buttons** - Only viewing, not unlocking
- **"Your Current Deity" badge** - On the tab for your current religion's deity

**4. Information Panel (Bottom)**
- Selected blessing details
- Requirements clearly stated (Favor/Prestige rank, prerequisites)
- Stats and effects listed
- **"Note: Unlocking requires joining a [Deity] religion"** reminder

### Implementation Details

#### New Files

```
PantheonWars/GUI/
├── BlessingComparisonDialog.cs        # Overlay dialog
├── BlessingComparisonManager.cs       # State management
└── UI/Renderers/
    ├── DeityTabRenderer.cs        # Deity selection tabs (reusable!)
    └── PreviewBlessingTreeRenderer.cs # Read-only blessing tree variant
```

#### Reuse Existing Components

**Can reuse from main Blessing Dialog:**
- `BlessingNodeRenderer.cs` (add "preview" state)
- `BlessingInfoRenderer.cs` (already read-only)
- `TooltipRenderer.cs` (tooltips work the same)
- `BlessingTreeLayout.cs` (same layout algorithm)

**New components needed:**
- `DeityTabRenderer.cs` - Tab system with 8 deities
- Preview state rendering logic

#### Technical Approach

**Data Loading:**
```csharp
// Load all deity blessing data from BlessingRegistry
public void LoadComparisonData()
{
    foreach (var deityType in Enum.GetValues<DeityType>())
    {
        var playerBlessings = _blessingRegistry.GetBlessingsForDeity(deityType, BlessingKind.Player);
        var religionBlessings = _blessingRegistry.GetBlessingsForDeity(deityType, BlessingKind.Religion);

        _comparisonData[deityType] = new DeityBlessingData
        {
            PlayerBlessings = playerBlessings,
            ReligionBlessings = religionBlessings
        };
    }
}
```

**Rendering Preview State:**
```csharp
public static void RenderBlessingNode(Blessing blessing, bool isPreviewMode)
{
    if (isPreviewMode)
    {
        // Use distinct preview color (blue-tinted)
        drawSetColor(c_preview_blue, 0.7f);
        drawImage(previewNodeSprite, x, y);

        // No glow, no unlock state
        drawTextFont(fNormal, blessing.Name, x, y);
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

    // Refresh blessing tree display
    RefreshTreeLayout(_comparisonData[deityType]);
}
```

### Implementation Phases

#### Phase 1: Foundation (2-3 hours)

**Tasks:**
- [ ] Create `BlessingComparisonDialog.cs` overlay dialog
- [ ] Create `BlessingComparisonManager.cs` state management
- [ ] Load all deity blessing data from BlessingRegistry
- [ ] Add "Compare Deities" button to main Blessing Dialog
- [ ] Implement open/close overlay functionality

#### Phase 2: Deity Tabs (1-2 hours)

**Tasks:**
- [ ] Create `DeityTabRenderer.cs` with tab system
- [ ] Implement 8 deity tabs with icons and names
- [ ] Add hover/selected states with audio feedback
- [ ] Highlight current deity differently
- [ ] Badge showing "10 blessings" on each tab

#### Phase 3: Preview Tree (2-3 hours)

**Tasks:**
- [ ] Create `PreviewBlessingTreeRenderer.cs` (variant of BlessingTreeRenderer)
- [ ] Add "preview" state to `BlessingNodeRenderer.cs`
- [ ] Render split-panel view (player + religion blessings)
- [ ] Draw connection lines between prerequisites
- [ ] Handle click to select (shows details, no unlock)

#### Phase 4: Polish & Details (1-2 hours)

**Tasks:**
- [ ] Add "Preview Mode" watermark
- [ ] Style preview blessings with blue tint
- [ ] Show selected blessing details in bottom panel
- [ ] Add "Your Current Deity" badge
- [ ] Add reminder text about needing to join religion
- [ ] Ensure tooltips work correctly

#### Phase 5: Integration & Testing (1 hour)

**Tasks:**
- [ ] Test tab switching across all 8 deities
- [ ] Verify all 80 blessings display correctly in preview
- [ ] Test with different screen sizes
- [ ] Check for memory leaks (loading all deity data)
- [ ] Ensure no conflicts with main blessing dialog

### Timeline Estimate

**Total: 7-11 hours**
- Phase 1: 2-3 hours
- Phase 2: 1-2 hours
- Phase 3: 2-3 hours
- Phase 4: 1-2 hours
- Phase 5: 1 hour

**Parallel with main dialog:** Can be done after main Blessing Dialog is complete, reuses 70% of components.

### Assets Needed

**Textures (3 additional files):**
- `tab_deity_normal.png` (96x32) - Deity tab inactive
- `tab_deity_hover.png` (96x32) - Deity tab hover
- `tab_deity_selected.png` (96x32) - Deity tab selected
- `blessing_preview.png` (64x64) - Preview node state (blue-tinted)

**Optional:**
- 8 deity-specific tab backgrounds (themed per deity)

**Sounds:**
- Reuse existing `click.ogg` for tab switching
- Reuse existing `tick.ogg` for hover

### User Experience Flow

**Scenario: Player wants to switch from Khoras to Lysa**

1. Player opens Blessing Dialog (P key)
2. Sees current Khoras blessing tree with unlocked blessings
3. Clicks "Compare Deities" button (bottom-left)
4. Comparison overlay opens, showing all 8 deity tabs
5. Khoras tab is highlighted as "Your Current Deity"
6. Player clicks Lysa tab
7. Lysa's blessing tree displays in preview mode (blue-tinted)
8. Player hovers over blessings to read descriptions
9. Player clicks on "Nature's Blessing" blessing
10. Bottom panel shows: "Required Rank: Initiate (Favor), Requires joining a Lysa religion"
11. Player reviews all 10 Lysa blessings
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
│  Player Blessings        │  Player Blessings                          │
│  • Warrior's Resolve │  • Nature's Blessing                    │
│  • Bloodlust         │  • Hunter's Precision                   │
│  • Iron Skin         │  • Silent Step                          │
│  ...                 │  ...                                    │
├──────────────────────────────────────────────────────────────┤
│  Religion Blessings      │  Religion Blessings                         │
│  • War Banner        │  • Pack Coordination                    │
│  ...                 │  ...                                    │
└────────────────────────────────────────────────────────────────┘
```

**Benefits:** Direct comparison, easier to see differences
**Drawbacks:** More complex UI, harder to implement
**Recommendation:** Single-deity view for v1.1, side-by-side for v1.2+

### Success Criteria

**Minimum Viable:**
- ✅ Can preview all 8 deity blessing trees
- ✅ Tab switching works smoothly
- ✅ Blessing details display correctly
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
- **Fallback:** Add modal warning on first open: "Preview Mode: You cannot unlock blessings here"

**Risk 3: Tab System Conflicts with Single-Deity Main Dialog**
- **Mitigation:** Separate overlay dialog, doesn't modify main dialog
- **Fallback:** Separate keybind (Shift+P for comparison)

### Future Enhancements (v1.2+)

1. **Side-by-side comparison** - Compare two deities directly
2. **Favorite/bookmark blessings** - Mark blessings you want to plan for
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
- Consistent visual style with Blessing Dialog
- Better UX for browsing religions
- Visual representation of deity choices
- Badge notifications for invitations
- Smooth animations and audio feedback

**Timeline:**
- Phase 1: Foundation (3-4 hours)
- Phase 2: Renderers (5-6 hours)
- Phase 3: Polish & Integration (4-5 hours)
- **Total: 12-15 hours**

**Priority:** Low (existing dialog works, blessing dialog is higher priority)

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
   - Show blessing details, requirements, stats on hover

2. **Add audio feedback** (~30 min)
   - Integrate sound files (click, unlock, tick, error)
   - Wire up audio calls in BlessingTreeRenderer and BlessingActionsRenderer
   - Test volume levels

3. **Animate glow effects** (~30 min)
   - Convert static glow to time-based lerp animation
   - Update BlessingNodeRenderer with deltaTime-based alpha cycling

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

---

## Phase 9: Favor & Prestige Progress Indicators (NEW)

**Date Added:** November 11, 2025
**Priority:** High (missing core progression feedback)
**Estimated Time:** 3-4 hours

### 🎯 Overview

Currently, the Blessing Dialog lacks visual feedback for:
1. **Player Favor Progress** - How close the player is to the next Favor Rank
2. **Religion Prestige Progress** - How close the religion is to the next Prestige Rank

These are critical UX elements that help players understand:
- How much favor/prestige they need to unlock the next tier of blessings
- Their current progression toward rank-up goals
- Whether they should focus on earning more favor/prestige

### 📍 Proposed Location

**Integration with Religion Header (Top Banner):**

```
┌──────────────────────────────────────────────────────────────┐
│  [Deity Icon]  Religion Name - Deity Name                    │
│                                                                │
│  Player Progress:    ████████░░  Zealot (234/500 Favor)      │
│  Religion Progress:  ██████░░░░  Renowned (1,245/2,000 Prestige) │
└──────────────────────────────────────────────────────────────┘
```

**Alternative: Separate Progress Panel**
- Position below Religion Header
- More space for detailed breakdown
- Can include "Next Unlock" preview

### 🎨 Visual Design

#### Progress Bar Specifications

**Player Favor Bar:**
- **Color:** Gold (#feae34) with darker gold background (#92806a)
- **Width:** 200-300px
- **Height:** 20px
- **Style:** Rounded corners (4px), subtle inner shadow
- **Animation:** Smooth fill on rank-up (lerp over 0.5 seconds)

**Religion Prestige Bar:**
- **Color:** Purple/Blue (#7b68ee) with darker background (#4b3a8e)
- **Width:** 200-300px
- **Height:** 20px
- **Style:** Rounded corners (4px), subtle inner shadow
- **Animation:** Smooth fill when prestige increases

**Text Label Format:**
```
Rank Name (Current/Required)
Examples:
- "Zealot (234/500 Favor)"
- "Renowned (1,245/2,000 Prestige)"
```

#### Visual States

**1. Normal State:**
- Standard progress bar with fill percentage
- Current rank name displayed
- Numeric progress shown

**2. Near Rank-Up (>80% progress):**
- Progress bar glows with pulsing animation
- Color intensifies (brighter gold/purple)
- Optional: "Almost there!" tooltip on hover

**3. Rank-Up Animation:**
- Fill completes (100%)
- Brief flash/shimmer effect
- Number transitions to new rank values
- Audio: "rank_up.ogg" sound effect

**4. Max Rank:**
- Progress bar shows "MAX" or full gold fill
- Different color scheme (platinum/rainbow gradient)
- No numeric values (already at maximum)

### 📐 Implementation Details

#### Phase 9.1: Data Integration (1 hour)

**Tasks:**
1. **Query Current Progress from Backend**
   ```csharp
   // In BlessingDialogManager.cs
   public PlayerFavorProgress GetPlayerFavorProgress()
   {
       var playerData = _religionDataManager.GetPlayerReligionData(_playerUID);
       return new PlayerFavorProgress
       {
           CurrentFavor = playerData.DivineFavor,
           RequiredFavor = GetRequiredFavorForNextRank(playerData.FavorRank),
           CurrentRank = playerData.FavorRank,
           NextRank = playerData.FavorRank + 1,
           IsMaxRank = playerData.FavorRank >= 4
       };
   }

   public ReligionPrestigeProgress GetReligionPrestigeProgress()
   {
       var religionData = _religionDataManager.GetReligionData(_currentReligionUID);
       return new ReligionPrestigeProgress
       {
           CurrentPrestige = religionData.Prestige,
           RequiredPrestige = GetRequiredPrestigeForNextRank(religionData.PrestigeRank),
           CurrentRank = religionData.PrestigeRank,
           NextRank = religionData.PrestigeRank + 1,
           IsMaxRank = religionData.PrestigeRank >= 4
       };
   }
   ```

2. **Create Progress Data Models**
   ```csharp
   // Models/PlayerFavorProgress.cs
   public class PlayerFavorProgress
   {
       public int CurrentFavor { get; set; }
       public int RequiredFavor { get; set; }
       public int CurrentRank { get; set; }
       public int NextRank { get; set; }
       public bool IsMaxRank { get; set; }
       public float ProgressPercentage => IsMaxRank ? 1f : (float)CurrentFavor / RequiredFavor;
   }

   // Models/ReligionPrestigeProgress.cs
   public class ReligionPrestigeProgress
   {
       public int CurrentPrestige { get; set; }
       public int RequiredPrestige { get; set; }
       public int CurrentRank { get; set; }
       public int NextRank { get; set; }
       public bool IsMaxRank { get; set; }
       public float ProgressPercentage => IsMaxRank ? 1f : (float)CurrentPrestige / RequiredPrestige;
   }
   ```

3. **Add Rank Requirement Lookup**
   ```csharp
   // In FavorSystem.cs or new helper class
   public static int GetRequiredFavorForNextRank(int currentRank)
   {
       return currentRank switch
       {
           0 => 100,  // Initiate → Devoted
           1 => 250,  // Devoted → Zealot
           2 => 500,  // Zealot → Champion
           3 => 1000, // Champion → Exalted
           4 => 0,    // Max rank
           _ => 0
       };
   }

   public static int GetRequiredPrestigeForNextRank(int currentRank)
   {
       return currentRank switch
       {
           0 => 500,   // Fledgling → Established
           1 => 1500,  // Established → Renowned
           2 => 3500,  // Renowned → Legendary
           3 => 7500,  // Legendary → Mythic
           4 => 0,     // Max rank
           _ => 0
       };
   }
   ```

**Files Created:**
- `PantheonWars/Models/PlayerFavorProgress.cs`
- `PantheonWars/Models/ReligionPrestigeProgress.cs`

**Files Modified:**
- `PantheonWars/GUI/BlessingDialogManager.cs` (add progress queries)
- `PantheonWars/Systems/FavorSystem.cs` (add rank requirement helpers)

#### Phase 9.2: Progress Bar Renderer (1-1.5 hours)

**Tasks:**
1. **Create ProgressBarRenderer.cs**
   ```csharp
   // UI/Renderers/Components/ProgressBarRenderer.cs
   internal static class ProgressBarRenderer
   {
       public static void DrawProgressBar(
           ImDrawListPtr drawList,
           float x, float y, float width, float height,
           float percentage,
           Vector4 fillColor,
           Vector4 backgroundColor,
           string labelText,
           bool showGlow = false)
       {
           // Background
           var bgMin = new Vector2(x, y);
           var bgMax = new Vector2(x + width, y + height);
           var bgColorU32 = ImGui.ColorConvertFloat4ToU32(backgroundColor);
           drawList.AddRectFilled(bgMin, bgMax, bgColorU32, 4f);

           // Fill (progress)
           if (percentage > 0)
           {
               var fillWidth = width * Math.Clamp(percentage, 0f, 1f);
               var fillMax = new Vector2(x + fillWidth, y + height);
               var fillColorU32 = ImGui.ColorConvertFloat4ToU32(fillColor);
               drawList.AddRectFilled(bgMin, fillMax, fillColorU32, 4f);

               // Glow effect (if >80% progress)
               if (showGlow && percentage > 0.8f)
               {
                   var glowAlpha = (float)(Math.Sin(ImGui.GetTime() * 3.0) * 0.3 + 0.7);
                   var glowColor = new Vector4(fillColor.X, fillColor.Y, fillColor.Z, glowAlpha);
                   var glowColorU32 = ImGui.ColorConvertFloat4ToU32(glowColor);
                   drawList.AddRect(bgMin, fillMax, glowColorU32, 4f, ImDrawFlags.None, 2f);
               }
           }

           // Border
           var borderColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.Gold * 0.5f);
           drawList.AddRect(bgMin, bgMax, borderColor, 4f, ImDrawFlags.None, 1f);

           // Label text (centered)
           var textSize = ImGui.CalcTextSize(labelText);
           var textPos = new Vector2(
               x + (width - textSize.X) / 2,
               y + (height - textSize.Y) / 2
           );
           var textColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.White);
           drawList.AddText(textPos, textColor, labelText);
       }
   }
   ```

2. **Add Animation Support**
   - Track previous percentage value
   - Lerp from old to new over time
   - Smooth transitions on favor/prestige gain

**Files Created:**
- `PantheonWars/GUI/UI/Renderers/Components/ProgressBarRenderer.cs`

#### Phase 9.3: Integrate with Religion Header (1 hour)

**Tasks:**
1. **Update ReligionHeaderRenderer.cs**
   ```csharp
   // Add after deity icon and name display
   public static float Draw(...)
   {
       // ... existing deity icon and name rendering ...

       var currentY = y + 60f; // After deity section

       // Player Favor Progress
       var favorProgress = manager.GetPlayerFavorProgress();
       var favorLabel = favorProgress.IsMaxRank
           ? $"{GetRankName(favorProgress.CurrentRank)} (MAX)"
           : $"{GetRankName(favorProgress.CurrentRank)} ({favorProgress.CurrentFavor}/{favorProgress.RequiredFavor} Favor)";

       // Label
       var labelPos = new Vector2(x + padding, currentY);
       var labelColor = ImGui.ColorConvertFloat4ToU32(ColorPalette.White);
       drawList.AddText(ImGui.GetFont(), 14f, labelPos, labelColor, "Player Progress:");
       currentY += 18f;

       // Progress bar
       ProgressBarRenderer.DrawProgressBar(
           drawList,
           x + padding + 120f, currentY - 18f, 300f, 20f,
           favorProgress.ProgressPercentage,
           ColorPalette.Gold,
           ColorPalette.DarkBrown,
           favorLabel,
           showGlow: favorProgress.ProgressPercentage > 0.8f
       );

       currentY += 8f;

       // Religion Prestige Progress
       var prestigeProgress = manager.GetReligionPrestigeProgress();
       var prestigeLabel = prestigeProgress.IsMaxRank
           ? $"{GetRankName(prestigeProgress.CurrentRank)} (MAX)"
           : $"{GetRankName(prestigeProgress.CurrentRank)} ({prestigeProgress.CurrentPrestige}/{prestigeProgress.RequiredPrestige} Prestige)";

       // Label
       labelPos = new Vector2(x + padding, currentY);
       drawList.AddText(ImGui.GetFont(), 14f, labelPos, labelColor, "Religion Progress:");
       currentY += 18f;

       // Progress bar
       ProgressBarRenderer.DrawProgressBar(
           drawList,
           x + padding + 120f, currentY - 18f, 300f, 20f,
           prestigeProgress.ProgressPercentage,
           new Vector4(0.48f, 0.41f, 0.93f, 1f), // Purple
           ColorPalette.DarkBrown,
           prestigeLabel,
           showGlow: prestigeProgress.ProgressPercentage > 0.8f
       );

       currentY += 12f;

       return currentY - y; // Total height used
   }
   ```

2. **Adjust Layout Heights**
   - Update `BlessingUIRenderer.cs` to account for taller header
   - Ensure progress bars don't overlap with blessing tree
   - Test at different window sizes

**Files Modified:**
- `PantheonWars/GUI/UI/Renderers/ReligionHeaderRenderer.cs`
- `PantheonWars/GUI/UI/BlessingUIRenderer.cs` (layout adjustments)

#### Phase 9.4: Tooltips & Polish (0.5-1 hour)

**Tasks:**
1. **Add Hover Tooltips**
   ```csharp
   // Show detailed breakdown on hover
   if (mouseOver(progressBarX, progressBarY, progressBarX + width, progressBarY + height))
   {
       var tooltipLines = new List<string>
       {
           $"Current Rank: {GetRankName(currentRank)}",
           $"Progress: {current}/{required}",
           $"Next Rank: {GetRankName(nextRank)}",
           "",
           $"Next Blessing Tier unlocks at {GetRankName(nextRank)}"
       };

       TooltipRenderer.Draw(api, tooltipLines, mouseX, mouseY);
   }
   ```

2. **Add "Next Unlock" Preview**
   - Show next tier blessing count
   - Example: "2 blessings unlock at Champion rank"
   - Display in tooltip

3. **Audio Feedback**
   - Play sound on rank-up: `rank_up.ogg` or `unlock.ogg`
   - Subtle tick sound on hover over progress bars

4. **Rank-Up Notification**
   - When player/religion ranks up, show brief notification overlay
   - "Congratulations! You reached Zealot rank!"
   - Option: Flash the progress bar with animation

**Files Modified:**
- `PantheonWars/GUI/UI/Renderers/ReligionHeaderRenderer.cs` (tooltips)
- `PantheonWars/GUI/UI/Renderers/TooltipRenderer.cs` (if needed)

### 🎨 Visual Examples

#### Normal State
```
Player Progress:    ████████░░  Zealot (234/500 Favor)
                    [=========>-----] 47%
```

#### Near Rank-Up (Glowing)
```
Player Progress:    ████████▓▓  Zealot (487/500 Favor)  ✨
                    [===============>] 97% (13 Favor to go!)
```

#### Max Rank
```
Player Progress:    ██████████  Exalted (MAX)
                    [================] 100% 👑
```

### 🧪 Testing Checklist

**Phase 9 Testing:**
- [ ] Progress bars display correctly for all ranks (0-4)
- [ ] Percentages calculate accurately
- [ ] Max rank displays "MAX" without errors
- [ ] Glow animation triggers at >80% progress
- [ ] Tooltips show detailed breakdown
- [ ] Rank-up animation plays smoothly
- [ ] Audio feedback on rank-up works
- [ ] Layout adapts to different window sizes
- [ ] Progress bars update in real-time when favor/prestige changes
- [ ] No performance issues with animations

### 📦 Assets Needed

**Optional Custom Textures:**
- `progress_bar_bg.png` (8x20) - 9-patch background
- `progress_bar_fill_gold.png` (8x20) - Gold fill gradient
- `progress_bar_fill_purple.png` (8x20) - Purple fill gradient
- `progress_bar_glow.png` (16x24) - Glow overlay effect

**Sound Effects:**
- `rank_up.ogg` - Played on player rank-up (triumphant tone)
- Reuse existing `tick.ogg` for progress bar hover

**Placeholder Approach:**
- Use ImGui `AddRectFilled()` for backgrounds/fills
- Use solid colors with alpha for glow effects
- Ship with placeholders, add polished textures in v1.1

### 🎯 Success Criteria

**Minimum Viable:**
- ✅ Player Favor progress bar displays current progress
- ✅ Religion Prestige progress bar displays current progress
- ✅ Both bars show rank name and numeric values
- ✅ Progress updates when favor/prestige changes
- ✅ Max rank displays correctly

**Polished:**
- ✅ Smooth fill animations on progress changes
- ✅ Glow effect when near rank-up (>80%)
- ✅ Tooltips show detailed breakdown
- ✅ Audio feedback on rank-up
- ✅ "Next Unlock" preview in tooltips
- ✅ Rank-up notification overlay

### ⏱️ Timeline Estimate

**Total: 3-4 hours**
- Phase 9.1: Data Integration (1 hour)
- Phase 9.2: Progress Bar Renderer (1-1.5 hours)
- Phase 9.3: Integration with Header (1 hour)
- Phase 9.4: Tooltips & Polish (0.5-1 hour)

**Fast Track:** 2.5 hours (skip advanced animations and notifications)

### 🔗 Dependencies

**Requires:**
- `PlayerReligionDataManager` access to favor/prestige data
- `FavorSystem` for rank requirement calculations
- `ReligionHeaderRenderer` integration point

**Optional:**
- Custom texture assets (can use placeholders)
- Rank-up sound effect (can reuse `unlock.ogg`)

### 📊 Priority Justification

**Why High Priority:**
1. **Core Feedback Loop** - Players need to see progression toward goals
2. **Motivation** - Visual progress encourages continued play
3. **Clarity** - Removes confusion about "how much favor do I need?"
4. **Standard UX** - Most progression systems show progress bars
5. **Quick Wins** - Relatively simple to implement for high impact

**Comparison to Other Features:**
- Higher priority than "Compare Deities" (nice-to-have)
- Higher priority than asset polish (functional > aesthetic)
- Equal priority to Phase 7 testing (both needed for v1.0)

**Recommendation:** Implement Phase 9 before v1.0 final release. This is a core UX feature, not optional polish.

---

## Updated Implementation Checklist

### Phase 9: Progress Indicators (NEW) 🆕
- [ ] PlayerFavorProgress model created
- [ ] ReligionPrestigeProgress model created
- [ ] Rank requirement lookup functions implemented
- [ ] ProgressBarRenderer.cs created
- [ ] Player Favor progress bar integrated into ReligionHeaderRenderer
- [ ] Religion Prestige progress bar integrated into ReligionHeaderRenderer
- [ ] Hover tooltips show detailed breakdown
- [ ] Glow animation triggers at >80% progress
- [ ] Rank-up audio feedback implemented
- [ ] Max rank displays correctly
- [ ] Layout tested at different window sizes
- [ ] Progress updates in real-time when favor/prestige changes

---

## Updated Timeline Estimate

**Original Total:** 22 hours
**Phase 9 Addition:** +3-4 hours
**New Total:** 25-26 hours

**Updated Status: ~92% Complete (23.5 / 26 hours)**
- Need Phase 9 implementation (3-4 hours)
- Need final testing (1 hour)

**Target: v1.0 Final with Progress Indicators**

---

**Current status: v1.0 RC with Phase 9 documented - ready for implementation!**
