#!/bin/bash

# Script to run PostgreSQL in the foreground

# Function to stop PostgreSQL and exit
cleanup() {
    echo "Stopping PostgreSQL service..."
    sudo systemctl stop postgresql
    exit 0
}

# Set up trap to catch SIGINT (Ctrl-C)
trap cleanup SIGINT

# Start PostgreSQL if it's not already running
if ! sudo systemctl is-active --quiet postgresql; then
    echo "Starting PostgreSQL service..."
    sudo systemctl start postgresql
    
    if [ $? -ne 0 ]; then
        echo "Failed to start PostgreSQL service. Please check the logs for more information."
        exit 1
    fi
fi

echo "PostgreSQL is running. Press Ctrl-C to stop."

# Keep the script running and show PostgreSQL log
sudo tail -f /var/log/postgresql/postgresql-*.log

# The script will only reach this point if tail is interrupted
cleanup