# XSkillsGilded UI System Analysis

**Date:** November 3, 2025
**Purpose:** Research XSkillsGilded's polished UI implementation to inform PantheonWars Blessing Dialog design

---

## Executive Summary

XSkillsGilded implements a professional-grade ImGui-based UI system with excellent architecture, visual polish, and interaction patterns. The system uses a **Renderer Pattern** with specialized static renderer classes, custom 9-patch image rendering, texture atlas-based fonts, and rich text markup (VTML).

**Key Repository:** `/home/quantumheart/RiderProjects/xskills-gilded`

---

## Architecture Overview

### Core Components

1. **Main Window System** (`main.cs`)
   - Full-screen centered window with custom background/frame
   - Dynamic sizing with screen constraints
   - No title bar, borders, or scrolling (custom chrome)

2. **Renderer Pattern** (`UI/SkillUIRenderer.cs`)
   - Central coordinator orchestrates specialized renderers
   - Each renderer is a static class with single `Draw()` method
   - Pure rendering logic, no internal state

3. **Specialized Renderers** (`UI/Renderers/`)
   - `SkillGroupTabRenderer.cs` - Top-level tabs
   - `SkillsTabRenderer.cs` - Skill sub-tabs
   - `AbilityRenderer.cs` - Ability tree with scrolling
   - `SkillDescriptionRenderer.cs` - Info panels
   - `SkillActionsRenderer.cs` - Action buttons
   - `TooltipRenderer.cs` - Hover tooltips

4. **Core Utilities** (`ImGuiUtil.cs`)
   - Drawing primitives (text, images, 9-patch, shapes)
   - Color utilities and animations
   - Mouse interaction helpers
   - Layout calculations

5. **Custom Components**
   - `Font.cs` - Texture atlas-based custom fonts
   - `VTML.cs` - Rich text markup parser (HTML-like)
   - `resourceLoader.cs` - Texture/asset loading
   - `levelPopup.cs` - Animated notification popups
   - `effectBox.cs` - Status effect HUD

---

## Key Patterns & Techniques

### 1. Renderer Architecture Pattern

**Signature:**
```csharp
internal static class SomeRenderer
{
    public static float Draw(
        SkillPageManager pageManager,
        ICoreClientAPI api,
        float x, float y, float width,
        ref string hoveringID,
        ref TooltipObject hoveringTooltip)
    {
        // Render logic
        return heightUsed;  // For layout
    }
}
```

**Benefits:**
- Clean separation of concerns
- Easy to test and extend
- No coupling between renderers
- Predictable call order

**Coordination via ref parameters:**
- `ref string hoveringID` - Track which element is hovered
- `ref TooltipObject hoveringTooltip` - Build tooltip data
- Cross-renderer communication without tight coupling

### 2. 9-Patch Image System

**Purpose:** Scalable UI elements without distortion

**Algorithm:**
```csharp
drawImage9patch(texture, x, y, width, height, padding)
{
    // Divide image into 9 sections:
    // [TL] [TC] [TR]
    // [ML] [MC] [MR]
    // [BL] [BC] [BR]

    // Corners: Fixed size (padding x padding)
    // Edges: Stretched in one dimension
    // Center: Stretched in both dimensions
}
```

**Use Cases:**
- Window frames
- Button backgrounds
- Panel borders
- Progress bars

### 3. Custom Font System

**Texture Atlas Approach:**
```csharp
Font.LoadedTexture(api, texture, jsonMapping)
{
    // JSON maps character codes to UV coordinates
    atlas['A'] = { u0, v0, u1, v1, width, height }

    // Per-character rendering
    drawCharColor(char c, Vector4 color) {
        ImGui.Image(texture, size, atlas[c].uv0, atlas[c].uv1, color)
    }
}
```

**Advantages:**
- Stylized fonts (scarab, scarab_gold)
- Per-character color control
- Letter spacing and scaling
- Consistent with game aesthetic

### 4. VTML Rich Text System

**Markup:**
```html
<font color="#7ac62f">Green text</font>
<strong>Bold text</strong>
<i>Italic text</i>
<br/>Line break
```

**Usage:**
```csharp
var blocks = VTML.parseVTML(description);
drawTextVTML(blocks, x, y, wrapWidth);
```

**Features:**
- Word-wrap support
- Color and opacity control
- Font styling (bold, italic)
- Parsed once, rendered many times

### 5. Tab System Pattern

**States:** Normal, Hovering, Selected

**Visual Feedback:**
```csharp
if (isSelected) {
    drawImage(Sprite("tab_selected"));
    drawTextFont(fTitleGold, name);  // Gold text
} else if (isHovering) {
    drawImage(Sprite("tab_hover"));
    drawTextFont(fTitle, name);
    if (ImGui.IsMouseClicked(Left)) {
        SetPage(tab);
        PlaySound("page.ogg");
    }
} else {
    drawImage(Sprite("tab_normal"));
    drawSetColor(c_white, 0.5f);  // Dimmed
    drawTextFont(fTitle, name);
}
```

**Badge Notifications:**
```csharp
if (availablePoints > 0) {
    drawSetColor(c_lime, 0.3f);
    drawImage9patch(glow, ...);
    drawTextFont(fSubtitle, points.ToString());
}
```

### 6. Smooth Animations

**Lerp-based transitions:**
```csharp
button.glowAlpha = lerpTo(button.glowAlpha, target, speed, deltaTime);

lerpTo(current, target, speed, dt) {
    return current + (target - current) * (1 - Math.Pow(speed, dt));
}
```

**Easing functions:**
```csharp
smoothstep(t) { return t * t * (3 - 2 * t); }
invLerp2(x, a, b, c, d) { /* Custom interpolation */ }
```

### 7. Audio Feedback

**All interactions have sound:**
- Click: `click.ogg`, `upgraded.ogg`, `downgraded.ogg`
- Hover state change: `tick.ogg` (volume 0.5)
- Page change: `page.ogg`
- Max tier reached: `upgradedmax.ogg`

### 8. Scrollable Content with Parallax

**Parallax offset based on mouse position:**
```csharp
var mrx = (mouseX - scrollX) / scrollWidth - 0.5f;  // -0.5 to 0.5
var mry = (mouseY - scrollY) / scrollHeight - 0.5f;

var offsetX = -paddingX * mrx;  // Shift content opposite to mouse
var offsetY = -paddingY * mry;

ImGui.BeginChild("ScrollArea", size, false, flags);
// Draw content at (baseX + offsetX, baseY + offsetY)
ImGui.EndChild();
```

**Creates depth illusion**

### 9. UI Scaling System

**Respect player's GUI scale:**
```csharp
public const float baseUiScale = 1.125f;
public static float uiScale = 1.125f;

_ui(pixelValue) { return pixelValue / baseUiScale * uiScale; }

// Update from game settings
uiScale = ClientSettings.GUIScale;
```

**Always use `_ui()` for dimensions:**
```csharp
var padding = _ui(16);
var buttonWidth = _ui(128);
```

### 10. Visual State Indicators

**Color Coding:**
- `c_lime` (#7ac62f) - Available, active
- `c_gold` (#feae34) - Maxed, selected
- `c_red` (#bf663f) - Locked, error
- `c_grey` (#92806a) - Disabled, inactive
- `c_dkgrey` (#392a1c) - Background

**Alpha Transparency:**
- 1.0 - Fully active
- 0.5 - Dimmed/disabled
- 0.3 - Subtle glow effect
- Animated - Transitions between states

**Glow Effects:**
```csharp
if (hasAvailablePoints) {
    drawSetColor(c_lime, 0.3f);
    drawImage9patch(glow, x-16, y-12, w+32, h+24, 15);
}
```

---

## File Structure

```
xSkillGilded/xSkillGilded/
├── main.cs                          # Entry point, window management
├── ImGuiUtil.cs                     # Core drawing utilities
├── font.cs                          # Custom font system
├── VTML.cs                          # Rich text parser
├── resourceLoader.cs                # Asset loading
├── levelPopup.cs                    # Level-up notification
├── effectBox.cs                     # Status effect display
│
├── Models/
│   ├── AbilityButton.cs             # Button state model
│   ├── DecorationLine.cs            # Connection lines
│   └── TooltipObject.cs             # Tooltip data
│
├── Managers/
│   └── SkillPageManager.cs          # State management, navigation
│
├── Utilities/
│   ├── AbilityFormatter.cs          # Text formatting
│   └── RequirementHelper.cs         # Validation logic
│
└── UI/
    ├── SkillUIRenderer.cs           # Main coordinator
    ├── UIHelpers.cs                 # Shared utilities
    └── Renderers/                   # Specialized renderers
        ├── AbilityRenderer.cs
        ├── SkillActionsRenderer.cs
        ├── SkillDescriptionRenderer.cs
        ├── SkillGroupTabRenderer.cs
        ├── SkillsTabRenderer.cs
        └── TooltipRenderer.cs
```

---

## Best Practices Identified

### ✅ Adopt These

1. **Renderer Pattern** - Clean, testable, extensible
2. **9-Patch System** - Scalable UI without distortion
3. **Audio Feedback** - Professional feel, all interactions
4. **State-Based Rendering** - Visual feedback (hover, selected, disabled)
5. **Layout Return Values** - Flexible vertical stacking
6. **Smooth Animations** - Lerp transitions with deltaTime
7. **Color Coding** - Consistent visual language
8. **UI Scaling** - Respect player settings

### 💡 Consider These

1. **VTML Rich Text** - Formatted descriptions (if needed)
2. **Custom Font System** - Stylized text (if default fonts insufficient)
3. **Glow Effects** - Visual emphasis for important elements
4. **Parallax Scrolling** - Depth in scrollable areas
5. **Tooltip System** - Consistent hover information

### ⚠️ Be Cautious

1. **Custom Font Atlas** - Adds complexity, consider if ImGui fonts suffice
2. **Static Renderers** - May need instances if per-renderer state required
3. **Global UI State** - Works but limits testability

---

## Key Code Patterns

### Pattern 1: Basic Renderer Template

```csharp
internal static class MyRenderer
{
    public static float Draw(
        BlessingPageManager pageManager,
        ICoreClientAPI api,
        float x, float y, float width,
        ref string hoveringID)
    {
        float startY = y;

        // Render content
        drawTextFont(fTitle, "Content", x, y);
        y += fTitle.getLineHeight();

        // Interaction
        if (mouseHover(x, y, x + width, y + height)) {
            hoveringID = "ElementID";
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) {
                api.Gui.PlaySound("click.ogg");
                // Handle click
            }
        }

        return y - startY;  // Height used
    }
}
```

### Pattern 2: Tab System

```csharp
foreach (var tab in tabs) {
    bool isSelected = (currentTab == tab);
    bool isHovering = mouseHover(tabX, tabY, tabX+w, tabY+h);

    if (isSelected) {
        drawImage(selectedSprite);
        drawTextFont(goldFont, tab.Name);
    } else if (isHovering) {
        drawImage(hoverSprite);
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left)) {
            currentTab = tab;
            api.Gui.PlaySound("tab.ogg");
        }
    } else {
        drawImage(normalSprite);
        drawSetColor(c_white, 0.5f);
        drawTextFont(normalFont, tab.Name);
        drawSetColor(c_white);
    }
}
```

### Pattern 3: Animated Button

```csharp
// Model
class Button {
    public float glowAlpha = 0f;
}

// Update
button.glowAlpha = lerpTo(button.glowAlpha, 0, .2f, deltaTime);

// Render
if (button.glowAlpha > 0) {
    drawSetColor(c_lime, button.glowAlpha);
    drawImage9patch(glow, x-pad, y-pad, w+pad*2, h+pad*2, 15);
}

// Trigger
if (clicked) {
    button.glowAlpha = 1f;  // Start animation
    api.Gui.PlaySound("click.ogg");
}
```

---

## Performance Considerations

1. **Lazy Initialization** - Wait for API/data availability
2. **VTML Caching** - Parse once, render many times
3. **Texture Preloading** - Load during init, not every frame
4. **Efficient Hit Testing** - Early exit on mouse position
5. **Viewport Culling** - Only render visible elements (in scrollable areas)

---

## Asset Requirements

### Textures Needed

- **Window elements**: background, frame (9-patch)
- **Tab elements**: normal, hover, selected
- **Button elements**: idle, hover, pressing (all 9-patch)
- **Icons**: deity icons, blessing icons
- **Effects**: glow, shadow overlays
- **Utility**: pixel (1x1 white for drawing rectangles)

### Fonts (Optional)

- Title font (large, stylized)
- Body font (readable, medium)
- Small font (details, numbers)

### Sounds

- Click/select: `click.ogg`
- Hover transition: `tick.ogg`
- Unlock blessing: `unlock.ogg`
- Max tier: `maxed.ogg`
- Error/invalid: `error.ogg`

---

## Recommended Reading Order

1. **`ImGuiUtil.cs`** - Core utilities (524 lines)
2. **`UI/SkillUIRenderer.cs`** - Coordination pattern (80 lines)
3. **`UI/Renderers/SkillGroupTabRenderer.cs`** - Tab system (140 lines)
4. **`UI/Renderers/AbilityRenderer.cs`** - Complex interactions (220 lines)
5. **`font.cs`** - Custom font system (153 lines)

---

## Conclusion

XSkillsGilded demonstrates a **production-quality UI system** with:
- Clean, maintainable architecture
- Polished visual design
- Rich interactions with audio/visual feedback
- Smooth animations and transitions
- Excellent code organization

**This is an ideal reference for building PantheonWars' Blessing Dialog UI.**
