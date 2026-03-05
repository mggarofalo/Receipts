# Stage 1: Build React SPA
FROM node:22-alpine AS client-build
WORKDIR /app/client

# Copy only files needed for npm install first (layer caching)
COPY src/client/package.json src/client/package-lock.json ./

RUN npm ci

# Copy OpenAPI spec (needed for type generation) and client source
COPY openapi/spec.yaml /openapi/spec.yaml
COPY src/client/ ./

RUN npm run build

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

ARG TARGETARCH

# Copy solution and project files first (layer caching)
COPY Receipts.slnx Directory.Packages.props Directory.Build.props* ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Common/Common.csproj src/Common/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Presentation/API/API.csproj src/Presentation/API/
COPY src/Receipts.ServiceDefaults/Receipts.ServiceDefaults.csproj src/Receipts.ServiceDefaults/
COPY src/Receipts.AppHost/Receipts.AppHost.csproj src/Receipts.AppHost/
COPY tests/Domain.Tests/Domain.Tests.csproj tests/Domain.Tests/
COPY tests/Common.Tests/Common.Tests.csproj tests/Common.Tests/
COPY tests/Application.Tests/Application.Tests.csproj tests/Application.Tests/
COPY tests/Infrastructure.Tests/Infrastructure.Tests.csproj tests/Infrastructure.Tests/
COPY tests/Presentation.API.Tests/Presentation.API.Tests.csproj tests/Presentation.API.Tests/
COPY tests/SampleData/SampleData.csproj tests/SampleData/

# Copy NuGet config and OpenAPI tooling (needed for restore/build)
COPY nuget.config* ./
COPY nswag.json ./
COPY openapi/ openapi/
COPY tools/ tools/

RUN case ${TARGETARCH} in \
      amd64) DOTNET_ARCH=x64 ;; \
      arm64) DOTNET_ARCH=arm64 ;; \
      arm)   DOTNET_ARCH=arm ;; \
      *) echo "Unsupported architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac && \
    dotnet restore Receipts.slnx -a ${DOTNET_ARCH}

# Copy remaining source
COPY src/ src/

RUN case ${TARGETARCH} in \
      amd64) DOTNET_ARCH=x64 ;; \
      arm64) DOTNET_ARCH=arm64 ;; \
      arm)   DOTNET_ARCH=arm ;; \
      *) echo "Unsupported architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac && \
    dotnet publish src/Presentation/API/API.csproj \
      -c Release -o /app/publish --no-restore \
      -p:PublishReadyToRun=true -a ${DOTNET_ARCH}

# Stage 3: Runtime (noble for wget-based healthchecks; security via docker-compose constraints)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

WORKDIR /app

# Copy published API
COPY --from=api-build /app/publish .

# Copy React SPA into wwwroot
COPY --from=client-build /app/client/dist ./wwwroot/

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "API.dll"]
