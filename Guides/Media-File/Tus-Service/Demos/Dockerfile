# Imagen del servicio de la demo. Tags de parche fijos (no "10.0", que se mueve).
FROM mcr.microsoft.com/dotnet/sdk:10.0.404 AS build
WORKDIR /src
COPY src/TusDemo.Web/TusDemo.Web.csproj src/TusDemo.Web/
RUN dotnet restore src/TusDemo.Web/TusDemo.Web.csproj
COPY src/TusDemo.Web/ src/TusDemo.Web/
RUN dotnet publish src/TusDemo.Web/TusDemo.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0.15
WORKDIR /app
COPY --from=build /app .
# La carpeta de datos es del usuario sin privilegios "app" (uid 1654) que traen las imágenes de .NET.
RUN mkdir -p /data && chown "$APP_UID" /data
USER $APP_UID
ENV Archivos__Raiz=/data/tus \
    Archivos__Registro=/data/registro.json
EXPOSE 8080
ENTRYPOINT ["dotnet", "TusDemo.Web.dll"]
