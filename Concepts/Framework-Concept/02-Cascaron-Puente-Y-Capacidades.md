---
doc_id: FC-TEM-02
doc_type: tema
title: Cascarón y servicios nativos — puente, capacidades, base local y encuestas como datos
status: borrador
origin: ia-generated
confidence: media
owner: Lab-Documentos
last_review: 2026-10-26
audience: [humano, agente]
traces: [FC-MARCO-REFERENCIA, FC-TEM-01, FC-TEM-03, FC-TEM-04, FC-TEM-07, FC-TEM-08, FC-ANEXO-99]
---

# Cascarón y servicios nativos — puente, capacidades, base local y encuestas como datos

## Índice

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Definición](#2-definición)
3. [Aplicación por escenario](#3-aplicación-por-escenario)
4. [Ejemplos concretos](#4-ejemplos-concretos)
5. [Preguntas guía](#5-preguntas-guía)
6. [Criterios de calidad](#6-criterios-de-calidad)
7. [Anexo — Plantilla comentada](#7-anexo--plantilla-comentada)
8. [Observaciones](#8-observaciones)
9. [Referencias](#9-referencias)

Viene de: [`FC-TEM-01`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/01-Actualizacion-Sin-Tienda.md) ·
Sigue en: [`FC-TEM-03`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/03-Integracion-Y-Versionado.md)

## 1. Resumen ejecutivo

El contenido que se baja no toca el teléfono directamente: todo lo que necesita del dispositivo (estado de la red,
guardar un documento, encolar una respuesta, la sesión) lo pide al **cascarón** por un único método, `Call(sobre)`,
con sobres JSON-RPC 2.0. Esa tabla de métodos, el **catálogo de capacidades** y el transporte forman el **contrato del
puente**, que tiene versión propia y se fija con una prueba de instantánea. El cascarón es dueño de la base SQLite y
del **outbox**; el contenido guarda documentos JSON por el puente y nunca hace DDL. Las encuestas viajan como **datos**
(definiciones con `encuestaId` y `version`) que unas páginas genéricas dibujan; cada respuesta guarda con qué versión se
tomó. Le sirve a quien escribe el cascarón y a quien escribe el contenido, porque el contrato es lo que comparten.

## 2. Definición

### 2.1 Qué es

**Puente.** El único canal por el que el contenido del WebView le pide algo al cascarón. En la demo es un método
`Call(sobre)` registrado con `SetInvokeJavaScriptTarget` de `HybridWebView`; los eventos vuelven por
`InvokeJavaScriptAsync` (P3).

**Cómo se aplica.** Desde una página Razor llamás, por ejemplo, `network.status`; el cascarón responde con
`{conectado, estado, motivo}`.

**Un caso.** Al cargar, el contenido hace `bridge.handshake` y recibe la versión del puente (1.1.0) y el catálogo con la
versión de cada entrada; con eso decide si muestra o esconde funciones `optional`.

**Catálogo de capacidades.** Lista constante en el código del cascarón, una entrada por capacidad del dispositivo, con
id estable y versión propia (P12). En la demo: `device.info` 1.0.0, `network.status` 1.1.0 y `auth.session` 1.0.0
([`CatalogoDeCapacidades.cs`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/src/FrameworkConcept.Core/Contracts/Puente/CatalogoDeCapacidades.cs)
L37–L47).

**Cómo se aplica.** Si tu contenido necesita una capacidad, la declarás en su manifiesto como `requires` (sin ella no
se activa) u `optional` (sin ella degrada).

**Un caso.** `declaracion-del-contenido.json` de la demo declara `camera.capture` como `optional`: el catálogo no la
tiene, y el contenido debe esconder la función en lugar de fallar.

**Outbox.** Tabla del cascarón donde cada acción que necesita servidor queda escrita en la misma transacción que el
cambio local, con clave de idempotencia y dueño, hasta que se envía (P5).

**Cómo se aplica.** Registrás la encuesta sin red; se guarda con su fila de outbox; el sincronizador la envía al volver
la red y sesión válida.

**Un caso.** E02 paso 2: «Por enviar: 1», `outbox: intento 1 → Pendiente (Connection failure)`; paso 3: «`intento 2 →
Enviada (HTTP 201)`».

**Definición de encuesta.** Archivo JSON con `encuestaId`, `version` entero monótono y `compatibleConContenido` con
techo, publicado como entrada propia del índice firmado (P1, P13).

**Cómo se aplica.** Para corregir una etiqueta, editás `definiciones/encuesta-movilidad/v2.json` con `version: 2`, y
publicás sólo la definición, sin paquete de contenido nuevo.

**Un caso.** La v2 de la demo corrige una etiqueta y suma una pregunta opcional (README de la demo, tramo A1).

### 2.2 Qué problema resuelve

Sin un contrato explícito, un contenido nuevo puede llamar a un método que el cascarón instalado no tiene, y el error
aparece en el teléfono de alguien. Con contrato versionado e instantánea, ese error aparece en el PR. Sin outbox, una
encuesta tomada sin red se pierde o queda a mitad de camino.

### 2.3 Qué NO es

| Se confunde con | Por qué no es lo mismo |
| --- | --- |
| Interfaces C# compartidas | El contrato es la **tabla de métodos + catálogo + transporte**, serializada en `contrato.json`; no interfaces que el contenido referencia (P3) |
| API web del navegador (`navigator.geolocation`, `getUserMedia`) | Prohibidas para el contenido: toda capacidad pasa por el puente, y la compuerta lo prueba (P8, P12) |
| `minShellVersion` | Salió del teléfono: la compatibilidad la decide un **predicado** con rangos con techo (P3) |
| Una migración de base al actualizar | El esquema es del cascarón y fijo (C-1, folio 023); una actualización de contenido nunca migra |
| «Sincronizar antes de actualizar» | Ninguna actualización espera a vaciar el outbox ni descarta pendientes (P13.7) |

### 2.4 El transporte: lo que dice Microsoft y lo que hizo la demo

La documentación de .NET 10 describe el evento nuevo que hace posible servir el contenido bajado (E01 §2, cita de la
documentación de *What's new*):

> «You can now intercept and respond to web requests made from `BlazorWebView` and `HybridWebView` … supplying local
> responses»

**Qué implica.** El cascarón no necesita un servidor HTTP local: atiende cada pedido del WebView respondiendo el archivo
desde la carpeta de la versión activa. E01 fijó las condiciones: usar `HybridWebView` con `WebResourceRequested` (no
`HybridRoot`, que sólo ve el APK), incluir `_framework/hybridwebview.js` y `<base href="/">`, WebView ≥ 94 y archivos
planos (C-1 a C-4).

**Dónde se materializa.** `src/FrameworkConcept.Shell/Hosting/ServidorDeContenido.cs` atiende los pedidos y deja
pasar sin atrapar `_framework/hybridwebview.js` y `POST /__hwvInvokeDotNet`, porque son el transporte del puente y su
dueño es el cascarón (comentario de la L10 y condición de la L53); el transporte figura en el contrato como
`Script: "_framework/hybridwebview.js"` (`Contracts/Puente/TransporteDelPuente.cs` L22). Un arreglo
de integración de E02 obligó a mandar el sobre en ASCII con escapes `\uXXXX`, porque en Android `hybridwebview.js` lleva
el cuerpo en un encabezado HTTP que no admite UTF-8 (en el g42, «¿Qué» llegaba como «�Qu�»;
`src/FrameworkConcept.Content/wwwroot/js/puente.js`).

### 2.5 El predicado de compatibilidad

Antes de activar un contenido, el cascarón negocia con lo que el contenido declara en su manifiesto y, al cargarlo,
el puente cierra el apretón de manos. El recorrido, según el predicado de abajo y la respuesta del handshake
([`Handshake.cs`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/src/FrameworkConcept.Core/Contracts/Puente/Handshake.cs) L3–L10: versión del puente, catálogo y huella; `bridge.ready` es el `Listo(version)`):

```mermaid
sequenceDiagram
    participant Cn as Contenido (manifiesto)
    participant Ca as Cascarón
    participant Pu as Puente
    Ca->>Cn: lee compatibleConPuente, capabilities.requires, motor mínimo
    alt puente fuera de rango, capacidad faltante o motor viejo
        Ca-->>Ca: no activa; sigue la versión anterior con el motivo
    else compatible
        Ca->>Pu: activa y sirve el contenido en el HybridWebView
        Cn->>Pu: bridge.handshake
        Pu-->>Cn: versión del puente + catálogo + huella
        Cn->>Pu: Listo(version)
        Note over Ca,Pu: sin Listo en 2 cargas → rollback por marca (FC-TEM-01)
    end
```

```csharp
        if (!manifiesto.CompatibleConPuente.Contiene(entorno.VersionDelPuente))
            motivos.Add($"puente {entorno.VersionDelPuente} fuera de {manifiesto.CompatibleConPuente}");

        foreach (var requisito in manifiesto.Capabilities.Requires)
        {
            var entrada = entorno.Catalogo.FirstOrDefault(e => string.Equals(e.Id, requisito.Id, StringComparison.Ordinal));
            if (entrada is null)
                motivos.Add($"requires «{requisito.Id}»: el catálogo del cascarón no la tiene");
            else if (!requisito.Rango.Contiene(entrada.Version))
                motivos.Add($"requires «{requisito.Id}»: versión {entrada.Version} fuera de {requisito.Rango}");
        }
```

*Fuente: [`Demo/src/FrameworkConcept.Core/Update/PredicadoDeCompatibilidad.cs`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/src/FrameworkConcept.Core/Update/PredicadoDeCompatibilidad.cs) L22–L32, árbol de trabajo del 2026-10-26 sin commit; versión fijada por `sha256` 62702aac9be5 del archivo (`sha256sum` de la ruta, primeros 12 dígitos).*

El predicado evalúa siempre las tres dimensiones (puente, `requires`, motor), aunque la primera falle, para que el
diagnóstico diga todo lo que falta (comentario de la L13). El motor mínimo nunca baja de
`ParametrosDeProducto.WebViewMinimo = 94`.

### 2.6 Cómo se clasifica un cambio del puente

| Cambio | Clase | Qué hacer |
| --- | --- | --- |
| Método nuevo, entrada nueva del catálogo, campo opcional nuevo en una respuesta | **Menor** | Subir el menor de `versionado/puente.json` y regenerar `contrato.json` |
| Método quitado, parámetro obligatorio nuevo, cambio de tipo o de forma, cambio del transporte | **Mayor** | Nuevo mayor del puente, índice nuevo `/canal/puente-N+1/` con su semilla, cascarón por tienda |

El incremento de red y autenticación fue todo menor: 1.0.0 → 1.1.0, con métodos `actions.*`, `outbox.sendNow`,
`auth.*` y campos opcionales `estado`, `motivo` en `network.status`
([DISENO §6](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/DISENO-RED-Y-AUTENTICACION.md#6-contrato-del-puente-110)).

### 2.7 Encuestas tomadas con una versión cuando llega otra (P13)

Diez reglas del dictamen, resumidas en las que el desarrollador toca:

1. Cada respuesta guarda `{encuestaId, versionDefinicion}` desde que se empieza a cargar.
2. La encuesta en curso termina con su versión; la nueva aplica a la próxima.
3. El cliente conserva toda definición con respuestas pendientes.
4. El servidor acepta versiones publicadas y no retiradas.
5. Retirar una versión de definición se anuncia en el índice con 30 días de aviso; una respuesta de una versión
   retirada va a carta muerta, visible, nunca se borra.
6. Dentro de un `encuestaId`, el nombre y el tipo de un campo publicado no cambian; agregar preguntas, corregir textos
   y aflojar restricciones sí. Un cambio destructivo es un `encuestaId` nuevo.

La regla 1 se materializa en el dominio portado del caso base, que suma dos campos a la respuesta
([`MAPA-CASO-BASE.md`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/MAPA-CASO-BASE.md),
fila `Domain/Entities/RespuestaDeEncuesta.cs`, «Adaptado»).

## 3. Aplicación por escenario

| Escenario | Qué aporta este tema | Según el contexto |
| --- | --- | --- |
| `ESC-1` | El `handshake` le dice al contenido de la semilla qué puede usar | — |
| `ESC-2` | `Listo(version)` es un método del puente: sin él no hay rollback por marca | — |
| `ESC-3` | El predicado rechaza antes de activar; `optional` lo resuelve el contenido | `CTX-2`: motor < 94 → pantalla «WebView viejo»; `CTX-3`: iOS expone el catálogo sólo a la semilla (rama iOS de P12, diseño) |
| `ESC-4` | Outbox con dueño y respuesta atada a su definición | `CTX-5`: sólo sale con la identidad de su dueño (ver [`FC-TEM-07`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/07-Red-Y-Autenticacion.md)) |
| `ESC-5` | La instantánea del contrato y la compuerta de definiciones bloquean en `fw-ok` | — |
| `ESC-6` | `auth.*` del puente 1.1.0 | `CTX-4`: `auth.status` informa `modo: dispositivo` |
| `ESC-7` | Una obligatoria no descarta ni migra pendientes | No ejecutado en la demo |

## 4. Ejemplos concretos

### 4.1 La respuesta v1 aceptada después de llegar v2 (corrida real)

**Qué demuestra.** P13: la versión de la definición viaja con la respuesta.
**Precondición.** Encuesta tomada sin red con la v1; publicadas la v2 y el contenido 1.1.0.
**Resultado.** E02 paso 5, `receptor-respuestas-final.json`: «1 aceptada: `versionDefinicion: 1`,
`versionContenido: 1.0.0+5d5c2b4`… Carta muerta vacía, 0 duplicados».

### 4.2 La prueba de humo del receptor (corrida de esta guía)

**Qué demuestra.** Que el receptor rechaza respuestas de definiciones no publicadas, inválidas o retiradas, y no
duplica reenvíos.
**Precondición.** Ninguna: `scripts/guion.sh humo` usa sus propias claves en `salida/humo/claves/`.
**Resultado.** Corrida del 2026-10-26 sobre una copia de la demo: «humo: 47 OK, 0 MAL», con carta muerta para
`v7 no está publicada`, `los valores no cumplen la definición v1` y `v1 se retiró … (P13.5)`, y
`reenviosQueNoSeDuplicaron: 1`.

### 4.3 La definición v1 (archivo real)

```json
{
  "encuestaId": "encuesta-movilidad",
  "version": 1,
  "compatibleConContenido": ">=1.0.0 <2.0.0",
  "titulo": "Encuesta de uso del transporte",
```

*Fuente: [`Demo/definiciones/encuesta-movilidad/v1.json`](/LAB/Lab-Documentos/Lab-Documentos/Concepts/Framework-Concept/Demo/definiciones/encuesta-movilidad/v1.json) L1–L5, árbol de trabajo del 2026-10-26 sin commit; versión fijada por `sha256` c4be9234f718 del archivo (`sha256sum` de la ruta, primeros 12 dígitos).*

`compatibleConContenido` tiene techo (`<2.0.0`): por eso el contenido v2 de la demo es 1.1.0 y no 2.0.0 (comentario del
`.csproj` del contenido).

## 5. Preguntas guía

1. ¿Lo que necesita mi página está en el catálogo? ¿Lo declaro como `requires` u `optional`?
2. ¿Mi cambio al puente es menor o mayor según la tabla de 2.6?
3. ¿Regeneré `contrato.json` (`FrameworkConcept.Tools contrato`) y subí `versionado/puente.json`?
4. ¿Mi cambio a la encuesta conserva nombre y tipo de cada campo publicado?
5. ¿La acción que agrego necesita servidor? ¿Va por el outbox?
6. ¿Algo de mi contenido usa una API web que pida permisos?

## 6. Criterios de calidad

| Bueno | Pobre |
| --- | --- |
| Un solo método `Call(sobre)` y una tabla de métodos versionada | Un método del puente por función, sin versión |
| Toda capacidad del dispositivo por el puente; pedida en el uso, con motivo y salida a ajustes | Permisos pedidos en el arranque «por las dudas» |
| Una capacidad nueva entra con declaración y uso en el mismo binario (P12) | Declarar un permiso para un contenido futuro |
| Outbox escrito en la misma transacción que el cambio local | Encolar después de guardar, con riesgo de perder la fila |
| Definición corregida con versión nueva, nunca reescrita | Editar `v1.json` publicado |

## 7. Anexo — Plantilla comentada

Plantilla para declarar un método nuevo del puente (criterio de esta guía):

```markdown
### Método `<area>.<verbo>`                <!-- ¿El área sigue la convención en inglés (network, outbox, auth…)? -->
- Parámetros: …                            <!-- ¿Alguno es obligatorio nuevo? Entonces es MAYOR -->
- Respuesta: …                             <!-- ¿Sólo agrega campos opcionales? Entonces es MENOR -->
- Capacidad del catálogo: …                <!-- ¿Existe la entrada? ¿Con qué versión y qué permiso declara? -->
- Clase de acción: SoloLocal | Encolable | SoloEnLinea   <!-- ¿Qué pasa sin red? (FC-TEM-07) -->
- Versión del puente: 1.1.0 → 1.2.0        <!-- ¿Coincide con versionado/puente.json y contrato.json? -->
- Prueba: Gate.Tests/Contrato/…            <!-- ¿La instantánea quedó en rojo antes de regenerarla? -->
```

## 8. Observaciones

- **Hecho.** `Permissions-Policy` para negar API web al contenido es defensa adicional NO VERIFICADA en `HybridWebView`
  (P12, folio 037 «deuda declarada»).
- **Hecho.** La aceptación de versiones viejas en un servidor real (ODK Central) es NO VERIFICADA; el receptor de la
  demo es un doble de prueba (P13).
- **Hecho.** iOS: el origen `app://0.0.0.1/` interceptable por esquema propio, ECDSA P-256 y piso 16.4 están
  documentados sin ejecutar (P9).

## 9. Referencias

- Microsoft, *HybridWebView – .NET MAUI* (2026-07-08): https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-10.0 (consultada el 2026-10-25, N2).
- Dossier [N2](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Bitacoras/Nucleos/N2-Cascaron-Y-Servicios-Nativos.md); [E01 §7](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E01-WASM-Descargado-En-WebView/INFORME.md); [E02](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Ensayos/E02-Demo-Integracion/INFORME.md); dictamen P3, P5, P12, P13 del [folio 035](/LAB/Lab-Documentos/Lab-Documentos.Documentacion/PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/035-informe-r2-jurado-veredicto.md).
