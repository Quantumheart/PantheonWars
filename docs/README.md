# PantheonWars Documentation

This directory contains all documentation for the PantheonWars mod, organized by topic.

## Directory Structure

### `/topics/reference/`
Reference documentation for game systems and content.

- `ability_reference.md` - Complete list of abilities and their effects
- `blessing_reference.md` - Blessing system reference and mechanics
- `deity_reference.md` - Information about all deities
- `favor_reference.md` - Favor system mechanics and progression
- `icon_quick_reference.md` - Quick reference for icon locations and usage

### `/topics/implementation/`
Implementation guides for core features.

- `BLESSING_STAT_APPLICATION_IMPLEMENTATION.md` - How blessing stats are applied
- `BUFF_IMPLEMENTATION_GUIDE.md` - Buff system implementation details
- `implementation_guide.md` - General implementation guidelines
- `special_effects_implementation_guide.md` - Special effects system guide

### `/topics/ui-design/`
User interface design and refactoring documentation.

- `BLESSING_UI_IMPLEMENTATION_PLAN.md` - Blessing UI implementation plan
- `ui-refactoring-plan.md` - Overall UI refactoring strategy
- `ui-refactoring-progress.md` - Progress tracking for UI refactoring

### `/topics/testing/`
Testing guides and methodologies.

- `balance_testing_guide.md` - Guidelines for testing game balance
- `BlessingCommandsTestingTaskBreakdown.md` - Task breakdown for blessing command tests
- `unit_testing_implementation_guide.md` - Unit testing guidelines
- `unit_tests_implementation_summary.md` - Summary of implemented tests

### `/topics/art-assets/`
Art asset specifications and generation.

- `icon_generation_guide.md` - Guide for generating mod icons
- `icon_manifest.json` - Manifest of all required icons
- `icon_specifications.md` - Detailed specifications for all icons

### `/topics/planning/`
Project planning and phase documentation.

- `passive_favor_integration_plan.md` - Plan for passive favor system
- `phase1_passive_favor_implementation.md` - Phase 1 implementation details
- `phase3_activity_bonuses_plan.md` - Phase 3 activity bonus planning
- `phase3_group_deity_blessings_guide.md` - Phase 3 group blessing guide
- `phase3_task_breakdown.md` - Phase 3 task breakdown
- `ScopeReduction.md` - Scope reduction decisions and rationale

### `/topics/integration/`
Integration guides for connecting different systems.

- `favor_progression_calculations.md` - Calculations for favor progression
- `land_claim_holy_site_integration.md` - Holy site integration with land claims
- `prayer_and_shrine_implementation.md` - Prayer and shrine system implementation

### `/topics/analysis/`
Analysis of external mods and exploration documentation.

- `EXPLORATION_INDEX.md` - Index of exploration findings
- `XSKILLS_ANALYSIS.md` - Analysis of XSkills mod
- `XSKILLS_EXPLORATION_SUMMARY.md` - Summary of XSkills exploration
- `XSKILLS_UI_ANALYSIS.md` - XSkills UI analysis

## Finding Documentation

### By Development Activity

- **Implementing a new feature?** → Start with `/topics/implementation/` and `/topics/planning/`
- **Working on UI?** → Check `/topics/ui-design/`
- **Writing tests?** → See `/topics/testing/`
- **Creating icons or art?** → See `/topics/art-assets/`
- **Need game system info?** → Check `/topics/reference/`
- **Integrating systems?** → See `/topics/integration/`
- **Researching external mods?** → Check `/topics/analysis/`

### By System

- **Blessings:** `reference/blessing_reference.md`, `implementation/BLESSING_STAT_APPLICATION_IMPLEMENTATION.md`, `ui-design/BLESSING_UI_IMPLEMENTATION_PLAN.md`
- **Favor:** `reference/favor_reference.md`, `integration/favor_progression_calculations.md`
- **Deities:** `reference/deity_reference.md`
- **Buffs:** `implementation/BUFF_IMPLEMENTATION_GUIDE.md`
- **Icons:** `art-assets/icon_specifications.md`, `art-assets/icon_generation_guide.md`
- **UI:** All files in `ui-design/`
- **Testing:** All files in `testing/`

## Contributing to Documentation

When adding new documentation:

1. Place it in the appropriate topic directory
2. Update this README with a link to your new document
3. Use descriptive filenames that clearly indicate the content
4. Follow markdown best practices for readability
