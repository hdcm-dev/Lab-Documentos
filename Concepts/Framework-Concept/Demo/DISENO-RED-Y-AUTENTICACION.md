# Diseño del incremento: red y autenticación con el sistema operativo

| Campo | Valor |
| --- | --- |
| Pedido | Folio 038 del expediente 0001: autenticación con los mecanismos del sistema operativo; manejo de red (detección de corte, aviso al usuario, reconexión automática) con bloqueo de las acciones que requieren internet |
| Diseño que manda | P4 del dictamen (folio 035) y el borrador del PO `../Framework-APP.md` §4, §6 y §7. Respeta P3 (contrato del puente), P5 (outbox), P12 (catálogo) |
| Efecto del folio 038 | Sustituye, **para la demo**, la «Decisión para la demo» de P4 (sin IdP). El diseño del framework de P4 entra entero |
| Estado | **Esqueleto.** Contrato 1.1.0, catálogo, tipos, interfaces, clases vacías cableadas, Keycloak de ensayo. La lógica la escriben dos constructores en paralelo (§8) |
| Fecha | 2026-10-26 |

Las rutas de este documento son relativas a esta carpeta (`Demo/`). Cada decisión lleva su ancla entre corchetes:
**[P4]** es el dictamen del folio 035; **[§6]** es el borrador del PO; **[038]** es el folio del pedido; **[arq]** es una
decisión de este documento que ninguna fuente fija, con su motivo.

## Índice

1. [Qué cambia y qué no](#1-qué-cambia-y-qué-no)
2. [Red combinada](#2-red-combinada)
3. [Sesión del usuario](#3-sesión-del-usuario)
4. [Política de acciones](#4-política-de-acciones)
5. [El receptor valida el token](#5-el-receptor-valida-el-token)
6. [Contrato del puente 1.1.0](#6-contrato-del-puente-110)
7. [Catálogo y manifiesto](#7-catálogo-y-manifiesto)
8. [Reparto entre los constructores](#8-reparto-entre-los-constructores)
9. [Emisor de ensayo: Keycloak en contenedor](#9-emisor-de-ensayo-keycloak-en-contenedor)
10. [Plan de pruebas](#10-plan-de-pruebas)
11. [Lo que no entra y lo que no está verificado](#11-lo-que-no-entra-y-lo-que-no-está-verificado)
12. [Correcciones del ensayo E03](#12-correcciones-del-ensayo-e03)

## 1. Qué cambia y qué no

| Tema | Antes (A1–A3) | Después del incremento |
| --- | --- | --- |
| Red | `Connectivity` y evento `network.changed {conectado}` | Estado combinado `SinRed / RedSinServidor / EnLinea / EnLineaMedida` con chequeo de salud propio, reconexión automática y aviso permanente **[§6, 038]** |
| Acciones | Todas se ejecutan; la encuesta va al outbox | Cada acción declarada con su clase `SoloLocal / Encolable / SoloEnLinea`; el **cascarón** evalúa; el botón se deshabilita y dice por qué **[§7, 038]** |
| Identidad | Sesión por dispositivo (id de instalación) | Dos modos fijados al compilar: **Dispositivo** (como antes) u **Oidc** (code + PKCE en el navegador del sistema, desbloqueo con la seguridad del teléfono) **[P4, 038]** |
| Outbox | Dueño = id de instalación; sale todo | Sale sólo con la identidad de su dueño y sesión válida; cerrar o vencer la sesión no borra pendientes **[P4, P5]** |
| Receptor | Acepta sin token | Con `--oidc-emisor`, exige un JWT del emisor validado contra su JWKS **[P4; arq: el receptor es el «servidor» del ensayo]** |
| Puente | 1.0.0 | **1.1.0**: todo es MENOR (métodos, eventos y entrada de catálogo nuevos; campos opcionales nuevos) **[P3]** |

Lo que **no** cambia: el esquema de la base (C-1, `EsquemaDelCascaron.Version = 1`), el transporte del puente, el
actualizador, el canal y su firma. **[arq]** Los datos de la sesión van a la tabla de configuración (§3.6): un cambio
de esquema obligaría a decidir sobre el sobre de C-1, y el incremento no lo necesita.

**Por qué dos modos.** **[arq]** Sin `-p:FcOidcAutoridad` el APK se comporta como en A3: `guion.sh demo`, la prueba de
humo y el recorrido headless siguen valiendo sin Keycloak. El modo Oidc es el del folio 038. El catálogo es el mismo en
los dos (es una constante del binario, P12): en modo Dispositivo `auth.status` informa `modo: dispositivo` y el
contenido no ofrece ingresar.

## 2. Red combinada

### 2.1 Estados **[§6]**

| Estado | Significado | Aviso permanente (texto orientativo) |
| --- | --- | --- |
| `SinRed` | El sistema dice que no hay conexión | «Sin conexión. Lo que hagas se guarda y se envía cuando vuelva.» |
| `RedSinServidor` | Hay red pero el servidor no responde (portal cautivo, servidor caído, llamadas que fallan) | «Hay red pero el servidor no responde. Reintentando…» |
| `EnLinea` | El chequeo de salud respondió | (sin aviso) |
| `EnLineaMedida` | En línea por datos móviles | «Datos móviles» (chip discreto; ninguna acción de la demo lo limita) |

Tipo: `Core/Red/EstadoDeRed.cs` (`EstadoDeRed`, `EstadoDeRedCombinado`). En el puente viaja en camelCase
(`sinRed`, `redSinServidor`, `enLinea`, `enLineaMedida`), por `JsonDelContrato`.

`conectado` sigue significando lo que significaba en el puente 1.0 (lo que dice el sistema, `NetworkAccess.Internet`).
**[P3]** Cambiarle el sentido sería un cambio de contrato que la instantánea no ve; el estado nuevo va en el campo
nuevo `estado`.

### 2.2 Transiciones **[§6; arq donde se indica]**

Función pura `TransicionesDeRed.Siguiente(actual, señal, perfilMedido)` (`Core/Red/TransicionesDeRed.cs`).
`perfilMedido` = el perfil vigente es celular sin WiFi ni Ethernet (`Connectivity.ConnectionProfiles`).

| Actual | Señal | Siguiente | Ancla |
| --- | --- | --- | --- |
| cualquiera | `SistemaSinRed` | `SinRed` | §6 |
| `SinRed` | `SistemaConRed` | `RedSinServidor` (y chequeo de salud inmediato) | §6 |
| `SinRed` | `SaludOk`, `SaludFalla`, `LlamadaFallida`, perfil | `SinRed` (respuesta vieja: se ignora) | arq |
| `RedSinServidor` | `SaludOk` | `EnLineaMedida` si `perfilMedido`, si no `EnLinea` | §6 |
| `RedSinServidor` | `SaludFalla`, `LlamadaFallida`, `SistemaConRed` | `RedSinServidor` | §6 |
| `EnLinea` | `SaludFalla`, `LlamadaFallida` | `RedSinServidor` | §6 |
| `EnLinea` | `PerfilMedido` | `EnLineaMedida` | §6 |
| `EnLineaMedida` | `PerfilNoMedido` | `EnLinea` | §6 |
| `EnLineaMedida` | `SaludFalla`, `LlamadaFallida` | `RedSinServidor` | arq: el diagrama de §6 no la dibuja; sin ella, datos móviles con el servidor caído quedaría «en línea» |
| `EnLinea`/`EnLineaMedida` | `SaludOk`, `SistemaConRed` | se recalcula con `perfilMedido` | arq |

`LlamadaFallida` = excepción de red, timeout o 5xx de una llamada del framework. Un 4xx **no** cuenta: el servidor
respondió. **[§6 «marca RedSinServidor si fallan las llamadas»]**

### 2.3 Chequeo de salud y reconexión automática **[§6, 038]**

- **Qué:** `GET {FcReceptorUrl}/salud` (el receptor ya lo sirve), sin token, timeout **3 s**
  (`ParametrosDeRed.TimeoutDeSalud`). 2xx = `SaludOk`; otra cosa = `SaludFalla`.
- **Caché:** un resultado vale **10 s** (`CacheDeSalud`): `Reevaluar()` dentro de ese plazo no vuelve a preguntar.
- **Cuándo:** al pasar a «hay red»; al volver la app al frente (`App.cs` → `IMonitorDeRed.Reevaluar`); en
  `RedSinServidor`, reintentos con backoff **5 s → 10 s → 20 s → 40 s → 60 s** y sigue cada 60 s sin límite
  (`ReintentoInicial`, `ReintentoMaximo`); en línea, uno de confirmación cada **60 s** (`ChequeoEnLinea`).
- **Reconexión:** al entrar en `EnLinea`/`EnLineaMedida` el sincronizador se despierta solo (ya cableado en
  `SincronizadorDelOutbox.Iniciar`) y vacía el outbox **[§7 «al volver EnLinea»]**.
- **Cliente:** un `HttpClient` propio del monitor, **sin** pasar por `ManejadorDeRed` (si no, la falla del chequeo se
  contaría dos veces) **[arq]**.

Todos los valores son criterio de la demo, no valores de producto del folio 036.

### 2.4 `DelegatingHandler` y registro **[§6 «Actividad»]**

`FabricaHttp` (`Shell/FabricaHttp.cs`, ya cableada) arma todo `HttpClient` del framework:

```text
receptor:  ManejadorDeSesion (Bearer, 401)  →  ManejadorDeRed  →  HttpClientHandler (nativo de Android)
canal:                                         ManejadorDeRed  →  HttpClientHandler
```

`ManejadorDeRed` (`Shell/Red/ManejadorDeRed.cs`) registra método, URL, código, duración y bytes en un **registro
circular en memoria de 200 entradas** (`ParametrosDeRed.RegistroDeLlamadas`) que se ve en la pantalla de Diagnóstico,
e informa al monitor `InformarLlamadaFallida` / `InformarLlamadaExitosa`. **[arq]** El borrador §6 pide la tabla en
SQLite; en la demo va en memoria para no cambiar el esquema (C-1). Reintentos con Polly: **no** en la demo; el
sincronizador ya tiene backoff propio (P5).

### 2.5 Eventos al contenido

| Evento | Cuándo | Parámetros |
| --- | --- | --- |
| `network.changed` | Cambia el estado combinado | `conectado`, `estado` (opcional desde 1.1), `motivo` (opcional) |
| `actions.changed` | Cambia la evaluación de alguna acción (por red o por sesión) | `acciones: EvaluacionDeAccion[]` |

El emisor sólo envía después del `handshake` (como en A3); el contenido pide `network.status` y `actions.policy` al
arrancar, así que no pierde nada.

### 2.6 Aviso permanente **[038 «notificación al usuario que no hay red»]**

`Content/Red/AvisoDeRed.razor`, montado en `MainLayout` arriba de toda página (ya montado, vacío). Una banda fija
mientras el estado no sea `EnLinea`, con el texto de §2.1 y el `motivo` del cascarón; desaparece sola al reconectar.
No es un toast: el folio 038 pide que se vea **mientras dure**. `data-testid="aviso-red"` y
`data-estado="{estado}"`.

## 3. Sesión del usuario

### 3.1 Modelo **[P4, §4]**

- **Entrada local y sesión con el servidor, separadas.** Entrar es el desbloqueo local; la sesión con el servidor es el
  token. La renovación corre en segundo plano y no bloquea la entrada. **[P4]**
- **Sin período de gracia local:** sin red se opera sin límite; al vencer la sesión se vuelve a ingresar con red antes
  de sincronizar. **[P4]**
- **La biometría no es el login:** es la llave que abre el refresh token guardado. La identidad la da el emisor. **[§4]**
- **El refresh token nunca cruza el puente.** Al contenido sólo le llega `SesionInformada` (modo, estado, usuario,
  desbloqueo, motivo). **[P3]** La compuerta lo prueba (`SinTokensEnElPuenteTests`).

### 3.2 Máquina de estados

Función pura `TransicionesDeSesion.Siguiente(actual, evento, antesDeIngresar)` (`Core/Auth/Sesion.cs`).

```text
                 IngresoIniciado            IngresoExitoso
  SinCuenta ───────────────────► Ingresando ──────────────► Activa ◄──────────────┐
      ▲                             │ IngresoFallido           │  ▲               │
      │                             ▼ (vuelve al previo)       │  │ Desbloqueo     │
      │ Cierre                (SinCuenta/Vencida/Revocada)     │  │ Exitoso        │
      │ (desde cualquiera)                          Bloquear   ▼  │                │
      │                                                    Bloqueada ── Desbloqueo │
      │                                                      │        Cancelado ──┘ (se queda)
      │                    SesionVencida (refresh expiró)    │ ClaveInvalidada
      │            Activa ───────────────────────► Vencida   ▼
      │            Activa ── RenovacionRechazada ─► Revocada ◄┘
      │                                              │
      └──── Vencida / Revocada ── IngresoIniciado ───┴──► Ingresando  (reingreso)
```

| Actual | Evento | Siguiente | Ancla |
| --- | --- | --- | --- |
| `SinCuenta`, `Vencida`, `Revocada` | `IngresoIniciado` | `Ingresando` | P4 primer ingreso / §4 «si falla o se revoca: volver al login» |
| `Ingresando` | `IngresoExitoso` | `Activa` | P4 |
| `Ingresando` | `IngresoFallido` | `antesDeIngresar` | arq: cerrar el navegador no debe perder la identidad previa |
| `Activa` | `Bloquear` | `Bloqueada` | §4 «re-pedir la seguridad del teléfono…» |
| `Bloqueada` | `DesbloqueoExitoso` | `Activa` | P4 |
| `Bloqueada` | `DesbloqueoCancelado` | `Bloqueada` | P4 |
| `Bloqueada` | `ClaveInvalidada` | `SinCuenta` | §4 «si se revoca»; **corrector E03**: no hubo prueba local, la entrada no se abre (§12) |
| `Activa` | `ClaveInvalidada` | `Revocada` | §4 «si se revoca» |
| `Activa` | `RenovacionRechazada` | `Revocada` | §4 «si falla o se revoca» |
| `Activa` | `SesionVencida` | `Vencida` | P4 «al vencer la sesión…» |
| cualquiera menos `SinCuenta` | `Cierre` | `SinCuenta` | §4 «cerrar sesión = borrar tokens y la clave» |
| otra combinación | — | sin cambio | arq |

**Estado al arrancar en frío** (sin evento; lo calcula `SesionDelUsuario.IniciarAsync`, sin red):

| Hay identidad guardada | Desbloqueo activado | Estado inicial |
| --- | --- | --- |
| no | — | `SinCuenta` |
| sí | sí | `Bloqueada` |
| sí | no | `SinCuenta` **[corrector E03, §12]**: sin desbloqueo no hay prueba local; se pide la cuenta (antes `Vencida` con la entrada abierta). El refresh sólo vivía en memoria |

Con desbloqueo activado y el refresh sellado ya vencido, el arranque es igual `Bloqueada`: la prueba local abre la
entrada y recién después la sesión pasa a `Vencida` (corrector E03). Quitar el bloqueo de pantalla con el desbloqueo
activado lleva a `SinCuenta` (antes `Revocada`).

`Bloquear` se dispara sólo con el desbloqueo **activado**: al arrancar en frío y al volver de segundo plano después de
**5 min** (`ParametrosDeSesion.BloqueoTrasSegundoPlano`, criterio de la demo; `App.cs` ya llama a
`AlPasarASegundoPlano`/`AlVolverAlFrente`). Sin desbloqueo activado no hay nada que bloquear: la sesión sigue `Activa`
hasta que venza.

**Corrector E03:** `Vencida` y `Revocada` (entrada abierta) sólo se alcanzan **en el mismo proceso** después de una
prueba local (ingreso con la cuenta o desbloqueo). En frío nunca.

`Vencida` vs `Revocada`: **[arq]** se distinguen localmente. `Vencida` = pasó `refresh_expires_in` (lo informa
Keycloak) o el proceso arrancó sin refresh recuperable. `Revocada` = el emisor contestó `invalid_grant` antes del
vencimiento conocido, o la clave del Keystore quedó invalidada. Los dos llevan al reingreso; el motivo que se muestra
cambia.

### 3.3 Primer ingreso: OIDC code + PKCE en el navegador del sistema **[P4, RFC 8252, RFC 7636]**

1. **Descubrimiento:** `GET {autoridad}/.well-known/openid-configuration` (D-07 = I1: cualquier emisor, nada de rutas
   de Keycloak). Se guarda en memoria (`DescubrimientoOidc`).
2. **PKCE:** `code_verifier` de 32 bytes aleatorios en base64url (43 caracteres); `code_challenge` = base64url(SHA-256)
   con `S256`; `state` aleatorio (`FlujoOidc`).
3. **Navegador del sistema:** `WebAuthenticator.Default.AuthenticateAsync(urlDeAutorizacion, redirect)` (MAUI
   Essentials). Abre Custom Tabs si el navegador lo ofrece (por eso el `<queries>` del catálogo, §7) o el navegador por
   `ACTION_VIEW`. **Nunca** un WebView embebido (RFC 8252 §8.12). **Corrector E03:** la URL lleva `prompt=login`
   (OIDC Core §3.1.2.1): cada ingreso interactivo pide la clave aunque el emisor tenga una sesión SSO viva (§12, P0).
4. **Vuelta:** `com.frameworkconcept.demo://callback?code=…&state=…` llega a `Shell/Auth/CallbackDeAutenticacion.cs`
   (ya declarada con su `[IntentFilter]`). Se compara `state`.
5. **Canje:** `POST token_endpoint` con `grant_type=authorization_code`, `code`, `redirect_uri`, `client_id` y
   `code_verifier`, por un `HttpClient` del cascarón **sin** `ManejadorDeSesion` (no hay token todavía) y con
   `ManejadorDeRed`. Cliente público: sin secreto. **Corrector E03:** desde la vuelta del navegador, el canje y las
   escrituras corren fuera del hilo principal (`ConfigureAwait(false)`) y se registran sus tiempos (H-3, §12).
6. **Identidad:** `sub` y `preferred_username` de las claims del `id_token`. **[arq]** Sin validar la firma del
   id_token en el teléfono: llegó por TLS directo del token endpoint (OIDC Core 1.0 §3.1.3.7, punto 6). El que valida
   firmas es el receptor (§5).
7. Estado `Activa`. Si `DesbloqueoLocal = Disponible`, el contenido ofrece **«¿Usar la seguridad del teléfono para
   entrar?»** **[§4 paso 2]**.

`auth.signIn` es la acción `sesion.ingresar`, `SoloEnLinea` (§4): sin servidor, el botón dice por qué.

**La CA de demostración y el navegador.** **[arq; NO VERIFICADO]** El `HttpClient` del cascarón confía en la CA de
demostración por `network_security_config` (sólo Debug), pero **el navegador del sistema no lee esa configuración**.
Para que Chrome abra `https://203.0.113.10:8443` sin error de certificado, la CA se instala en el emulador como
**certificado de CA de usuario** (Ajustes → Seguridad → Cifrado y credenciales → Instalar un certificado → Certificado
de CA). Exige bloqueo de pantalla, que el recorrido configura igual (§10.4). Que Chrome confíe en CAs de usuario lo
tiene que comprobar el constructor de autenticación en el emulador; si no, BLOQUEO con la prueba.

### 3.4 Desbloqueo local con la seguridad del teléfono **[P4, D-03 = T1]**

`Shell/Auth/CofreDeSesion.cs`:

- **Clave:** AES-256-GCM en `AndroidKeyStore`, alias `fc.sesion.refresh`, con
  `setUserAuthenticationRequired(true)`, `setUserAuthenticationParameters(0, AUTH_BIOMETRIC_STRONG | AUTH_DEVICE_CREDENTIAL)`
  (timeout 0: cada uso pide autenticación) y `setInvalidatedByBiometricEnrollment(true)`. **[P4: `setUserAuthenticationParameters`, API ≥ 30]**
- **Sellar/abrir:** `android.hardware.biometrics.BiometricPrompt` (de la plataforma, sin paquete AndroidX nuevo) con
  `CryptoObject(Cipher)` y `setAllowedAuthenticators(BIOMETRIC_STRONG | DEVICE_CREDENTIAL)`: huella, rostro, PIN o patrón.
  **[P4, §4 «Por plataforma»]**
- **Cuándo se ofrece (D-03 = T1):** `Build.VERSION.SdkInt >= 30` **y** `KeyguardManager.IsDeviceSecure`. Si no,
  `DesbloqueoLocal = NoDisponible` con motivo («Este teléfono no tiene bloqueo de pantalla» / «Android 11 o más nuevo»),
  y el refresh no se persiste. **[P4 D-03]**
- **Invalidación:** `KeyPermanentlyInvalidatedException` (huella nueva, bloqueo quitado) o clave inexistente con texto
  sellado presente (restauración de copia de seguridad: `allowBackup=true`) → se borran clave y texto, evento
  `ClaveInvalidada` → `SinCuenta` desde `Bloqueada` (corrector E03). **[§4 «Reglas»]** **Apartamiento declarado del
  borrador §4:** enrolar una huella nueva **no** invalida la clave, porque P4 acepta PIN o patrón
  (`DEVICE_CREDENTIAL`) y Android sólo invalida por enrolamiento las claves exclusivamente biométricas (H-1, §12).
- **Cancelación** del BiometricPrompt → `CodigosDeError.AutenticacionCancelada` (-32006), la sesión sigue `Bloqueada`.
- **Después de abrir:** estado `Activa` de inmediato (la entrada no espera a la red) y renovación del access token en
  segundo plano. Sin red, se sigue trabajando; con `invalid_grant`, `Revocada`. **[P4]**

### 3.5 Outbox con dueño **[P4, P5]**

- **Dueño de una fila nueva:** `ISesionDelUsuario.ObtenerDuenoAsync()` (ya cableado en el despachador).
  Modo Oidc: `oidc:{sub}` (`PropiedadDelOutbox.DuenoOidc`). Modo Dispositivo: el id de instalación de A3.
  En `SinCuenta`/`Bloqueada`/`Ingresando` no hay entrada abierta y la acción no llega al puente (§4).
- **Qué sale:** `PropiedadDelOutbox.PuedeEnviarse(dueñoDeLaFila, dueñoActivo, sesión)` → sólo si la fila es del dueño
  activo **y** la sesión es válida (Oidc: `Activa`; Dispositivo: siempre). El sincronizador ya filtra con
  `ISesionDelUsuario.PuedeEnviarFilaDe` **antes** del orden por entidad, para que una fila ajena no trabe a nadie.
- **Con qué token:** el de la misma identidad. Nunca se envía con otra identidad. **[P4]** **Corrector E03:** justo
  antes de enviar cada fila, el sincronizador pide `ObtenerAccessTokenParaAsync(fila.Dueno)`: dueño y token se deciden
  juntos bajo el cerrojo con que cambia la identidad (ingreso, cierre, renovación); el token viaja en el pedido y
  `ManejadorDeSesion` lo respeta. Con cuenta, nada sale sin token (401 local, reintentar).
- **Qué se ve:** `outbox.status` y `outbox.changed` cuentan sólo las filas de `ISesionDelUsuario.DuenoVisible` (ya
  cableado: `IDb.ContarOutboxDeAsync`). Las de otro dueño son invisibles. Diagnóstico (pantalla del cascarón, para
  quien opera) sigue mostrando todas.
- **401 del receptor:** no es carta muerta: la fila se reintenta y la sesión intenta renovar una vez
  (`InformarNoAutorizadoAsync`); si el emisor rechaza, `Revocada`. Cambia `ClienteDelReceptor` (constructor de
  autenticación).
- **Cerrar o vencer la sesión no borra filas.** Quedan con su dueño y salen cuando el mismo dueño vuelve con red.
  `auth.signOut` informa `pendientesConservados`. **[P4]**
- **Cambio de modo:** filas `dispositivo-…` de una instalación vieja no salen en modo Oidc (otro dueño). **[arq]** La
  demo instala limpio; se declara.

### 3.6 Qué se guarda y dónde **[arq: C-1, sin cambio de esquema]**

| Dato | Dónde | Notas |
| --- | --- | --- |
| Access token | Memoria | Nunca a disco ni al puente |
| Refresh token en claro | Memoria, mientras `Activa` | Nunca al puente (P3) |
| Refresh sellado (IV + texto cifrado, base64) | Configuración `sesion.refreshSellado` | Sólo con desbloqueo activado |
| Vencimiento del refresh (ms Unix) | Configuración `sesion.refreshExpira` | Para `Vencida` |
| `sub`, dueño, usuario | Configuración `sesion.sub`, `sesion.dueno`, `sesion.usuario` | Identidad presente; para mostrar «ingresá de nuevo como …» |
| Desbloqueo activado | Configuración `sesion.desbloqueo` = `activado` | |
| Clave AES | AndroidKeyStore `fc.sesion.refresh` | Cierre de sesión la borra |

### 3.7 Contenido: la puerta de la sesión **[P4, 038]**

> **2026-10-30 — La cara del login es del contenido** (decisión del PO). Por defecto (`-p:FcPantallasDeSesion=Contenido`),
> el cascarón ya no tapa esta puerta con `IngresoPage` y `DesbloqueoPage`: la puerta del contenido es la pantalla de
> ingreso y de desbloqueo, y se actualiza sin tienda. El canje PKCE, los tokens (Keystore) y el diálogo del sistema
> (huella, PIN o patrón) siguen en el cascarón por `auth.session`; el contenido nunca ve credenciales ni tokens. El
> ingreso abre una Custom Tab (WebAuthenticator), como pide RFC 8252. `-p:FcPantallasDeSesion=Cascaron` vuelve a las
> pantallas XAML.

`Content/Auth/PuertaDeSesion.razor` envuelve `@Body` en `MainLayout` (ya montada; hoy deja pasar):

| Sesión | Qué muestra |
| --- | --- |
| Sin `auth.session` en el catálogo, o modo Dispositivo | La página (como A3) |
| `SinCuenta` | Panel «Ingresar con tu cuenta» con el botón de `sesion.ingresar` (deshabilitado + motivo sin servidor) |
| `Ingresando` | «Completá el ingreso en el navegador…» |
| `Activa` | La página. Si `DesbloqueoLocal = Disponible` y todavía no se preguntó: «¿Usar la seguridad del teléfono para entrar?» (Sí → `auth.enableUnlock`) |
| `Bloqueada` | Panel «Desbloquear» → `auth.unlock` (el diálogo es nativo, del sistema) |
| `Vencida`, `Revocada` | La página **más** una banda «Tu sesión venció / fue revocada: ingresá para enviar lo pendiente» con el botón de reingreso. Se sigue trabajando local **[P4 sin período de gracia; sin red se opera]**. Sólo en el proceso donde hubo prueba local (corrector E03, §12) |

En la línea de estado: el usuario y «Cerrar sesión», con una confirmación que dice que lo pendiente se conserva.
`data-testid` previstos: `puerta-ingresar`, `puerta-desbloquear`, `sesion-usuario`, `sesion-cerrar`.

### 3.8 Cierre de sesión **[P4, §4]**

`auth.signOut`: borra access y refresh de memoria, `sesion.refreshSellado`, `sesion.refreshExpira`, `sesion.sub`,
`sesion.dueno`, `sesion.usuario`, `sesion.desbloqueo` y la clave del Keystore; intenta el `end_session`/`revocation`
del emisor si hay red (sin red, no espera). **Corrector E03:** se usa la revocación (RFC 7009), no el `end_session`
del navegador. En el ensayo, la revocación con red cerró la sesión SSO del emisor; sin red queda viva y la protege
`prompt=login` (§3.3). **No toca el outbox.** Devuelve `pendientesConservados` (las filas
pendientes del dueño que se fue). Estado `SinCuenta`.

## 4. Política de acciones

### 4.1 Dónde se evalúa: en el cascarón **[038 «evaluada por el cascarón»; §7]**

- `IGuardiaDeAcciones` (`Shell/Red/GuardiaDeAcciones.cs`) evalúa con el estado de red y de sesión del momento, con la
  función pura `EvaluadorDeAcciones.Evaluar` (`Core/Red/PoliticaDeAcciones.cs`).
- **Doble cerrojo:** el contenido deshabilita el botón con el motivo (lo que ve la persona) **y** el despachador del
  puente rechaza el método con `AccionBloqueada` (-32005) si la acción no está permitida (ya cableado en
  `DespachadorDelPuente.AtenderAsync`). Un contenido viejo o con un error no puede saltearse la política.
- El contenido no decide nada: muestra `EvaluacionDeAccion` tal como llega (`actions.policy`, `actions.changed`).

### 4.2 Dónde se declara **[arq]**

Una **constante del cascarón**, `PoliticaDeAcciones.Acciones`, como el catálogo (P12): cada acción con su clase y los
métodos del puente que la ejecutan. El borrador §7 deja abierto que venga en el manifiesto del contenido para cambiarla
sin tienda; la demo no lo hace porque la clase de una acción depende de qué hace el cascarón con ese método (el outbox
es suyo), y un contenido no debería poder relajarla. Queda como evolución: el manifiesto podría sólo **endurecer** una
clase (de `Encolable` a `SoloEnLinea`), nunca relajarla.

### 4.3 Reglas del evaluador **[§7, P4]**

Definiciones: **entrada abierta** = modo Dispositivo, o sesión `Activa`, `Vencida` o `Revocada`. **Sesión válida**
= modo Dispositivo, o `Activa`. **Con servidor** = `EnLinea` o `EnLineaMedida`.

| Clase | Permitida si | Queda pendiente si | Motivo cuando no está permitida o queda pendiente |
| --- | --- | --- | --- |
| `SoloLocal` | entrada abierta | nunca | sin entrada: «Ingresá con tu cuenta» / «Desbloqueá la app» / «Completá el ingreso en el navegador» |
| `Encolable` | entrada abierta | no hay servidor o la sesión no es válida | «Sin conexión: se guarda y se envía cuando vuelva» / «El servidor no responde: se guarda y se envía cuando responda» / «Se guarda; se envía cuando vuelvas a ingresar» |
| `SoloEnLinea` | entrada abierta, con servidor y sesión válida | nunca | «Sin conexión» / «El servidor no responde» / «Tu sesión venció: ingresá de nuevo» |
| `SoloWiFi` | como `SoloEnLinea` y además `EnLinea` (no medida) | nunca | además: «Sólo con WiFi» |

`sesion.ingresar` es especial (es la puerta de la entrada): **permitida** si el modo es Oidc, la sesión es `SinCuenta`,
`Vencida` o `Revocada`, y hay servidor. En modo Dispositivo: no permitida, «Esta app no usa cuenta». En `Activa`: no
permitida, «Ya ingresaste».

### 4.4 Tabla de la demo **[038]**

| Acción | Clase | Métodos del puente | Dónde está el botón |
| --- | --- | --- | --- |
| `localidades.guardar` | `SoloLocal` | `documents.put`, `documents.delete` | Localidades: Guardar, Eliminar |
| `encuesta.enviar` | `Encolable` | `encuestas.enqueueRespuesta` | Encuesta: Registrar |
| `outbox.enviarAhora` | `SoloEnLinea` | `outbox.sendNow` | Línea de estado: «Enviar ahora» junto a «Por enviar: N» — **la que demuestra el bloqueo** |
| `sesion.ingresar` | `SoloEnLinea` | `auth.signIn` | Puerta de sesión: «Ingresar con tu cuenta» |

### 4.5 Interfaz **[§7 «botón deshabilitado + motivo»]**

Un componente `Content/Red/BotonDeAccion.razor` (`Accion`, `Texto`, `OnClick`) que se deshabilita según la evaluación
y muestra el motivo debajo (`data-testid="motivo-{accion}"`). Las páginas lo usan en lugar de `<button>` para las
cuatro acciones. Una `Encolable` pendiente se ejecuta igual y la página dice «quedó pendiente» con el motivo.

## 5. El receptor valida el token **[P4; arq]**

`tools/FrameworkConcept.Receptor/AutenticacionDelReceptor.cs` (gancho ya cableado en `Program.cs`; sin
`--oidc-emisor` no hace nada):

- `Microsoft.AspNetCore.Authentication.JwtBearer` `$(VersionAspNetCore)`; `Authority = --oidc-emisor`;
  `Audience = fc-receptor` (`--oidc-audiencia`); `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime` (con
  `ClockSkew` 30 s), firma contra el JWKS del descubrimiento; `MapInboundClaims = false`.
- **TLS hacia el emisor:** `BackchannelHttpHandler` con un `X509Chain` de `CustomRootTrust` sobre `--oidc-ca`
  (`claves/tls/ca_demo_tls.crt`). Nada de desactivar la validación.
- **Qué exige token:** `POST /respuestas` y, desde el corrector E03, `GET /respuestas` y `GET /respuestas/{clave}`,
  que devuelven sólo las del `sub` del token (una ajena da 404). Libres: `/salud` y el canal estático. El operador lee
  el almacén (`guion.sh respuestas` hace `cat salida/receptor/respuestas.json` en modo Oidc).
- **Corrector E03 — sólo el dueño toca su respuesta:** si la clave existe con otro `sub`, 403 (ni reenvío ni
  modificación); `If-Match: *` ya no se acepta (412): modificar exige la versión (D-04).
- **Dueño:** el claim `sub` se guarda con la respuesta (`dueno` en `AlmacenDeRespuestas`) y se muestra en
  `GET /respuestas`. Así el recorrido comprueba que cada respuesta llegó con la identidad de su dueño.
- 401 con `WWW-Authenticate: Bearer` sin token o con uno inválido.
- **Red:** el receptor corre en el contenedor `fc-demo-receptor` y llega al emisor por `https://203.0.113.10:8443`.
  NO VERIFICADO desde dentro del contenedor (el emulador sí llega, §9).

## 6. Contrato del puente 1.1.0

**Clasificación [P3]:** todo es MENOR. Comprobado con la compuerta contra una base 1.0.0 (el `contrato.json` de A3):
`LaVersionSubeSegunLaClasificacionFrenteALaBase` en verde con `FC_GATE_BASE` armada a mano.

### 6.1 Métodos nuevos o cambiados

| Método | Parámetros | Respuesta | Capacidad | Atiende |
| --- | --- | --- | --- | --- |
| `network.status` (cambia) | — | `conectado: bool`, **`estado?: EstadoDeRed`**, **`motivo?: string`** | `network.status` 1.1.0 | `Shell/Red/AtencionDeRed.cs` |
| `actions.policy` | — | `acciones: EvaluacionDeAccion[]` | — | `AtencionDeRed` |
| `actions.check` | `accion: string` | `evaluacion: EvaluacionDeAccion` | — | `AtencionDeRed` |
| `outbox.sendNow` | — | `enviadas: int`, `pendientes: int`, `rechazadas: int` | — | `AtencionDeRed` (usa `SincronizadorDelOutbox.PasadaAsync`, que ya devuelve el resultado) |
| `auth.status` | — | `sesion: SesionInformada` | `auth.session` 1.0.0 | `Shell/Auth/AtencionDeSesion.cs` |
| `auth.signIn` | — | `sesion: SesionInformada` | `auth.session` | `AtencionDeSesion` |
| `auth.enableUnlock` | — | `sesion: SesionInformada` | `auth.session` | `AtencionDeSesion` |
| `auth.unlock` | — | `sesion: SesionInformada` | `auth.session` | `AtencionDeSesion` |
| `auth.signOut` | — | `sesion: SesionInformada`, `pendientesConservados: int` | `auth.session` | `AtencionDeSesion` |

Nombres: área del framework en inglés (`actions`, `auth`, como `bridge`, `documents`, `outbox`, `network`);
tipos y valores del dominio en español.

### 6.2 Eventos

| Evento | Parámetros | Cambio |
| --- | --- | --- |
| `network.changed` | `conectado: bool`, **`estado?: EstadoDeRed`**, **`motivo?: string`** | campos opcionales nuevos (MENOR) |
| `actions.changed` | `acciones: EvaluacionDeAccion[]` | nuevo (MENOR) |
| `auth.changed` | `sesion: SesionInformada` | nuevo (MENOR) |

### 6.3 Tipos que viajan

| Tipo | Campos | Archivo |
| --- | --- | --- |
| `EstadoDeRed` | `sinRed`, `redSinServidor`, `enLinea`, `enLineaMedida` | `Core/Red/EstadoDeRed.cs` |
| `EvaluacionDeAccion` | `accion`, `clase` (`soloLocal`, `encolable`, `soloEnLinea`, `soloWiFi`), `permitida`, `quedaPendiente`, `motivo?` | `Core/Red/PoliticaDeAcciones.cs` |
| `SesionInformada` | `modo` (`dispositivo`, `oidc`), `estado` (`sinCuenta`, `ingresando`, `activa`, `bloqueada`, `vencida`, `revocada`), `usuario?`, `desbloqueoLocal` (`noDisponible`, `disponible`, `activado`), `motivo?` | `Core/Auth/Sesion.cs` |

### 6.4 Códigos de error nuevos (`Core/Contracts/Puente/Sobre.cs`)

| Código | Nombre | Cuándo |
| --- | --- | --- |
| -32005 | `AccionBloqueada` | La guardia no permite la acción. `message` = motivo; `data` = `EvaluacionDeAccion` |
| -32006 | `AutenticacionCancelada` | La persona cerró el navegador o canceló el diálogo del sistema |
| -32007 | `AutenticacionFallida` | `invalid_grant`, clave invalidada, sin bloqueo de pantalla, emisor inalcanzable durante el ingreso |

### 6.5 Fuentes de la versión (las tres dicen 1.1.0)

`ContratoDelPuente.Version`, `versionado/puente.json` y `src/FrameworkConcept.Core/Contracts/contrato.json`
(regenerado con `FrameworkConcept.Tools contrato`). El contenido declara `compatibleConPuente: ">=1.1.0 <2.0.0"`
(`declaracion-del-contenido.json`): un contenido que llama a `actions.policy` no debe activarse sobre un cascarón
1.0 (el actualizador lo frena por el predicado de P3).

**El contrato queda congelado para el incremento.** Si un constructor necesita otro campo, lo pide: cada cambio
vuelve a subir la versión y regenerar la instantánea, y eso no se hace en paralelo.

## 7. Catálogo y manifiesto **[P12]**

| Entrada | Versión | Android | iOS (diseño, P9) | Por qué |
| --- | --- | --- | --- | --- |
| `device.info` | 1.0.0 | — | — | sin cambios |
| `network.status` | **1.1.0** | `ACCESS_NETWORK_STATE` | — | campos opcionales nuevos en la respuesta y el evento |
| `auth.session` | **1.0.0 (nueva)** | `USE_BIOMETRIC`; `<queries>` `android.support.customtabs.action.CustomTabsService` | `NSFaceIDUsageDescription` | BiometricPrompt; visibilidad del navegador de Custom Tabs con API ≥ 30 |

- `EntradaDelCatalogo` suma `ConsultasAndroid` (acciones de intent de `<queries>`), y la compuerta compara `<queries>`
  con el catálogo (`QueriesCoincidenConElCatalogo`, nueva). Antes la compuerta prohibía todo `<queries>` porque el
  catálogo no lo podía nombrar; ahora puede.
- `USE_BIOMETRIC` es un permiso «normal»: se concede al instalar. El catálogo de la demo sigue sin permisos de tiempo
  de ejecución (P12).
- La actividad del redirect no va en el manifiesto fuente: la genera `[IntentFilter]` en
  `Shell/Auth/CallbackDeAutenticacion.cs` (verificado en el manifiesto combinado del APK).
- **Declaración y uso en el mismo binario [P12]:** el APK de este incremento tiene que salir con la función visible
  (puerta de sesión y desbloqueo). El esqueleto declara sin usar: no se publica así.

## 8. Reparto entre los constructores

Ya hecho por el arquitecto y **compartido** (no se toca sin avisar a la presidencia):

| Archivo | Qué quedó |
| --- | --- |
| `src/FrameworkConcept.Core/Contracts/**`, `contrato.json`, `versionado/puente.json` | Contrato 1.1.0 congelado |
| `src/FrameworkConcept.Shell/Platforms/Android/AndroidManifest.xml` | Permiso y `<queries>` |
| `tests/FrameworkConcept.Gate.Tests/Catalogo/…`, `Contrato/SinTokensEnElPuenteTests.cs` | Compuerta |
| `Shell/Bridge/DespachadorDelPuente.cs` | Enrutado a `AtencionDeRed`/`AtencionDeSesion`, guardia antes de ejecutar, dueño de la fila, conteo por dueño |
| `Shell/Sync/SincronizadorDelOutbox.cs` | Despierta con red y sesión; filtra por dueño; `PasadaAsync` devuelve `ResultadoDePasada` |
| `Shell/FabricaHttp.cs`, `Shell/Actualizacion/AdaptadoresDelActualizador.cs` (cliente del canal) | Cadena de `DelegatingHandler` |
| `Shell/MauiProgram.cs`, `Shell/App.cs`, `Shell/Pages/ArranquePage.xaml.cs` | Registro e inicio de red y sesión; ganchos de segundo plano |
| `Shell/Data/IDb.cs`, `DbSqlite.cs` | `ContarOutboxDeAsync(dueño)` |
| `Shell/Catalogo/ImplementacionesDelCatalogo.cs`, `Shell/ConfiguracionDelCascaron.cs`, `FrameworkConcept.Shell.csproj` | Catálogo atendido por servicios; `FcOidcAutoridad`, `FcOidcClienteId` |
| `Content/Bridge/BridgeClient.cs` (partial), `BridgeSession.cs` (`EventoRecibido`), `Program.cs`, `_Imports.razor`, `Layout/MainLayout.razor`, `declaracion-del-contenido.json` | Cableado del contenido |
| `Content/Testing/puente-simulado.js`, `recorrido.mjs` | Secciones reservadas por constructor |
| `infra/keycloak/**` | Emisor de ensayo verificado (§9) |

**Constructor de red** — sólo estos archivos:

| Archivo | Qué hace |
| --- | --- |
| `Core/Red/**` | `TransicionesDeRed.Siguiente`, `EvaluadorDeAcciones.Evaluar` (los dos hoy lanzan `NotImplementedException`) |
| `Shell/Red/**` | `MonitorDeRed` (salud, backoff, perfil, registro), `ManejadorDeRed`, `GuardiaDeAcciones` (evaluar, suscribirse, emitir `actions.changed`), `AtencionDeRed` (`outbox.sendNow`) |
| `Shell/Pages/DiagnosticoPage.*` | Registro de llamadas y estado de red |
| `Content/Red/**` | `EstadoDeRedDelContenido`, `AvisoDeRed.razor`, `BotonDeAccion.razor`, `BridgeClient.Red.cs` |
| `Content/Pages/Localidades.razor`, `Content/Pages/Encuesta.razor`, `Content/Survey/**` | Botones con `BotonDeAccion` |
| `Content/Layout/MainLayout.razor`, **sólo** el bloque `<footer>` | «Enviar ahora» junto a «Por enviar» |
| `Content/wwwroot/css/app.css` | Estilos del aviso y del motivo (agregar al final, bloque «red») |
| `tests/FrameworkConcept.Core.Tests/Red/**` | Pruebas de §10.1 |
| `puente-simulado.js` y `recorrido.mjs`, **sección «red»** | Simulación y comprobaciones |

**Constructor de autenticación** — sólo estos archivos:

| Archivo | Qué hace |
| --- | --- |
| `Core/Auth/**` | `TransicionesDeSesion`, `FlujoOidc`, `PropiedadDelOutbox.PuedeEnviarse` |
| `Shell/Auth/**` | `SesionDelUsuario` (modo Oidc), `CofreDeSesion`, `ManejadorDeSesion` (401), `AtencionDeSesion` |
| `Shell/Sync/ClienteDelReceptor.cs`, `Shell/Sync/IdentidadDelDispositivo.cs` | 401 → reintentar y avisar a la sesión |
| `Content/Auth/**` | `SesionDelContenido`, `PuertaDeSesion.razor`, `BridgeClient.Auth.cs` |
| `Content/wwwroot/css/app.css` | Estilos de la puerta (agregar al final, bloque «sesión») |
| `tools/FrameworkConcept.Receptor/**` | JwtBearer, dueño guardado con la respuesta (§5) |
| `scripts/guion.sh` | Si hace falta: `servir` con `--oidc-*` y `apk` con `FcOidcAutoridad` (bloques nuevos, sin tocar los existentes) |
| `infra/keycloak/**` | Ajustes del realm si el recorrido lo pide |
| `tests/FrameworkConcept.Core.Tests/Auth/**` | Pruebas de §10.1 |
| `puente-simulado.js` y `recorrido.mjs`, **sección «sesión»** | Simulación y comprobaciones |

Interfaces entre los dos (congeladas): `IMonitorDeRed`, `IGuardiaDeAcciones`, `ISesionDelUsuario`, `SesionInformada`,
`EvaluacionDeAccion`. La guardia (red) lee `ISesionDelUsuario.Actual` y escucha `Cambio`; la sesión (autenticación)
no necesita nada de red salvo `IMonitorDeRed.Actual.ConServidor` si quiere evitar el canje sin servidor.

**El APK compartido** se compila una sola vez al integrar. Cada constructor compila en su copia o en turnos; los dos
usan el mismo `bin/obj` de la demo, así que no se compila en paralelo sobre la misma carpeta **[arq: dos `dotnet build`
simultáneos sobre el mismo `obj/` no son seguros; no se midió acá]**.

## 9. Emisor de ensayo: Keycloak en contenedor **[P4 D-07 = I1]**

`infra/keycloak/keycloak.sh` (documentado en su cabecera) y `infra/keycloak/realm-fc-demo.json`:

| Pieza | Valor |
| --- | --- |
| Imagen | `quay.io/keycloak/keycloak:26.7.5` fijada por digest (publicada el 2026-09-30) |
| Contenedor | `fc-keycloak`, `start-dev --import-realm`, sólo HTTPS en `:8443` |
| Emisor | `https://203.0.113.10:8443/realms/fc-demo` |
| TLS | `claves/tls/keycloak.{crt,key}` firmados por la CA de demostración existente (`guion.sh tls`); SAN `localhost`, `127.0.0.1`, `203.0.113.10` |
| Cliente | `fc-demo-app`, público, sólo authorization code, PKCE `S256` obligatorio, redirect `com.frameworkconcept.demo://callback`, mapper de audiencia `fc-receptor` |
| Tiempos | access 5 min; sesión 30 min sin uso, 10 h máximo (`refresh_expires_in` = 1799 s) |
| Usuarios | `encuestador` / `demo-encuestador`, `supervisora` / `demo-supervisora` (dos dueños, para P4) |
| Admin | `admin`, clave aleatoria en `salida/keycloak/admin.txt` (no se versiona) |

**Verificado el 2026-10-26** (`keycloak.sh levantar`, `estado`, `probar`): listo en 33 s; descubrimiento con
`code_challenge_methods_supported` `S256`; JWKS con clave `RS256`; `probar` recorre ingreso → redirect al esquema
privado con el mismo `state` → canje **sin** `code_verifier` rechazado (`400 invalid_grant «PKCE code verifier not
specified»`) → canje con verificador: `aud = [fc-receptor]`, `iss` = el emisor, mismo `sub` en access e id token,
`refresh_expires_in = 1799`; igual con `supervisora`. Desde el emulador `fc-android-emu-hpdev`: el puerto 8443 de
`203.0.113.10` acepta conexión y responde una alerta TLS a un saludo inválido (servidor TLS vivo); el 8444 rechaza.
**NO VERIFICADO:** que Chrome del emulador acepte el certificado (§3.3).

Uso para ensayar el modo Oidc:

```bash
FC_TLS_IPS=203.0.113.10 scripts/guion.sh tls        # si no existe la CA
infra/keycloak/keycloak.sh levantar
infra/keycloak/keycloak.sh probar                      # flujo completo por curl
scripts/contenedor.sh dotnet build src/FrameworkConcept.Shell -c Debug -p:RuntimeIdentifier=android-x64 \
  -p:AndroidPackageFormat=apk -p:FcCanalUrl=https://203.0.113.10:8099 \
  -p:FcOidcAutoridad=https://203.0.113.10:8443/realms/fc-demo
infra/keycloak/keycloak.sh parar
```

## 10. Plan de pruebas

### 10.1 Unitarias de la lógica pura (`tests/FrameworkConcept.Core.Tests/Red/` y `Auth/`)

| Suite | Casos |
| --- | --- |
| `Red/TransicionesDeRedTests` | Una fila por renglón de §2.2 (`[TestCase]`), incluida `EnLineaMedida + SaludFalla → RedSinServidor` y la respuesta vieja en `SinRed` |
| `Red/EvaluadorDeAccionesTests` | Matriz clase × {SinRed, RedSinServidor, EnLinea, EnLineaMedida} × {Dispositivo, SinCuenta, Bloqueada, Activa, Vencida}; todo `Permitida = false` lleva motivo; `Encolable` nunca bloqueada con entrada abierta; reglas de `sesion.ingresar` |
| `Red/PoliticaDeAccionesTests` | Ids únicos; cada método de la política existe en `TablaDeMetodos`; ningún método en dos acciones; las cuatro acciones de §4.4 con su clase |
| `Auth/TransicionesDeSesionTests` | Una fila por renglón de §3.2; `IngresoFallido` vuelve a `antesDeIngresar`; `Cierre` desde cada estado |
| `Auth/FlujoOidcTests` | Vector del Apéndice B de RFC 7636 (`dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk` → `E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM`); verificador de 43 caracteres base64url; la URL lleva `response_type=code`, `code_challenge_method=S256`, `state`, `redirect_uri` codificado; `LeerRespuestaDeTokens` calcula vencimientos con `ahora`; `LeerIdentidad` saca `sub` y `preferred_username` |
| `Auth/PropiedadDelOutboxTests` | Nunca sale una fila de otro dueño; `Vencida`/`Revocada`/`Bloqueada` no envían; Dispositivo envía las suyas |

Rojo provocado de cada suite (P8): invertir una fila de la tabla y ver la prueba en rojo; queda anotado en el informe.

### 10.2 Compuerta

`ci-local.sh` completo en verde (build con avisos como error, Core, Update y Gate, contenido empaquetado, APK).
La instantánea (`ElCodigoCoincideConContratoJson`, `LaVersionSube…`, `LasFuentesDeLaVersion…`), el catálogo frente al
manifiesto (permisos y `<queries>`) y `SinTokensEnElPuenteTests` ya están en verde con el esqueleto.

### 10.3 Contenido en navegador headless (puente simulado)

`probar-en-navegador.sh`: las 40 comprobaciones de A3 siguen en verde con el puente simulado 1.1.0, y cada constructor
agrega las suyas en su sección: aviso de red con cada `estado` (`puenteSimulado.cambiarEstadoDeRed`), «Enviar ahora»
deshabilitado con motivo, encuesta pendiente sin red; puerta de sesión en `?sesion=oidc-sinCuenta`, `oidc-bloqueada`,
`oidc-vencida`, `dispositivo`.

### 10.4 Recorrido en el emulador `fc-android-emu-hpdev`

`adb` siempre como en `~/fc-work/demo/a3/emulador-hpdev.txt` (`-H 203.0.113.20 -P 5042 -s emulator-5554`,
por `docker exec adb-contenedor`; nunca `adb connect` ni `kill-server`). Sólo el paquete `com.frameworkconcept.demo`.
Cada paso deja captura (`exec-out screencap -p`) con su `.txt` de tres líneas y el logcat `-s FC`.

| # | Preparación o acción | Qué tiene que pasar |
| --- | --- | --- |
| R0 | APK x64 **sin** `FcOidcAutoridad`; `guion.sh servir` | Igual que A3 (regresión): semilla, encuesta, envío |
| R1 | `cmd connectivity airplane-mode enable` | Aviso «Sin conexión»; «Enviar ahora» deshabilitado con motivo; encuesta registrada queda «pendiente» |
| R2 | `cmd connectivity airplane-mode disable` | Sin tocar nada: `RedSinServidor` → `EnLinea` en ≤ 10 s; el outbox se vacía solo (reconexión automática) |
| R3 | `guion.sh parar` con red | Al próximo chequeo o envío: «El servidor no responde»; «Enviar ahora» deshabilitado |
| R4 | `guion.sh servir` | Vuelve solo a `EnLinea` dentro del backoff (≤ 60 s) |
| R5 | `svc wifi disable` (datos móviles del emulador activos) | `EnLineaMedida` (chip «Datos móviles»); `svc wifi enable` → `EnLinea` |
| A0 | `keycloak.sh levantar`; CA de demostración como CA de usuario en el emulador; `locksettings set-pin 1234`; huella enrolada en Ajustes con `adb emu finger touch 1` | Precondiciones (D-03: hay bloqueo de pantalla) |
| A1 | APK x64 **con** `FcOidcAutoridad`; receptor con `--oidc-emisor`; abrir | `SinCuenta`: panel de ingreso. Con modo avión: «Ingresar» deshabilitado con motivo |
| A2 | Ingresar como `encuestador` | Custom Tab o navegador; vuelta por el redirect; `Activa`; se ofrece la seguridad del teléfono |
| A3 | Aceptar; `adb emu finger touch 1` | `DesbloqueoLocal = Activado` |
| A4 | Encuesta y envío | `guion.sh respuestas` muestra la respuesta con el `sub` de `encuestador` |
| A5 | `am force-stop` y abrir | `Bloqueada`; desbloquear con huella (o PIN `1234` con `input text`) → `Activa` sin pedir red |
| A6 | Modo avión; encuesta; cerrar sesión | `pendientesConservados = 1`; `SinCuenta`; la fila sigue en la base (Diagnóstico) |
| A7 | Sin modo avión; ingresar como `supervisora` | «Por enviar: 0» (la fila de `encuestador` es invisible) y **no** sale |
| A8 | Cerrar sesión; ingresar como `encuestador` | La fila pendiente sale con el token de `encuestador` |
| A9 | Revocar desde la consola de admin (cerrar las sesiones del usuario) y forzar renovación | `Revocada`: banda de reingreso; se sigue trabajando local |
| A10 | Enrolar otra huella | Al desbloquear: `ClaveInvalidada` → `Revocada`, clave borrada |
| A11 | `locksettings clear --old 1234` | `DesbloqueoLocal = NoDisponible` con motivo (D-03 = T1) |

Al terminar: `pm clear com.frameworkconcept.demo` o desinstalar, quitar el PIN y la CA de usuario si la presidencia
lo pide, `keycloak.sh parar`, `guion.sh parar`, borrar `bin/` y `obj/`.

## 11. Lo que no entra y lo que no está verificado

**No entra en la demo** (se declara en la guía):

- Borrar las filas pendientes propias con confirmación que dice cuántas se pierden (P4): diseño, sin pantalla.
- Evaluar la versión mínima del cascarón **al ingresar** (P4, fila «Versión mínima del cascarón»): sigue evaluándose en
  el arranque en frío como en A3.
- iOS (Keychain `userPresence`, LocalAuthentication): diseño, P9.
- Reintentos con Polly; tabla SQLite del registro de red (§2.4).
- Política de acciones en el manifiesto del contenido (§4.2).
- Cuenta de demostración para tiendas y borrado de cuenta (P4, fila «Tiendas»): el emisor es de ensayo.

**NO VERIFICADO al cerrar el esqueleto:**

- Chrome del emulador con la CA de demostración instalada como CA de usuario (§3.3).
- El receptor en contenedor alcanzando al emisor por la IP de Tailscale (§5).
- `HttpClientHandler` de la `FabricaHttp` con la configuración de red de Android en el dispositivo: compila y es el
  mismo manejador que `new HttpClient()` usaba en A3, pero no se instaló todavía este APK.
- `BiometricPrompt` con `DEVICE_CREDENTIAL` y `CryptoObject` en API 33 del emulador, y `adb emu finger touch` contra
  el adb server remoto.
- Las cifras de tiempo de §2.3 en el emulador.

## 12. Correcciones del ensayo E03

Corrector del incremento, 2026-10-27. Detalle, pruebas y capturas: `OUTPUTs/Ensayos/E03-Red-Y-Autenticacion/correcciones.md`.

| Brecha | Decisión | Dónde |
| --- | --- | --- |
| P0 — el reingreso entraba solo por la sesión SSO del emisor (H-2) | `prompt=login` en todo ingreso interactivo. La revocación con red ya cerraba la sesión SSO; sin red la cubre `prompt=login`. No se agregó `end_session` | `Core/Auth/Oidc.cs` |
| P1 — red caída durante el ingreso: `Ingresando` para siempre (H-4) | Toda excepción que no sea de autenticación ni cancelación pedida es falla (`FallasDelEmisor.EsFalla`): ingreso → estado previo; renovación → null; revocación → se registra | `Core/Auth/Oidc.cs`, `Shell/Auth/SesionDelUsuario.cs`, `Shell/Sync/ClienteDelReceptor.cs` |
| P1 — ANR al volver del navegador (H-3) | El trabajo posterior a la vuelta corre fuera del hilo principal; tiempos del canje y de las escrituras en el registro | `Shell/Auth/SesionDelUsuario.cs` |
| P1 — huella nueva no invalida la clave (H-1) | **Mesa: se mantiene P4** (huella, rostro, PIN o patrón). Apartamiento declarado del borrador §4 | §3.4 |
| P1 — `Vencida`/`Revocada` abrían la entrada sin prueba local | En frío la entrada exige prueba local: `Bloqueada` o `SinCuenta`. `Bloqueada` + `ClaveInvalidada` → `SinCuenta` | `Core/Auth/Sesion.cs`, `SesionDelUsuario.IniciarAsync` |
| P1 — carrera entre el dueño de la fila y el token | Token por fila bajo el cerrojo de la identidad; nada sale sin token con cuenta; un 401 sólo toca la sesión si el token rechazado es el actual | `Shell/Sync/SincronizadorDelOutbox.cs`, `Shell/Auth/ManejadorDeSesion.cs` |
| P1 — el receptor dejaba a otra identidad pisar una respuesta | 403 si el `sub` no es el dueño; sin `If-Match: *`; `GET /respuestas*` con token y filtrado por dueño | `tools/FrameworkConcept.Receptor/` |

**Decisión de mesa sobre H-1 (apartamiento declarado del borrador §4).** El borrador §4 pide invalidar la clave al
enrolar una huella nueva; P4 (folio 035, que manda) pide desbloquear con huella, rostro, PIN **o patrón**. En Android
`setInvalidatedByBiometricEnrollment` sólo invalida claves exclusivamente biométricas: con `DEVICE_CREDENTIAL` no
actúa (A10, `a10-huella-nueva-no-invalida.txt`). Se mantiene P4 porque (1) quien enrola una huella ya tuvo que
ingresar el PIN o patrón del teléfono, que también abre la clave; (2) una clave sólo biométrica dejaría afuera a quien
no tiene huella o rostro, contra P4 y D-03 = T1. Se conserva la invalidación cuando se quita el bloqueo de pantalla.
Una variante con dos claves (biométrica invalidable + credencial) queda como alternativa no implementada. Esta decisión
contradice un texto del PO (borrador §4): **se le avisa** en el informe del ensayo.

El contrato del puente no cambia: `SesionInformada` tiene la misma forma (sólo cambia cuándo se llega a cada estado).
`ISesionDelUsuario` es interna del cascarón (no viaja por el puente).
