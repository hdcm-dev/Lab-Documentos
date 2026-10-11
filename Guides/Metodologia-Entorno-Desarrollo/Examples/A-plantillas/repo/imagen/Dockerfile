# Imagen de desarrollo y CI con los TRES runtimes (plantilla del Anexo A).
# Cada runtime se toma de su imagen OFICIAL fijada por digest; no se instala nada
# "latest" desde apt. Las versiones deben coincidir con los archivos del repo
# (global.json, .nvmrc, tools/gen/.python-version): lo comprueba la compuerta (A.11), no esta imagen.
# SDK por etiqueta FIJA + digest: el PR de digest del bot trae parches del SO, no otro SDK (guía 10.2.6).

ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0.425-bookworm-slim@sha256:ec9c0a0dc5f60adc2065762050638dd5ed5facfc53716d4580a42d86027c8e80
ARG NODE_IMAGE=node:24-bookworm-slim@sha256:d6aa754f16b3197301076f047b5def2f02ea1dbbc2ca920407d46d7ec7f87b20
ARG UV_IMAGE=ghcr.io/astral-sh/uv:0.12.23@sha256:61d393e44e249f2e4b526b6c7ddcecce245946826e608e11c93ad4f5bba55b21

FROM ${NODE_IMAGE} AS node
FROM ${UV_IMAGE} AS uv

# Base: el SDK de .NET (Debian 12, la misma familia que node:24-bookworm-slim,
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
# EditorConfig: lo importa el paso de texto de la compuerta (A.11).
RUN test -n "$PYTHON_VERSION" \
 && uv python install "$PYTHON_VERSION" \
 && ln -s "$(uv python find "$PYTHON_VERSION")" /usr/local/bin/python3 \
 && uv pip install --system --break-system-packages --python /usr/local/bin/python3 EditorConfig==0.17.1 \
 && chmod -R a+rX /opt/uv-python \
 && rm -rf /tmp/uv-cache
# Después de construir la imagen no se descargan más intérpretes (N3: deriva).
ENV UV_PYTHON_DOWNLOADS=never
