#!/bin/bash

# Prompt the user for the migration name
read -p "Enter the migration name: " migration_name

# Check if the migration name is not empty
if [ -n "$migration_name" ]; then
    # Run the migration command
    dotnet ef migrations add "$migration_name" --output-dir Migrations --project Receipts/Infrastructure
    
    echo "Migration '$migration_name' created successfully."
else
    echo "Error: Migration name cannot be empty."
    exit 1
fi
