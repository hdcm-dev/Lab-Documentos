# Build de la pieza .NET dentro de BuildKit para que BuildKit emita la procedencia SLSA v1 (in-toto).
# La imagen base va por DIGEST (N5): la procedencia la registra en resolvedDependencies.
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743 AS build
WORKDIR /src
COPY . .
# CI=true activa ContinuousIntegrationBuild por el Directory.Build.props de la pieza (rutas /_/).
ENV CI=true DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
RUN dotnet build -c Release -nologo -v q
# Sólo los artefactos salen del build: la procedencia los nombra por digest como "subject".
FROM scratch
COPY --from=build /src/bin/Release/net8.0/Pieza.dll /src/bin/Release/net8.0/Pieza.pdb /
