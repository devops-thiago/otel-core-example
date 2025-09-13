#!/bin/bash

# Bash script to run tests with coverage

CONFIGURATION=${1:-Release}
COVERAGE_THRESHOLD=${2:-80}

echo "Running tests with coverage analysis..."

# Clean previous results
if [ -d "coverage" ]; then
    rm -rf coverage
fi

# Run tests with coverage
echo "Running unit tests..."
dotnet test --configuration $CONFIGURATION --collect:"XPlat Code Coverage" --results-directory ./coverage --logger trx --logger "console;verbosity=normal"

if [ $? -ne 0 ]; then
    echo "Tests failed!"
    exit 1
fi

# Install report generator if not already installed
echo "Installing report generator..."
dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.2.0

# Generate coverage report
echo "Generating coverage report..."
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html;Cobertura;OpenCover"

# Check coverage threshold
echo "Checking coverage threshold..."
COVERAGE_FILE=$(find coverage -name "coverage.cobertura.xml" | head -1)

if [ -n "$COVERAGE_FILE" ]; then
    LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$COVERAGE_FILE" | grep -o '[0-9.]*')
    COVERAGE_PERCENT=$(echo "$LINE_RATE * 100" | bc -l | xargs printf "%.2f")

    echo "Coverage: ${COVERAGE_PERCENT}%"

    if (( $(echo "$COVERAGE_PERCENT < $COVERAGE_THRESHOLD" | bc -l) )); then
        echo "Coverage is below threshold of ${COVERAGE_THRESHOLD}%!"
        exit 1
    else
        echo "Coverage meets threshold of ${COVERAGE_THRESHOLD}%!"
    fi
else
    echo "Coverage file not found!"
    exit 1
fi

echo "All tests passed and coverage requirements met!"
echo "Coverage report available at: coverage/report/index.html"

