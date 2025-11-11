# Icon Generation Guide - Pantheon Wars

This guide provides practical steps for generating the 88 icons needed for Pantheon Wars.

## Quick Start

**What you need:**
- 8 deity symbols (32x32 PNG)
- 80 perk icons (32x32 PNG)
- Total: 88 icons

**Where to put them:**
- Deity symbols: `PantheonWars/assets/pantheonwars/textures/icons/deities/`
- Perk icons: `PantheonWars/assets/pantheonwars/textures/icons/perks/{deity_name}/`

**Reference documents:**
- `icon_specifications.md` - Detailed visual descriptions for each icon
- `icon_manifest.json` - JSON manifest for tracking progress

---

## Generation Methods

### Method 1: AI Image Generation (Recommended for Speed)

**Tools:**
- **Midjourney** (Discord-based, high quality)
- **DALL-E 3** (via ChatGPT Plus)
- **Stable Diffusion** (Free, local)
- **Leonardo.ai** (Free tier available)

**Example Prompt Template:**
```
32x32 pixel art icon, [DEITY/PERK THEME],
[COLOR PALETTE], [VISUAL ELEMENTS],
simple design, clear at small size,
fantasy game style, transparent background,
vintage story aesthetic
```

**Example - Khoras Deity Symbol:**
```
32x32 pixel art icon, god of war symbol,
crossed battle swords with shield,
deep red orange and metallic gray colors,
bold aggressive sharp edges,
simple design, clear at small size,
fantasy game style, transparent background
```

**Workflow:**
1. Generate images at higher resolution (256x256 or 512x512)
2. Downscale to 32x32 using proper tools (see below)
3. Clean up artifacts
4. Add transparency/remove background
5. Optimize file size

---

### Method 2: Manual Pixel Art Creation

**Recommended Tools:**
- **Aseprite** ($19.99 or compile free) - Industry standard for pixel art
- **Piskel** (Free, browser-based) - https://www.piskelapp.com/
- **GraphicsGale** (Free) - Windows only
- **Pyxel Edit** ($9) - Great for tilesets and icons

**Workflow:**
1. Create new 32x32 canvas
2. Use the specifications in `icon_specifications.md` as reference
3. Start with simple shapes and main elements
4. Add details carefully (remember: 32x32 is small!)
5. Use limited color palette per deity theme
6. Export as PNG with transparency
7. Test visibility at actual size

**Tips for Pixel Art:**
- Use a limited palette (8-12 colors per icon)
- Focus on silhouette clarity
- Avoid single-pixel details that get lost
- Use anti-aliasing sparingly (can look muddy at small sizes)
- Keep important elements centered

---

### Method 3: Hybrid Approach (AI + Manual Refinement)

**Best of both worlds:**
1. Generate base icon with AI
2. Import into pixel art tool
3. Manually clean up and refine
4. Ensure proper sizing and clarity
5. Add final touches

---

## Image Processing Pipeline

### Step 1: Resize to 32x32

**Using ImageMagick (batch processing):**
```bash
# Install ImageMagick first
# Ubuntu/Debian: sudo apt-get install imagemagick
# macOS: brew install imagemagick
# Windows: Download from https://imagemagick.org/

# Resize single image
magick convert input.png -resize 32x32 -background none -gravity center -extent 32x32 output.png

# Batch resize all images in folder
for file in *.png; do
  magick convert "$file" -resize 32x32 -background none -gravity center -extent 32x32 "resized_$file"
done
```

**Using Python (PIL/Pillow):**
```python
from PIL import Image

def resize_icon(input_path, output_path):
    img = Image.open(input_path)
    # Use LANCZOS for high-quality downsampling
    img = img.resize((32, 32), Image.Resampling.LANCZOS)
    img.save(output_path, 'PNG')

# Batch process
import os
for filename in os.listdir('input_folder'):
    if filename.endswith('.png'):
        resize_icon(f'input_folder/{filename}', f'output_folder/{filename}')
```

### Step 2: Remove Background / Add Transparency

**Using GIMP (Free):**
1. Open image
2. Layer → Transparency → Add Alpha Channel
3. Select → By Color → Click background
4. Edit → Clear (or press Delete)
5. Export as PNG

**Using Photopea (Free, browser-based):**
1. Go to https://www.photopea.com/
2. Open image
3. Use Magic Wand tool to select background
4. Delete selection
5. File → Export As → PNG

**Using remove.bg API (automated):**
```bash
# Free tier: 50 images/month
curl -X POST https://api.remove.bg/v1.0/removebg \
  -H "X-Api-Key: YOUR_API_KEY" \
  -F "image_file=@input.png" \
  -F "size=auto" \
  -o output.png
```

### Step 3: Optimize File Size

**Using TinyPNG (web-based):**
- Go to https://tinypng.com/
- Drag and drop your PNGs
- Download optimized versions
- Free tier: 20 images at a time

**Using pngquant (CLI):**
```bash
# Install pngquant
# Ubuntu/Debian: sudo apt-get install pngquant
# macOS: brew install pngquant

# Optimize single file
pngquant --quality=65-80 input.png -o output.png

# Batch optimize
pngquant --quality=65-80 *.png --ext=.png --force
```

---

## Batch Generation Scripts

### Python Script for Batch Processing

```python
#!/usr/bin/env python3
"""
Batch icon processor for Pantheon Wars
Resizes, removes background, and optimizes icons
"""

from PIL import Image
import os
import json

def process_icon(input_path, output_path, size=(32, 32)):
    """Process a single icon: resize and ensure transparency"""
    try:
        img = Image.open(input_path).convert('RGBA')
        img = img.resize(size, Image.Resampling.LANCZOS)
        img.save(output_path, 'PNG', optimize=True)
        print(f"✓ Processed: {os.path.basename(output_path)}")
        return True
    except Exception as e:
        print(f"✗ Failed: {os.path.basename(input_path)} - {e}")
        return False

def batch_process_deity_icons(input_dir, output_dir):
    """Process all deity icons"""
    deities = ['khoras', 'lysa', 'morthen', 'aethra', 'umbros', 'tharos', 'gaia', 'vex']

    for deity in deities:
        input_file = os.path.join(input_dir, f"{deity}.png")
        output_file = os.path.join(output_dir, f"{deity}.png")

        if os.path.exists(input_file):
            process_icon(input_file, output_file)

def batch_process_perk_icons(input_dir, output_base_dir):
    """Process all perk icons organized by deity"""

    # Load manifest to get all perk IDs
    with open('docs/icon_manifest.json', 'r') as f:
        manifest = json.load(f)

    for deity, data in manifest['perk_icons']['by_deity'].items():
        deity_input_dir = os.path.join(input_dir, deity)
        deity_output_dir = os.path.join(output_base_dir, deity)

        os.makedirs(deity_output_dir, exist_ok=True)

        for perk in data['icons']:
            perk_id = perk['id']
            input_file = os.path.join(deity_input_dir, f"{perk_id}.png")
            output_file = os.path.join(deity_output_dir, f"{perk_id}.png")

            if os.path.exists(input_file):
                process_icon(input_file, output_file)

if __name__ == "__main__":
    # Configuration
    INPUT_DEITY_DIR = "raw_icons/deities"
    INPUT_PERK_DIR = "raw_icons/perks"
    OUTPUT_DEITY_DIR = "PantheonWars/assets/pantheonwars/textures/icons/deities"
    OUTPUT_PERK_DIR = "PantheonWars/assets/pantheonwars/textures/icons/perks"

    print("Starting batch icon processing...")
    print("\nProcessing deity icons...")
    batch_process_deity_icons(INPUT_DEITY_DIR, OUTPUT_DEITY_DIR)

    print("\nProcessing perk icons...")
    batch_process_perk_icons(INPUT_PERK_DIR, OUTPUT_PERK_DIR)

    print("\nDone!")
```

### Bash Script for Validation

```bash
#!/bin/bash
# validate_icons.sh - Check if all required icons exist

echo "Validating Pantheon Wars Icons..."
echo "=================================="

missing=0

# Check deity symbols
deities=("khoras" "lysa" "morthen" "aethra" "umbros" "tharos" "gaia" "vex")
echo -e "\nChecking deity symbols..."
for deity in "${deities[@]}"; do
  file="PantheonWars/assets/pantheonwars/textures/icons/deities/${deity}.png"
  if [ -f "$file" ]; then
    echo "✓ $deity"
  else
    echo "✗ MISSING: $deity"
    ((missing++))
  fi
done

# Check perk icons (simplified check)
echo -e "\nChecking perk icons..."
for deity in "${deities[@]}"; do
  count=$(find "PantheonWars/assets/pantheonwars/textures/icons/perks/${deity}" -name "*.png" 2>/dev/null | wc -l)
  expected=10
  if [ "$count" -eq "$expected" ]; then
    echo "✓ $deity: $count/$expected perks"
  else
    echo "✗ $deity: $count/$expected perks (INCOMPLETE)"
    ((missing+=expected-count))
  fi
done

echo -e "\n=================================="
if [ $missing -eq 0 ]; then
  echo "✓ All 88 icons present!"
else
  echo "✗ Missing $missing icons"
  exit 1
fi
```

---

## Recommended Workflows by Skill Level

### Beginner: Use AI Generation + Web Tools
1. Generate icons with ChatGPT/DALL-E using prompts from specifications
2. Download generated images
3. Use Photopea (browser) to remove backgrounds
4. Use TinyPNG (browser) to optimize
5. Manually resize if needed using any image editor

**Time estimate:** 4-6 hours for all 88 icons

---

### Intermediate: AI + Aseprite Refinement
1. Generate high-res icons with Midjourney/Stable Diffusion
2. Import into Aseprite
3. Manually trace/refine as 32x32 pixel art
4. Export with transparency
5. Batch optimize with pngquant

**Time estimate:** 8-12 hours for all 88 icons (higher quality)

---

### Advanced: Full Pixel Art Creation
1. Create all icons from scratch in Aseprite
2. Use consistent style and palette per deity
3. Export and organize automatically
4. Run batch optimization script

**Time estimate:** 20-30 hours for all 88 icons (maximum control)

---

### Expert: Automated AI Pipeline
1. Write script to generate prompts from specifications
2. Use Stable Diffusion API/CLI for batch generation
3. Automated background removal (remove.bg API)
4. Automated resizing and optimization (ImageMagick + pngquant)
5. Validation script to check all icons exist

**Time estimate:** 2-3 hours setup + 1 hour generation

---

## Quality Checklist

Before considering an icon complete, verify:

- [ ] Correct size: exactly 32x32 pixels
- [ ] Format: PNG with alpha channel (transparency)
- [ ] Background: Fully transparent (no white background)
- [ ] Visibility: Icon is clear and recognizable at actual size
- [ ] Colors: Matches deity theme from specifications
- [ ] File name: Matches manifest exactly (lowercase, underscores)
- [ ] File size: Under 5KB (ideally under 2KB)
- [ ] File location: Correct directory in assets folder

---

## Testing Icons In-Game

### Quick Test Setup

1. **Copy icons to mod assets:**
   ```bash
   cp -r PantheonWars/assets/* /path/to/vintagestory/Mods/PantheonWars/assets/
   ```

2. **Reference icons in code:**
   Icons can be referenced in C# code as:
   ```csharp
   string iconPath = "pantheonwars:textures/icons/deities/khoras.png";
   ```

3. **Hot reload:**
   Vintage Story supports asset hot-reloading. Change an icon file and it should update in-game without restart.

### Create Test GUI

Create a simple test dialog to preview all icons:

```csharp
// Test dialog to preview icons
foreach (var deity in deities)
{
    string iconPath = $"pantheonwars:textures/icons/deities/{deity}.png";
    // Display icon in GUI...
}
```

---

## Troubleshooting

**Icons appear pixelated/blurry:**
- Use nearest-neighbor scaling, not bilinear/bicubic
- Ensure source is high quality before downscaling

**Icons have white borders:**
- Background not fully transparent
- Use proper alpha channel removal
- Check with transparency checker

**Icons look wrong in-game:**
- Vintage Story may have specific icon requirements
- Check existing VS mod icons for reference
- Ensure proper file paths

**File size too large:**
- Use pngquant or TinyPNG to compress
- Reduce color palette (256 colors max needed)
- Remove unnecessary metadata

---

## Resources

### Tools
- **Aseprite:** https://www.aseprite.org/
- **Piskel:** https://www.piskelapp.com/
- **Photopea:** https://www.photopea.com/
- **ImageMagick:** https://imagemagick.org/
- **remove.bg:** https://remove.bg/

### AI Generation
- **Midjourney:** https://midjourney.com/
- **Stable Diffusion:** https://github.com/AUTOMATIC1111/stable-diffusion-webui
- **Leonardo.ai:** https://leonardo.ai/

### Optimization
- **TinyPNG:** https://tinypng.com/
- **pngquant:** https://pngquant.org/

### Learning Resources
- **Pixel Art Tutorial:** https://blog.studiominiboss.com/pixelart
- **Vintage Story Modding:** https://wiki.vintagestory.at/index.php/Modding:Getting_Started

---

## Next Steps

1. **Review specifications:** Read `icon_specifications.md` carefully
2. **Choose method:** Pick a generation method that fits your skills/time
3. **Start with deity symbols:** Generate the 8 deity icons first (easier)
4. **Batch process perks:** Use the manifest to track progress by deity
5. **Test in-game:** Verify icons look good in actual game UI
6. **Iterate:** Refine any icons that don't look right

**Good luck with your icon generation! 🎨**
