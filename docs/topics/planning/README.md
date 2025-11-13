# PantheonWars Planning Documentation

This directory contains planning documents, implementation guides, and feature specifications for the PantheonWars mod.

---

## Current Feature: Ban Kicked Members

### Quick Start
- **[Implementation Summary](./IMPLEMENTATION_SUMMARY.md)** - High-level overview and task breakdown
- **[Detailed Implementation Plan](./ban-kicked-members-feature.md)** - Complete specification with code examples

### Supporting Documentation
- **[Religion System Overview](./RELIGION_SYSTEM_OVERVIEW.md)** - Comprehensive guide to religion system components
- **[Religion System Architecture](./RELIGION_SYSTEM_ARCHITECTURE.md)** - Diagrams and data flows
- **[Religion System Code Snippets](./RELIGION_SYSTEM_CODE_SNIPPETS.md)** - Code examples and patterns

---

## Previous Features

### Passive Favor System
- **[Passive Favor Integration Plan](./passive_favor_integration_plan.md)** - Overall integration strategy
- **[Phase 1 Implementation](./phase1_passive_favor_implementation.md)** - Initial passive favor implementation

### Phase 3 Enhancements
- **[Task Breakdown](./phase3_task_breakdown.md)** - Detailed task list for phase 3
- **[Activity Bonuses Plan](./phase3_activity_bonuses_plan.md)** - Activity-based favor bonuses
- **[Group Deity Blessings Guide](./phase3_group_deity_blessings_guide.md)** - Group blessing system

### Development Process
- **[Scope Reduction](./ScopeReduction.md)** - Feature scope management

---

## Document Types

### 📋 Planning Documents
Documents that outline features before implementation:
- `ban-kicked-members-feature.md` - Ban system feature plan
- `passive_favor_integration_plan.md` - Passive favor system plan
- `phase3_*.md` - Phase 3 feature plans

### 📚 Reference Documents
Documents that explain existing systems:
- `RELIGION_SYSTEM_OVERVIEW.md` - System overview
- `RELIGION_SYSTEM_ARCHITECTURE.md` - Architecture diagrams
- `RELIGION_SYSTEM_CODE_SNIPPETS.md` - Code examples

### 📝 Implementation Guides
Step-by-step implementation instructions:
- `IMPLEMENTATION_SUMMARY.md` - Quick reference for current feature
- `phase1_passive_favor_implementation.md` - Phase 1 implementation steps

### 🎯 Process Documents
Development process and methodology:
- `ScopeReduction.md` - Scope management approach

---

## How to Use This Directory

### For New Features
1. Create a detailed planning document (e.g., `feature-name.md`)
2. Create an implementation summary (e.g., `FEATURE_NAME_SUMMARY.md`)
3. Document any new systems or changes to existing systems
4. Update this README with links to new documents

### For Existing Features
1. Refer to the relevant planning documents for context
2. Use reference documents to understand system architecture
3. Follow implementation guides for step-by-step instructions
4. Update documents as implementation progresses

### Document Naming Conventions
- **Planning**: `feature-name-plan.md` or `feature-name.md`
- **Implementation**: `FEATURE_NAME_SUMMARY.md` or `phase#_feature_implementation.md`
- **Reference**: `SYSTEM_NAME_TYPE.md` (e.g., `RELIGION_SYSTEM_OVERVIEW.md`)
- **Process**: `ProcessName.md` (e.g., `ScopeReduction.md`)

---

## Current Development Status

### ✅ Completed
- Religion system core functionality
- Kick member functionality
- Passive favor system (phases 1-3)

### 🚧 In Progress
- **Ban Kicked Members Feature** (Sprint 1: Foundation)
  - Branch: `claude/ban-kicked-members-feature-011CV6HbsJBgedQBdvfC88s7`
  - Status: Planning complete, ready for implementation
  - Next: Implement data model (ReligionData updates)

### 📅 Planned
- Ban system GUI enhancements
- Additional moderation tools
- Religion hierarchy system (if applicable)

---

## Contributing

When adding new planning documents:
1. Follow the naming conventions above
2. Include clear section headers
3. Add code examples where applicable
4. Update this README with links
5. Keep documents focused on a single feature or system
6. Use markdown formatting for readability

---

## Quick Links

### Implementation Order
1. [Ban System - Phase 1: Data Model](./ban-kicked-members-feature.md#phase-1-data-model-updates)
2. [Ban System - Phase 2: Business Logic](./ban-kicked-members-feature.md#phase-2-manager-layer-updates)
3. [Ban System - Phase 3: Commands](./ban-kicked-members-feature.md#phase-3-command-updates)
4. [Ban System - Phase 4: GUI](./ban-kicked-members-feature.md#phase-4-gui-updates)
5. [Ban System - Phase 5: Testing](./ban-kicked-members-feature.md#phase-6-testing)

### Key Reference Sections
- [Religion Data Model](./RELIGION_SYSTEM_OVERVIEW.md#1-data-models)
- [Religion Manager API](./RELIGION_SYSTEM_OVERVIEW.md#2-management-systems)
- [Kick Implementation](./RELIGION_SYSTEM_OVERVIEW.md#kick-command-implementation)
- [Network Protocol](./RELIGION_SYSTEM_OVERVIEW.md#3-commands-and-network)
- [Permission System](./RELIGION_SYSTEM_OVERVIEW.md#6-permission-system)

---

**Last Updated**: 2025-11-13
**Current Feature Branch**: `claude/ban-kicked-members-feature-011CV6HbsJBgedQBdvfC88s7`
