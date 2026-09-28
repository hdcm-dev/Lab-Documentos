# Publicar en NuGet

## Consideraciones para proyectos con múltiples paquetes interdependientes

Cuando un proyecto tiene varios paquetes donde unos dependen de otros, el orden importa:

- Empaquetar y publicar primero los paquetes **sin dependencias internas**
- Luego los que dependen de los anteriores
- Incrementar la versión en **todos** los `.csproj` involucrados antes de empaquetar

---

## NuGet Privado (GitHub Packages)

### Aclaración: autenticación con token, no con contraseña

El comando `dotnet nuget add source` usa los parámetros `--username` y `--password`, pero GitHub **no acepta contraseñas**. En este contexto:

- `--username`: puede ser cualquier string; por convención se usa el nombre de usuario de GitHub, pero no es validado
- `--password`: es el **Personal Access Token (PAT)**, no una contraseña. GitHub usa Basic Auth donde el token va en ese campo

#### Cómo crear el PAT en GitHub

1. Ir a **GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)**
2. Hacer clic en **Generate new token (classic)**
3. Asignar un nombre descriptivo (ej. `nuget-push-notionsgroups`)
4. Seleccionar los siguientes scopes:
   - `write:packages` — para publicar paquetes
   - `read:packages` — para consumir paquetes desde la fuente privada
   - `delete:packages` — opcional, solo si se necesita eliminar versiones
   - `repo` — requerido cuando el repositorio es **privado** (GitHub lo exige para acceder a sus packages)
5. Generar y copiar el token (solo se muestra una vez)

> **Importante:** guardar el token en un lugar seguro (ej. un gestor de contraseñas). Si se pierde, hay que regenerarlo.

### Requisitos previos

- Tener un Personal Access Token (PAT) de GitHub (ver sección anterior)
- Registrar la fuente una sola vez:
  ```powershell
  dotnet nuget add source https://nuget.pkg.github.com/<owner>/index.json \
    --name NotionsGroups-GitHub \
    --username <GITHUB_USER> \
    --password <PAT>
  ```

### Pasos por cada paquete (en orden de dependencias)

1. **Incrementar la versión** en el `.csproj` correspondiente:
   ```xml
   <Version>1.0.6</Version>
   ```

2. **Empaquetar:**
   ```powershell
   dotnet pack NombreProyecto/NombreProyecto.csproj -c Release
   ```

3. **Publicar** (reemplazar nombre y versión):
   ```powershell
   dotnet nuget push NombreProyecto/bin/Release/NombreProyecto.1.0.6.nupkg \
     --source NotionsGroups-GitHub
   ```

Repetir para cada paquete en orden, del menos dependiente al más dependiente.

> **Nota:** Si algún proyecto auxiliar (ej. `NgForms.Utils`) fue modificado, ejecutar primero desde su carpeta:
> ```powershell
> dotnet build -c Release
> ```

---

## NuGet.org (Público)

### Requisitos previos

- Tener una cuenta en [nuget.org](https://www.nuget.org) y obtener una API key desde **Account → API Keys**
- La API key debe tener permiso de push sobre los paquetes a publicar

### Pasos por cada paquete (en orden de dependencias)

1. **Incrementar la versión** en el `.csproj` correspondiente:
   ```xml
   <Version>1.0.6</Version>
   ```

2. **Empaquetar en Release** (incluye símbolos si se desea depuración remota):
   ```powershell
   dotnet pack NombreProyecto/NombreProyecto.csproj -c Release
   ```

3. **Publicar en nuget.org** (reemplazar nombre, versión y API key):
   ```powershell
   dotnet nuget push NombreProyecto/bin/Release/NombreProyecto.1.0.6.nupkg \
     --source https://api.nuget.org/v3/index.json \
     --api-key <NUGET_API_KEY>
   ```

Repetir para cada paquete en orden, del menos dependiente al más dependiente.

> **Nota:** nuget.org puede demorar unos minutos en indexar el paquete. Si un paquete dependiente falla al resolverse durante la publicación, esperar y reintentar.
