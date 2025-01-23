#!/bin/bash

# Don't require sudo - this script shouldn't need root privileges
if [ "$EUID" -eq 0 ]; then
    echo "Error: This script should NOT be run with sudo privileges"
    exit 1
fi

# Check if script is being run from project root
if [ ! -f "scripts/run.sh" ]; then
    echo "Error: This script must be run from the project root directory"
    exit 1
fi

echo "Starting PostgreSQL..."
# Use sudo only for PostgreSQL
sudo ./scripts/postgres.sh &

echo "Starting WebAPI..."
(cd src/Presentation/API && dotnet run) &

echo "Starting Blazor WASM Client..."
(cd src/Presentation/Client && dotnet run) &

wait
