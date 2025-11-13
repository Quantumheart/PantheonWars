#!/bin/bash

# PantheonWars Code Coverage Report Generator
# This script runs tests with coverage and generates an HTML report

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}PantheonWars Code Coverage Generator${NC}"
echo -e "${GREEN}========================================${NC}"

# Clean previous test results
echo -e "\n${YELLOW}Cleaning previous test results...${NC}"
find . -type d -name "TestResults" -exec rm -rf {} + 2>/dev/null || true
rm -rf coverage-report 2>/dev/null || true

# Run tests with coverage
echo -e "\n${YELLOW}Running tests with coverage collection...${NC}"
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --verbosity normal

# Find the coverage file
echo -e "\n${YELLOW}Locating coverage file...${NC}"
COVERAGE_FILE=$(find PantheonWars.Tests/TestResults -name "coverage.cobertura.xml" 2>/dev/null | sort -r | head -n 1)

if [ -z "$COVERAGE_FILE" ]; then
    echo -e "${RED}ERROR: Coverage file not found!${NC}"
    echo "Make sure tests ran successfully and coverlet.collector is installed."
    exit 1
fi

echo -e "${GREEN}Found coverage file: $COVERAGE_FILE${NC}"

# Check if ReportGenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    echo -e "\n${YELLOW}ReportGenerator not found. Installing globally...${NC}"
    dotnet tool install --global dotnet-reportgenerator-globaltool

    # Add to PATH if needed
    if ! command -v reportgenerator &> /dev/null; then
        export PATH="$PATH:$HOME/.dotnet/tools"
    fi
fi

# Generate HTML report
echo -e "\n${YELLOW}Generating HTML coverage report...${NC}"
reportgenerator \
  -reports:"$COVERAGE_FILE" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;Badges;TextSummary"

# Display summary
echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}Coverage Summary${NC}"
echo -e "${GREEN}========================================${NC}"

if [ -f "coverage-report/Summary.txt" ]; then
    cat "coverage-report/Summary.txt"
else
    echo "Summary file not found"
fi

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}Report generated successfully!${NC}"
echo -e "${GREEN}========================================${NC}"

echo -e "\nHTML report location: ${YELLOW}$(pwd)/coverage-report/index.html${NC}"

# Try to open the report automatically
if command -v xdg-open &> /dev/null; then
    echo -e "\n${YELLOW}Opening report in browser...${NC}"
    xdg-open coverage-report/index.html
elif command -v open &> /dev/null; then
    echo -e "\n${YELLOW}Opening report in browser...${NC}"
    open coverage-report/index.html
else
    echo -e "\n${YELLOW}Please open coverage-report/index.html in your browser${NC}"
fi

echo -e "\n${GREEN}Done!${NC}"
