# Prueba de concepto — ayuda por rol

Demuestra la propuesta de la sección 7 de [`Sistema-Documentador-Tecnico-Guide.md`](../../Sistema-Documentador-Tecnico-Guide.md) en dos pasos:

| Carpeta | Qué es |
|---------|--------|
| [`Consola/`](Consola/) | El motor aislado: catálogo, filtro por rol, `llms.txt` y validador, con tres defectos plantados |
| [`Audiencias/`](Audiencias/) | La prueba extendida de la resolución 399 del expediente 0003 (E-1 a E-6): tres roles en jerarquía y el integrador, dos alcances, panel y paquete, API por exposición, un formulario con ayuda de campo trazada a su regla y el nivel contra la visibilidad del repositorio. **Contenido ficticio** |
| [`Web/`](Web/) | Un panel Blazor (.NET 10, render estático de servidor) con ingreso simulado por papel, ayuda contextual, 404 por rol, fuente `.md`, `llms.txt` y dos idiomas |
| [`.devcontainer/`](.devcontainer/devcontainer.json) | Entorno reproducible: imagen `mcr.microsoft.com/devcontainers/dotnet:1-10.0`, puerto 5190 |
| [`capturas/`](capturas/) | Lo que se ve, capturado el 2026-09-19 |

No es código de Lab-Geometria. El contenido de `Web/Ayuda/` deriva de los casos de uso de la unidad Web de ese proyecto en el SHA `5f07b12` (`CU-10002@1.5`, `CU-10003@1.5`, `CU-10004@1.9`, `CU-10005@1.2`, `CU-10006@1.1`, `CU-10007@1.3` y `CU-10009@1.1`): cada página lo declara en `traces` y tiene su fila en el [registro de requerimientos documentales](_requisitos.md) (`RD-21` a `RD-27`).

## Levantarla

Con VS Code: *Reopen in Container*. Sin VS Code, con el CLI oficial:

```bash
npx -y @devcontainers/cli up   --workspace-folder .
npx -y @devcontainers/cli exec --workspace-folder . \
  dotnet run --project Web/AyudaDemo.Web.csproj --no-launch-profile
```

Abrir `http://localhost:5190`. Al arrancar, el log dice `Catálogo de ayuda: 9 páginas, validador con 0 fallas`. El catálogo cuenta pares cultura-página: son 7 páginas fuente en español y 2 traducciones al inglés.

Para mostrarla afuera sin tocar DNS ni cuentas, un túnel rápido de Cloudflare (la URL `*.trycloudflare.com` sale en el log y muere al borrar el contenedor):

```bash
docker run -d --name demo-ayuda-por-rol-tunnel --network host \
  cloudflare/cloudflared:latest tunnel --no-autoupdate --url http://localhost:5190
docker logs demo-ayuda-por-rol-tunnel 2>&1 | grep trycloudflare
```

Si se expone, correrla con `--remote-env ASPNETCORE_ENVIRONMENT=Production` en el `exec`: en `Development`, ASP.NET muestra la página de excepciones con trazas internas.

## Qué probar

| Acción | Resultado esperado |
|--------|--------------------|
| Entrar como **Alumno** | Panel con *Enviar un trabajo*, *Mis trabajos* y *Tu contraseña*, cada uno con su «¿Cómo se hace?» |
| Entrar como **Administrador** | Panel con *Cuentas*, *Reseteo*, *Revisión* y *Tu contraseña* |
| Alumno pide `/ayuda/administrador/gobernar-las-cuentas` | `404`. El HTML no contiene ni el título de la página |
| `/ayuda/llms.txt` | Sólo las páginas del papel de la sesión |
| `/ayuda/alumno/enviar-un-trabajo.md` | El Markdown fuente, con su frontmatter |
| Selector **EN** | Interfaz en inglés (`Resources/Textos.en.resx`); dos páginas traducidas; el resto en español con aviso y marca `es` en la navegación |
| Sin sesión, `/ayuda` | `302` a `/ingresar` |

## La consola

```bash
docker run --rm -u $(id -u):$(id -g) -e HOME=/tmp -v "$PWD/Consola":/w -w /w \
  mcr.microsoft.com/dotnet/sdk:10.0 dotnet run
```

[`Consola/salida.txt`](Consola/salida.txt) termina con `Validador: 3 fallas` y `[exit=1]`, **a propósito**:

| Defecto | Dónde | Plantado |
|---------|-------|----------|
| Enlace a una página inexistente | `administrador/habilitar-cuentas.md` → `comun/no-existe.md` | Sí |
| Página de alumno enlaza a una de administrador | `alumno/entregar-trabajo.md` → `administrador/habilitar-cuentas.md` | Sí |
| Página común enlaza a una sólo de alumno | `comun/ingresar.md` → `alumno/entregar-trabajo.md` | **No**: lo encontró el validador |

El `<script>` de `Consola/Ayuda/comun/ingresar.md` también es deliberado: sale escapado (`&lt;script&gt;`).

## La prueba extendida: audiencias y entrega

```bash
docker run --rm -u $(id -u):$(id -g) -e HOME=/tmp -v "$PWD/Audiencias":/w -w /w \
  mcr.microsoft.com/dotnet/sdk:10.0 dotnet run
```

**Todo es ficticio.** Las audiencias, las aplicaciones `app-a` y `app-b`, los avisos, los puntos finales de la API y
la regla `RN-DEMO-ASUNTO` son inventados: no describen ningún producto real. Este repositorio es público, así que sólo
guarda fuentes de nivel `publico` o `producto` (regla `nivel-vs-repo` de [`TEM-05`](../../TEM-05-Ayuda-Al-Usuario-Final.md#aplicación-por-escenario)).
Lo `restringido` y lo `interno` no existe como archivo: el documento del integrador se genera desde puntos finales
ficticios, y la clave restringida y la página interna de las mitades rojas se plantan **en memoria** al correr.

| Pieza | Qué es |
|-------|--------|
| [`_audiencias.md`](Audiencias/_audiencias.md) | Registro `AUD-DEMO-AYR`: la visibilidad del repositorio y cinco audiencias con rol, alcance, superficie, nivel y `Incluye` |
| [`Ayuda/`](Audiencias/Ayuda/) | Cinco páginas con `delivery:`; dos con `contexto: app-a/…` o `app-b/…` |
| [`Reglas/RN-DEMO-ASUNTO.md`](Audiencias/Reglas/RN-DEMO-ASUNTO.md) | Ficha de la regla del campo, con las anclas `valida` y `muestra` y lo que cada una `contiene` |
| `Formulario.cs`, `Microtexto.resx` | El modelo con `[Display]` y `[StringLength(ReglaAsunto.Maximo)]`; pista y error con el límite interpolado |
| `Servidor.cs` | Un servidor en memoria (`TestServer`): ayuda filtrada por audiencia y aplicación, y dos documentos OpenAPI con `AddOpenApi` |
| [`salida.txt`](Audiencias/salida.txt) | La corrida registrada: termina con `Validador: 8/8 pares` y `[exit=0]` |

Cada control corre dos veces: tal cual (tiene que dar verde) y con un defecto plantado (tiene que dar rojo). La
consola sale con 0 sólo si los ocho pares se comportan así.

| Id | Control | Mitad roja: qué se planta | Dónde se ve |
|----|---------|---------------------------|-------------|
| E-1 | `cruce-de-audiencia` | Una página de `usuario` que enlaza a una de `admin-raiz` | `salida.txt`, l. 14-19 |
| E-2 | Prueba pareada de alcance | Un servidor que ignora la aplicación de la sesión: el administrador de `app-a` recibe lo de `app-b` | l. 21-31 |
| E-3 | `paquete-sin-fuga` | La clave `Integracion.Credencial` (nivel `restringido`) en el `.resources` compilado del paquete | l. 33-41 |
| E-4a | `contrato-por-exposicion` | `/api/diagnostico` sin grupo: cae en los dos documentos | l. 43-52 |
| E-4b | Prueba pareada 200 / distinto de 200 | El documento de integradores sin autorización | l. 53-61 |
| E-5a | `anclas` (`muestra`, `valida`, `contiene`) | La regla pasa a 100 y el texto quedó copiado con 80, no interpolado | l. 63-74 |
| E-5b | `aria-describedby` | El campo sin el atributo | l. 75-83 |
| E-6 | `nivel-vs-repo` | Una página entregada a `equipo` (nivel `interno`) | l. 85-90 |

**La no entrega, declarada.** La ayuda responde 404 a quien no la recibe; el documento OpenAPI de integradores
responde 401 sin sesión y 403 con una sesión sin el rol (l. 53-55). En los dos casos, sin contenido.

**Lo que se midió.** El `ValidationMessage` de Blazor emite `<div class="validation-message">` sin `id`, y el
`InputText` emite `aria-invalid` pero no `aria-describedby` (l. 75-76): la asociación la pone la vista, con un `id`
en el contenedor del mensaje y el atributo en el campo. No se midió el empaquetado de satélites `.resx` en MAUI
Android: el paquete de E-3 es una carpeta que simula la app instalada, con el `.resources` escrito por
`ResourceWriter`.

En `Web/Ayuda/en/` hay dos enlaces a páginas que no tienen traducción (`comun/cambiar-la-contrasena.md` y `alumno/consultar-mis-trabajos.md`). También son deliberados: el panel los resuelve con el respaldo de cultura (sección 7.6 de la guía) y sirve la página en español con un aviso. Un verificador de enlaces sobre archivos los ve rotos; la compuerta de documentación del repositorio los excluye con nombre y motivo, igual que el defecto plantado de la consola.
