# Guía de gestión de archivos, multimedia y video en arquitecturas de microservicios

> Documento complementario a la Guía de Arquitectura de Microservicios. Enfoque primario en .NET, con diseño API-first para consumo desde cualquier tipo de cliente.

## Índice de navegación

- [1. Gestión de archivos](#1-gestión-de-archivos)
  - [1.1 Principios de diseño](#11-principios-de-diseño)
  - [1.2 Abstracción de storage (patrón proveedor)](#12-abstracción-de-storage-patrón-proveedor)
  - [1.3 Proveedores de almacenamiento](#13-proveedores-de-almacenamiento)
  - [1.4 Integración con Google Drive](#14-integración-con-google-drive)
  - [1.5 Integración con FTP/SFTP](#15-integración-con-ftpsftp)
  - [1.6 Integración con S3 y compatibles](#16-integración-con-s3-y-compatibles)
  - [1.7 Almacenamiento local y sistema de archivos](#17-almacenamiento-local-y-sistema-de-archivos)
  - [1.8 Microservicio de archivos en la arquitectura](#18-microservicio-de-archivos-en-la-arquitectura)
  - [1.9 Seguridad y validación de archivos](#19-seguridad-y-validación-de-archivos)
- [2. Gestión de imágenes y multimedia](#2-gestión-de-imágenes-y-multimedia)
  - [2.1 Pipeline de procesamiento de imágenes](#21-pipeline-de-procesamiento-de-imágenes)
  - [2.2 Thumbnails y variantes](#22-thumbnails-y-variantes)
  - [2.3 Estrategia WhatsApp: caché local + descarga bajo demanda](#23-estrategia-whatsapp-caché-local--descarga-bajo-demanda)
  - [2.4 Lazy loading progresivo (blur-up)](#24-lazy-loading-progresivo-blur-up)
  - [2.5 CDN y distribución geográfica](#25-cdn-y-distribución-geográfica)
  - [2.6 Manejo de multimedia desde apps móviles](#26-manejo-de-multimedia-desde-apps-móviles)
- [3. Gestión de video](#3-gestión-de-video)
  - [3.1 Escenarios de video en la plataforma](#31-escenarios-de-video-en-la-plataforma)
  - [3.2 Formatos, codecs y contenedores](#32-formatos-codecs-y-contenedores)
  - [3.3 Protocolos de streaming](#33-protocolos-de-streaming)
  - [3.4 Pipeline de procesamiento con FFmpeg](#34-pipeline-de-procesamiento-con-ffmpeg)
  - [3.5 Grabación local y upload diferido](#35-grabación-local-y-upload-diferido)
  - [3.6 Fuentes de video: cámaras IP, webcam, pantalla](#36-fuentes-de-video-cámaras-ip-webcam-pantalla)
  - [3.7 Streaming en vivo (live)](#37-streaming-en-vivo-live)
  - [3.8 Video bajo demanda (VOD)](#38-video-bajo-demanda-vod)
- [4. Compresión de video y archivos](#4-compresión-de-video-y-archivos)
  - [4.1 Compresión de archivos generales](#41-compresión-de-archivos-generales)
  - [4.2 Compresión de imágenes](#42-compresión-de-imágenes)
  - [4.3 Compresión y transcodificación de video](#43-compresión-y-transcodificación-de-video)
  - [4.4 Perfiles de compresión recomendados](#44-perfiles-de-compresión-recomendados)
- [5. Firma digital de video y archivos](#5-firma-digital-de-video-y-archivos)
  - [5.1 Firma digital de documentos](#51-firma-digital-de-documentos)
  - [5.2 Firma de video (integridad y autenticidad)](#52-firma-de-video-integridad-y-autenticidad)
  - [5.3 Marca de agua digital (watermarking)](#53-marca-de-agua-digital-watermarking)
  - [5.4 Cadena de custodia digital](#54-cadena-de-custodia-digital)
  - [5.5 DRM y protección de contenido](#55-drm-y-protección-de-contenido)
- [6. Arquitectura integrada](#6-arquitectura-integrada)
  - [6.1 Cómo encaja el File Service en el ecosistema](#61-cómo-encaja-el-file-service-en-el-ecosistema)
  - [6.2 Estructura de carpetas del microservicio](#62-estructura-de-carpetas-del-microservicio)
  - [6.3 Eventos de integración](#63-eventos-de-integración)
- [7. Glosario de términos](#7-glosario-de-términos)
- [8. Guía para agentes IA](#8-guía-para-agentes-ia)
  - [8.1 Tabla G1 — Roles y especialidades](#81-tabla-g1--roles-y-especialidades)
  - [8.2 Tabla G2 — Selección de estructura por solicitud](#82-tabla-g2--selección-de-estructura-por-solicitud)
  - [8.3 Tabla G3 — Selección de proveedor de storage](#83-tabla-g3--selección-de-proveedor-de-storage)
  - [8.4 Tabla G4 — Selección de estrategia de video](#84-tabla-g4--selección-de-estrategia-de-video)
  - [8.5 Tabla G5 — Selección de compresión](#85-tabla-g5--selección-de-compresión)
  - [8.6 Tabla G6 — Prompts de referencia por rol](#86-tabla-g6--prompts-de-referencia-por-rol)

---

## 1. Gestión de archivos

### 1.1 Principios de diseño

Todo acceso a archivos en la plataforma debe seguir estos principios:

**Abstracción de proveedor.** Ningún microservicio debe acoplarse a un proveedor de storage específico (S3, Azure Blob, disco local). Se define una interfaz `IFileStorage` y cada proveedor la implementa. El microservicio trabaja contra la interfaz.

**Almacenamiento centralizado.** Un microservicio dedicado (File Service) gestiona el ciclo de vida de los archivos: upload, metadata, variantes, permisos, expiración. Los demás microservicios le delegan la responsabilidad de storage.

**Archivos como recursos con identidad.** Cada archivo recibe un ID único (GUID), se almacena con metadata (nombre original, MIME type, tamaño, tenant, fecha, hash SHA-256), y se referencia por ese ID desde cualquier microservicio.

**URLs firmadas de corta duración.** Los archivos nunca se sirven directamente. Se generan URLs pre-firmadas con expiración (5-60 minutos) que autorizan el acceso temporal sin exponer credenciales.

**Multi-tenancy en el storage.** Los archivos de cada tenant se aíslan por prefijo en el bucket/container: `{tenant-id}/{service-id}/{año}/{mes}/{file-id}.ext`. Esto permite políticas de retención, cuotas y backups per-tenant.

### 1.2 Abstracción de storage (patrón proveedor)

La industria utiliza el patrón Strategy/Provider para desacoplar la lógica de negocio del proveedor de almacenamiento. En .NET hay dos caminos:

**Opción A — Interfaz propia (recomendada para control total):**

```csharp
public interface IFileStorage
{
    Task<string> UploadAsync(string path, Stream content, string contentType,
        CancellationToken ct = default);
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string path, TimeSpan expiration,
        CancellationToken ct = default);
    Task<FileMetadata> GetMetadataAsync(string path, CancellationToken ct = default);
}
```

Se implementa `S3FileStorage`, `AzureBlobFileStorage`, `LocalFileStorage`, `FtpFileStorage`. El DI registra la implementación según configuración.

**Opción B — FluentStorage (librería de abstracción polycloud):**

FluentStorage (antes Storage.NET) provee una interfaz unificada `IBlobStorage` con implementaciones para AWS S3, Azure Blob, Google Cloud Storage, FTP, SFTP, disco local. Es open-source, licencia MIT, soporta .NET 5+.

```csharp
// Registro en DI
services.AddSingleton(StorageFactory.Blobs.FromConnectionString(
    configuration["Storage:ConnectionString"]));

// El connection string define el proveedor:
// "disk://path=/data/files"
// "s3://accessKey:secretKey@us-east-1/my-bucket"
// "azure.blob://accountName:key@container"
// "ftp://user:pass@host/path"
```

**Cuándo usar cada opción:**

| Criterio | Interfaz propia | FluentStorage |
|---|---|---|
| Control total sobre la API | Sí | Limitado a la API de la librería |
| Funciones avanzadas (presigned URLs, lifecycle policies) | Implementación específica por proveedor | Soporte parcial |
| Velocidad de implementación | Más lenta (hay que implementar cada proveedor) | Inmediata (instalar NuGet y configurar) |
| Mantenimiento | Propio | Comunidad open-source |
| Recomendación | Producción con requerimientos específicos | Prototipos o cuando la funcionalidad estándar alcanza |

### 1.3 Proveedores de almacenamiento

| Proveedor | Tipo | Cuándo usar | NuGet (.NET) | Costo relativo |
|---|---|---|---|---|
| **AWS S3** | Object storage cloud | Producción, alta disponibilidad, escalabilidad ilimitada | AWSSDK.S3 | Bajo (pay-per-use) |
| **MinIO** | Object storage S3-compatible, self-hosted | Cuando querés compatibilidad S3 pero en tu infraestructura | Minio | Gratis (self-hosted) |
| **Azure Blob Storage** | Object storage cloud | Ecosistema Azure, integración con otros servicios Azure | Azure.Storage.Blobs | Bajo (pay-per-use) |
| **Google Cloud Storage** | Object storage cloud | Ecosistema Google, integración con Drive/Docs | Google.Cloud.Storage.V1 | Bajo (pay-per-use) |
| **Sistema de archivos local** | Disco del servidor | Desarrollo local, deploy on-premise en IIS | System.IO (built-in) | Costo del disco |
| **FTP/SFTP** | Protocolo de transferencia | Integración con sistemas legacy, intercambio con terceros | FluentFTP / SSH.NET | Costo del servidor |
| **Google Drive** | Cloud storage orientado a usuario | Integración con documentos del tenant, compartir archivos | Google.Apis.Drive.v3 | Gratis hasta 15GB por cuenta |

### 1.4 Integración con Google Drive

Google Drive no es un object storage como S3. Es un servicio orientado a documentos con permisos por usuario. Se integra como fuente externa, no como storage primario.

**Escenarios de integración:**

- El tenant quiere importar documentos desde su Google Drive al sistema de formularios
- El tenant quiere exportar reportes generados al Drive de su organización
- Sincronización bidireccional de documentos entre la plataforma y Drive

**Implementación en .NET:**

Se usa el SDK `Google.Apis.Drive.v3` con autenticación OAuth2 (el usuario autoriza acceso a su Drive). La integración se implementa como un `IExternalStorageConnector` que no reemplaza al storage primario sino que importa/exporta desde/hacia él.

**Modelo de permisos:** Google Drive usa OAuth2 con scopes como `drive.file` (solo archivos creados por la app) o `drive.readonly` (lectura de todo el Drive). Se recomienda usar el scope mínimo necesario.

### 1.5 Integración con FTP/SFTP

FTP sigue siendo relevante para integración con sistemas legacy, intercambio de archivos con bancos y organismos públicos, y procesos batch de importación/exportación.

**Librerías .NET recomendadas:**

| Librería | Protocolo | Licencia | Estado |
|---|---|---|---|
| FluentFTP | FTP/FTPS | MIT | Activamente mantenida |
| SSH.NET | SFTP/SCP | MIT | Activamente mantenida |
| WinSCP .NET Assembly | FTP/SFTP/SCP | GPL (wrapper del cliente WinSCP) | Activa |

**Patrón de integración:** un background worker (Hosted Service) hace polling del servidor FTP en intervalos configurables, descarga los archivos nuevos, los sube al storage primario (S3/Azure), registra la metadata en la base de datos, y opcionalmente mueve o elimina el archivo del FTP.

### 1.6 Integración con S3 y compatibles

S3 (Simple Storage Service) de Amazon es el estándar de facto. Su API se convirtió en protocolo de facto: otros proveedores la implementan para compatibilidad (MinIO, Wasabi, Backblaze B2, DigitalOcean Spaces, Cloudflare R2).

**Conceptos clave de S3:**

- **Bucket:** contenedor de nivel superior (equivalente a una carpeta raíz por microservicio o por tenant)
- **Object key:** la ruta completa del archivo dentro del bucket (`tenantA/chat/2025/01/abc123.jpg`)
- **Presigned URL:** URL temporal firmada criptográficamente que permite acceso sin credenciales. Se usa para upload directo desde el browser (PUT presigned) y download sin exponer el bucket
- **Lifecycle policies:** reglas automáticas para mover archivos a storage más barato (Glacier/Archive) o eliminarlos después de X días
- **Versioning:** mantiene todas las versiones de un archivo, permite recuperar versiones anteriores

**MinIO como alternativa self-hosted:** MinIO es 100% compatible con la API de S3. Se deploya como container Docker. Es ideal para desarrollo local (reemplaza S3 en docker-compose) y para producción on-premise. El mismo código que funciona con S3 funciona con MinIO cambiando solo la URL del endpoint.

### 1.7 Almacenamiento local y sistema de archivos

Para deploy en IIS o desarrollo local sin cloud, se implementa `LocalFileStorage` que guarda archivos en disco. La estructura de carpetas en disco replica la convención de S3: `{base-path}/{tenant-id}/{service-id}/{year}/{month}/{file-id}.ext`.

Se debe considerar: backups del disco, espacio disponible, permisos del Application Pool en IIS (la carpeta de storage debe ser accesible por el pool del File Service), y que no es escalable horizontalmente (si hay múltiples instancias del servicio, necesitan acceso a un disco compartido o NAS).

### 1.8 Microservicio de archivos en la arquitectura

El File Service es un microservicio transversal, similar al Identity Service o al Config Service. Los demás microservicios le delegan el almacenamiento y lo referencian por ID.

**Flujo de upload (ejemplo: usuario sube imagen al chat):**

1. El Chat UI solicita al File Service un presigned URL de upload via `POST /api/files/upload-url`
2. El File Service genera la URL firmada y la retorna (junto con el `fileId` asignado)
3. El Chat UI sube el archivo directamente al storage (S3/Azure) usando la URL presigned, sin pasar por el backend
4. El Chat UI notifica al Chat Service que el mensaje tiene un archivo adjunto, enviando el `fileId`
5. El Chat Service registra el `fileId` en su base de datos (tabla `Messages`) y publica un evento `MessageWithAttachmentSent`
6. El File Service escucha el evento y registra la relación archivo-servicio en su metadata

**Flujo de download:**

1. El Chat UI necesita mostrar una imagen adjunta. Solicita al File Service `GET /api/files/{fileId}/url`
2. El File Service verifica permisos (el usuario pertenece al tenant dueño del archivo), genera una URL presigned de lectura con expiración de 15 minutos, y la retorna
3. El Chat UI usa la URL para mostrar la imagen directamente desde S3/Azure

**Ventaja de este diseño:** el archivo viaja directo entre el browser y el storage cloud, sin pasar por los servidores de la plataforma. Reduce carga y latencia.

### 1.9 Seguridad y validación de archivos

**Validación en upload:**

- Verificar extensión y MIME type contra whitelist (`image/jpeg`, `image/png`, `application/pdf`, etc.)
- Verificar magic bytes (los primeros bytes del archivo) para confirmar que el MIME type declarado es real. Una imagen `.jpg` debe empezar con `FF D8 FF`
- Limitar tamaño máximo por tipo de archivo y por tenant (configurable via manifest)
- Escanear con antivirus (ClamAV via `nClam` para .NET, o un servicio de scanning como AWS GuardDuty)
- Generar hash SHA-256 del contenido para detección de duplicados e integridad

**Protección en acceso:**

- URLs presigned de corta duración (5-60 min) — nunca URLs públicas permanentes
- Verificar `tenant_id` del usuario contra el `tenant_id` del archivo
- Logs de acceso a archivos para auditoría
- Encriptación at-rest (S3 SSE, Azure Storage Encryption) y in-transit (HTTPS)

---

## 2. Gestión de imágenes y multimedia

### 2.1 Pipeline de procesamiento de imágenes

Cuando un usuario sube una imagen, el sistema ejecuta un pipeline asíncrono de procesamiento:

1. **Validación:** verificar formato, tamaño, magic bytes
2. **Generación de thumbnails:** crear variantes en múltiples resoluciones
3. **Extracción de metadata:** EXIF data (fecha, ubicación, cámara). Opcionalmente sanitizar EXIF para privacidad
4. **Compresión:** optimizar peso sin pérdida visible de calidad
5. **Almacenamiento:** guardar original + variantes en el storage
6. **Registro:** persistir metadata en la base de datos del File Service

**Librerías de procesamiento de imágenes en .NET:**

| Librería | Plataforma | Licencia | Uso recomendado |
|---|---|---|---|
| **ImageSharp** | Cross-platform, managed .NET | Apache 2.0 (dual license) | Producción multiplataforma, Docker |
| **SkiaSharp** | Cross-platform via Skia (nativo) | MIT | Alto rendimiento, operaciones complejas |
| **System.Drawing** | Windows only | Built-in | Solo en deploy IIS Windows (no Docker Linux) |
| **MagickNET** | Cross-platform (ImageMagick wrapper) | Apache 2.0 | Formatos exóticos, operaciones batch avanzadas |

**Recomendación:** ImageSharp para la mayoría de los casos. Es 100% managed .NET (sin dependencias nativas), funciona en Linux/Docker, y soporta resize, crop, format conversion, watermark, EXIF stripping.

### 2.2 Thumbnails y variantes

Al subir una imagen, se generan múltiples variantes para diferentes contextos de uso:

| Variante | Resolución | Calidad | Formato | Uso |
|---|---|---|---|---|
| `thumb` | 150x150 (crop cuadrado) | 70% | WebP/JPEG | Listados, avatares, grids |
| `preview` | 800px ancho máx | 80% | WebP/JPEG | Vista previa en chat, galerías |
| `original` | Sin modificar | Original | Original | Descarga del archivo original |
| `blur` | 32x32 | 30% | JPEG | Placeholder blur-up mientras carga |

Las variantes se almacenan con el mismo `fileId` pero diferente sufijo: `{fileId}_thumb.webp`, `{fileId}_preview.webp`, `{fileId}_blur.jpg`. El frontend solicita la variante apropiada según el contexto.

### 2.3 Estrategia WhatsApp: caché local + descarga bajo demanda

WhatsApp implementa una estrategia sofisticada que optimiza ancho de banda y almacenamiento. Su enfoque se puede replicar en nuestra plataforma:

**Cómo funciona WhatsApp:**

- Los mensajes de texto se almacenan localmente en SQLite en el dispositivo. El servidor solo los retiene hasta que el receptor los recibe, luego los elimina
- Las imágenes y videos se suben al servidor (encriptados end-to-end), y el servidor envía una referencia (URL + clave de desencriptación) al receptor
- El receptor recibe primero un thumbnail borroso de muy bajo peso (incluido en el mensaje). La imagen completa se descarga solo cuando el usuario toca para verla, o si tiene activada la descarga automática
- Los archivos descargados se cachean localmente en la estructura `WhatsApp/Media/{tipo}/`. El servidor los retiene por un tiempo limitado (aproximadamente 30 días)
- Si el usuario necesita un archivo antiguo que ya no está en el servidor, WhatsApp no puede recuperarlo. Por eso ofrece backup a Google Drive/iCloud

**Cómo replicar esta estrategia en nuestra plataforma:**

1. **Upload:** el usuario sube una imagen al chat. El File Service genera thumbnail, preview y blur placeholder
2. **Notificación:** el mensaje de chat incluye el `fileId` y el `blur` placeholder (inline como base64 de ~200 bytes)
3. **Renderizado inmediato:** el frontend muestra el blur placeholder instantáneamente (el usuario ve una versión borrosa de la imagen mientras carga)
4. **Descarga progresiva:** el frontend solicita la variante `preview` (800px). Cuando llega, reemplaza el blur con una transición suave
5. **Full resolution bajo demanda:** solo si el usuario toca "ver original" se descarga la resolución completa
6. **Caché local:** las imágenes descargadas se cachean en el browser (Cache API / IndexedDB para apps web) o en disco local (para apps mobile)
7. **Política de retención:** los archivos en el storage se mueven a almacenamiento frío (S3 Glacier / Azure Cool) después de 90 días. Si el usuario los necesita, se restauran bajo demanda (con un delay de minutos a horas)

### 2.4 Lazy loading progresivo (blur-up)

La técnica "blur-up" (o LQIP, Low Quality Image Placeholder) es el estándar de la industria para UX de carga de imágenes:

1. Generar un placeholder de 32x32 pixels, pesantemente comprimido (200-500 bytes)
2. Codificarlo en base64 y guardarlo en la base de datos junto con la metadata del archivo
3. Al renderizar la lista de mensajes, la imagen se muestra inmediatamente con el blur placeholder (ya está en el JSON del mensaje)
4. En paralelo, el browser solicita la imagen real
5. Cuando llega la imagen real, se transiciona con un fade de 300ms del blur al real

**Alternativa moderna:** BlurHash. Un algoritmo que codifica una representación compacta de la imagen en ~20-30 caracteres ASCII. Se almacena en la base de datos como string y se decodifica en el cliente para generar el placeholder. Librería .NET: `BlurHashSharp`.

### 2.5 CDN y distribución geográfica

Para imágenes y archivos estáticos, se coloca un CDN (Content Delivery Network) delante del storage:

- **CloudFront** (para S3), **Azure CDN** (para Azure Blob), o **Cloudflare** (proveedor-agnóstico)
- El CDN cachea los archivos en edge locations cercanas al usuario, reduciendo latencia
- Las URLs presigned se generan para el dominio del CDN, no del bucket directamente
- Se configuran headers `Cache-Control` apropiados: thumbnails con cache largo (30 días), originales con cache medio (1 día)

### 2.6 Manejo de multimedia desde apps móviles

Las aplicaciones móviles (MAUI, nativas, Flutter) tienen consideraciones especiales:

**Upload desde mobile:**

- Comprimir imágenes antes de subir: reducir resolución (max 2048px) y calidad (85%) en el cliente
- Upload resumible: usar protocolos como tus (protocolo de upload resumible) para archivos grandes. Si la conexión se corta, el upload se retoma desde donde quedó
- Upload en background: usar capacidades del OS para continuar el upload aunque la app esté en background
- Metadata de la cámara: incluir orientación EXIF para que el servidor procese correctamente la rotación

**Caché local en mobile:**

- Base de datos local (SQLite via Entity Framework Core / LiteDB) para metadata y referencias
- Archivos en disco local con política LRU (Least Recently Used): cuando el cache excede un tamaño configurable (ej: 500MB), se eliminan los archivos más antiguos no accedidos
- Al abrir una conversación: primero se muestran los blur placeholders desde la DB local, luego se descargan los previews, y bajo demanda los originales

---

## 3. Gestión de video

### 3.1 Escenarios de video en la plataforma

| Escenario | Tipo | Fuente | Ejemplo |
|---|---|---|---|
| Videomensaje en chat | Upload + VOD | Cámara del dispositivo | El usuario graba un video de 30s y lo envía en el chat |
| Video de soporte | Upload + VOD | Pantalla/webcam | El agente graba su pantalla mostrando cómo resolver un problema |
| Videoconferencia | Live bidireccional | Webcam + micrófono | Llamada de soporte en vivo |
| Vigilancia/CCTV | Live unidireccional | Cámara IP (RTSP) | Monitor de seguridad en tiempo real |
| Training/onboarding | VOD | Archivo pre-grabado | Videos de capacitación para los usuarios del tenant |
| Grabación de reuniones | Record + VOD | WebRTC capture | Grabar la videoconferencia para revisión posterior |

### 3.2 Formatos, codecs y contenedores

**Conceptos fundamentales:**

- **Codec:** algoritmo de compresión/descompresión de video o audio. Ejemplo: H.264, H.265 (HEVC), VP9, AV1
- **Contenedor:** formato de archivo que empaqueta video, audio, subtítulos y metadata. Ejemplo: MP4 (.mp4), WebM (.webm), MKV (.mkv), MPEG-TS (.ts)
- **Bitrate:** cantidad de datos por segundo de video. Más bitrate significa más calidad pero más peso

| Codec de video | Compresión | Compatibilidad | Uso recomendado |
|---|---|---|---|
| **H.264 (AVC)** | Buena | Universal (todos los browsers y dispositivos) | Compatibilidad máxima, streaming estándar |
| **H.265 (HEVC)** | 50% mejor que H.264 | Apple, hardware moderno (limitado en browsers) | iOS/macOS, archivado, ahorro de bandwidth |
| **VP9** | Similar a H.265 | Chrome, Firefox, Android (no Safari) | YouTube, WebM, entornos Google |
| **AV1** | 30% mejor que H.265 | Chrome, Firefox, Edge (soporte creciente) | Futuro estándar, Netflix, YouTube ya lo usan |

**Recomendación:** H.264 en contenedor MP4 como formato universal. WebM/VP9 como alternativa para clientes que lo soporten. Generar ambas variantes si el ahorro de bandwidth justifica el costo de transcodificación.

### 3.3 Protocolos de streaming

| Protocolo | Tipo | Latencia | Compatibilidad | Uso |
|---|---|---|---|---|
| **HLS** (HTTP Live Streaming) | Adaptativo sobre HTTP | 6-30 seg | Universal (todos los browsers, mobile) | VOD, live streaming general |
| **DASH** (Dynamic Adaptive Streaming over HTTP) | Adaptativo sobre HTTP | 6-30 seg | Amplia (no Safari nativo) | VOD, alternativa abierta a HLS |
| **WebRTC** | Peer-to-peer en tiempo real | < 500ms | Todos los browsers modernos | Videoconferencia, ultra-baja latencia |
| **RTSP** | Streaming en tiempo real | 1-3 seg | Clientes nativos, VLC (no browsers) | Cámaras IP, vigilancia |
| **RTMP** | Streaming de publicación | 1-5 seg | Servidores de ingest (no browsers) | Ingest de OBS/cámaras al servidor |
| **LL-HLS** (Low-Latency HLS) | Adaptativo baja latencia | 2-5 seg | Safari, soporte creciente | Live con baja latencia sobre HTTP |

**Recomendación:**

- Para VOD (videos pregrabados): HLS con adaptive bitrate. Universalmente compatible.
- Para videoconferencia: WebRTC. Menor latencia posible.
- Para ingest de cámaras IP: RTSP desde la cámara, transcodificación a HLS con FFmpeg para servir al browser.
- Para live streaming unidireccional (ej: streaming de un evento): RTMP para ingest desde OBS, transcodificación a HLS para distribución.

### 3.4 Pipeline de procesamiento con FFmpeg

FFmpeg es la herramienta estándar de la industria para procesamiento de video. En .NET se integra de dos formas:

**Opción A — FFmpeg como proceso externo (recomendada):**

Se ejecuta el binario de FFmpeg como un proceso hijo desde .NET. Librerías wrapper:

| Librería | Descripción | NuGet |
|---|---|---|
| **Xabe.FFmpeg** | Wrapper fluent, descarga automática de binarios | Xabe.FFmpeg |
| **FFMpegCore** | API fluent, buena documentación | FFMpegCore |
| **CliWrap** | Wrapper genérico de procesos CLI (no específico de FFmpeg) | CliWrap |

Ejemplo con FFMpegCore (transcodificar a HLS multi-quality):

```csharp
await FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath, overwrite: true, options => options
        .WithVideoCodec(VideoCodec.LibX264)
        .WithAudioCodec(AudioCodec.Aac)
        .WithCustomArgument("-preset veryfast")
        .WithCustomArgument("-hls_time 6")
        .WithCustomArgument("-hls_playlist_type vod")
        .WithCustomArgument("-f hls"))
    .ProcessAsynchronously();
```

**Opción B — MediaToolkit / libav bindings:**

Para casos donde se necesita procesamiento frame-by-frame en memoria sin escribir a disco. Más complejo pero más eficiente para pipelines de procesamiento intensivo.

**Pipeline típico de procesamiento de video:**

1. El usuario sube un video (upload directo a S3 via presigned URL)
2. El File Service publica un evento `VideoUploadedEvent` con el `fileId` y la ubicación en S3
3. Un worker (Video Processing Service) escucha el evento:
   a. Descarga el video original desde S3
   b. Extrae un frame como thumbnail (al 10% de la duración)
   c. Genera un blur placeholder del thumbnail
   d. Transcodifica a múltiples resoluciones (480p, 720p, 1080p) en formato HLS
   e. Sube los segmentos HLS y playlists a S3
   f. Actualiza la metadata en la base de datos con las variantes disponibles
   g. Publica un evento `VideoProcessedEvent`
4. El frontend del chat recibe la notificación y ya puede reproducir el video con adaptive bitrate

### 3.5 Grabación local y upload diferido

Para escenarios donde la conexión es inestable o el video es largo:

**En el browser (Blazor Interactive Server):**

- MediaRecorder API (JavaScript interop desde Blazor) captura webcam/pantalla
- Los chunks se almacenan en IndexedDB del browser durante la grabación
- Al finalizar, se concatenan y suben via presigned URL
- Si la conexión falla durante el upload, se implementa retry con upload resumible

**En apps mobile (MAUI):**

- La grabación usa las APIs nativas del dispositivo (Camera2 en Android, AVFoundation en iOS)
- El video se guarda en almacenamiento local del dispositivo
- Un servicio en background lo sube cuando hay conexión WiFi (configurable)
- Se muestra progreso de upload y se permite cancelar/reintentar

**Protocolo de upload resumible (tus-protocol):**

Para archivos de video grandes (cientos de MB), se recomienda el protocolo tus (https://tus.io). Permite: pausar y reanudar uploads, continuar después de una desconexión, upload en chunks paralelos para maximizar throughput. Librería .NET server: `tusdotnet`.

### 3.6 Fuentes de video: cámaras IP, webcam, pantalla

**Cámaras IP (RTSP):**

Las cámaras IP transmiten via RTSP (Real-Time Streaming Protocol). Para mostrarlas en un browser (que no soporta RTSP nativamente), se necesita un servidor de transcodificación:

1. FFmpeg consume el stream RTSP de la cámara: `ffmpeg -i rtsp://camera-ip:554/stream -f hls -hls_time 2 -hls_list_size 5 output.m3u8`
2. Los segmentos HLS se sirven via HTTP (Nginx, Kestrel, o directamente desde el File Service)
3. El browser reproduce el HLS con un player como hls.js o Video.js

**Alternativa open-source:** MediaMTX (antes rtsp-simple-server). Servidor que acepta RTSP de cámaras y lo re-publica como HLS/WebRTC automáticamente, sin necesidad de configurar FFmpeg manualmente.

**Webcam (browser):**

- Se accede via la API `navigator.mediaDevices.getUserMedia()` desde JavaScript
- Desde Blazor Server, se usa JS interop para acceder a la cámara y enviar el stream
- Para grabación: MediaRecorder API
- Para videoconferencia en vivo: WebRTC via librerías como LiveKit, Janus, o mediasoup

**Captura de pantalla:**

- Se accede via `navigator.mediaDevices.getDisplayMedia()` desde JavaScript
- Mismo flujo que webcam pero captura la pantalla en lugar de la cámara

### 3.7 Streaming en vivo (live)

**Arquitectura para live streaming:**

1. **Ingest:** la fuente (OBS, cámara, browser WebRTC) envía el video al servidor via RTMP o WebRTC
2. **Transcodificación:** el servidor transcodifica en tiempo real a múltiples calidades HLS
3. **Distribución:** los segmentos HLS se distribuyen via CDN a los espectadores
4. **Reproducción:** el browser del espectador consume el HLS con adaptive bitrate

**Soluciones en .NET / self-hosted:**

| Solución | Tipo | Latencia | Complejidad | Uso |
|---|---|---|---|---|
| **LiveKit** | WebRTC SFU | < 500ms | Media | Videoconferencia, salas interactivas |
| **MediaMTX** | RTSP/RTMP/HLS server | 2-6 seg | Baja | Rebroadcast de cámaras, simple live |
| **Nginx-RTMP** | RTMP ingest + HLS output | 6-15 seg | Baja | Live streaming clásico |
| **OvenMediaEngine** | Multi-protocol server | 1-3 seg | Media | Live con baja latencia |
| **Azure Media Services** | SaaS managed | 6-30 seg | Baja | Sin mantenimiento de infraestructura |
| **AWS MediaLive** | SaaS managed | 6-30 seg | Baja | Sin mantenimiento de infraestructura |

### 3.8 Video bajo demanda (VOD)

Para videos pre-grabados que se reproducen on-demand (training, mensajes de video, grabaciones):

**Flujo completo:**

1. Upload del video original a S3 (via presigned URL)
2. Transcodificación asíncrona a HLS multi-quality con FFmpeg (en un worker)
3. Generación de thumbnail, blur placeholder, y metadata (duración, resolución, codec)
4. Los segmentos HLS quedan en S3, servidos via CDN
5. El frontend reproduce con un player HLS (hls.js, Video.js, Plyr)

**Estructura de archivos HLS en S3:**

```
{tenant-id}/videos/{video-id}/
├── master.m3u8           ← Playlist maestra (lista las calidades)
├── 480p/
│   ├── stream.m3u8       ← Playlist de la calidad 480p
│   ├── segment_000.ts    ← Segmentos de video de ~6 segundos
│   ├── segment_001.ts
│   └── ...
├── 720p/
│   ├── stream.m3u8
│   └── ...
├── 1080p/
│   ├── stream.m3u8
│   └── ...
├── thumb.jpg              ← Thumbnail del video
└── blur.txt               ← BlurHash placeholder
```

---

## 4. Compresión de video y archivos

### 4.1 Compresión de archivos generales

| Algoritmo | Extensión | Velocidad | Ratio | Librería .NET | Uso |
|---|---|---|---|---|---|
| **Gzip** | .gz | Rápida | Moderado | System.IO.Compression (built-in) | HTTP compression, archivos individuales |
| **Brotli** | .br | Media | Superior a gzip | System.IO.Compression (built-in) | HTTP compression (mejor ratio que gzip) |
| **Deflate** | .zip | Rápida | Moderado | System.IO.Compression (built-in) | Archivos ZIP |
| **Zstandard (Zstd)** | .zst | Muy rápida | Excelente | ZstdSharp.Port | Archivado, backups, alta velocidad |
| **LZ4** | .lz4 | Extremadamente rápida | Menor | K4os.Compression.LZ4 | Caché, comunicación en tiempo real |

**Recomendación:** Brotli para respuestas HTTP (middleware `app.UseResponseCompression()` con Brotli provider). Zstd para archivado y backups. ZIP para descargas que el usuario abrirá en su computadora.

### 4.2 Compresión de imágenes

| Formato | Tipo | Compresión | Soporte browsers | Uso recomendado |
|---|---|---|---|---|
| **JPEG** | Con pérdida | Buena (70-85% calidad es el sweet spot) | Universal | Fotos, imágenes con gradientes |
| **PNG** | Sin pérdida | Moderada | Universal | Capturas de pantalla, imágenes con texto/transparencia |
| **WebP** | Con o sin pérdida | 25-34% menor que JPEG equivalente | Todos los browsers modernos | Reemplazo general de JPEG y PNG |
| **AVIF** | Con o sin pérdida | 50% menor que JPEG equivalente | Chrome, Firefox (soporte creciente) | Futuro estándar, usar con fallback a WebP |

**Estrategia de serving:**

El servidor detecta los formatos que el browser acepta (header `Accept: image/avif, image/webp, image/*`) y sirve el formato más eficiente soportado. Se almacenan múltiples formatos de cada imagen o se transcodifica al vuelo con cache.

### 4.3 Compresión y transcodificación de video

La transcodificación es el proceso de convertir un video de un codec/resolución/bitrate a otro. Es intensiva en CPU (o GPU con hardware acceleration).

**Perfiles de transcodificación con FFmpeg:**

```bash
# 1080p alta calidad
ffmpeg -i input.mp4 -c:v libx264 -preset slow -crf 22 -c:a aac -b:a 128k output_1080p.mp4

# 720p balance calidad/peso
ffmpeg -i input.mp4 -c:v libx264 -preset medium -crf 24 -vf scale=-2:720 -c:a aac -b:a 96k output_720p.mp4

# 480p liviano (mobile con conexión lenta)
ffmpeg -i input.mp4 -c:v libx264 -preset fast -crf 28 -vf scale=-2:480 -c:a aac -b:a 64k output_480p.mp4
```

**Parámetros clave:**

- **CRF (Constant Rate Factor):** controla la calidad. Rango 0-51. Valores típicos: 18 (alta calidad, archivo grande), 23 (default, buen balance), 28 (calidad aceptable, archivo pequeño)
- **Preset:** controla la velocidad vs eficiencia de compresión. `ultrafast` es rápido pero archivos grandes. `slow` es lento pero archivos más pequeños con la misma calidad
- **Scale:** redimensionar. `-2:720` escala la altura a 720px manteniendo aspect ratio. `-2` asegura que el ancho sea divisible por 2 (requerimiento de H.264)

**Hardware acceleration:**

Para procesamiento más rápido, FFmpeg soporta codificación por GPU:

| Motor | GPU | Flag FFmpeg | Ganancia |
|---|---|---|---|
| NVENC | NVIDIA | `-c:v h264_nvenc` | 5-10x más rápido |
| QSV | Intel | `-c:v h264_qsv` | 3-5x más rápido |
| AMF | AMD | `-c:v h264_amf` | 3-5x más rápido |
| VideoToolbox | Apple | `-c:v h264_videotoolbox` | 3-5x más rápido |

### 4.4 Perfiles de compresión recomendados

| Escenario | Codec | CRF | Preset | Resolución | Bitrate audio | Peso aprox. por minuto |
|---|---|---|---|---|---|---|
| Videomensaje en chat | H.264 | 26 | fast | 720p | 64k AAC | ~5 MB |
| Video de soporte | H.264 | 24 | medium | 1080p | 96k AAC | ~12 MB |
| Training/onboarding | H.264 | 22 | slow | 1080p | 128k AAC | ~18 MB |
| Grabación de reunión | H.264 | 28 | veryfast | 720p | 64k AAC | ~3 MB |
| CCTV archivado | H.265 | 30 | medium | 720p | No | ~1.5 MB |

---

## 5. Firma digital de video y archivos

### 5.1 Firma digital de documentos

La firma digital garantiza que un documento no fue alterado después de firmado y que proviene de quien dice.

**Estándares:**

| Estándar | Uso | Librería .NET |
|---|---|---|
| **PKCS#7 / CMS** | Firma genérica de archivos | System.Security.Cryptography.Pkcs (built-in) |
| **PAdES** | Firma de PDFs (estándar europeo eIDAS) | iText (comercial), Docusign SDK |
| **XAdES** | Firma de XML (facturas electrónicas) | Microsoft.Xades, FirmaXadesNet |
| **CAdES** | Firma de cualquier archivo binario | BouncyCastle |
| **JSON Web Signature (JWS)** | Firma de datos JSON (APIs) | Microsoft.IdentityModel.Tokens (built-in) |

**Flujo en la plataforma:**

1. El usuario sube un documento a firmar
2. El servicio calcula el hash SHA-256 del documento
3. El hash se firma con la clave privada del firmante (puede ser un certificado X.509 emitido por una CA, o un par de claves RSA/ECDSA generado por la plataforma)
4. La firma (hash firmado + certificado público) se almacena como metadata del archivo
5. Para verificar: se recalcula el hash del documento, se desencripta la firma con la clave pública, y se comparan. Si coinciden, el documento no fue alterado

### 5.2 Firma de video (integridad y autenticidad)

Firmar un video completo como un solo archivo es ineficiente para archivos grandes. La industria usa dos enfoques:

**Enfoque 1 — Hash del archivo completo:**

Se calcula SHA-256 del archivo de video completo y se firma el hash. Simple pero requiere descargar todo el video para verificar.

```csharp
using var stream = File.OpenRead(videoPath);
var hash = await SHA256.HashDataAsync(stream);
var signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
```

**Enfoque 2 — Hash por segmentos (Merkle tree):**

Se divide el video en segmentos (los mismos segmentos HLS), se calcula el hash de cada segmento, y se construye un árbol Merkle. La raíz del árbol se firma. Esto permite verificar la integridad de cualquier segmento individual sin necesidad de descargar todo el video.

**Enfoque 3 — Metadata embebida:**

Se embeben hashes y firmas en los metadata containers del archivo de video (atoms de MP4, tags de MKV). Librerías: `TagLib#` para leer/escribir metadata de archivos multimedia.

### 5.3 Marca de agua digital (watermarking)

El watermarking embebe información invisible (o visible) en el contenido para rastreo de distribución:

**Watermark visible:**

- Logo o texto superpuesto en el video usando FFmpeg: `ffmpeg -i input.mp4 -i logo.png -filter_complex "overlay=10:10" output.mp4`
- Desde .NET con FFMpegCore o Xabe.FFmpeg como proceso

**Watermark invisible (steganography):**

- Se altera levemente los frames del video para embeber un identificador (tenant ID, user ID, timestamp)
- Resistente a re-codificación y captura de pantalla
- Librerías especializadas: OpenStego, técnicas basadas en DCT (Discrete Cosine Transform)

**Watermark forense por sesión:**

Cada vez que un usuario ve un video, se genera una variante con un watermark invisible único vinculado a su sesión. Si el video se filtra, se puede rastrear quién lo distribuyó. Netflix y Disney+ usan este enfoque.

### 5.4 Cadena de custodia digital

Para videos con valor legal (grabaciones de seguridad, evidencia, videoconferencias contractuales):

1. **Timestamp autoritativo:** al grabar, se solicita un timestamp firmado a una TSA (Time Stamping Authority) certificada. Esto prueba que el video existía en ese momento (RFC 3161)
2. **Hash chain:** se encadenan los hashes de cada segmento con el hash del segmento anterior, creando una cadena inmutable. Cualquier alteración rompe la cadena
3. **Log de acceso inmutable:** cada acceso al video (quién, cuándo, desde dónde) se registra en un log append-only
4. **Almacenamiento WORM:** Write Once Read Many. S3 Object Lock o Azure Immutable Blob Storage impiden la modificación o eliminación del archivo durante un período definido

### 5.5 DRM y protección de contenido

DRM (Digital Rights Management) es necesario cuando se distribuye contenido premium que los usuarios no deben poder descargar libremente.

| Sistema DRM | Plataformas | Integración | Uso |
|---|---|---|---|
| **Widevine** (Google) | Chrome, Firefox, Android, smart TVs | Licencia gratuita, requiere servidor de licencias | Streaming protegido cross-platform |
| **FairPlay** (Apple) | Safari, iOS, Apple TV | Requiere Apple Developer account | Protección en ecosistema Apple |
| **PlayReady** (Microsoft) | Edge, Windows, Xbox | Licencia comercial | Protección en ecosistema Microsoft |

**Implementación básica sin DRM completo:**

Para casos donde no se necesita DRM de nivel Netflix pero sí protección básica:

- URLs presigned de corta duración (el enlace expira en minutos)
- Encriptación AES-128 de los segmentos HLS (FFmpeg soporta esto nativamente)
- Token de autorización en cada request de segmento
- Headers HTTP que previenen caching del browser (`Cache-Control: no-store`)

---

## 6. Arquitectura integrada

### 6.1 Cómo encaja el File Service en el ecosistema

El File Service es un microservicio transversal que todos los demás servicios consumen:

- **Chat Service** le delega el storage de imágenes, videos y documentos enviados en conversaciones
- **AI Assistant** le delega el storage de archivos que el usuario sube para análisis
- **Forms Service** le delega el storage de adjuntos de formularios
- **Payment Service** le delega el storage de comprobantes y facturas

Cada servicio referencia archivos por `fileId` (GUID). El File Service es el único que conoce dónde físicamente están almacenados (S3, Azure, disco local) y cómo accederlos.

### 6.2 Estructura de carpetas del microservicio

```
src/Services/FileStorage/
├── FileStorage.Domain/
│   ├── Entities/
│   │   ├── StoredFile.cs            ← ID, nombre, MIME, tamaño, hash, tenant
│   │   ├── FileVariant.cs           ← Variante (thumb, preview, original, HLS)
│   │   └── FileAccessLog.cs         ← Auditoría de acceso
│   ├── Enums/
│   │   ├── FileCategory.cs          ← Image, Video, Document, Audio
│   │   └── VariantType.cs           ← Original, Thumb, Preview, Blur, HLS
│   └── Interfaces/
│       ├── IFileStorage.cs          ← Abstracción de proveedor
│       └── IFileRepository.cs       ← Persistencia de metadata
│
├── FileStorage.Application/
│   ├── Commands/
│   │   ├── RequestUploadUrlCommand.cs
│   │   ├── ConfirmUploadCommand.cs
│   │   ├── DeleteFileCommand.cs
│   │   └── ProcessVideoCommand.cs
│   ├── Queries/
│   │   ├── GetFileMetadataQuery.cs
│   │   ├── GetDownloadUrlQuery.cs
│   │   └── ListFilesQuery.cs
│   └── EventHandlers/
│       └── VideoUploadedEventHandler.cs
│
├── FileStorage.Infrastructure/
│   ├── Providers/
│   │   ├── S3FileStorage.cs         ← Implementación AWS S3 / MinIO
│   │   ├── AzureBlobFileStorage.cs  ← Implementación Azure Blob
│   │   ├── LocalFileStorage.cs      ← Implementación disco local
│   │   └── FtpFileStorage.cs        ← Implementación FTP/SFTP
│   ├── Processing/
│   │   ├── ImageProcessor.cs        ← Thumbnails, resize, blur (ImageSharp)
│   │   ├── VideoProcessor.cs        ← Transcodificación HLS (FFmpeg)
│   │   └── FileValidator.cs         ← Magic bytes, antivirus
│   ├── Persistence/
│   │   ├── FileStorageDbContext.cs
│   │   └── Repositories/
│   └── DependencyInjection.cs
│
├── FileStorage.API/
│   ├── Controllers/
│   │   ├── FilesController.cs       ← Upload URL, download URL, metadata
│   │   ├── StreamController.cs      ← Serve HLS playlists/segments
│   │   └── AdminController.cs       ← Manifest + settings
│   ├── Program.cs
│   └── Dockerfile
│
└── FileStorage.Admin.RCL/            ← Panel admin: cuotas, métricas de uso
    ├── Pages/
    │   └── StorageDashboard.razor
    └── FileStorageAdminModule.cs
```

### 6.3 Eventos de integración

| Evento | Publicado por | Consumido por | Payload |
|---|---|---|---|
| `FileUploadConfirmed` | File Service | Chat, Forms, AI | `{ fileId, tenantId, category, mimeType, size }` |
| `ImageProcessed` | File Service | Chat, Forms | `{ fileId, variants: [thumb, preview, blur] }` |
| `VideoProcessingStarted` | File Service | Chat (para mostrar progreso) | `{ fileId, estimatedDuration }` |
| `VideoProcessed` | File Service | Chat, cualquier consumidor | `{ fileId, variants: [480p, 720p, 1080p], hlsManifestUrl }` |
| `VideoProcessingFailed` | File Service | Chat (para notificar error) | `{ fileId, error }` |
| `FileDeleted` | File Service | Todos los consumidores | `{ fileId, tenantId }` |
| `StorageQuotaExceeded` | File Service | Admin Portal, notificaciones | `{ tenantId, currentUsage, quota }` |

---

## 7. Glosario de términos

| Término | Definición |
|---|---|
| **ABR** | Adaptive Bitrate Streaming. El player cambia automáticamente entre calidades de video según la velocidad de conexión del usuario. |
| **AES-128** | Advanced Encryption Standard con clave de 128 bits. Usado para encriptar segmentos HLS y proteger contenido de video. |
| **AV1** | Codec de video open-source desarrollado por Alliance for Open Media. Superior compresión a H.265, soporte creciente en browsers. |
| **Bitrate** | Cantidad de datos procesados por unidad de tiempo en un stream de video o audio. Se mide en kbps o Mbps. Mayor bitrate significa mayor calidad y mayor peso. |
| **Blob** | Binary Large Object. Término genérico para cualquier archivo binario almacenado en cloud storage (imagen, video, documento, etc.). |
| **BlurHash** | Algoritmo que codifica una representación compacta de una imagen en ~20-30 caracteres ASCII. Se decodifica en el cliente para generar un placeholder borroso instantáneo. |
| **Bucket** | Contenedor de nivel superior en object storage (S3, GCS). Equivalente a una carpeta raíz. Cada bucket tiene sus propias políticas de acceso y lifecycle. |
| **CDN** | Content Delivery Network. Red de servidores distribuidos geográficamente que cachean contenido estático (imágenes, videos, archivos) cerca del usuario final para reducir latencia. |
| **CRF** | Constant Rate Factor. Parámetro de FFmpeg que controla la calidad de compresión de video. Rango 0 (sin pérdida) a 51 (máxima compresión). Default: 23. |
| **DASH** | Dynamic Adaptive Streaming over HTTP. Protocolo de streaming adaptativo, alternativa abierta a HLS. Estándar ISO/IEC 23009. |
| **DRM** | Digital Rights Management. Tecnología de protección de contenido que controla cómo y quién puede reproducir un video. Widevine (Google), FairPlay (Apple), PlayReady (Microsoft). |
| **EXIF** | Exchangeable Image File Format. Metadata embebida en fotos: fecha, hora, GPS, modelo de cámara, apertura, etc. Se debe sanitizar por privacidad antes de compartir. |
| **FFmpeg** | Framework open-source para grabar, convertir y transmitir audio y video. Herramienta de línea de comandos estándar de la industria para procesamiento multimedia. |
| **FTP/SFTP** | File Transfer Protocol / SSH File Transfer Protocol. Protocolos para transferir archivos entre sistemas. FTP es inseguro (texto plano), SFTP encripta la conexión. |
| **H.264 (AVC)** | Codec de video más utilizado. Excelente compatibilidad, soportado por todos los browsers y dispositivos. Estándar ITU-T H.264 / ISO/IEC 14496-10. |
| **H.265 (HEVC)** | Sucesor de H.264 con 50% mejor compresión. Soporte limitado en browsers (patentes). Común en dispositivos Apple y cámaras modernas. |
| **HLS** | HTTP Live Streaming. Protocolo de streaming desarrollado por Apple. Divide el video en segmentos de 2-10 segundos servidos via HTTP. Soportado universalmente. |
| **Ingest** | Punto de entrada donde un stream de video llega al servidor desde la fuente (cámara, OBS, encoder). Protocolos: RTMP, RTSP, SRT, WebRTC. |
| **LQIP** | Low Quality Image Placeholder. Técnica donde se muestra una versión extremadamente pequeña y borrosa de una imagen mientras la versión real carga. Mejora percepción de velocidad. |
| **LRU** | Least Recently Used. Política de evicción de caché: cuando el caché está lleno, se elimina el elemento que hace más tiempo no se accedió. |
| **M3U8** | Formato de playlist usado por HLS. Un archivo de texto que lista los segmentos de video (.ts) a reproducir en orden. La playlist maestra lista las diferentes calidades disponibles. |
| **Magic Bytes** | Los primeros bytes de un archivo que identifican su formato real (independiente de la extensión). JPEG comienza con `FF D8 FF`, PNG con `89 50 4E 47`. Se usan para validar que un archivo es realmente lo que dice ser. |
| **Merkle Tree** | Estructura de datos en árbol donde cada nodo hoja contiene el hash de un bloque de datos, y cada nodo padre contiene el hash de sus hijos. Permite verificar la integridad de cualquier bloque sin procesar todos los demás. |
| **MinIO** | Object storage open-source 100% compatible con la API de S3. Se puede deployar como container Docker. Ideal para desarrollo local y producción on-premise. |
| **MPEG-TS** | MPEG Transport Stream. Contenedor de video usado como formato de segmentos en HLS. Extensión `.ts`. Diseñado para transmisión, tolerante a errores. |
| **Object Key** | La ruta completa de un archivo dentro de un bucket de object storage. Ejemplo: `tenantA/chat/2025/01/abc123.jpg`. |
| **Object Storage** | Sistema de almacenamiento que gestiona datos como "objetos" (archivo + metadata + ID). Escalable, económico, ideal para archivos grandes. S3, Azure Blob, GCS. |
| **Presigned URL** | URL temporal firmada criptográficamente que autoriza acceso a un recurso en object storage sin necesidad de credenciales. Tiene fecha de expiración. |
| **RTMP** | Real-Time Messaging Protocol. Protocolo desarrollado por Adobe para streaming de video. Usado para ingest (enviar video al servidor). No funciona en browsers modernos. |
| **RTSP** | Real-Time Streaming Protocol. Protocolo de streaming usado por cámaras IP y sistemas de vigilancia. No soportado nativamente en browsers. |
| **SFU** | Selective Forwarding Unit. Servidor que recibe streams de video de múltiples participantes y los reenvía selectivamente a los demás, sin transcodificar. Usado en videoconferencia (LiveKit, Janus). |
| **Transcodificación** | Proceso de convertir un video de un codec/resolución/bitrate a otro. Es intensiva en CPU (o GPU). Necesaria para generar múltiples calidades para ABR. |
| **TSA** | Time Stamping Authority. Servicio que emite timestamps firmados digitalmente, probando que un dato existía en un momento específico. Estándar RFC 3161. |
| **tus** | Protocolo open-source para upload resumible de archivos. Permite pausar y reanudar uploads, continuar después de desconexiones. Server .NET: tusdotnet. |
| **VOD** | Video On Demand. Video pregrabado que el usuario puede reproducir en cualquier momento, pausar, adelantar, retroceder. Opuesto a live streaming. |
| **VP9** | Codec de video open-source de Google. Similar compresión a H.265 pero libre de patentes. Soportado en Chrome, Firefox, Android. Formato contenedor: WebM. |
| **Watermark** | Marca (visible o invisible) embebida en el contenido para identificar la fuente, prevenir piratería, o rastrear distribución no autorizada. |
| **WebP** | Formato de imagen de Google que soporta compresión con y sin pérdida. 25-34% menor tamaño que JPEG equivalente. Soportado en todos los browsers modernos. |
| **WebRTC** | Web Real-Time Communication. Tecnología para comunicación peer-to-peer en tiempo real (audio, video, datos) directamente entre browsers. Latencia sub-segundo. |
| **WORM** | Write Once Read Many. Política de almacenamiento donde los datos, una vez escritos, no pueden ser modificados ni eliminados durante un período definido. Para compliance y evidencia legal. |

---

## 8. Guía para agentes IA

### 8.1 Tabla G1 — Roles y especialidades

| ID | Rol | Especialidad | Tecnologías clave |
|---|---|---|---|
| G1-01 | **Arquitecto de almacenamiento** | Diseña la estrategia de storage, elige proveedores, define políticas de retención y acceso | IFileStorage, S3/MinIO, Azure Blob, presigned URLs, lifecycle policies |
| G1-02 | **Desarrollador de procesamiento multimedia** | Implementa pipelines de procesamiento de imágenes y video | FFmpeg (via FFMpegCore/Xabe.FFmpeg), ImageSharp, BlurHash, HLS |
| G1-03 | **Desarrollador de streaming** | Configura infraestructura de streaming en vivo y VOD | HLS, DASH, WebRTC, MediaMTX, LiveKit, hls.js |
| G1-04 | **Especialista en seguridad de archivos** | Implementa validación, firma digital, DRM, cadena de custodia | SHA-256, PKCS#7, presigned URLs, AES-128, magic bytes, ClamAV |
| G1-05 | **Desarrollador de integración de storage** | Conecta con Google Drive, FTP, S3, Azure Blob via abstracción | FluentStorage, Google.Apis.Drive, FluentFTP, SSH.NET, AWSSDK.S3 |
| G1-06 | **Desarrollador frontend multimedia** | Implementa players de video, galerías de imágenes, upload UX | hls.js, Video.js, MediaRecorder API, blur-up/LQIP, JS interop desde Blazor |
| G1-07 | **Desarrollador mobile multimedia** | Implementa captura de foto/video, caché local, upload resumible | .NET MAUI camera APIs, SQLite local DB, tus protocol, background upload |

### 8.2 Tabla G2 — Selección de estructura por solicitud

| El usuario pide... | Rol (G1-XX) | Estructura a aplicar | Archivos clave a generar |
|---|---|---|---|
| "Implementar upload de archivos al chat" | G1-01 + G1-06 | File Service + presigned URLs + JS interop | IFileStorage, S3FileStorage, FilesController, upload-interop.js |
| "Procesar imágenes (thumbnails, resize)" | G1-02 | Pipeline async con ImageSharp | ImageProcessor.cs, ProcessImageCommand, variantes en S3 |
| "Mostrar imágenes con carga progresiva" | G1-06 | Blur-up con BlurHash | BlurHash generation en ImageProcessor, componente Blazor con fade transition |
| "Implementar streaming de video HLS" | G1-03 | FFmpeg transcoding + S3 + CDN | VideoProcessor.cs, HLS profiles, StreamController, hls.js player |
| "Conectar con Google Drive del tenant" | G1-05 | OAuth2 + Google Drive SDK | GoogleDriveConnector.cs, OAuth callback, import/export commands |
| "Grabar video desde webcam" | G1-06 | MediaRecorder API + upload resumible | JS interop para MediaRecorder, tusdotnet en backend, componente Blazor |
| "Integrar cámaras IP RTSP" | G1-03 | FFmpeg RTSP → HLS + MediaMTX | docker-compose con MediaMTX, CameraStreamController, HLS player |
| "Firmar digitalmente un documento" | G1-04 | PKCS#7 con X.509 certificate | DocumentSigningService.cs, hash + firma, verificación endpoint |
| "Proteger videos con watermark" | G1-02 + G1-04 | FFmpeg overlay (visible) o steganography (invisible) | WatermarkService.cs, FFmpeg filter_complex, watermark per-session |
| "Implementar caché local estilo WhatsApp" | G1-06 + G1-07 | IndexedDB/SQLite + blur placeholder + descarga lazy | CacheService (JS o MAUI), metadata en DB local, download on-demand |
| "Configurar CDN para archivos estáticos" | G1-01 | CloudFront/Azure CDN delante de S3 | CDN distribution config, Cache-Control headers, origin access identity |
| "Upload desde app mobile con conexión inestable" | G1-07 | tus protocol + background upload | tusdotnet server, MAUI upload service con retry, progress tracking |

### 8.3 Tabla G3 — Selección de proveedor de storage

| Escenario | Proveedor recomendado | Motivo |
|---|---|---|
| Producción cloud, máxima disponibilidad | AWS S3 o Azure Blob | Escalabilidad ilimitada, SLA 99.99%, lifecycle policies |
| Desarrollo local / docker-compose | MinIO (S3-compatible) | Mismo código que producción, sin costo, sin internet |
| On-premise en IIS / Windows Server | LocalFileStorage + NAS compartido | Sin dependencia cloud, control total |
| Integración con sistema legacy | FTP/SFTP | Protocolo universalmente soportado |
| Documentos del tenant (importar/exportar) | Google Drive como fuente externa | Integración con el ecosistema del tenant |
| Archivado a largo plazo (compliance) | S3 Glacier / Azure Archive | 90% más barato que storage estándar |
| Videos VOD con distribución global | S3 + CloudFront CDN | Baja latencia en todo el mundo |

### 8.4 Tabla G4 — Selección de estrategia de video

| Escenario | Protocolo | Herramienta | Latencia | Resoluciones |
|---|---|---|---|---|
| Video pregrabado (training, VOD) | HLS | FFmpeg + S3 + hls.js | N/A (on-demand) | 480p, 720p, 1080p |
| Videomensaje corto en chat | MP4 directo (sin HLS) | FFmpeg compress + `<video>` tag | N/A | 720p |
| Videoconferencia en vivo | WebRTC | LiveKit / Janus | < 500ms | Adaptativo |
| Live streaming unidireccional | RTMP ingest → HLS | Nginx-RTMP + FFmpeg | 6-15 seg | 480p, 720p, 1080p |
| Cámara IP de vigilancia | RTSP → HLS | MediaMTX | 2-6 seg | Original de cámara |
| Grabación de reunión | WebRTC record | LiveKit recording | N/A | 720p |

### 8.5 Tabla G5 — Selección de compresión

| Tipo de contenido | Formato recomendado | Herramienta .NET | Calidad / CRF | Peso objetivo |
|---|---|---|---|---|
| Foto de chat | WebP (con fallback JPEG) | ImageSharp | 80% quality | < 200KB |
| Thumbnail | WebP | ImageSharp | 70% quality, 150x150 | < 15KB |
| Avatar de usuario | WebP | ImageSharp | 80%, 256x256 | < 30KB |
| Documento PDF | Sin compresión (ya comprimido) | — | — | Original |
| Video de chat | H.264 MP4 | FFmpeg (FFMpegCore) | CRF 26, fast | ~5MB/min |
| Video training | H.264 HLS multi-quality | FFmpeg (FFMpegCore) | CRF 22, slow | ~18MB/min |
| Backup / archivo | ZIP o Zstd | System.IO.Compression / ZstdSharp | Máxima compresión | Variable |

### 8.6 Tabla G6 — Prompts de referencia por rol

**G1-01 — Arquitecto de almacenamiento:**

> Sos un arquitecto de almacenamiento especializado en .NET. Diseñá la estrategia de storage usando el patrón `IFileStorage` con implementaciones para S3 (via AWSSDK.S3), Azure Blob (via Azure.Storage.Blobs), MinIO (S3-compatible para desarrollo local), y disco local. Los archivos se organizan como `{tenant-id}/{service-id}/{year}/{month}/{file-id}.ext`. Usá presigned URLs de corta duración para upload y download (nunca URLs públicas). Multi-tenancy se implementa por prefijo de objeto. El File Service es un microservicio transversal con Clean Architecture (Domain, Application, Infrastructure, API).

**G1-02 — Desarrollador de procesamiento multimedia:**

> Sos un desarrollador .NET especializado en procesamiento multimedia. Usá ImageSharp para procesamiento de imágenes (thumbnails, resize, crop, format conversion a WebP). Usá FFMpegCore o Xabe.FFmpeg como wrapper de FFmpeg para procesamiento de video (transcodificación H.264, generación de HLS multi-quality, extracción de thumbnails). El procesamiento es siempre asíncrono: el upload dispara un evento, un worker procesa, y publica el resultado. Generá BlurHash de cada imagen para placeholders.

**G1-03 — Desarrollador de streaming:**

> Sos un desarrollador especializado en streaming de video con .NET. Para VOD usá HLS con FFmpeg generando segmentos de 6 segundos en múltiples calidades (480p/720p/1080p), almacenados en S3, servidos via CDN, reproducidos con hls.js. Para videoconferencia usá WebRTC via LiveKit. Para cámaras IP usá MediaMTX para transcodificar RTSP a HLS. Para live streaming usá RTMP ingest transcodificado a HLS.

**G1-04 — Especialista en seguridad de archivos:**

> Sos un especialista en seguridad de archivos con .NET. Validá uploads con magic bytes verification, whitelist de MIME types, y escaneo antivirus (nClam/ClamAV). Firmá documentos con PKCS#7 usando System.Security.Cryptography.Pkcs. Calculá SHA-256 de cada archivo para integridad. Usá presigned URLs de 15 minutos máximo. Encriptá at-rest con S3 SSE o Azure Storage Encryption. Para video con valor legal, implementá cadena de custodia con timestamp autoritativo (RFC 3161) y almacenamiento WORM.

**G1-05 — Desarrollador de integración de storage:**

> Sos un desarrollador .NET de integración de almacenamiento. Implementá conectores para Google Drive (Google.Apis.Drive.v3 con OAuth2 scope drive.file), FTP/SFTP (FluentFTP para FTP, SSH.NET para SFTP), y cualquier S3-compatible (AWSSDK.S3 cambiando solo ServiceURL para MinIO/Wasabi/R2). Cada conector implementa la interfaz del servicio que corresponda. Para FTP legacy, creá un background worker que haga polling, descargue archivos nuevos, y los suba al storage primario.

**G1-06 — Desarrollador frontend multimedia:**

> Sos un desarrollador Blazor Interactive Server especializado en multimedia. Para video usá hls.js via JS interop para reproducir HLS. Para imágenes usá la técnica blur-up: mostrá un BlurHash placeholder inmediato, luego transicioná a la imagen real con CSS fade de 300ms. Para upload usá presigned URLs (el archivo va directo de browser a S3, sin pasar por el servidor). Para grabación de webcam/pantalla usá MediaRecorder API via JS interop. Todo el acceso multimedia pasa por el File Service API.

---

*Documento complementario a la Guía de Arquitectura de Microservicios. Versión 1.0.*
