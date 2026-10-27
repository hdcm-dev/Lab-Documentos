---
doc_id: DEMO-MU-32
doc_type: guia-practica
title: Compilar la API y hacer el primer pedido
description: Cómo bajar MovilidadUrbana.ApiWeb al SHA fijado, compilarla, pedir su contrato OpenAPI y dar de alta una localidad por HTTP.
status: borrador
origin: ia-generated
confidence: alta
owner: Lab-Documentos
last_review: 2026-10-27
audience: [desarrollo, agente]
traces: [RD-02, RN-LOC-01@1.1, RN-LOC-02@1.0]
---

# Compilar la API y hacer el primer pedido

## Índice

1. [Qué necesitás](#1-qué-necesitás)
2. [Bajar el código al SHA fijado](#2-bajar-el-código-al-sha-fijado)
3. [Compilar y arrancar](#3-compilar-y-arrancar)
4. [Pedir el contrato](#4-pedir-el-contrato)
5. [Primer pedido: alta de una localidad](#5-primer-pedido-alta-de-una-localidad)
6. [Lo que el contrato no dice](#6-lo-que-el-contrato-no-dice)

## 1. Qué necesitás

| Herramienta | Versión | Para qué |
| --- | --- | --- |
| SDK de .NET | 10 | Compilar y correr `MovilidadUrbana.ApiWeb` (el proyecto apunta a `net10.0`) |
| git | cualquiera reciente | Bajar el commit fijado |
| curl | cualquiera | Pedir el contrato y hacer el alta |

No hace falta clonar el repositorio entero ni tener el SDK de MAUI: la API es un proyecto independiente.

## 2. Bajar el código al SHA fijado

El SHA está en [`source.lock`](../../source.lock). Se baja ese commit y sólo la carpeta de la API:

```bash
git init --bare /tmp/mu.git
git --git-dir=/tmp/mu.git fetch --depth 1 https://github.com/hdcm-dev/Lab-E2E.WebBlazor.git 8fb4c3a8b1cc629027f456c7c3ce19e041164c06
mkdir -p /tmp/mu && git --git-dir=/tmp/mu.git archive FETCH_HEAD src/MovilidadUrbana.ApiWeb | tar -x -C /tmp/mu
```

`verificar.sh fuente`, en la raíz de la demo, hace lo mismo con los valores de `source.lock`.

## 3. Compilar y arrancar

```bash
dotnet build /tmp/mu/src/MovilidadUrbana.ApiWeb/MovilidadUrbana.ApiWeb.csproj -c Release -o /tmp/mu/out
cd /tmp/mu/out
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:5250 \
  ConnectionStrings__BaseDeDatos="Data Source=/tmp/mu/movilidad.db" dotnet MovilidadUrbana.ApiWeb.dll
```

| Variable | Por qué |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT=Development` | El contrato (`/openapi/v1.json`) y la página navegable (`/scalar/v1`) sólo se publican en desarrollo |
| `ASPNETCORE_URLS` | Puerto fijo para los ejemplos |
| `ConnectionStrings__BaseDeDatos` | La base SQLite queda fuera del árbol del código. Al arrancar, la API la crea y carga dos localidades de ejemplo (Corrientes y Resistencia) |

## 4. Pedir el contrato

```bash
curl -s http://127.0.0.1:5250/openapi/v1.json > openapi.json
```

El contrato tiene 5 rutas y 8 operaciones:

| Operación | Qué hace |
| --- | --- |
| `GET /api/v1/localidades` | Lista las localidades de la sesión |
| `POST /api/v1/localidades` | Da de alta una localidad |
| `GET /api/v1/localidades/{id}` | Devuelve una localidad |
| `PUT /api/v1/localidades/{id}` | Modifica una localidad |
| `DELETE /api/v1/localidades/{id}` | Da de baja una localidad |
| `GET /api/v1/encuestas/contador` | Cuántas encuestas registró la sesión |
| `POST /api/v1/encuestas/pasos/{paso}/validacion` | Valida un paso de la encuesta sin guardar |
| `POST /api/v1/encuestas` | Registra una encuesta completa |

La copia versionada es [`contracts/openapi.json`](../../contracts/openapi.json), sin el bloque `servers` (cambia con
el puerto). El workflow de la demo la compara en cada PR con la que emite la API en el SHA.

## 5. Primer pedido: alta de una localidad

Cada cliente se identifica con el encabezado `X-Sesion-Id` y sólo ve sus datos. Si no lo manda, la API le asigna uno
y lo devuelve en la respuesta, para que lo repita.

```bash
curl -s -i -X POST http://127.0.0.1:5250/api/v1/localidades \
  -H "Content-Type: application/json" -H "X-Sesion-Id: demo-tutorial" \
  -d '{"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":89959}'
```

Salida obtenida en el SHA fijado (2026-10-27, sin las líneas `Date` y `Server`):

```text
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Location: http://127.0.0.1:5250/api/v1/localidades/3
Transfer-Encoding: chunked
X-Sesion-Id: demo-tutorial

{"id":3,"nombre":"Goya","provincia":"Corrientes","codigoPostal":"3450","habitantes":89959}
```

Repetir el alta con `"nombre":"goya "` da 400 por la regla [RN-LOC-02](../fichas/RN-LOC-02.md):

```text
{"title":"La solicitud tiene datos inválidos.","status":400,"errors":{"nombre":["Ya existe una localidad con ese nombre en la provincia."]}}
```

## 6. Lo que el contrato no dice

El esquema `LocalidadRequest` declara los cuatro campos como obligatorios, pero no declara largos. Las reglas viven en
el dominio y el contrato no las ve. La más visible es [RN-LOC-01](../fichas/RN-LOC-01.md): la API también acepta un
nombre de 61 caracteres (responde `201`), aunque la ayuda de la app promete 60. Es la misma divergencia que registra
la ficha; mientras una persona no la decida, un cliente de la API no debería mandar más de 60.
