# Imagen de CI "después del descongelamiento": la misma receta que la imagen
# del laboratorio N2 (N2-politica-de-texto-y-remediacion/tools/Dockerfile),
# con la base movida al SDK 10. Subir el SDK = mover global.json Y esta línea
# FROM en el MISMO PR. Base fijada por digest (ver N5).
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c
RUN apt-get update \
 && apt-get install -y --no-install-recommends python3 python3-venv \
 && rm -rf /var/lib/apt/lists/* \
 && python3 -m venv /opt/ec \
 && /opt/ec/bin/pip install --no-cache-dir EditorConfig==0.17.1
ENV PATH="/opt/ec/bin:${PATH}" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
