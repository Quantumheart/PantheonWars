# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Changed
- **[BREAKING - Deprecated]** Migrated deity system from Phase 1-2 to Phase 3 religion-based architecture
  - `PlayerDataManager.HasDeity()` is now obsolete - use `PlayerReligionDataManager.GetOrCreatePlayerData()` and check `ActiveDeity != DeityType.None` instead
  - `PlayerDataManager.SetDeity()` is now obsolete - use `PlayerReligionDataManager.JoinReligion()` instead
  - `PlayerDeityData` is now obsolete - use `PlayerReligionData` instead
  - All favor, ability, and deity commands now use religion-based deity system
  - Passive favor generation now correctly works for religion members
  - FavorSystem updated to use PlayerReligionDataManager

### Fixed
- Fixed player deity not being recognized after joining religion
- Fixed passive favor generation not working for religion members
- Fixed favor commands showing "not pledged" error for religion members
- Fixed ability commands not working for religion members
- Fixed deity commands not recognizing religion-based deity assignments

### Technical
- Updated FavorSystem.cs to use PlayerReligionDataManager for deity checks
- Updated FavorCommands.cs to validate deity through religion system
- Updated AbilityCommands.cs to check deity via PlayerReligionData
- Updated DeityCommands.cs to use religion-based deity assignments
- Marked legacy Phase 1-2 deity system methods as obsolete
- Maintained backward compatibility - no breaking changes to save data

### Migration Timeline
- **v1.5.0** (Current): Legacy APIs marked as obsolete (warnings)
- **v1.8.0** (Planned): Obsolete APIs emit errors instead of warnings
- **v2.0.0** (Planned): Complete removal of Phase 1-2 legacy system

## Previous Releases

See git history for previous releases.
