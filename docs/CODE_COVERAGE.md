# Code Coverage Guide

This guide explains how to generate and view code coverage reports for the PantheonWars test suite.

## Prerequisites

- .NET 8.0 SDK installed
- `coverlet.collector` package (already added to project)
- Optional: `ReportGenerator` for HTML reports

## Quick Start

### 1. Run Tests with Coverage (Basic)

```bash
cd /path/to/PantheonWars
dotnet test --collect:"XPlat Code Coverage"
```

This will:
- Run all tests
- Generate a coverage file in: `PantheonWars.Tests/TestResults/{guid}/coverage.cobertura.xml`

### 2. Run Tests with Coverage (With Settings)

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

### 3. Generate HTML Report

First, install ReportGenerator globally:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

Then generate the HTML report:

```bash
# Find the latest coverage file
COVERAGE_FILE=$(find PantheonWars.Tests/TestResults -name "coverage.cobertura.xml" | sort -r | head -n 1)

# Generate HTML report
reportgenerator \
  -reports:"$COVERAGE_FILE" \
  -targetdir:"coverage-report" \
  -reporttypes:Html
```

Open the report:
```bash
open coverage-report/index.html  # macOS
xdg-open coverage-report/index.html  # Linux
start coverage-report/index.html  # Windows
```

## Coverage Configuration

### Create coverlet.runsettings

Create a `coverlet.runsettings` file in the project root to configure coverage:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat Code Coverage">
        <Configuration>
          <Format>cobertura,opencover,json</Format>
          <Exclude>[*Tests]*,[*]*.Generated.*</Exclude>
          <ExcludeByAttribute>Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute,ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
          <IncludeDirectory>../PantheonWars/bin/</IncludeDirectory>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

### Exclude Patterns

Common exclusions:
- `[*Tests]*` - Exclude test projects
- `[PantheonWars]PantheonWars.Models.*` - Exclude model classes
- `[PantheonWars]*.Generated.*` - Exclude generated code

## Continuous Coverage Tracking

### Option 1: Using coverlet.msbuild

Add to `PantheonWars.Tests.csproj`:

```xml
<PackageReference Include="coverlet.msbuild" Version="6.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

Then run:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### Option 2: Using dotnet-coverage (Microsoft)

Install:
```bash
dotnet tool install --global dotnet-coverage
```

Collect coverage:
```bash
dotnet-coverage collect "dotnet test" -f xml -o coverage.xml
```

## Integration with CI/CD

### GitHub Actions Example

```yaml
- name: Test with Coverage
  run: dotnet test --collect:"XPlat Code Coverage" --logger trx

- name: Generate Coverage Report
  run: |
    dotnet tool install --global dotnet-reportgenerator-globaltool
    reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html

- name: Upload Coverage Report
  uses: actions/upload-artifact@v3
  with:
    name: coverage-report
    path: coverage-report/
```

### Codecov Integration

```yaml
- name: Upload to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: '**/coverage.cobertura.xml'
    fail_ci_if_error: true
```

## Viewing Coverage in IDE

### Visual Studio 2022

1. Run tests with coverage: `Test` → `Analyze Code Coverage for All Tests`
2. View results in `Code Coverage Results` window
3. Click on items to see line-by-line coverage

### JetBrains Rider

1. Run tests with coverage: Right-click test project → `Cover Unit Tests`
2. View results in `Unit Test Coverage` window
3. Shows coverage percentage per namespace, class, and method

### VS Code

1. Install extension: "Coverage Gutters"
2. Run tests with coverage
3. Click "Watch" in status bar to display coverage in editor

## Coverage Metrics

### Target Coverage Levels

Based on TEST_PLAN.md:

- **Phase 2 (Core Systems)**: >80% coverage ✅
- **Phase 3 (Commands & GUI)**: >70% coverage
- **Phase 4 (Abilities & Integration)**: >60% coverage
- **Overall Project**: >75% coverage

### What to Cover

**High Priority (>90%)**:
- Core business logic (Systems/)
- Data managers
- Favor/Prestige calculations

**Medium Priority (>70%)**:
- Commands
- GUI state management
- Network packets

**Lower Priority (>50%)**:
- Models (mostly data classes)
- Constants/Enums
- Generated code

## Interpreting Results

### Cobertura Format

The XML report includes:
- `line-rate`: Percentage of lines covered
- `branch-rate`: Percentage of branches (if/else) covered
- `complexity`: Cyclomatic complexity

### Key Metrics

- **Line Coverage**: % of executable lines executed
- **Branch Coverage**: % of decision points (if/else) tested
- **Method Coverage**: % of methods called
- **Class Coverage**: % of classes instantiated

### Example Output

```
+------------------+--------+--------+--------+
| Module           | Line   | Branch | Method |
+------------------+--------+--------+--------+
| PantheonWars     | 82.5%  | 78.3%  | 85.1%  |
| - Systems        | 91.2%  | 87.5%  | 93.4%  |
| - Commands       | 68.4%  | 62.1%  | 71.2%  |
| - Models         | 45.3%  | N/A    | 50.2%  |
+------------------+--------+--------+--------+
```

## Troubleshooting

### "No test results" Error

Ensure `Microsoft.NET.Test.Sdk` package is installed:
```bash
dotnet add package Microsoft.NET.Test.Sdk
```

### Coverage File Not Generated

Check that `coverlet.collector` is properly referenced:
```bash
dotnet list package | grep coverlet
```

### Inaccurate Results

- Clear test results: `rm -rf **/TestResults`
- Rebuild solution: `dotnet clean && dotnet build`
- Run tests fresh: `dotnet test --collect:"XPlat Code Coverage"`

## Quick Commands Reference

```bash
# Basic coverage
dotnet test --collect:"XPlat Code Coverage"

# With settings file
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Generate HTML report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html

# Open report (macOS)
open coverage-report/index.html

# View coverage summary
dotnet test --collect:"XPlat Code Coverage" --logger:"console;verbosity=detailed"
```

## Additional Resources

- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [Microsoft Code Coverage](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)
- [Cobertura Format](https://cobertura.github.io/cobertura/)
