#!/bin/bash
set -e

echo "Running database migrations..."
dotnet tools/DbMigrator/DbMigrator.dll

echo "Seeding database..."
dotnet tools/DbSeeder/DbSeeder.dll

echo "Starting API..."
exec dotnet API.dll
