# UI Refactoring Plan

## 📊 Current State Analysis

### Largest Classes (by line count)
1. **ReligionManagementOverlay.cs** - 771 lines
2. **BlessingDialog.cs** - 745 lines
3. **CreateReligionOverlay.cs** - 690 lines
4. **ReligionBrowserOverlay.cs** - 580 lines
5. **ReligionManagementDialog.cs** - 470 lines

## 🔍 Key Issues Identified

### 1. Massive Code Duplication
The same UI components are reimplemented across multiple files:
- `DrawButton()` - appears in 4+ files with slight variations
- `DrawCloseButton()` - duplicated in 3+ files
- `DrawTextInput()` - duplicated in 2 files
- `DrawScrollbar()` - duplicated in 2 files
- Color constants - duplicated in every file
- Deity utilities (`GetDeityColor`, `GetDeityTitle`, deity lists) - duplicated in 3 files

### 2. Mixed Responsibilities
Classes are doing too much:
- **BlessingDialog.cs**: Window management + event handling + overlay coordination + network communication + state management
- **ReligionManagementOverlay.cs**: Rendering + input handling + scrolling logic + validation + state
- **CreateReligionOverlay.cs**: Form rendering + dropdown logic + validation + deity selection

### 3. Monolithic Rendering Methods
Single methods with 100+ lines handling:
- Layout calculation
- User input
- State updates
- Drawing operations
- Event handling

### 4. No Separation of State and Presentation
State variables mixed with rendering logic in static classes makes testing and reuse difficult.

## 💡 Recommended Refactoring Strategy

### Phase 1: Extract Shared UI Components
Create a `GUI/UI/Components` folder with reusable components:

```
Components/
├── Buttons/
│   ├── ButtonRenderer.cs (unified button drawing)
│   ├── CloseButton.cs
│   └── TabButton.cs
├── Inputs/
│   ├── TextInput.cs
│   ├── MultilineTextInput.cs
│   ├── Dropdown.cs
│   └── Checkbox.cs
├── Lists/
│   ├── ScrollableList.cs
│   └── Scrollbar.cs
└── Layout/
    └── UIConstants.cs (colors, spacing, etc.)
```

**Impact**: Reduces code by ~300-400 lines, eliminates duplication

### Phase 2: Extract Utilities
Create shared utilities:

```
GUI/UI/Utilities/
├── ColorPalette.cs (centralized color definitions)
├── DeityHelper.cs (deity colors, titles, lists)
└── LayoutHelper.cs (common layout calculations)
```

**Impact**: Eliminates ~150-200 lines of duplication

### Phase 3: Separate State from Rendering
Split overlay classes into state + renderer pairs:

```
ReligionManagementOverlay.cs (771 lines) →
    ├── ReligionManagementState.cs (~100 lines)
    ├── ReligionManagementRenderer.cs (~400 lines)
    └── Components/
        ├── MemberListRenderer.cs (~150 lines)
        └── DescriptionEditor.cs (~100 lines)
```

**Impact**: Each file becomes 100-400 lines (manageable size)

### Phase 4: Extract Event Handlers from BlessingDialog
Break down BlessingDialog.cs (745 lines):

```
BlessingDialog.cs (745 lines) →
    ├── BlessingDialog.cs (~250 lines - window lifecycle only)
    ├── BlessingDialogEventHandlers.cs (~200 lines)
    ├── BlessingDialogState.cs (~100 lines)
    └── OverlayCoordinator.cs (~150 lines)
```

**Impact**: Maximum file size reduced from 745 to ~250 lines

### Phase 5: Create Form Builder Pattern
For complex forms (CreateReligionOverlay):

```
CreateReligionOverlay.cs (690 lines) →
    ├── CreateReligionState.cs (~80 lines)
    ├── CreateReligionForm.cs (~200 lines)
    └── (Uses shared components from Phase 1)
```

**Impact**: Reduces from 690 to ~280 lines total

## 📈 Expected Results

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Largest file | 771 lines | ~400 lines | 48% reduction |
| Code duplication | ~600 lines | ~50 lines | 92% reduction |
| Testability | Low | High | ✅ |
| Reusability | Low | High | ✅ |
| Maintainability | Difficult | Easy | ✅ |

## 📋 Detailed Task Breakdown

### **Phase 1: Foundation - Shared Components** (Priority: HIGH)
*Start here - provides immediate value and reduces future duplication*

#### Task 1.1: Create UI Constants & Color Palette
- Create `GUI/UI/Utilities/ColorPalette.cs`
- Centralize all color definitions (ColorGold, ColorWhite, ColorGrey, ColorDarkBrown, ColorLightBrown, ColorBackground)
- Add spacing/size constants
- **Files affected**: All overlay files
- **Effort**: 2-3 hours

#### Task 1.2: Extract Button Components
- Create `GUI/UI/Components/Buttons/ButtonRenderer.cs`
  - Unified DrawButton method with all variations
  - DrawCloseButton
  - DrawSmallButton
  - DrawActionButton
- Replace duplicated button code in all overlays
- **Files affected**: ReligionManagementOverlay, CreateReligionOverlay, ReligionBrowserOverlay
- **Effort**: 3-4 hours

#### Task 1.3: Extract Input Components
- Create `GUI/UI/Components/Inputs/TextInput.cs`
- Create `GUI/UI/Components/Inputs/MultilineTextInput.cs`
- Add cursor management, keyboard input handling
- Replace duplicated input code
- **Files affected**: ReligionManagementOverlay, CreateReligionOverlay
- **Effort**: 4-5 hours

#### Task 1.4: Extract Scrollbar Component
- Create `GUI/UI/Components/Lists/Scrollbar.cs`
- Add mouse wheel handling
- Replace duplicated scrollbar code
- **Files affected**: ReligionManagementOverlay, ReligionBrowserOverlay
- **Effort**: 2-3 hours

#### Task 1.5: Extract Dropdown & Checkbox Components
- Create `GUI/UI/Components/Inputs/Dropdown.cs`
- Create `GUI/UI/Components/Inputs/Checkbox.cs`
- Replace implementations in CreateReligionOverlay
- **Files affected**: CreateReligionOverlay
- **Effort**: 3-4 hours

### **Phase 2: Domain Utilities** (Priority: MEDIUM)
*Eliminates deity-related duplication*

#### Task 2.1: Create Deity Helper Utility
- Create `GUI/UI/Utilities/DeityHelper.cs`
- Extract GetDeityColor method (appears in ReligionBrowserOverlay, ReligionHeaderRenderer)
- Extract GetDeityTitle method (appears in CreateReligionOverlay, ReligionBrowserOverlay, ReligionHeaderRenderer)
- Extract deity names array
- **Files affected**: CreateReligionOverlay, ReligionBrowserOverlay, ReligionHeaderRenderer
- **Effort**: 2 hours

### **Phase 3: Separate State & Rendering** (Priority: HIGH)
*Most impactful for maintainability*

#### Task 3.1: Refactor ReligionManagementOverlay
- Create `GUI/UI/State/ReligionManagementState.cs`
  - Extract all static state variables
  - Add state initialization/update methods
- Create `GUI/UI/Renderers/Components/MemberListRenderer.cs`
  - Extract DrawMemberList + DrawMemberItem (~150 lines)
- Slim down ReligionManagementOverlay.cs to ~400 lines
- **Files affected**: ReligionManagementOverlay.cs (771 → ~400 lines)
- **Effort**: 5-6 hours

#### Task 3.2: Refactor CreateReligionOverlay
- Create `GUI/UI/State/CreateReligionState.cs`
  - Extract form state (_religionName, _selectedDeityIndex, _isPublic, etc.)
- Use shared components from Phase 1
- Slim down to ~200-250 lines
- **Files affected**: CreateReligionOverlay.cs (690 → ~250 lines)
- **Effort**: 4-5 hours

#### Task 3.3: Refactor ReligionBrowserOverlay
- Create `GUI/UI/State/ReligionBrowserState.cs`
  - Extract filter/selection/scroll state
- Create `GUI/UI/Renderers/Components/ReligionListRenderer.cs`
  - Extract religion list rendering logic (~200 lines)
- Use shared tab component
- Slim down to ~300 lines
- **Files affected**: ReligionBrowserOverlay.cs (580 → ~300 lines)
- **Effort**: 4-5 hours

### **Phase 4: Decompose BlessingDialog** (Priority: MEDIUM)
*Improves main coordinator class*

#### Task 4.1: Extract Event Handlers
- Create `GUI/BlessingDialogEventHandlers.cs`
  - Move all On* event handler methods (~300 lines)
  - Keep dialog lifecycle methods in BlessingDialog
- **Files affected**: BlessingDialog.cs (745 → ~450 lines)
- **Effort**: 3-4 hours

#### Task 4.2: Extract Overlay Coordinator
- Create `GUI/OverlayCoordinator.cs`
  - Manage overlay open/close state
  - Handle overlay transitions
  - Coordinate between overlays
- **Files affected**: BlessingDialog.cs (450 → ~300 lines)
- **Effort**: 3-4 hours

#### Task 4.3: Extract Dialog State
- Create `GUI/State/BlessingDialogState.cs`
  - Move window state tracking
  - Move data loading flags
- **Files affected**: BlessingDialog.cs (300 → ~250 lines)
- **Effort**: 2-3 hours

### **Phase 5: Advanced Components** (Priority: LOW)
*Nice to have, but lower priority*

#### Task 5.1: Create Scrollable List Component
- Create `GUI/UI/Components/Lists/ScrollableList.cs`
  - Generic scrollable list with item renderer callback
  - Built-in scrollbar
  - Mouse wheel support
- Replace custom implementations
- **Effort**: 4-5 hours

#### Task 5.2: Create Tab Control Component
- Create `GUI/UI/Components/Tabs/TabControl.cs`
- Replace tab logic in ReligionBrowserOverlay
- **Effort**: 3-4 hours

#### Task 5.3: Create Form Builder
- Create `GUI/UI/Components/Forms/FormBuilder.cs`
- Simplify form construction in CreateReligionOverlay
- **Effort**: 4-5 hours

## 🎯 Recommended Approaches

### Option A: **Quick Wins First** (Recommended)
Start with Phase 1 & 2 to get immediate benefits:
1. Extract shared components (Tasks 1.1-1.5) - **~15-20 hours**
2. Extract utilities (Task 2.1) - **~2 hours**
3. Then tackle Phase 3 as separate PRs

**Benefits**:
- Immediate code reduction (~400 lines)
- Stop duplication from spreading
- Low risk, high reward

### Option B: **One File at a Time**
Refactor one large class completely before moving to the next:
1. CreateReligionOverlay (Tasks 1.1, 1.3, 1.5, 2.1, 3.2)
2. ReligionManagementOverlay (Tasks 1.1, 1.2, 1.3, 1.4, 3.1)
3. ReligionBrowserOverlay (Tasks 1.1, 1.2, 1.4, 2.1, 3.3)
4. BlessingDialog (Tasks 4.1-4.3)

**Benefits**:
- See complete transformation of each file
- Easier to test one file at a time
- Clear milestones

## 📊 Total Effort Estimate

| Phase | Effort |
|-------|--------|
| Phase 1 | 15-20 hours |
| Phase 2 | 2 hours |
| Phase 3 | 13-16 hours |
| Phase 4 | 8-11 hours |
| Phase 5 | 11-14 hours |
| **Total** | **49-63 hours** |

Approximately 1-2 weeks of focused work.

## 🚀 Next Steps

1. Review and approve this refactoring plan
2. Choose approach (Option A recommended)
3. Create feature branch for refactoring work
4. Start with Phase 1, Task 1.1 (Color Palette)
5. Submit PRs incrementally to maintain code review quality

---

*Document created: 2025-11-06*
*Analysis based on: phase3-group-deity-blessings-user-interface branch*
