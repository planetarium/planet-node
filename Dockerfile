FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./PlanetNode/PlanetNode.csproj ./PlanetNode/
COPY ./Libplanet.Headless/Libplanet.Headless.csproj ./Libplanet.Headless/

RUN dotnet restore PlanetNode
RUN dotnet restore Libplanet.Headless

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -r linux-x64 -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/out .

# Install native deps & utilities for production
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev jq curl \
     && rm -rf /var/lib/apt/lists/*

# Runtime settings
EXPOSE 5000
VOLUME /data

ENTRYPOINT ["/app/PlanetNode"]
