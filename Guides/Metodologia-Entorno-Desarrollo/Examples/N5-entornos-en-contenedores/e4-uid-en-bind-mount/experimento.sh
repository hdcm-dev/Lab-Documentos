#!/usr/bin/env bash
# E5-4 — Quién es dueño de lo que el contenedor escribe en un bind mount (host Linux nativo).
# En Linux el bind mount conserva UID/GID numéricos: lo que crea root dentro del contenedor
# queda de root en el host. (En Docker Desktop sobre Windows/macOS la capa de archivos compartidos
# de la VM media en el acceso; este experimento no lo cubre.)
set -u
IMG='mcr.microsoft.com/dotnet/sdk:8.0@sha256:78235e09001f52b6592c458ac010775ebac6725422e80cd0c1650590f67b2743'
W="$(mktemp -d)"
echo "host: usuario $(id -un) uid=$(id -u)"
echo "## 1. Contenedor como root (default de la imagen) escribe obj/ en el repo montado"
docker run --rm -v "$W":/w -w /w "$IMG" bash -c 'mkdir -p obj && touch obj/artefacto && id -u'
stat -c '%U(%u) %n' "$W/obj" "$W/obj/artefacto"
rm -rf "$W/obj" 2>&1 | head -1 || true
echo "## 2. Mismo contenedor con --user = UID/GID del host"
docker run --rm --user "$(id -u):$(id -g)" -v "$W":/w -w /w "$IMG" bash -c 'mkdir -p obj2 && touch obj2/artefacto && id -u'
stat -c '%U(%u) %n' "$W/obj2" "$W/obj2/artefacto"
# Limpieza: lo de root sólo se puede borrar desde un contenedor root (no hay sudo en este host).
docker run --rm -v "$W":/w "$IMG" rm -rf /w/obj /w/obj2; rmdir "$W" && echo "limpieza OK"
