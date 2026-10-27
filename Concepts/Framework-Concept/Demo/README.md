# Demo del framework — apps .NET MAUI con pantallas Blazor actualizables sin tienda

Una app Android que se instala una vez por tienda (el **cascarón**) y cuyas pantallas de negocio (el **contenido**,
páginas Razor compiladas a Blazor WebAssembly) y encuestas (las **definiciones**, datos JSON) se actualizan
bajándolas de un canal firmado, sin pasar por la tienda.

La explicación completa está en la guía, `../Developer-Guide.md`. Este archivo sólo dice cómo se compila y se
corre la demo y en qué estado está.

## Qué hace falta

- Docker y conexión a internet la primera vez. No hace falta tener .NET instalado: la imagen de compilación sale de
  la receta versionada `contenedor/Dockerfile` (SDK .NET 10.0.400, workload set 10.0.400.1 con `maui-android`
  10.0.20, JDK 17, SDK de Android, python3 y openssl). `scripts/contenedor.sh` la construye sola si no existe
  (`scripts/imagen.sh`, unos minutos la primera vez) y la etiqueta `fc-demo-build:<huella de la receta>`; si la
  receta cambia, se construye otra.
- Para ver el contenido Blazor: un dispositivo Android con Android System WebView 94 o mayor (P1, condición C-3 del
  ensayo E01). Un emulador con WebView 83 sólo llega a la pantalla «WebView viejo» del cascarón. La integración se
  corrió en un emulador Android 13 x86_64 con WebView 155 (ver «Recorrido en el dispositivo»).
- `adb` contra el dispositivo, y que el dispositivo alcance por IP a la máquina que corre el receptor. Ese es el
  único camino de red medido (P11), y se midió sólo con el emulador `fc-android-emu-hpdev` (por Tailscale).
- **Desvío declarado de P11 con el teléfono de medida.** El moto g42 no alcanza por red a la máquina del receptor.
  Con él, el único medio que funcionó (E01) es `guion.sh tunel`: un túnel efímero de cloudflared, que P11 excluye
  como camino de la demo y que acá queda como **medio de laboratorio** (sólo el contenido de la demo, sin secretos,
  se baja con `guion.sh parar`). El candidato de P11, el reenvío inverso de puertos de adb, sigue **NO VERIFICADO**:
  con un servidor adb remoto el reenvío llega a la máquina de ese servidor, no a la del receptor.

## Cómo se compila y se prueba

Todo corre dentro del contenedor, con `scripts/contenedor.sh`, desde esta carpeta:

```bash
scripts/contenedor.sh dotnet build FrameworkConcept.Demo.SinMaui.slnf -c Release   # todo menos el cascarón MAUI
scripts/contenedor.sh dotnet test tests/FrameworkConcept.Core.Tests -c Release      # dominio y contrato
scripts/contenedor.sh dotnet build src/FrameworkConcept.Shell -c Debug \
  -p:RuntimeIdentifier=android-arm64 -p:AndroidPackageFormat=apk                   # el APK del cascarón
```

`FrameworkConcept.Demo.SinMaui.slnf` es la parte que se compila sin el workload de Android: la que corre en la
compuerta rápida del PR. Con `CI=true` los avisos se tratan como errores (`Directory.Build.props`).

Al terminar, `bin/` y `obj/` se pueden borrar sin perder nada; `.gitignore` los excluye.

El equivalente local de la compuerta del PR es `scripts/ci-local.sh` (mismos pasos y orden que
`.github/workflows/framework-concept-demo.yml`): build sin MAUI con avisos como error, las tres suites, publish y
empaquetado del contenido, y el APK.

## Cómo se corre la demo

Todo pasa por el guion único, `scripts/guion.sh` (`scripts/guion.sh` sin argumentos lista los subcomandos). Desde
esta carpeta, con `IP` = la IP con la que el dispositivo llega a esta máquina:

```bash
IP=203.0.113.10                                  # en el laboratorio: la IP de Tailscale de i7infra
scripts/guion.sh claves                             # par ECDSA de demostración (la privada no se versiona); ver abajo
scripts/guion.sh publicar v1                        # contenido 1.0.0, sin AOT, archivos planos
scripts/guion.sh definicion v1                      # encuesta-movilidad v1
scripts/guion.sh firmar                             # índice firmado, expires = ahora + 30 días
scripts/guion.sh semilla                            # lo que ofrece el índice → semilla/ (va dentro del APK)
FC_TLS_IPS=$IP scripts/guion.sh tls                 # CA y certificado HTTPS de demostración (claves/tls/, no se versiona)
scripts/guion.sh servir                             # receptor didáctico en https://0.0.0.0:8099
FC_RID=android-x64 FC_URL_CANAL=https://$IP:8099 scripts/guion.sh apk   # android-arm64 para un teléfono
export FC_ADB_CONTENEDOR=adb-contenedor           # sólo si adb vive en un contenedor
scripts/guion.sh dispositivo -H <host> -P <puerto> -s <serie>   # comprueba WebView ≥ 94
scripts/guion.sh instalar    -H <host> -P <puerto> -s <serie>
```

**Las claves en un clon nuevo.** El clon trae `claves/publica-demo.pem` y `semilla/` (firmada con su privada), pero no
la privada. `guion.sh claves` se niega a pisar la pública sin `--forzar`. Con `claves --forzar` se genera un par nuevo,
y entonces hay que rehacer `publicar v1`, `definicion v1`, `firmar` y `semilla` **antes** de compilar el APK: si no, el
cascarón descarta la semilla («semilla: firma rechazada») y arranca sin contenido. La compuerta lo frena:
`Gate.Tests` verifica `semilla/indice.json.sig` con `claves/publica-demo.pem` (`SemillaTests`). `guion.sh demo` hace
todo eso solo.

**Con el teléfono de medida (desvío de P11):** en lugar de `FC_URL_CANAL`, `scripts/guion.sh tunel` después de
`servir`, y `scripts/guion.sh apk` toma la URL del túnel. Al terminar, `scripts/guion.sh parar` baja el túnel.

Después, el recorrido v1 → v2 (lo mismo que hace `scripts/guion.sh demo`, que pide un paso manual):

| Paso | Comando | Qué tiene que pasar |
| --- | --- | --- |
| 1 | abrir la app | instala la semilla (contenido 1.0.0, definición v1) y la muestra |
| 2 | `parar`; tomar una encuesta en la app | queda «Por enviar: 1» en el outbox, con la definición v1 |
| 3 | `publicar v2`, `definicion v2`, `firmar`, `servir`; abrir en frío | se sigue viendo 1.0.0 (N = 0); el outbox se envía; v2 se baja a staging |
| 4 | abrir en frío | se activa 1.1.0 antes de mostrar (cabecera azul, «Novedades»); la encuesta nueva usa v2 |
| 5 | `respuestas` | la respuesta tomada con v1 figura aceptada con `versionDefinicion: 1` (P13) |
| 6 | `firma-invalida`; abrir en frío; `firmar` | `Rechazado FirmaInvalida`, sigue 1.1.0 |
| 7 | `publicar sin-listo`, `firmar`; abrir en frío cuatro veces | 1.2.0 se baja; se activa y empieza a cargar sin avisar «Listo»; se reintenta; en el cuarto arranque vuelve a 1.1.0 y queda como fallida |

**Cuándo una versión cuenta como fallida (P2, rollback por marca).** Al activar una versión, el cascarón le pone la
marca «pendiente», que el contenido borra con `Listo(version)`. La marca sola no prueba que el contenido falló: la
persona pudo cerrar la app mientras cargaba. Por eso el cascarón anota cada arranque en que el WebView **empezó a
cargar** la versión pendiente (pidió `_framework/dotnet.js`), y sólo cuando eso pasó
`Actualizador.CargasSinListoParaFallar` veces (2) sin `Listo`, el arranque siguiente vuelve a `previous` y anota la
versión como fallida. Un arranque cortado antes de cargar no cuenta. El 2 es criterio de la demo, no un valor de
producto del folio 036. Mientras una versión espera su `Listo`, lo que haya en `staging/` no se activa.

Al terminar: `scripts/guion.sh parar` y, si se quiere, `scripts/guion.sh limpiar` (borra `salida/`, `bin/`, `obj/`).

**Por qué hay una CA de demostración.** P2 pide «sólo HTTPS» y P11 excluye el túnel público. El receptor escucha en
HTTPS con un certificado firmado por una CA que genera `guion.sh tls` en la máquina de quien corre la demo. El APK de
**Debug** confía en esa CA porque va en los `debug-overrides` de la configuración de red de Android
(`src/FrameworkConcept.Shell/Platforms/Android/Red/ConCaDeDemo/`). Un APK de Release ignora ese bloque, y sin
`claves/tls/` el APK sale con `Red/SinCa/` (sólo las autoridades del sistema).

`scripts/guion.sh humo` prueba las herramientas y el receptor sin dispositivo (47 comprobaciones, con claves propias
en `salida/humo/claves/`: no toca `claves/` ni `semilla/`), y
`src/FrameworkConcept.Content/Testing/probar-en-navegador.sh` prueba el contenido en Chromium headless con el puente
simulado (40 comprobaciones).

## Qué hay en cada carpeta

| Carpeta | Qué es |
| --- | --- |
| `src/FrameworkConcept.Core/` | Núcleo sin MAUI. `Domain/` y `Application/` portados del caso base (ver `MAPA-CASO-BASE.md`); `Contracts/` con los tipos del contrato; `Update/` con las interfaces del actualizador |
| `src/FrameworkConcept.Shell/` | El cascarón: app MAUI para Android con sus pantallas XAML, el actualizador, el servidor de contenido y el puente |
| `src/FrameworkConcept.Content/` | El contenido: Blazor WebAssembly **sin AOT** |
| `tools/FrameworkConcept.Tools/` | Herramientas: claves de demostración, empaquetar, firmar el índice, publicar una definición, retirar, regenerar `contrato.json` |
| `tools/FrameworkConcept.Receptor/` | «Receptor didáctico»: un doble de prueba que sirve el canal y recibe las respuestas |
| `definiciones/encuesta-movilidad/` | La encuesta de tres pasos del caso base como datos: `v1.json`, `v2.json` y su `esquema.json` |
| `versionado/` | Fuentes únicas de versión: cascarón, puente, contenido, y la fase (`diseño`) |
| `semilla/` | Contenido y definición que el cascarón trae embebidos (los genera el guion) |
| `claves/` | Sólo la clave **pública** de demostración; la privada no se versiona |
| `tests/` | `Core.Tests` (dominio y contrato), `Update.Tests` (actualizador), `Gate.Tests` (compuerta) |
| `scripts/` | `contenedor.sh` e `imagen.sh` (el contenedor de compilación), `guion.sh` (el guion único) y `ci-local.sh` (la compuerta en local) |
| `contenedor/` | La receta del contenedor de compilación (`Dockerfile`) |
| `etapas/` | Los parches de rojo provocado de P8: M-13, un aviso promovido a error y un error de compilación del cascarón |
| `infra/keycloak/` | Emisor OIDC de ensayo (Keycloak en contenedor, realm `fc-demo`) para el modo con cuenta del incremento de red y autenticación (folio 038) |

## Dónde está cada decisión en el código

Las decisiones del dictamen (P1–P13) se marcan en el código con un comentario `// P<n>` donde no son obvias. Los
puntos de entrada:

| Decisión | Dónde mirar |
| --- | --- |
| P1 — pantallas sin AOT; encuestas como datos | `src/FrameworkConcept.Content/FrameworkConcept.Content.csproj`; `Contracts/Encuestas/DefinicionEncuesta.cs`; `definiciones/` |
| P2 — índice firmado por canal y mayor; expires; retiradas; claves | `Contracts/Canal/`; `Update/` |
| P3 — puente `Call(sobre)`; contrato = tabla + catálogo + transporte; rango con techo | `Contracts/Puente/`; `Contracts/RangoDeVersion.cs`; `Contracts/contrato.json` |
| P7 — líneas de versión | `versionado/`; `Contracts/VersionSemantica.cs` |
| P12 — capacidades | `Contracts/Puente/CatalogoDeCapacidades.cs`; `Contracts/Contenido/ManifiestoContenido.cs` |
| P13 — respuesta con su definición; evolución compatible | `Domain/Entities/RespuestaDeEncuesta.cs`; `Contracts/Encuestas/`; `definiciones/encuesta-movilidad/` |
| Valores de producto (folio 036) | `Contracts/ParametrosDeProducto.cs` |
| P4, borrador §6–§7 — red combinada, política de acciones y sesión (folio 038) | `DISENO-RED-Y-AUTENTICACION.md`; `Core/Red/`, `Core/Auth/`; `Shell/Red/`, `Shell/Auth/`; `Content/Red/`, `Content/Auth/` |

## Estado de construcción

### Tramo A1 — núcleo (2026-10-25)

**Hecho.**

- Layout completo: los ocho proyectos (`Core`, `Shell`, `Content`, `Tools`, `Receptor` y las tres suites),
  `FrameworkConcept.Demo.sln` (formato `.sln`) y `FrameworkConcept.Demo.SinMaui.slnf` (todo menos `Shell`),
  `global.json` (SDK 10.0.400, `rollForward: latestPatch`; sin `workloadVersion`, ver abajo),
  `Directory.Build.props` (`LangVersion`, `Nullable`, avisos como error con `CI=true`, `MauiVersion` 10.0.20,
  versiones de paquetes como propiedades `$(VersionXxx)`), `versionado/`, `.gitignore`.
- `Core/Domain` y `Core/Application` portados del caso base, archivo por archivo, en `MAPA-CASO-BASE.md`. Cuatro
  archivos adaptados por P13.2 o por comentarios; el resto, tal cual.
- `Core/Contracts`: sobre JSON-RPC 2.0 + `v`, tabla de métodos (11 métodos, 2 eventos), catálogo (`device.info`,
  `network.status`, con versión por entrada), transporte, instantánea `contrato.json` (generada con
  `FrameworkConcept.Tools contrato`), índice del canal (`expires`, `retiradas`, `keyId`, entradas `contenido` y
  `definicion` con `size` y `sha256`, retiros de definiciones con su aviso), firma ES256 y claves, manifiesto del
  contenido (`compatibleConPuente`, `capabilities {requires, optional}` con rango, motor mínimo), definición de
  encuesta y envío de respuesta, `VersionSemantica` (SemVer 2.0.0) y `RangoDeVersion` (forma canónica con techo).
- `Core/Update`: sólo interfaces y resultados (`IActualizador`, `IAlmacenDeContenido`, `IReloj`, `IClienteDeCanal`,
  `IVerificadorDeFirma`, `IEntornoDelCascaron`, `IPredicadoDeCompatibilidad`, `IAlmacenDeDefiniciones`).
- `definiciones/encuesta-movilidad/`: `v1.json` (la encuesta del caso base), `v2.json` (corrige una etiqueta y suma
  una pregunta opcional) y `esquema.json` (JSON Schema 2020-12).
- `tests/FrameworkConcept.Core.Tests`: 65 casos, adaptados del caso base y nuevos.
- `Shell`, `Content`, `Tools`, `Receptor`, `Update.Tests` y `Gate.Tests`: esqueletos que compilan, para que el tramo
  A2 los complete.
- `scripts/contenedor.sh`.

**Verificado** (en contenedor `lab-e2e-maui-dev:net10`, SDK 10.0.400, el 2026-10-25):

```text
$ scripts/contenedor.sh env CI=true dotnet build FrameworkConcept.Demo.SinMaui.slnf -c Release
Compilación correcta.
    0 Advertencia(s)
    0 Errores

$ scripts/contenedor.sh dotnet test tests/FrameworkConcept.Core.Tests -c Release
Correctas! - Con error: 0, Superado: 65, Omitido: 0, Total: 65, Duración: 364 ms - FrameworkConcept.Core.Tests.dll (net10.0)

Rojo provocado: v1.json con "maximo": 111 en la edad
Con error! - Con error: 2, Superado: 63, Omitido: 0, Total: 65   (V1CoincideConElDominio, ElValidadorEquivaleAlCasoBase)

$ scripts/contenedor.sh dotnet build src/FrameworkConcept.Shell -c Debug -p:RuntimeIdentifier=android-arm64 -p:AndroidPackageFormat=apk
Se ha restaurado /work/src/FrameworkConcept.Shell/FrameworkConcept.Shell.csproj (en 3,02 s).
Compilación correcta.  0 Advertencia(s)  0 Errores   → com.frameworkconcept.demo-Signed.apk
```

`v1.json` y `v2.json` validan contra `esquema.json` (jsonschema de Python, Draft 2020-12, en contenedor
`python:3.12-slim`), y el esquema rechaza cuatro rojos provocados: piso sin techo, tipo desconocido, opción sin
opciones y versión 0.

**NO VERIFICADO en A1.** El APK no se instaló en ningún dispositivo. El contenido no se publicó ni se empaquetó.
`workloadVersion` 10.0.400.1 en `global.json` se aceptó en el contenedor, pero en el runner (SDK 10.0.403) rompió la compuerta con MSB4242 «Workload version 10.0.400.1 … was not found» (A5, run 40432670449): se quitó del `global.json`. El contenedor sigue instalando ese workload set; MAUI lo fija `MauiVersion`.

**Pendiente para los tramos siguientes.**

- A2: actualizador, servidor de contenido, puente, catálogo, SQLite + outbox y las pantallas XAML del cascarón;
  páginas Razor, renderizador de encuestas y cliente del puente del contenido; las herramientas (claves, empaquetar,
  firmar, publicar, retirar) y el receptor; las suites `Update.Tests` y `Gate.Tests` con sus rojos provocados;
  `claves/publica-demo.pem`.
- A3: `scripts/guion.sh`, `scripts/ci-local.sh`, el workflow y la corrida en el teléfono.
- ~~P11 pide una receta de contenedor versionada en `Demo/`.~~ Resuelto en las correcciones: `contenedor/Dockerfile`.
- Desvío de nombre: el dictamen (P7) ubica la fase en `versionado/rutas.json`; el layout de la demo la nombra
  `versionado/fase.json`.

### Tramo A2 — actualizador, cascarón, contenido, herramientas y compuerta (2026-10-26)

**Hecho.** El actualizador (`Core/Update/`) con su suite; el cascarón MAUI (`Shell/`); el contenido Blazor sin AOT
(`Content/`); las herramientas, el receptor y `scripts/guion.sh`; la compuerta (`Gate.Tests`, `ci-local.sh` y el
workflow). Los detalles y sus rojos provocados están en el expediente (E02-Demo-Integracion).

### Tramo A3 — integración (2026-10-26)

**Hecho.**

- Solución completa (`FrameworkConcept.Demo.sln`, Debug, `CI=true`): 0 avisos, 0 errores.
- `scripts/ci-local.sh` en verde: Core.Tests 65/65, Update.Tests 96/96, Gate.Tests 46/46, contenido empaquetado,
  APK arm64 con 0 avisos.
- `scripts/guion.sh humo`: 46 OK, 0 MAL. Contenido en Chromium headless con el puente simulado: 40 de 40.
- Recorrido completo en un emulador Android 13 x86_64 con WebView 155, que llega al receptor por la IP de Tailscale
  de la máquina del receptor y en HTTPS: semilla v1, encuesta sin red en el outbox, v2 al reabrir en frío, la
  respuesta v1 aceptada con `versionDefinicion: 1` al volver el receptor, índice con firma inválida rechazado y
  contenido sin «Listo» que vuelve a `previous`.

**Arreglos de integración.**

- El puente manda el sobre en ASCII, con los caracteres no ASCII escapados como `\uXXXX` en JSON. En Android,
  `hybridwebview.js` lleva el cuerpo en un encabezado HTTP, que no admite UTF-8, y en el moto g42 «¿Qué» llegaba
  como «�Qu�» (`Content/wwwroot/js/puente.js`).
- Variante defectuosa `-p:ContenidoSinListo=true` del contenido y `guion.sh publicar sin-listo`, para la demo del
  rollback por marca.
- HTTPS sin túnel: `guion.sh tls`, el receptor en HTTPS (Kestrel lee el PEM por configuración), la configuración de
  red de Android con la CA de demostración sólo en Debug, `guion.sh respuestas`, y la prueba de humo aislada del
  certificado.
- `guion.sh demo` rehecho como recorrido completo y sin túnel por defecto. El logcat de `arrancar` ahora filtra la
  etiqueta `FC`, que antes no capturaba.

**NO VERIFICADO.**

- La corrida en el moto g42 (el teléfono de medida de P1) y, por eso, el umbral de primer render (TPR ≤ 5 000 ms)
  en la condición declarada. Las cifras del emulador no sirven para el umbral. Un intento anterior en el g42 midió
  unos 8,2 s de TPR en arranques en frío con la semilla (ver «Correcciones»: la semilla salió del camino crítico).
- `guion.sh demo` corrido de punta a punta como un solo comando. Sus pasos se corrieron uno por uno.
- La corrida del workflow en el runner.
- Un sobre con caracteres no ASCII en el dispositivo después del arreglo del puente.

### Correcciones de la revisión adversarial (2026-10-26)

**Hecho.**

- Receta del contenedor versionada: `contenedor/Dockerfile` (base `mcr.microsoft.com/dotnet/sdk:10.0.400-noble`
  fijada por digest, workload set 10.0.400.1). `scripts/contenedor.sh` y `guion.sh` ya no dependen de una imagen
  local: `scripts/imagen.sh` la construye si falta.
- La semilla salió del camino crítico del arranque (P1): se lee y se verifica sólo en el primer arranque o cuando
  cambia la instalación del APK (`IFuenteDeSemilla`, `PunteroDeContenido.SemillaRevisada`).
- Rollback por marca con evidencia de carga (P2): ver «Cuándo una versión cuenta como fallida».
- Claves: `claves` no pisa una pública existente sin `--forzar`; `humo` usa sus propias claves; `Gate.Tests`
  verifica la semilla contra la pública versionada.
- Rojos provocados de P8 versionados en `etapas/` (M-13, aviso promovido a error, error de compilación del cascarón).
- Plantilla de PR del repositorio (`.github/pull_request_template.md`) con los dos campos obligatorios de P6.6.
- `guion.sh tunel` habla HTTPS con el receptor cuando hay `claves/tls/`; el desvío de P11 con el g42 queda declarado.

**Verificado** (contenedor de la receta, 2026-10-26): `ci-local.sh` en verde (0/0, Core.Tests 65/65, Update.Tests
102/102, Gate.Tests 47/47, contenido empaquetado, APK arm64 0/0); `guion.sh humo` 47 OK sin tocar
`claves/publica-demo.pem`; los rojos de `etapas/` (M-13: 6 pruebas de `Core.Tests` en rojo y `ci-local.sh` con
rc 1; aviso: 1 error con `CI=true`, 1 aviso sin `CI`; cascarón: CS0029). En el emulador `fc-android-emu-hpdev`: la
semilla ya no se carga después del primer arranque; dos arranques cortados antes de cargar no cuentan; dos cargas
sin «Listo» llevan a `previous`. Los detalles, en el expediente (E02-Demo-Integracion, `correcciones.md`).

**NO VERIFICADO.** El TPR en el g42 después de la corrección (reservado para otra sesión). Con el desglose del
intento anterior, la semilla costaba unos 0,7 s de los 8,2 s: es probable que el umbral siga roto. El PR en rojo con
M-13 en el runner (tramo A5).

### Incremento A4 — red y autenticación con el sistema operativo (folio 038)

Diseño completo en `DISENO-RED-Y-AUTENTICACION.md`:

- estado de red combinado, con chequeo de salud propio y reconexión automática;
- política de acciones evaluada por el cascarón (`SoloLocal`, `Encolable`, `SoloEnLinea`);
- sesión OIDC (code + PKCE en el navegador del sistema) con desbloqueo por la seguridad del teléfono;
- outbox con dueño;
- un Keycloak de ensayo.

Contrato del puente **1.1.0**. Todo el cambio es MENOR:

- métodos nuevos: `actions.policy`, `actions.check`, `outbox.sendNow` y `auth.*`;
- eventos nuevos: `actions.changed` y `auth.changed`;
- campos opcionales `estado` y `motivo` en `network.status` y `network.changed`.

Catálogo: `network.status` 1.1.0 y `auth.session` 1.0.0, nueva. El manifiesto declara `USE_BIOMETRIC` y `<queries>`.

#### Cómo se levanta con cuenta (emisor de ensayo)

Desde esta carpeta, después de `guion.sh tls` (la CA de demostración firma también el certificado del emisor):

```bash
IP=203.0.113.10
EMISOR=https://$IP:8443/realms/fc-demo
infra/keycloak/keycloak.sh certificado              # claves/tls/keycloak.{crt,key}, no se versiona
infra/keycloak/keycloak.sh levantar                 # contenedor fc-keycloak, realm fc-demo importado al arrancar
infra/keycloak/keycloak.sh probar                   # code + PKCE por curl; rechaza el canje sin verificador
FC_OIDC_EMISOR=$EMISOR scripts/guion.sh servir      # el receptor exige un JWT válido del emisor
FC_RID=android-x64 FC_URL_CANAL=https://$IP:8099 FC_OIDC_EMISOR=$EMISOR scripts/guion.sh apk
```

Sin `FC_OIDC_EMISOR` el APK queda en modo Dispositivo (sin cuenta, como en A3) y el receptor no pide token.

**Credenciales de PRUEBA, sólo para este emisor de ensayo.** Son públicas a propósito y no sirven para nada más:

| Usuario | Clave | Para qué |
| --- | --- | --- |
| `encuestador` | `demo-encuestador` | dueño de las encuestas del recorrido |
| `supervisora` | `demo-supervisora` | segundo dueño (P4: una cuenta no ve ni envía filas ajenas) |

El usuario `admin` de Keycloak tiene una clave aleatoria en `salida/keycloak/admin.txt`, que no se versiona.

Al terminar: `infra/keycloak/keycloak.sh parar` (el realm se reimporta al levantar) y `scripts/guion.sh parar`.

#### Preparar el dispositivo

- **CA de demostración en el navegador del sistema.** El formulario de Keycloak se abre en Chrome (Custom Tab), que no
  lee la configuración de red del APK. Hay que instalar `claves/tls/ca_demo_tls.crt` como CA de usuario, desde los Ajustes
  de seguridad del teléfono (la ruta exacta cambia según la versión; no se verificó por pantalla). En el emulador de la integración se copió a
  `/data/misc/user/0/cacerts-added/` con `adb root`.
- **Bloqueo de pantalla.** Sin bloqueo, el desbloqueo con la seguridad del teléfono no se ofrece y la app dice por qué
  (D-03 = T1). En un emulador: `adb shell locksettings set-pin 1234`. Después se enrola una huella en Ajustes,
  apoyando el dedo simulado con `adb emu finger touch 1` cuando lo pide.

#### Cómo se prueba el desbloqueo

1. Abrir la app → «Ingresar con tu cuenta» → formulario de Keycloak en el navegador del sistema → vuelve a la app
   como `encuestador` y pregunta «¿Usar la seguridad del teléfono para entrar?» → «Sí, usarla» → diálogo del sistema
   (huella o PIN).
2. Cerrar la app (`adb shell am force-stop com.frameworkconcept.demo`) y abrirla: aparece la pantalla «Desbloquear»
   con el diálogo del sistema.
   - Con huella: `adb emu finger touch 1`. Ese comando va a la consola del emulador; si el servidor adb es remoto,
     se corre en la máquina del emulador.
   - Con PIN: «Use PIN» y `adb shell input text 1234` + Enter.
   - En los dos casos entra sin pedir cuenta.
3. «Cerrar sesión» pide confirmación y avisa que lo pendiente se conserva. Después, la app vuelve a pedir cuenta.
4. Con el desbloqueo activado, quitar el bloqueo (`adb shell locksettings clear --old 1234`) y reabrir: la sesión
   queda «Revocada» («La seguridad del teléfono cambió…») y hay que ingresar con cuenta.
5. La sesión del emisor vence a los 30 min sin uso: la app abre «Vencida», con la banda de reingreso. Se sigue
   trabajando local.

#### Cómo se prueba la red

| Acción | Qué se ve |
| --- | --- |
| `adb shell cmd connectivity airplane-mode enable` | Banda «Sin conexión». Una encuesta registrada queda pendiente («Por enviar: 1»). «Enviar ahora» se deshabilita con su motivo |
| `scripts/guion.sh parar` (o `docker stop fc-demo-receptor`) y `airplane-mode disable` | «Sin acceso al servidor. Hay red pero el servidor no responde. Reintentando…» |
| `FC_OIDC_EMISOR=$EMISOR scripts/guion.sh servir`, sin tocar la app | Vuelve sola a «En línea» y el outbox se vacía. `scripts/guion.sh respuestas` muestra cada respuesta con su `dueno` |

#### Estado

**Verificado** (2026-10-26, ensayo E03 en `Lab-Documentos.Documentacion/.../OUTPUTs/Ensayos/E03-Red-Y-Autenticacion/`):

- `ci-local.sh` en verde con `rc=0`: Core 202, Update 102, Gate 52 y el APK;
- el contenido headless, 76/76;
- el recorrido completo en el emulador `fc-android-emu-hpdev`: primer ingreso, huella, PIN, sin conexión, sin
  servidor, reconexión automática con el dueño registrado, cierre de sesión que conserva pendientes, y clave
  invalidada al quitar el bloqueo.

**Hallazgos abiertos para la mesa** (detalle en el INFORME de E03):

- con `BIOMETRIC_STRONG | DEVICE_CREDENTIAL`, una huella nueva no invalida la clave (A10, decisión de diseño);
- cerrar sesión sin red deja viva la sesión SSO del emisor, y el reingreso no pide clave;
- un ANR al volver del navegador, visto una vez;
- el ingreso queda colgado en «Ingresando» si la red se cae mientras se descubre el emisor (`WebException` fuera del
  `catch` de `SesionDelUsuario.IngresarAsync`).

**NO VERIFICADO:** `EnLineaMedida` en un dispositivo (el emulador no tiene datos móviles), la regresión R0 sin cuenta
en el emulador, y todo en un teléfono físico.
