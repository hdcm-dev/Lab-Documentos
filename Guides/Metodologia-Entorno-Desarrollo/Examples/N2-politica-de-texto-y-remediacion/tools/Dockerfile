# Imagen del laboratorio N2: SDK de .NET 8 (git, iconv) + Python con la
# implementación de referencia de EditorConfig, para que la compuerta lea
# la política desde el propio .editorconfig.
# La base va fijada por digest (ver N5): la misma imagen en cualquier máquina.
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743
RUN apt-get update \
 && apt-get install -y --no-install-recommends python3 python3-venv \
 && rm -rf /var/lib/apt/lists/* \
 && python3 -m venv /opt/ec \
 && /opt/ec/bin/pip install --no-cache-dir EditorConfig==0.17.1
ENV PATH="/opt/ec/bin:${PATH}" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
