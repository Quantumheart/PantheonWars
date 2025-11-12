# PantheonWars Code Coverage Report Generator (PowerShell)
# This script runs tests with coverage and generates an HTML report

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Green
Write-Host "PantheonWars Code Coverage Generator" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

# Clean previous test results
Write-Host "`nCleaning previous test results..." -ForegroundColor Yellow
Get-ChildItem -Path . -Recurse -Directory -Filter "TestResults" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "coverage-report" -Recurse -Force -ErrorAction SilentlyContinue

# Run tests with coverage
Write-Host "`nRunning tests with coverage collection..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Find the coverage file
Write-Host "`nLocating coverage file..." -ForegroundColor Yellow
$coverageFile = Get-ChildItem -Path "PantheonWars.Tests/TestResults" -Recurse -Filter "coverage.cobertura.xml" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $coverageFile) {
    Write-Host "ERROR: Coverage file not found!" -ForegroundColor Red
    Write-Host "Make sure tests ran successfully and coverlet.collector is installed." -ForegroundColor Red
    exit 1
}

Write-Host "Found coverage file: $coverageFile" -ForegroundColor Green

# Check if ReportGenerator is installed
$reportGenInstalled = $null
try {
    $reportGenInstalled = Get-Command reportgenerator -ErrorAction SilentlyContinue
} catch {
    $reportGenInstalled = $null
}

if (-not $reportGenInstalled) {
    Write-Host "`nReportGenerator not found. Installing globally..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool

    # Refresh environment variables
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" +
                [System.Environment]::GetEnvironmentVariable("Path","User")
}

# Generate HTML report
Write-Host "`nGenerating HTML coverage report..." -ForegroundColor Yellow
reportgenerator `
    "-reports:$coverageFile" `
    "-targetdir:coverage-report" `
    "-reporttypes:Html;Badges;TextSummary"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to generate report!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Display summary
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Coverage Summary" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

if (Test-Path "coverage-report/Summary.txt") {
    Get-Content "coverage-report/Summary.txt"
} else {
    Write-Host "Summary file not found" -ForegroundColor Yellow
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Report generated successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$reportPath = Join-Path $PWD "coverage-report/index.html"
Write-Host "`nHTML report location: $reportPath" -ForegroundColor Yellow

# Try to open the report automatically
Write-Host "`nOpening report in browser..." -ForegroundColor Yellow
Start-Process $reportPath

Write-Host "`nDone!" -ForegroundColor Green
