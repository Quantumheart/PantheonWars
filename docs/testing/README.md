# Testing Documentation

This directory contains all documentation related to testing the PantheonWars mod.

## 📚 Documents

### Core Testing Documents

- **[TEST_PLAN.md](TEST_PLAN.md)** - Comprehensive testing strategy
  - Interface extraction plan
  - Testing roadmap (Phases 1-5)
  - Test examples and patterns
  - Success criteria for each phase
  - 80+ blessing test coverage goals

- **[CODE_COVERAGE.md](CODE_COVERAGE.md)** - Code coverage guide
  - How to generate coverage reports
  - Configuration and setup
  - IDE integration (VS2022, Rider, VS Code)
  - CI/CD integration examples
  - Coverage metrics interpretation

### Testing Exclusions

- **[STATIC_CLASS_TESTING_ANALYSIS.md](STATIC_CLASS_TESTING_ANALYSIS.md)** - Static class analysis
  - Detailed analysis of all 14 static classes
  - Classification criteria for test exclusions
  - Rationale for each exclusion decision
  - Impact assessment (~2,500+ LOC excluded)
  - Recommendations for coverage configuration

- **[EXCLUDE_FROM_TESTING.txt](EXCLUDE_FROM_TESTING.txt)** - Quick reference
  - Simple list of files to exclude from testing
  - Coverlet configuration snippets
  - Test files to consider removing

## 🎯 Quick Start

1. **Planning tests?** Start with [TEST_PLAN.md](TEST_PLAN.md)
2. **Running tests?** See [CODE_COVERAGE.md](CODE_COVERAGE.md)
3. **Coverage too low?** Check [EXCLUDE_FROM_TESTING.txt](EXCLUDE_FROM_TESTING.txt)

## 📊 Current Status

- ✅ **Phase 1 Complete** - Interface extraction (6 interfaces)
- ✅ **Phase 2 Complete** - Core system tests (8 test classes, 160+ tests)
- 🎯 **Phase 3 Next** - Command & GUI tests
- 🎯 **Phase 4 Planned** - Ability & integration tests
- 🎯 **Phase 5 Planned** - CI/CD integration

## 🔗 Related Documentation

- **[Test Suite README](../../PantheonWars.Tests/README.md)** - Test infrastructure and usage
- **[Main Project README](../../README.md)** - Project overview

## 📝 Coverage Targets

| Phase | Component | Target Coverage |
|-------|-----------|----------------|
| Phase 2 | Core Systems | >80% ✅ |
| Phase 3 | Commands & GUI | >70% 🎯 |
| Phase 4 | Abilities & Integration | >60% 🎯 |
| Overall | Full Project | >75% 🎯 |

---

**Last Updated:** 2025-11-12
