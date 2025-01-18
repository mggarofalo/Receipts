#!/bin/bash

echo "Starting WebAPI..."
(cd ../src/Presentation/API && dotnet run) &

echo "Starting Blazor WASM Client..."
(cd ../src/Presentation/Client && dotnet run) &

wait
