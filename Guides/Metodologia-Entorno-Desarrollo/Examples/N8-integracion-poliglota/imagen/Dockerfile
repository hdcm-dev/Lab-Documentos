# Imagen de desarrollo y CI con los TRES runtimes (variante "imagen única" de N8).
# Cada runtime se toma de su imagen OFICIAL fijada por digest; no se instala nada
# "latest" desde apt. Las versiones deben coincidir con los archivos del repo
# (global.json, .nvmrc, tools/gen/.python-version): lo comprueba verificar.sh, no esta imagen.

ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743
ARG NODE_IMAGE=node:22-bookworm-slim@sha256:d649c27dae7ba0137b3cef5dd75baa422c08dc3d9e3fc0c23dfb172dc3cc6436
ARG UV_IMAGE=ghcr.io/astral-sh/uv:0.12.23@sha256:61d393e44e249f2e4b526b6c7ddcecce245946826e608e11c93ad4f5bba55b21

FROM ${NODE_IMAGE} AS node
FROM ${UV_IMAGE} AS uv

# Base: el SDK de .NET (Debian 12, la misma familia que node:22-bookworm-slim,
# así el binario de Node encuentra la glibc que espera).
FROM ${SDK_IMAGE}

# Node + npm copiados de la imagen oficial (no de apt, que trae otra versión).
COPY --from=node /usr/local/bin/node /usr/local/bin/node
COPY --from=node /usr/local/lib/node_modules/npm /usr/local/lib/node_modules/npm
RUN ln -s ../lib/node_modules/npm/bin/npm-cli.js /usr/local/bin/npm \
 && ln -s ../lib/node_modules/npm/bin/npx-cli.js /usr/local/bin/npx

# uv como gestor e instalador del intérprete de Python.
COPY --from=uv /uv /uvx /usr/local/bin/

# Python: la versión EXACTA llega como argumento leído de .python-version.
ARG PYTHON_VERSION
ENV UV_PYTHON_INSTALL_DIR=/opt/uv-python \
    UV_PYTHON_PREFERENCE=only-managed \
    UV_CACHE_DIR=/tmp/uv-cache \
    UV_LINK_MODE=copy \
    DOTNET_NOLOGO=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NPM_CONFIG_UPDATE_NOTIFIER=false
RUN test -n "$PYTHON_VERSION" \
 && uv python install "$PYTHON_VERSION" \
 && ln -s "$(uv python find "$PYTHON_VERSION")" /usr/local/bin/python3 \
 && chmod -R a+rX /opt/uv-python \
 && rm -rf /tmp/uv-cache
# Después de construir la imagen no se descargan más intérpretes (N3: deriva).
ENV UV_PYTHON_DOWNLOADS=never
