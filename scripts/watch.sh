#!/bin/bash

echo "Starting WebAPI with watch..."
(cd ../src/Presentation/API && dotnet watch run) &

echo "Starting Blazor WASM Client with watch..."
(cd ../src/Presentation/Client && dotnet watch run) &

wait
