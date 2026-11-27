# Demo: servicio de subas reanudables con tus en .NET 10

Demo que acompaña a la [guía de desarrollo](../Tus-Net-Service-Devel-Guide.md). Un solo servicio ASP.NET Core con:

- el endpoint tus en `/archivos` (tusdotnet 2.12.0);
- una API con controllers en `/api/archivos` y `/descargas/<id>`, con OpenAPI en `/openapi/v1.json` y Scalar en `/scalar`;
- una página Blazor Interactive Server en `/` para subir, pausar y retomar.

Más un cliente de consola que sube con un corte simulado, retoma y compara huellas.

## Correr

```bash
cd src/TusDemo.Web && dotnet run          # http://localhost:5080
# o, sin SDK:
docker compose up --build                  # http://localhost:8090
```

`/scalar` y `/openapi/v1.json` sólo existen en el entorno Development (el de `dotnet run` y el de `compose.yaml`).

La clave de la demo está en `appsettings.json` (`Archivos:ClaveDemo`) y va en la cabecera `X-Clave-Demo`.

## Probar

`scripts/probar.sh` compila, levanta el servicio y recorre los casos de la sección 6 de la guía. Pensado para
correr dentro de `mcr.microsoft.com/dotnet/sdk:10.0.404`, desde esta carpeta.
