# N5 — Entornos en contenedores: experimentos

Siete experimentos mínimos que responden «¿se puede congelar el entorno con contenedores?» con salidas reales. Cada uno es un `experimento.sh` que trabaja sobre un directorio temporal (`mktemp -d`) y lo borra al terminar: no deja `bin/`, `obj/`, contenedores ni imágenes derivadas en el repositorio. Las salidas capturadas el 2026-10-07 están en [`salidas/`](salidas/).

**Requisitos del host:** Docker (probado con Docker Engine 26.1 y buildx 0.35 en Debian 13), `git`, `iconv`, `file`, `python3`; para E5-5, además `node`/`npx`. No hace falta tener .NET instalado: todo compila dentro de contenedores.

## Índice

| ID | Carpeta | Qué demuestra | Salida |
|---|---|---|---|
| E5-1 | [`e1-que-fija-el-contenedor/`](e1-que-fija-el-contenedor/) | Una imagen por digest fija SO, glibc, SDK y git; **no** fija los bytes del fuente montado (un `.cs` en cp1252 falla igual) ni la configuración global de git del host. | [`E5-1`](salidas/E5-1-que-fija-el-contenedor.txt) |
| E5-2 | [`e2-etiqueta-vs-digest/`](e2-etiqueta-vs-digest/) | La etiqueta `sdk:8.0` (y también la «fija» `sdk:8.0.425`) apunta hoy a otro contenido que la copia local (construida el 2026-09-19): mismo SDK, distinto OpenSSL, Perl, expat, tzdata. El digest no se mueve y por eso tampoco recibe esos parches. | [`E5-2`](salidas/E5-2-etiqueta-vs-digest.txt) |
| E5-3 | [`e3-clon-windows-en-linux/`](e3-clon-windows-en-linux/) | Un repo clonado con `core.autocrlf=true` y sin bit de ejecución, montado en Linux: `dubious ownership`, archivos «modificados» sin cambios, `bash\r` y `Permission denied`. Con `.gitattributes` (`eol=lf`) y `git update-index --chmod=+x` en el repo, el mismo clon queda limpio. | [`E5-3`](salidas/E5-3-clon-windows-en-linux.txt) |
| E5-4 | [`e4-uid-en-bind-mount/`](e4-uid-en-bind-mount/) | En host Linux, lo que el contenedor escribe como root en el bind mount queda de root (el usuario no lo puede borrar); con `--user` queda del usuario. | [`E5-4`](salidas/E5-4-uid-en-bind-mount.txt) |
| E5-5 | [`e5-devcontainer-por-digest/`](e5-devcontainer-por-digest/) | `devcontainer.json` con imagen por digest y feature de Node fijada; `devcontainer-lock.json` congela el digest de la feature; `--frozen-lockfile` rechaza un cambio de referencia no registrado; `outdated` informa sin modificar. | [`E5-5`](salidas/E5-5-devcontainer-por-digest.txt), [`E5-5b`](salidas/E5-5b-cli-flags.txt) |
| E5-6 | [`e6-imagen-dev-vs-ci/`](e6-imagen-dev-vs-ci/) | Dos imágenes «de .NET 10» (la oficial del SDK y una de devcontainer bajada en otro momento) traen SDK distintos (10.0.400 frente a 10.0.100-rc.2) y producen DLL distintas del mismo código; con la misma imagen por digest el hash se repite; un `global.json` en el repo rechaza la imagen de desarrollo desalineada. | [`E5-6`](salidas/E5-6-imagen-dev-vs-ci.txt) |
| E5-7 | [`e7-llaves-en-gitattributes/`](e7-llaves-en-gitattributes/) | Git (2.39.5 en la imagen, 2.47.3 en el host) **no expande llaves** en `.gitattributes`: `*.{cmd,bat} text eol=crlf` —y la receta literal de VS Code con `*.{cmd,[cC][mM][dD]}`— deja `.cmd`/`.bat` en `eol=lf`. Un patrón por extensión (`*.[cC][mM][dD]`) sí funciona. Agregado por la verificación adversarial (2026-10-07). | [`E5-7`](salidas/E5-7-llaves-en-gitattributes.txt) |
| E5-8 | [`e8-capa4-y-artefacto/`](e8-capa4-y-artefacto/) | Con el mismo commit y el mismo SDK (8.0.425, `ContinuousIntegrationBuild`), tres bases Linux (Debian 12, Ubuntu 24.04 y Debian 13) en tres rutas dan la misma DLL (`9815b804…`) y el mismo PDB; el control con otro SDK (10.0.400) da otra DLL (`7cd2a836…`). Los bytes del artefacto gestionado los decide la capa 2, no la 4. Windows y macOS no se midieron. Agregado en el ciclo 3 (2026-10-09). | [`E5-8`](salidas/E5-8-capa4-y-artefacto.txt) |

## Cómo correrlos

Desde esta carpeta:

```bash
./e1-que-fija-el-contenedor/experimento.sh
./e2-etiqueta-vs-digest/experimento.sh      # baja la versión actual de sdk:8.0 POR DIGEST (no mueve la etiqueta local)
./e3-clon-windows-en-linux/experimento.sh
./e4-uid-en-bind-mount/experimento.sh
./e5-devcontainer-por-digest/experimento.sh  # usa npx -y @devcontainers/cli@0.89.0
./e6-imagen-dev-vs-ci/experimento.sh
./e7-llaves-en-gitattributes/experimento.sh
sh ./e8-capa4-y-artefacto/experimento.sh      # instala el SDK 8.0.425 con dotnet-install.sh en dos bases (necesita red)
```

Lo que cada script ejecuta en Docker, en esencia:

```bash
# E5-1: compilar el mismo proyecto con la imagen fijada por digest, montando el fuente
docker run --rm --user "$(id -u):$(id -g)" -e HOME=/tmp -e NUGET_PACKAGES=/tmp/nuget \
  -v "$W":/w -w /w \
  mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743 \
  dotnet build

# E5-2: a qué digest apunta hoy la etiqueta, sin bajar la imagen
docker buildx imagetools inspect mcr.microsoft.com/dotnet/sdk:8.0 --format '{{.Manifest.Digest}}'

# E5-5: levantar el devcontainer exigiendo el lock versionado
npx -y @devcontainers/cli@0.89.0 up --workspace-folder "$W" --mount-workspace-git-root false --frozen-lockfile
```

## Notas para reproducir

- **Los resultados de E5-2 y E5-6 dependen del momento.** Los digests escritos en los scripts son los de las imágenes presentes en la máquina del laboratorio el 2026-10-07. Si no los tenés, `docker pull` por digest los baja tal cual (mientras el registro los conserve). La comparación con el registro de E5-2 va a dar otro digest «remoto» el día que lo corras: ese es justamente el punto.
- **E5-3 no necesita Windows.** La estación Windows se simula con `git -c core.autocrlf=true -c core.fileMode=false clone`, que aplica la configuración sólo a ese comando y no toca la del usuario.
- **E5-4 vale para host Linux nativo.** En Docker Desktop (Windows o macOS) el daemon corre en una VM y el acceso a archivos compartidos pasa por esa capa; no se probó acá.
- **E5-5 monta la raíz git por defecto.** La CLI de devcontainers 0.89.0 tiene `--mount-workspace-git-root` en `true` por defecto (ver `E5-5b`): sin el flag, montaría el repositorio completo y no sólo la carpeta. El script trabaja sobre una copia temporal fuera de git y además lo desactiva.
- **Nombre del lock.** El README de la CLI lo nombra `.devcontainer-lock.json`; lo que la versión 0.89.0 generó en este laboratorio es `.devcontainer/devcontainer-lock.json` (archivo versionado en [`e5-devcontainer-por-digest/.devcontainer/`](e5-devcontainer-por-digest/.devcontainer/)).
