#!/bin/bash

# Clean up old test results and coverage reports
echo "Cleaning up old test results and coverage reports..."
rm -rf coveragereport
find . -name "coverage.cobertura.xml" -type f -delete
find . -path "**/TestResults" -type d -exec rm -rf {} +

# Run tests with code coverage
echo "Running tests with code coverage..."
dotnet test --collect:"XPlat Code Coverage" --settings "$(dirname "$0")/coverlet.runsettings"

# Find all coverage files
coverage_files=$(find . -name "coverage.cobertura.xml" -type f)

if [ -n "$coverage_files" ]; then
    echo "Coverage files found:"
    echo "$coverage_files"

    # Generate the report using all coverage files
    echo "Generating coverage report..."
    reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

    # Open the report in the default browser
    report_path="$PWD/coveragereport/index.html"
    if [ -f "$report_path" ]; then
        echo "Opening coverage report..."
        if [[ "$OSTYPE" == "darwin"* ]]; then
            open "$report_path"
        elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
            xdg-open "$report_path"
        else
            echo "Error: Unsupported OS for automatic browser opening"
        fi

        echo "Coverage report opened in browser"
    else
        echo "Error: Coverage report not found at $report_path"
    fi
else
    echo "Error: No coverage files found"
fi

echo "Code coverage test completed"