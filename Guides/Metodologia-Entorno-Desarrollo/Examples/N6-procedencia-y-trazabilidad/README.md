# N6 — Procedencia y trazabilidad: experimentos

Seis experimentos que responden con hechos a «¿con qué entorno se construyó esta pieza y cómo lo demuestro después?». Los tres primeros miran **qué lleva grabado** el artefacto y **cuándo dos builds dan los mismos bytes**; los otros tres miran lo que viaja **al lado** del artefacto: el SBOM (qué contiene) y la procedencia firmada (cómo y dónde se construyó).

La pieza es sintética: una consola .NET de una clase (`fuente-dotnet/`), un paquete npm con una dependencia (`fuente-node/`) y un paquete Python con dos backends de build (`fuente-python/`). Ningún experimento deja artefactos en el repositorio: cada script copia la pieza a un directorio temporal del host, la convierte en un repo git con **un commit de autor y fecha fijos** (`comun/preparar-repo.sh`, así el SHA es el mismo en cualquier máquina) y construye ahí.

## Requisitos

- Docker. Python 3 y git en el host (sólo para preparar el repo de juguete y leer los resultados).
- Red hacia `mcr.microsoft.com`, `ghcr.io`, Docker Hub, nuget.org, registry.npmjs.org y pypi.org.
- Imágenes y herramientas observadas el 2026-10-07:

| Imagen / herramienta | Versión observada |
|---|---|
| `mcr.microsoft.com/dotnet/sdk:8.0` | `sha256:78235e09…2743`, SDK 8.0.425 |
| `mcr.microsoft.com/dotnet/sdk:10.0` | `sha256:e1ffd2a9…a64c`, SDK 10.0.400 |
| `mcr.microsoft.com/dotnet/sdk:11.0-preview` | `sha256:b7e3c026…91be`, SDK 11.0.100-preview.7.26381.103, NuGet 7.10.0-rc |
| `node:24-alpine` | `sha256:ebfe2f90…c1c1`, Node 24.21.0, npm 11.19.0 |
| `python:3.12-slim` | `sha256:78387bc3…84ea`; setuptools 84.0.0, hatchling 1.32.4, build 1.6.1, cyclonedx-bom 7.5.0 (fijados en el script) |
| BuildKit / buildx | buildx v0.35.0, driver `docker-container` |
| `ghcr.io/sigstore/cosign/cosign:v3.1.3` | cosign v3.1.3 |
| SBOM .NET | `CycloneDX` 6.2.0 y `Microsoft.Sbom.DotNetTool` 4.1.13 (instalados en el contenedor) |

## Cómo correrlos

Desde cualquier directorio, con la ruta del script:

```sh
./e1-metadatos-dotnet/experimento.sh
./e2-determinismo-dotnet/experimento.sh      # ~5 min: 15 builds en contenedores
./e2-determinismo-dotnet/verificar-sdk11-rc1.sh  # E6-2b: casos (7) y (8) con el SDK 11 RC1
./e3-node-pack-y-sbom/experimento.sh
./e4-python-wheel-y-sbom/experimento.sh
./e5-procedencia-slsa-firmada/experimento.sh # crea y borra un builder BuildKit temporal
./e6-sbom-dotnet/experimento.sh
```

La salida de la corrida del 2026-10-07 está en `salidas/eN-*.txt`.

Nota sobre E6-5: el script usa un `DOCKER_CONFIG` temporal y vacío para descargar imágenes públicas sin credenciales (en la máquina del laboratorio las credenciales guardadas de Docker Hub estaban vencidas y bloqueaban hasta las descargas anónimas). No modifica la configuración del usuario. El builder `docker-container` es necesario porque el driver `docker` por defecto rechaza las atestaciones («Attestation is not supported for the docker driver»).

## Qué demuestra cada uno

| ID | Carpeta | Pregunta | Resultado observado |
|---|---|---|---|
| E6-1 | `e1-metadatos-dotnet/` | ¿Qué metadatos de procedencia graba solo el SDK de .NET y cuáles hay que agregar? | `InformationalVersion = 1.4.0+<SHA completo>`, `AssemblyMetadata RepositoryUrl`, mapa SourceLink `"/_/*" → raw.githubusercontent.com/…/<SHA>/*` dentro del PDB, `<repository … commit="<SHA>"/>` en el `.nuspec` del `.nupkg` y del `.snupkg`. La versión del SDK y el digest de la imagen **no** los graba el SDK: se agregaron como `AssemblyMetadata`. Con un cambio local sin commitear, el binario cambia y la versión informativa sigue diciendo el mismo SHA. |
| E6-2 | `e2-determinismo-dotnet/` | ¿Dos builds del mismo commit dan los mismos bytes? ¿Qué lo rompe? | Con `ContinuousIntegrationBuild` (rutas y contenedores distintos): DLL y PDB **iguales**, `.nupkg` **distinto**. Rompen: build sin CI (ruta en la DLL), `Deterministic=false`, otro SDK (8 vs 10), y `PathMap` a mano (no normaliza el mapa de SourceLink). El `.nupkg` difiere por las fechas del zip y el nombre aleatorio del `.psmdcp`; `SOURCE_DATE_EPOCH` no alcanza con SDK 8; con SDK 11 preview + `SOURCE_DATE_EPOCH` el `.nupkg` sale **idéntico**. |
| E6-3 | `e3-node-pack-y-sbom/` | ¿`npm pack` es reproducible? ¿Qué SBOM saca npm? | Mismo `shasum` e `integrity` en dos rutas con fechas de archivo distintas: npm fija el `mtime` de cada entrada en 499162500 (1985-10-26T08:15:00Z). `npm sbom` emite SPDX 2.3 y CycloneDX 1.5 desde el lock; dos SBOM del mismo lock difieren en `serialNumber` y `timestamp`; en CycloneDX el `name` del componente raíz sale del **nombre del directorio**, no del `package.json` (el `purl` sí es correcto). |
| E6-4 | `e4-python-wheel-y-sbom/` | ¿La rueda sale igual? ¿Qué hace `SOURCE_DATE_EPOCH`? ¿Y un SBOM adentro (PEP 770)? | setuptools: distinta sin `SOURCE_DATE_EPOCH`, idéntica con él (fechas del zip = la del commit). hatchling: reproducible por defecto (fecha fija 2020-02-02), pero un SBOM con `timestamp` y `serialNumber` metido en `.dist-info/sboms/` la vuelve **distinta**; con `cyclonedx-py --output-reproducible`, idéntica. `METADATA` no tiene campo para el commit. |
| E6-5 | `e5-procedencia-slsa-firmada/` | ¿Cómo es una procedencia SLSA real? ¿Qué agrega la firma? | BuildKit emite un in-toto Statement v1 con `predicateType https://slsa.dev/provenance/v1`; los `subject` coinciden con el sha256 de `Pieza.dll` y `Pieza.pdb`; `resolvedDependencies` registra la imagen del SDK por digest; el SHA del commit aparece (buildx lee el `.git` del contexto); `builder.id` vacío (build local). La DLL es idéntica a la de una build independiente con `docker run` en otra ruta. Sin firma, la procedencia se edita sin rastro; firmada con cosign (DSSE), la verificación falla si cambia un byte del artefacto o si se edita la procedencia. |
| E6-6 | `e6-sbom-dotnet/` | ¿Qué SBOM sale de una pieza .NET con una dependencia NuGet? | `dotnet-CycloneDX` 6.2.0 → CycloneDX 1.7 con `Newtonsoft.Json@13.0.3`, purl y SHA-512. `sbom-tool` 4.1.13 → SPDX 2.2 con los **archivos** de la carpeta de salida (SHA256/SHA1) más los paquetes, relaciones `DESCRIBES` / `DEPENDS_ON`. Dos herramientas, dos vistas: grafo declarado frente a lo que efectivamente se entrega. |

## Límites

- E6-5 firma con una **clave local** y sin log de transparencia (`--tlog-upload=false`, `--insecure-ignore-tlog`). Sirve para ver qué protege la firma; no es SLSA Build L2, que exige que la procedencia la genere y firme una **plataforma de build alojada** cuyo secreto no esté al alcance de quien define los pasos de la build.
- No se ejecutó la procedencia de npm (`npm publish --provenance`), las atestaciones de PyPI ni las de GitHub/GitLab: requieren un runner alojado con OIDC y publicar en un registro. Quedan documentadas con fuente en la bitácora N6.
- E6-2b (2026-10-07) repitió los casos (7) y (8) con `11.0.100-rc.1`, con el mismo resultado (`salidas/e2b-nupkg-sdk11-rc1.txt`).
- `11.0-preview` es una versión preliminar: el comportamiento de `dotnet pack` puede cambiar antes de la versión final.
