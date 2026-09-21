# Prueba de concepto — ayuda por rol

Demuestra la propuesta de la sección 7 de [`Sistema-Documentador-Tecnico-Guide.md`](../../Sistema-Documentador-Tecnico-Guide.md) en dos pasos:

| Carpeta | Qué es |
|---------|--------|
| [`Consola/`](Consola/) | El motor aislado: catálogo, filtro por rol, `llms.txt` y validador, con tres defectos plantados |
| [`Web/`](Web/) | Un panel Blazor (.NET 10, render estático de servidor) con ingreso simulado por papel, ayuda contextual, 404 por rol, fuente `.md`, `llms.txt` y dos idiomas |
| [`.devcontainer/`](.devcontainer/devcontainer.json) | Entorno reproducible: imagen `mcr.microsoft.com/devcontainers/dotnet:1-10.0`, puerto 5190 |
| [`capturas/`](capturas/) | Lo que se ve, capturado el 2026-09-19 |

No es código de Lab-Geometria. El contenido de `Web/Ayuda/` deriva de los casos de uso `CU-00022`, `CU-00023`, `CU-00024`, `CU-00026`, `CU-00028` y `CU-00029` (versión 1.1) de ese proyecto, y cada página lo declara en `traces`.

## Levantarla

Con VS Code: *Reopen in Container*. Sin VS Code, con el CLI oficial:

```bash
npx -y @devcontainers/cli up   --workspace-folder .
npx -y @devcontainers/cli exec --workspace-folder . \
  dotnet run --project Web/AyudaDemo.Web.csproj --no-launch-profile
```

Abrir `http://localhost:5190`. Al arrancar, el log dice `Catálogo de ayuda: 9 páginas, validador con 0 fallas`.

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
