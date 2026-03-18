# Stage 1: Build React SPA
FROM node:22-alpine AS client-build
WORKDIR /app/client

# Copy only files needed for npm install first (layer caching)
COPY src/client/package.json src/client/package-lock.json ./

RUN npm ci

# Copy OpenAPI spec (needed for type generation) and client source
COPY openapi/spec.yaml /openapi/spec.yaml
COPY src/client/ ./

# Inject version info and Sentry config after npm ci so the install layer stays cached
ARG VITE_APP_VERSION=dev
ARG VITE_COMMIT_HASH=local
ARG VITE_SENTRY_DSN=
ARG VITE_SENTRY_ENVIRONMENT=production
ENV VITE_APP_VERSION=${VITE_APP_VERSION}
ENV VITE_COMMIT_HASH=${VITE_COMMIT_HASH}
ENV VITE_SENTRY_DSN=${VITE_SENTRY_DSN}
ENV VITE_SENTRY_ENVIRONMENT=${VITE_SENTRY_ENVIRONMENT}

RUN npm run build

# Stage 2: Build .NET API and CLI tools
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

ARG TARGETARCH

# Copy project files needed for restore (layer caching)
COPY Directory.Packages.props Directory.Build.props* ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Common/Common.csproj src/Common/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Presentation/API/API.csproj src/Presentation/API/
COPY src/Receipts.ServiceDefaults/Receipts.ServiceDefaults.csproj src/Receipts.ServiceDefaults/
COPY src/Tools/DbMigrator/DbMigrator.csproj src/Tools/DbMigrator/
COPY src/Tools/DbSeeder/DbSeeder.csproj src/Tools/DbSeeder/

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
    dotnet restore src/Presentation/API/API.csproj -r linux-${DOTNET_ARCH} -p:PublishReadyToRun=true && \
    dotnet restore src/Tools/DbMigrator/DbMigrator.csproj -r linux-${DOTNET_ARCH} && \
    dotnet restore src/Tools/DbSeeder/DbSeeder.csproj -r linux-${DOTNET_ARCH}

# Copy remaining source
COPY src/ src/

# Download ONNX model for local embedding generation
RUN mkdir -p src/Infrastructure/Models/AllMiniLmL6V2 && \
    curl -sL -o src/Infrastructure/Models/AllMiniLmL6V2/model.onnx \
      "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx" && \
    curl -sL -o src/Infrastructure/Models/AllMiniLmL6V2/vocab.txt \
      "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt"

RUN case ${TARGETARCH} in \
      amd64) DOTNET_ARCH=x64 ;; \
      arm64) DOTNET_ARCH=arm64 ;; \
      arm)   DOTNET_ARCH=arm ;; \
      *) echo "Unsupported architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac && \
    dotnet publish src/Presentation/API/API.csproj \
      -c Release -o /app/publish --no-restore \
      -p:PublishReadyToRun=true -r linux-${DOTNET_ARCH} && \
    dotnet publish src/Tools/DbMigrator/DbMigrator.csproj \
      -c Release -o /app/tools/DbMigrator --no-restore \
      -r linux-${DOTNET_ARCH} && \
    dotnet publish src/Tools/DbSeeder/DbSeeder.csproj \
      -c Release -o /app/tools/DbSeeder --no-restore \
      -r linux-${DOTNET_ARCH}

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime

LABEL org.opencontainers.image.title="Receipts" \
      org.opencontainers.image.description="Receipt management API with React SPA" \
      org.opencontainers.image.source="https://github.com/mggarofalo/Receipts"

RUN apt-get update && \
    apt-get install -y --no-install-recommends gosu curl && \
    rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

WORKDIR /app

# Copy published API
COPY --from=api-build /app/publish .

# Copy CLI tools
COPY --from=api-build /app/tools ./tools/

# Copy React SPA into wwwroot
COPY --from=client-build /app/client/dist ./wwwroot/

# Copy entrypoint script
COPY docker-entrypoint.sh .
RUN chmod +x docker-entrypoint.sh

# Create mount points
RUN mkdir -p /secrets /data

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=120s --retries=5 \
    CMD curl -sf http://localhost:8080/api/health || exit 1

ENTRYPOINT ["./docker-entrypoint.sh"]
