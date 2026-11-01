---
doc_id: GP-SDEI-LANZAR
doc_type: guia-practica
title: Lanzar el ejemplo de ayuda por rol
description: Guía práctica para levantar la prueba de ayuda por rol en su devcontainer, comprobar que responde y, si hace falta, mostrarla afuera por un túnel.
status: borrador
origin: ia-assisted
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-01
audience: [humano, agente, desarrollo]
traces: [IDX-SDEI, TEM-SDEI-05]
---

# Lanzar el ejemplo de ayuda por rol

Guía práctica para levantar la [prueba de concepto de ayuda por rol](Prueba-Ayuda-Por-Rol/README.md) y comprobar que
hace lo que el cuerpo dice de ella. Lo que la prueba demuestra está en `TEM-05` §3.3
([Ayuda por rol, con un cruce que nadie había plantado](../TEM-05-Ayuda-Al-Usuario-Final.md#33-ayuda-por-rol-con-un-cruce-que-nadie-había-plantado)).

## Índice

1. [Objetivo](#1-objetivo)
2. [Prerrequisitos](#2-prerrequisitos)
3. [Variables](#3-variables)
4. [Pasos](#4-pasos)
5. [Resultado](#5-resultado)
6. [Validaciones](#6-validaciones)
7. [Errores frecuentes](#7-errores-frecuentes)
8. [Bajar la prueba](#8-bajar-la-prueba)

## 1. Objetivo

Dejar corriendo el panel web de la prueba (Blazor, .NET 10, ingreso simulado por rol, ayuda contextual, 404 por rol,
`llms.txt` filtrado y dos idiomas) y ver con peticiones HTTP que cada rol recibe sólo su ayuda. El ingreso es simulado
y la prueba no tiene datos reales.

## 2. Prerrequisitos

| Qué | Para qué |
| --- | --- |
| Un clon de Lab-Documentos | La prueba vive en `Examples/Prueba-Ayuda-Por-Rol/` de este cuerpo |
| Docker | El devcontainer y, si se usa, el túnel corren en contenedores |
| Node.js con `npx` | Para el CLI de devcontainers, en la versión fija de §3 |
| `curl` | Para las validaciones de §6 |

El devcontainer de la prueba (`Prueba-Ayuda-Por-Rol/.devcontainer/devcontainer.json`) usa la imagen
`mcr.microsoft.com/devcontainers/dotnet:1-10.0` y el puerto `5190`.

## 3. Variables

| Variable | Qué es | De dónde sale |
| --- | --- | --- |
| `$LD` | Raíz del clon de Lab-Documentos | La ruta donde quien ejecuta tiene el clon; se pregunta o se busca, no se supone |
| `$P` | Carpeta de la prueba | `$LD/Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/Examples/Prueba-Ayuda-Por-Rol` |
| `$CID` | Identificador del contenedor del devcontainer | Campo `containerId` de la salida del paso 1 |
| `$W` | Carpeta de trabajo dentro del contenedor | Campo `remoteWorkspaceFolder` de la salida del paso 1 |
| `$U` | URL base del panel | `http://localhost:5190`, o la URL del túnel si se hace el paso 5 |

Versiones fijas:

| Herramienta | Versión |
| --- | --- |
| CLI de devcontainers | `@devcontainers/cli@0.89.0` |
| Imagen del túnel (sólo paso 5) | `cloudflare/cloudflared:2026.10.5` |

## 4. Pasos

Se ejecutan en orden. Cada paso se verifica antes del siguiente; si uno falla, se detiene y se busca la causa en §7.

**Paso 1. Levantar el devcontainer.**

```bash
cd "$P"
npx -y @devcontainers/cli@0.89.0 up --workspace-folder .
```

La salida es un JSON con `"outcome":"success"`, `containerId` (es `$CID`) y `remoteWorkspaceFolder` (es `$W`). Si el
devcontainer ya existía para esta misma carpeta, el CLI lo reutiliza.

**Paso 2. Compilar.**

```bash
npx -y @devcontainers/cli@0.89.0 exec --workspace-folder . \
  dotnet build Web/AyudaDemo.Web.csproj -nologo -v q
```

Esperado: `0 Error(s)`.

**Paso 3. Lanzar la app desacoplada de la sesión, en `Production`.**

La app no se lanza como proceso en segundo plano de la terminal o del agente: al cerrarse la sesión, muere. Se lanza
con `setsid nohup` dentro del contenedor:

```bash
CID=<containerId del paso 1>
W=<remoteWorkspaceFolder del paso 1>
docker exec "$CID" pkill -f AyudaDemo.Web || true
docker exec -d -u vscode -w "$W/Web" \
  -e ASPNETCORE_URLS=http://0.0.0.0:5190 -e ASPNETCORE_ENVIRONMENT=Production -e DOTNET_CLI_TELEMETRY_OPTOUT=1 \
  "$CID" sh -c 'setsid nohup dotnet run --no-build --project AyudaDemo.Web.csproj --no-launch-profile > /tmp/ayuda-demo.log 2>&1 &'
```

Se espera hasta 30 s y se comprueba:

```bash
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:5190/ingresar     # 200
docker exec "$CID" grep -E 'Catálogo de ayuda|Hosting environment' /tmp/ayuda-demo.log
```

**Paso 4. Validar por HTTP.** Con `U=http://localhost:5190`, se corren las validaciones de §6.

**Paso 5 (opcional). Mostrarla afuera por un túnel temporal.** Sólo si alguien que no está en la máquina tiene que
verla, y sólo con la app en `Production`. El túnel rápido de Cloudflare no pide cuenta ni DNS y su URL cambia cada vez
que se recrea:

```bash
docker rm -f demo-ayuda-por-rol-tunnel 2>/dev/null
docker run -d --name demo-ayuda-por-rol-tunnel --network host \
  cloudflare/cloudflared:2026.10.5 tunnel --no-autoupdate --url http://localhost:5190
docker logs demo-ayuda-por-rol-tunnel 2>&1 | grep -oE 'https://[a-z0-9-]+\.trycloudflare\.com' | head -1
```

Con esa URL como `$U`, se repiten las validaciones de §6. Este paso no toca otros contenedores ni otros túneles que
haya en la máquina: sólo el que crea, por su nombre.

## 5. Resultado

El panel responde en `$U`. El registro de la app dice `Catálogo de ayuda: 9 páginas, validador con 0 fallas` (7
páginas en castellano y 2 traducciones) y `Hosting environment: Production`. Cada rol ve sólo su ayuda.

## 6. Validaciones

Todas en una sesión de alumno, con la cookie guardada entre peticiones:

| Petición | Esperado | Qué comprueba |
| --- | --- | --- |
| `GET $U/ingresar` | `200` | Que la app está arriba |
| `GET $U/entrar/Student` (guardar la cookie) | `302` | El ingreso simulado del alumno |
| `GET $U/ayuda/alumno/enviar-un-trabajo` | `200` | La ayuda del propio rol |
| `GET $U/ayuda/administrador/gobernar-las-cuentas` | `404` | Que el servidor no entrega la ayuda de otro rol |
| `GET $U/ayuda/llms.txt` | `200`, cuatro entradas, ninguna de administrador | El mismo filtro en el canal para agentes |

Toda afirmación sobre la prueba sale de una validación corrida en esa misma vez: no se dice «arriba» sin el `200`.

## 7. Errores frecuentes

| Si ves | Causa probable | Qué hacer |
| --- | --- | --- |
| El paso 1 corta por tiempo | La imagen base todavía se está bajando | Bajarla aparte (`docker pull mcr.microsoft.com/devcontainers/dotnet:1-10.0`) y repetir el paso 1 |
| El CLI no reutiliza un devcontainer que existe | Se creó cuando la carpeta estaba en otra ruta | Buscarlo (`docker ps -a --filter label=devcontainer.local_folder`) y borrarlo con `docker rm -f` |
| `curl` devuelve `000` o el puerto no responde | La app no terminó de arrancar, o murió con la sesión | Mirar `/tmp/ayuda-demo.log` en el contenedor; relanzar el paso 3 con `setsid nohup` |
| `Hosting environment: Development` | Faltó `ASPNETCORE_ENVIRONMENT=Production` | Relanzar el paso 3; nunca exponer la app en `Development`, que muestra trazas internas |
| `000` intermitente por la URL del túnel en los primeros minutos | El nombre nuevo que dio el túnel todavía se propaga | Reintentar cada petición hasta cinco veces con 3 s de espera; avisar a quien reciba el enlace |
| `0 Error(s)` no aparece en el paso 2 | El código de la prueba no compila | Reportarlo; esta guía lanza la prueba, no la modifica |

## 8. Bajar la prueba

```bash
docker rm -f demo-ayuda-por-rol-tunnel 2>/dev/null
docker exec "$CID" pkill -f AyudaDemo.Web
```

El devcontainer se conserva parado: recrearlo cuesta minutos. Se borra con `docker rm -f "$CID"` sólo si ya no se va a
usar.

Volver a la [prueba de concepto](Prueba-Ayuda-Por-Rol/README.md) o a
[`TEM-05`](../TEM-05-Ayuda-Al-Usuario-Final.md#33-ayuda-por-rol-con-un-cruce-que-nadie-había-plantado).
