# Guía de arquitectura de microservicios y estrategias de frontend

## Índice de navegación

- [1. Fundamentos y glosario](#1-fundamentos-y-glosario)
  - [1.1 Glosario de términos](#11-glosario-de-términos)
  - [1.2 Principios arquitectónicos](#12-principios-arquitectónicos)
- [2. Arquitectura de microservicios](#2-arquitectura-de-microservicios)
  - [2.1 Qué es y cuándo usarla](#21-qué-es-y-cuándo-usarla)
  - [2.2 Scaffolding estándar por microservicio](#22-scaffolding-estándar-por-microservicio)
  - [2.3 Clean Architecture (capas)](#23-clean-architecture-capas)
  - [2.4 Comunicación entre microservicios](#24-comunicación-entre-microservicios)
  - [2.5 Base de datos por servicio](#25-base-de-datos-por-servicio)
- [3. Autenticación y autorización](#3-autenticación-y-autorización)
  - [3.1 Identity Provider centralizado](#31-identity-provider-centralizado)
  - [3.2 Flujos OAuth2 por tipo de cliente](#32-flujos-oauth2-por-tipo-de-cliente)
  - [3.3 Multi-tenancy en el JWT](#33-multi-tenancy-en-el-jwt)
- [4. API Gateway](#4-api-gateway)
  - [4.1 Rol y responsabilidades](#41-rol-y-responsabilidades)
  - [4.2 YARP vs Ocelot vs alternativas](#42-yarp-vs-ocelot-vs-alternativas)
- [5. Gestión de configuración y parametría](#5-gestión-de-configuración-y-parametría)
  - [5.1 Secrets vs parametría operativa](#51-secrets-vs-parametría-operativa)
  - [5.2 Patrón manifest-driven](#52-patrón-manifest-driven)
  - [5.3 Settings store multi-tenant](#53-settings-store-multi-tenant)
- [6. Estrategias de frontend](#6-estrategias-de-frontend)
  - [6.1 Los dos planos: admin vs producto](#61-los-dos-planos-admin-vs-producto)
  - [6.2 Patrón 1: cada micro con su mini-front](#62-patrón-1-cada-micro-con-su-mini-front)
  - [6.3 Patrón 2: front monolítico](#63-patrón-2-front-monolítico)
  - [6.4 Patrón 2.5: monolito modular con RCL](#64-patrón-25-monolito-modular-con-rcl)
  - [6.5 Patrón 3: app shell + micro-frontends](#65-patrón-3-app-shell--micro-frontends)
  - [6.6 Patrón 4: BFF (Backend for Frontend)](#66-patrón-4-bff-backend-for-frontend)
  - [6.7 Patrón 5: API-first para consumo externo](#67-patrón-5-api-first-para-consumo-externo)
- [7. Extensibilidad del Admin Portal](#7-extensibilidad-del-admin-portal)
  - [7.1 Manifest-driven (descubrimiento dinámico)](#71-manifest-driven-descubrimiento-dinámico)
  - [7.2 RCL plugins (UI custom)](#72-rcl-plugins-ui-custom)
  - [7.3 Enfoque híbrido recomendado](#73-enfoque-híbrido-recomendado)
- [8. Consumo multi-cliente](#8-consumo-multi-cliente)
  - [8.1 Blazor Interactive Server (principal)](#81-blazor-interactive-server-principal)
  - [8.2 SPAs (React, Angular, Vue)](#82-spas-react-angular-vue)
  - [8.3 Aplicaciones móviles (iOS, Android, MAUI)](#83-aplicaciones-móviles-ios-android-maui)
  - [8.4 Integraciones de terceros (webhooks, widgets)](#84-integraciones-de-terceros-webhooks-widgets)
- [9. Despliegue](#9-despliegue)
  - [9.1 Docker y orquestación](#91-docker-y-orquestación)
  - [9.2 IIS (Windows Server)](#92-iis-windows-server)
- [10. Criterios de selección](#10-criterios-de-selección)
  - [10.1 Tabla de decisión: arquitectura backend](#101-tabla-de-decisión-arquitectura-backend)
  - [10.2 Tabla de decisión: estrategia de frontend](#102-tabla-de-decisión-estrategia-de-frontend)
  - [10.3 Tabla de decisión: autenticación](#103-tabla-de-decisión-autenticación)
  - [10.4 Matriz de madurez evolutiva](#104-matriz-de-madurez-evolutiva)
- [11. Guía para agentes IA](#11-guía-para-agentes-ia)
  - [11.1 Roles y contexto](#111-roles-y-contexto)
  - [11.2 Tabla de selección de estructura por tipo de solicitud](#112-tabla-de-selección-de-estructura-por-tipo-de-solicitud)
  - [11.3 Prompts de referencia por rol](#113-prompts-de-referencia-por-rol)
- [12. Casos reales de la industria](#12-casos-reales-de-la-industria)

---

## 1. Fundamentos y glosario

### 1.1 Glosario de términos

| Término | Definición | Ejemplo concreto |
|---|---|---|
| **Scaffolding** | Estructura inicial de carpetas y archivos base que se genera para arrancar un proyecto. Es el esqueleto sobre el que se construye, no código productivo. También llamado "boilerplate" o "project template". | Cuando corrés `dotnet new webapi` y te crea `Program.cs`, `Controllers/`, `appsettings.json`, eso es scaffolding. |
| **Tenant** | Organización cliente que contrata la plataforma. No es el usuario final, es la empresa. El término viene de "inquilino": la plataforma es el edificio, cada tenant alquila su departamento. | "Farmacia del Pueblo" contrata tu chat de soporte. La farmacia es el tenant. Los clientes que le escriben a la farmacia son los usuarios finales. |
| **Multi-tenancy** | Capacidad de una aplicación de servir a múltiples tenants desde la misma instancia de código, aislando sus datos. Cada registro en la base de datos tiene una columna `TenantId`. | Una sola instancia del Chat Service atiende a 50 empresas. Cada una ve solo sus conversaciones gracias al filtro `WHERE TenantId = @tenantId`. |
| **SaaS** | Software as a Service. Software que el cliente usa por internet pagando suscripción, sin instalarse nada. | Gmail, Slack, Netflix, Jira. |
| **B2B** | Business to Business. Le vendés a empresas, no a personas individuales. Lo opuesto es B2C (Business to Consumer). | Slack le vende a empresas para que sus empleados chateen. Netflix le vende a personas para que vean series. Slack es B2B, Netflix es B2C. |
| **SaaS B2B** | Software por suscripción que le vendés a empresas. Es la combinación más común en plataformas de servicios. | Zendesk (soporte al cliente), Jira (gestión de proyectos), HubSpot (CRM). La plataforma que estamos diseñando es SaaS B2B. |
| **RCL (Razor Class Library)** | Proyecto .NET que contiene componentes Blazor (archivos `.razor`) empaquetados como librería reutilizable. No se ejecuta solo. Es como un paquete NuGet pero con páginas y componentes de UI. | `Chat.Admin.RCL` contiene `ChatDashboard.razor`. El Admin Portal referencia esta RCL y la página aparece como si fuera propia. |
| **Micro-frontend** | Arquitectura donde el frontend se divide en módulos independientes, cada uno propiedad de un equipo distinto, con deploy propio. Extiende el concepto de microservicios al frontend. | Mercado Libre: la búsqueda es una app React, el detalle del producto es otra, el carrito es otra. Cada una la mantiene un equipo diferente. |
| **App Shell** | Aplicación host que provee la estructura exterior (navbar, sidebar, autenticación, routing) y dentro carga módulos de contenido. | Google Workspace: la barra superior con el waffle menu es el shell. Gmail, Calendar, Drive son módulos que se cargan dentro. |
| **Clean Architecture** | Patrón de organización de código en capas concéntricas donde las dependencias apuntan hacia adentro. El dominio (centro) no depende de nada externo. También llamada Onion Architecture o Hexagonal Architecture. | Un microservicio con 4 capas: Domain → Application → Infrastructure → API. El dominio define las entidades y reglas de negocio sin saber que existe SQL Server ni HTTP. |
| **CQRS** | Command Query Responsibility Segregation. Patrón que separa las operaciones de lectura (queries) de las de escritura (commands). | `SendMessageCommand` modifica datos. `GetConversationsQuery` solo lee. Cada uno puede tener su propia pipeline de validación y su propio modelo de datos. |
| **API Gateway** | Punto de entrada único para todas las solicitudes del cliente. Recibe las peticiones, las rutea al microservicio correcto, y opcionalmente aplica autenticación, rate limiting y transformaciones. | El cliente llama a `gateway.myplatform.com/api/chat/rooms`. El gateway redirige internamente a `chat-service:8080/rooms`. |
| **Identity Provider (IdP)** | Servicio centralizado que gestiona la autenticación de usuarios y emite tokens de acceso. Implementa protocolos como OAuth2 y OpenID Connect. | Duende IdentityServer, Keycloak, Auth0, Azure AD. El usuario se autentica una vez contra el IdP y recibe un JWT que usa para acceder a todos los microservicios. |
| **JWT (JSON Web Token)** | Token compacto y autocontenido que lleva claims (información) sobre el usuario: quién es, a qué tenant pertenece, qué permisos tiene. Lo emite el Identity Provider y lo validan los microservicios. | `{ "sub": "user123", "tenant_id": "farmacia-del-pueblo", "scope": "chat:read chat:write" }` |
| **OAuth2** | Protocolo estándar de autorización. Define flujos para que una aplicación obtenga acceso a recursos en nombre de un usuario, sin que el usuario comparta su contraseña. | Cuando hacés "Iniciar sesión con Google", la app redirige a Google, vos autorizás, y Google le da un token a la app. Eso es OAuth2 Authorization Code flow. |
| **ROPC** | Resource Owner Password Credentials. Flujo de OAuth2 donde la app recibe directamente usuario y contraseña. Desaconsejado por OAuth 2.1 porque es inseguro: la app ve las credenciales. | La app muestra un formulario de login, recibe email y password, y los envía al IdP. El IdP devuelve un token. El problema: la app manipula la contraseña directamente. |
| **PKCE** | Proof Key for Code Exchange. Extensión de OAuth2 que protege el flujo Authorization Code contra interceptación. Requerido para apps públicas (SPAs, mobile). | El cliente genera un `code_verifier` aleatorio, envía su hash al IdP. Al canjear el código por token, envía el verifier original. Solo quien generó el hash puede obtener el token. |
| **SignalR** | Librería de .NET para comunicación en tiempo real. Usa WebSockets como transporte principal, con fallback a Server-Sent Events y Long Polling. | Un chat en vivo: cuando un usuario envía un mensaje, SignalR lo pushea instantáneamente a todos los demás participantes de la sala sin que refresquen la página. |
| **YARP** | Yet Another Reverse Proxy. Librería de Microsoft para construir proxies reversos y API gateways en .NET. Configuración declarativa via JSON. | `yarp.json` define que `/api/chat/*` se redirige a `chat-service:8080` y `/api/payments/*` a `payment-service:8080`. |
| **MediatR** | Librería .NET que implementa el patrón Mediator. Desacopla el emisor de una solicitud del manejador que la procesa. Usado para implementar CQRS. | El controller envía `new SendMessageCommand(...)`. MediatR busca el handler correspondiente y lo ejecuta. El controller no sabe nada sobre la lógica de negocio. |
| **Design System** | Conjunto de componentes de UI reutilizables, guías de estilo, tokens de diseño (colores, tipografía, espaciado) y principios que aseguran consistencia visual en toda la plataforma. | Mercado Libre tiene "Andes" (librería de componentes React). Google tiene "Material Design". Microsoft tiene "Fluent UI". |
| **Manifest** | Archivo JSON que un microservicio expone describiendo sus capacidades administrativas: qué configuraciones acepta, qué entidades gestiona, qué health checks tiene. Lo consume el Admin Portal para generar formularios dinámicos. | `GET /admin/manifest` retorna `{ "serviceId": "chat-service", "settings": [{ "key": "MaxMessagesPerMinute", "type": "number" }] }` |
| **BFF (Backend for Frontend)** | Capa backend intermedia diseñada específicamente para las necesidades de un tipo de frontend particular. Agrega datos de múltiples microservicios y los entrega en la forma que el frontend necesita. | La app móvil necesita una sola llamada que devuelva usuario + últimos mensajes + notificaciones pendientes. El BFF móvil hace 3 llamadas internas y devuelve todo junto. |
| **Health Check** | Endpoint que reporta si un servicio está funcionando correctamente. Retorna HTTP 200 (saludable) o 503 (no saludable). Puede verificar dependencias (base de datos, cache, servicios externos). | `GET /health` responde `{ "status": "Healthy", "checks": { "database": "Healthy", "redis": "Degraded" } }` |
| **Rate Limiting** | Mecanismo que limita la cantidad de solicitudes que un cliente puede hacer en un período de tiempo. Protege contra abuso y garantiza calidad de servicio. | Máximo 100 solicitudes por minuto por tenant. La solicitud 101 recibe HTTP 429 (Too Many Requests). |
| **Feature Flag** | Mecanismo para activar o desactivar funcionalidades en runtime sin redesplegar. Permite rollout gradual y personalización por tenant. | El tenant "Farmacia" tiene el módulo de IA activado. El tenant "Estudio Contable" no lo contrató, así que el flag `ai-assistant` está en `false` y el módulo no aparece. |

### 1.2 Principios arquitectónicos

Los siguientes principios guían todas las decisiones de esta guía:

**Autonomía de equipo.** Cada microservicio y su frontend asociado pueden ser desarrollados, testeados y deployados por un equipo independiente sin coordinar con otros equipos.

**Base de datos por servicio.** Cada microservicio es dueño exclusivo de sus datos. Ningún otro servicio accede directamente a su base de datos. La comunicación es siempre vía API o eventos.

**Smart endpoints, dumb pipes.** La lógica de negocio vive en los microservicios, no en el bus de mensajes ni en el gateway. Los mecanismos de transporte son simples y genéricos.

**Design for failure.** Todo falla eventualmente. Los servicios implementan circuit breakers, retries con backoff, health checks y timeouts. Un servicio caído no debe tumbar toda la plataforma.

**Evolutionary architecture.** Las decisiones de hoy no deben cerrar puertas para mañana. El sistema debe poder evolucionar de monolito modular a micro-frontends sin reescritura masiva.

---

## 2. Arquitectura de microservicios

### 2.1 Qué es y cuándo usarla

Un microservicio es una aplicación pequeña, independiente y desplegable por sí sola, que resuelve un problema de negocio específico. Un ecosistema de microservicios es un conjunto de estos servicios que colaboran para proveer una plataforma completa.

**Cuándo usarla:**

- Múltiples dominios de negocio claramente separados (chat, pagos, formularios, IA)
- Equipos que necesitan deployar de forma independiente
- Requerimientos de escalabilidad heterogéneos (el chat necesita mucha concurrencia, los formularios no)
- Tecnologías potencialmente diferentes por servicio

**Cuándo NO usarla:**

- Producto pequeño con un solo equipo de 1-3 personas
- MVP o prototipo donde la velocidad de desarrollo es prioridad
- Dominio de negocio cohesivo sin fronteras claras

**Ejemplo aplicado a esta plataforma:**

| Microservicio | Dominio | Base de datos | Tecnología específica |
|---|---|---|---|
| Chat Service | Mensajería en tiempo real | ChatDb (SQL Server) | SignalR, Redis para presencia |
| AI Assistant | Asistencia con LLM | AIDb (SQL Server) | Integración OpenAI/Anthropic |
| Payment Gateway | Procesamiento de pagos | PaymentDb (SQL Server) | Stripe SDK, MercadoPago SDK |
| Forms Service | Formularios dinámicos | FormsDb (SQL Server) | Motor de renderizado JSON Schema |
| Identity Service | Autenticación/autorización | IdentityDb (SQL Server) | Duende IdentityServer |
| Config Service | Parametría centralizada | ConfigDb (SQL Server) | Key-value store + cache |

### 2.2 Scaffolding estándar por microservicio

Cada microservicio sigue la misma estructura de 4 capas (Clean Architecture), lo que permite que cualquier desarrollador del equipo navegue cualquier servicio sin curva de aprendizaje:

```
src/Services/{NombreServicio}/
├── {Nombre}.Domain/                    ← Capa 1: Dominio puro
│   ├── Entities/                       ← Clases de negocio (ChatRoom, Message)
│   ├── ValueObjects/                   ← Objetos inmutables (MessageContent)
│   ├── Enums/                          ← Enumeraciones del dominio
│   ├── Events/                         ← Eventos de dominio (MessageSentEvent)
│   ├── Interfaces/                     ← Contratos de repositorio (IChatRoomRepository)
│   └── Exceptions/                     ← Excepciones del dominio
│
├── {Nombre}.Application/              ← Capa 2: Casos de uso
│   ├── Commands/                       ← Operaciones de escritura (SendMessageCommand)
│   ├── Queries/                        ← Operaciones de lectura (GetRoomsQuery)
│   ├── DTOs/                           ← Objetos de transferencia
│   ├── Validators/                     ← Reglas de validación (FluentValidation)
│   ├── Mappings/                       ← Perfiles de AutoMapper
│   ├── Behaviors/                      ← Pipeline behaviors de MediatR
│   └── Interfaces/                     ← Contratos de servicios de aplicación
│
├── {Nombre}.Infrastructure/           ← Capa 3: Implementaciones concretas
│   ├── Persistence/
│   │   ├── {Nombre}DbContext.cs        ← EF Core DbContext
│   │   ├── Configurations/             ← Fluent API (mapeo entidades → tablas)
│   │   ├── Repositories/              ← Implementaciones de IRepository
│   │   └── Migrations/                ← Migraciones de EF Core
│   ├── Services/                      ← Servicios externos (Stripe, OpenAI)
│   └── DependencyInjection.cs         ← Registro de servicios en el contenedor
│
├── {Nombre}.API/                      ← Capa 4: Presentación
│   ├── Controllers/
│   │   ├── {Nombre}Controller.cs       ← Endpoints de negocio
│   │   └── AdminController.cs         ← ★ Endpoints de admin (hereda base)
│   ├── Middleware/                     ← Error handling, tenant resolution
│   ├── Hubs/                          ← SignalR hubs (si aplica)
│   ├── Program.cs                     ← Punto de entrada
│   ├── appsettings.json               ← Configuración
│   └── Dockerfile                     ← Imagen Docker
│
├── {Nombre}.Admin.RCL/                ← OPCIONAL: Plugin admin
│   ├── {Nombre}AdminModule.cs         ← Implementa IAdminModule
│   ├── Pages/                         ← Páginas Blazor custom para admin
│   └── Components/                    ← Componentes Blazor custom
│
└── {Nombre}.UserFacing.RCL/           ← OPCIONAL: Plugin producto
    ├── {Nombre}ProductModule.cs       ← Implementa IProductModule
    ├── Pages/                         ← Páginas para el usuario final
    └── Components/
```

### 2.3 Clean Architecture (capas)

La regla fundamental es que **las dependencias apuntan hacia adentro**:

- **Domain** no depende de nada externo. No conoce Entity Framework, ni HTTP, ni JSON. Define las reglas de negocio puras.
- **Application** depende solo de Domain. Define los casos de uso con Commands/Queries (MediatR). No sabe cómo se persisten los datos.
- **Infrastructure** depende de Domain y Application. Implementa los contratos: repositorios con EF Core, servicios con SDKs externos, caché con Redis.
- **API** depende de todo. Es la capa más externa. Recibe HTTP requests, las traduce a Commands/Queries, y retorna respuestas. Es la capa más fácil de reemplazar.

**Ejemplo concreto — flujo de enviar un mensaje:**

1. El controller recibe `POST /api/messages` con `{ "roomId": "abc", "content": "Hola" }`
2. Crea un `SendMessageCommand` y lo envía via MediatR
3. MediatR ejecuta los behaviors en pipeline: LoggingBehavior → ValidationBehavior → el handler
4. El `SendMessageCommandHandler` usa `IMessageRepository` (interfaz del dominio) para persistir
5. Infrastructure resuelve `IMessageRepository` → `MessageRepository` que usa `ChatDbContext`
6. El handler publica un `MessageSentEvent` (evento de dominio)
7. Un handler de eventos notifica via SignalR a los participantes de la sala

### 2.4 Comunicación entre microservicios

| Tipo | Mecanismo | Cuándo usar | Ejemplo |
|---|---|---|---|
| Síncrona | HTTP/REST via API Gateway | Request-response, el llamante necesita la respuesta inmediata | El Forms Service valida un pago llamando a `GET /api/payments/{id}/status` |
| Síncrona | gRPC | Comunicación interna service-to-service de alta frecuencia | Chat Service consulta al Identity Service los datos del usuario |
| Asíncrona | Eventos via message broker (RabbitMQ + MassTransit) | Notificaciones, procesos que no necesitan respuesta inmediata | Payment Service publica `PaymentCompletedEvent`. Chat Service lo escucha y envía un mensaje al usuario. |
| Asíncrona | Outbox pattern | Garantía de entrega: el evento se persiste en la misma transacción que la operación | Se guarda el pago y el evento `PaymentCompleted` en la misma transacción SQL. Un worker los publica al broker. |

### 2.5 Base de datos por servicio

Cada microservicio tiene su propia base de datos en SQL Server. Ningún servicio accede directamente a la base de datos de otro.

**Ventajas:** independencia de esquema, independencia de deploy, escalabilidad independiente, aislamiento de fallos.

**Consecuencia:** si el Chat Service necesita el nombre del usuario, no hace un JOIN a la tabla de usuarios del Identity Service. Llama a la API del Identity Service, o mantiene una copia local sincronizada via eventos.

---

## 3. Autenticación y autorización

### 3.1 Identity Provider centralizado

Se recomienda migrar de ROPC a un Identity Provider centralizado como microservicio independiente. Opciones:

| Opción | Licencia | Ventaja principal | Ideal para |
|---|---|---|---|
| Duende IdentityServer | Comercial (gratis para ingresos < $1M USD) | Integración nativa .NET, máxima flexibilidad | Ecosistemas .NET puros |
| Keycloak | Open source (Apache 2.0) | Sin costo, UI de admin incluida, federation | Ecosistemas heterogéneos |
| Auth0 / Entra ID | SaaS (pago por uso) | Cero mantenimiento de infraestructura | Equipos pequeños sin DevOps |

### 3.2 Flujos OAuth2 por tipo de cliente

| Cliente | Flujo OAuth2 | Motivo |
|---|---|---|
| Blazor Interactive Server | Authorization Code + PKCE | El servidor mantiene la sesión. El usuario se redirige al IdP para autenticarse. |
| SPA (React, Angular, Vue) | Authorization Code + PKCE | El token se obtiene en el browser. PKCE protege contra interceptación. |
| App móvil (iOS, Android, MAUI) | Authorization Code + PKCE | Mismo flujo que SPA pero con deep links para el redirect. |
| Service-to-service | Client Credentials | Sin usuario involucrado. El servicio se autentica con su client_id y secret. |
| API pública (terceros) | Authorization Code + PKCE | El desarrollador externo autoriza su app a acceder en nombre de sus usuarios. |
| ~~ROPC~~ | ~~Desaconsejado~~ | ~~La app ve las credenciales del usuario. Eliminado en OAuth 2.1.~~ |

### 3.3 Multi-tenancy en el JWT

El Identity Provider incluye el `tenant_id` como claim en el JWT al momento del login:

```json
{
  "sub": "user-456",
  "name": "María García",
  "email": "maria@farmacia.com",
  "tenant_id": "farmacia-del-pueblo",
  "tenant_name": "Farmacia del Pueblo",
  "role": "admin",
  "scope": "chat:read chat:write payments:read"
}
```

Cada microservicio extrae `tenant_id` del token (via `TenantMiddleware`) y filtra automáticamente todos los queries. El usuario nunca ve datos de otro tenant.

---

## 4. API Gateway

### 4.1 Rol y responsabilidades

El API Gateway es el punto de entrada único. Sus responsabilidades son:

- **Routing:** redirige `/api/chat/*` al Chat Service, `/api/payments/*` al Payment Service
- **Autenticación:** valida el JWT antes de dejar pasar la request
- **Rate limiting:** limita solicitudes por tenant para prevenir abuso
- **CORS:** centraliza las políticas de cross-origin
- **TLS termination:** maneja HTTPS, los servicios internos pueden usar HTTP
- **Health aggregation:** reporta el estado de todos los servicios downstream

Lo que el gateway **NO** hace: lógica de negocio, transformación de datos, composición de respuestas (eso es tarea del BFF).

### 4.2 YARP vs Ocelot vs alternativas

| Característica | YARP | Ocelot | Kong/Envoy |
|---|---|---|---|
| Mantenedor | Microsoft (.NET team) | Comunidad | Kong Inc / CNCF |
| Performance | Excelente (usa Kestrel internamente) | Buena | Excelente |
| Configuración | JSON + código C# | JSON | YAML / Admin API |
| Extensibilidad | Middleware pipeline completo de .NET | Delegating handlers | Plugins (Lua/Go) |
| Health checks | Integrado | Plugin | Integrado |
| Estado | Activamente mantenido | Mantenimiento mínimo | Activamente mantenido |
| **Recomendación** | **Para ecosistemas .NET** | No recomendado para nuevos proyectos | Para ecosistemas heterogéneos |

---

## 5. Gestión de configuración y parametría

### 5.1 Secrets vs parametría operativa

| Tipo | Qué contiene | Dónde se guarda | Ejemplo |
|---|---|---|---|
| **Secrets** | Credenciales, connection strings, API keys | Azure Key Vault o HashiCorp Vault | `ConnectionStrings:ChatDb`, `Stripe:SecretKey` |
| **Parametría operativa** | Configuraciones de negocio, feature flags | Config Service con SQL Server + cache | `MaxMessagesPerMinute = 60`, `EnableFileSharing = true` |
| **Parametría por tenant** | Configuraciones específicas de cada tenant | Config Service (tabla con `TenantId`) | Tenant A: `RetentionDays = 30`. Tenant B: `RetentionDays = 365`. |

### 5.2 Patrón manifest-driven

Cada microservicio expone `GET /admin/manifest` describiendo su esquema de administración. El Admin Portal consume estos manifests y genera formularios dinámicos. El microservicio nuevo no necesita tocar ni una línea del portal.

El manifest define: nombre y versión del servicio, ícono, health endpoint, lista de settings con su tipo y scope (global o per-tenant), entidades administrables con sus operaciones CRUD, scopes OAuth2 requeridos, y si tiene una RCL con UI custom.

### 5.3 Settings store multi-tenant

El almacenamiento de settings sigue una jerarquía de precedencia: el valor per-tenant sobreescribe el valor global, que sobreescribe el default del manifest.

**Ejemplo:** El manifest del Chat Service define `MaxMessagesPerMinute` con default `60`. Un admin configura el global en `100`. Luego, para el tenant "Empresa Grande", lo sube a `500`. El tenant "Startup Chica" no tiene override, así que hereda `100`.

---

## 6. Estrategias de frontend

### 6.1 Los dos planos: admin vs producto

Toda plataforma SaaS B2B tiene dos tipos de frontend que sirven a audiencias diferentes y requieren estrategias diferentes:

| Aspecto | Plano admin | Plano producto |
|---|---|---|
| **Usuarios** | Tu equipo interno, operadores | Clientes finales del tenant |
| **Objetivo** | Configurar, monitorear, gestionar | Usar la funcionalidad del servicio |
| **Frecuencia de cambio** | Baja-media | Alta |
| **Prioridad UX** | Funcionalidad sobre estética | Estética y UX son críticas |
| **Ejemplo** | Panel de control de microservicios | Chat de soporte al cliente |
| **Estrategia recomendada** | Monolito modular (Blazor Server) | App shell + módulos (Blazor Server) |

### 6.2 Patrón 1: cada micro con su mini-front

Cada microservicio incluye su propia aplicación frontend independiente.

**Cómo funciona:** el Chat Service se accede en `chat.myplatform.com`, el Payment Service en `payments.myplatform.com`. Cada uno tiene su propio login, su propio layout, su propio estilo.

**Ventajas:** máxima independencia, cada equipo es dueño total de su UI, no hay coordinación necesaria.

**Desventajas:** UX fragmentada (el usuario navega entre múltiples URLs), duplicación de código (cada app tiene su propio login, navbar, layout), inconsistencia visual, múltiples sesiones que mantener.

**Quién lo usa:** nadie intencionalmente. Es lo que ocurre cuando no se planifica una estrategia de frontend.

**Cuándo tiene sentido:** servicios completamente independientes que no comparten usuarios (ej: un servicio interno de DevOps y un servicio de facturación que usan personas diferentes).

### 6.3 Patrón 2: front monolítico

Una sola aplicación frontend que consume todas las APIs y reúne todas las pantallas.

**Cómo funciona:** un solo proyecto Blazor Server con todas las páginas adentro. `Pages/Chat/`, `Pages/Payments/`, `Pages/Forms/`, todo en el mismo `.csproj`. Un solo deploy, una sola URL, un solo login.

**Ventajas:** UX unificada, un solo deploy, un solo login, simplicidad máxima al inicio.

**Desventajas:** a medida que crece, se vuelve difícil de mantener. Todos los desarrolladores tocan el mismo proyecto. Los merge conflicts se multiplican. Un bug en la página de pagos bloquea el deploy de la página de chat. No es posible que un equipo deploya su módulo sin deployar todo.

**Quién lo usa:** Atlassian (Jira clásico), Salesforce Classic, la mayoría de los SaaS B2B con equipos de hasta 5 desarrolladores frontend.

**Cuándo tiene sentido:** equipo pequeño (1-3 devs frontend), producto en etapa temprana, prioridad en velocidad de desarrollo.

**Limitación crítica:** si todo el código está en un solo `.csproj`, extraer un módulo en el futuro requiere una refactorización significativa. Es un callejón sin salida arquitectónico.

### 6.4 Patrón 2.5: monolito modular con RCL

Este es el patrón recomendado para la mayoría de los equipos. No es patrón 2 ni patrón 3, sino un punto intermedio diseñado para poder evolucionar.

**Cómo funciona:** un solo deploy (como patrón 2), pero el código está separado en Razor Class Libraries independientes (un `.csproj` por módulo). El shell principal carga las RCLs por assembly scanning. Desde afuera parece un monolito, por dentro cada módulo es independiente.

**Ventajas:** UX unificada (un deploy, una URL, un login), código modular (cada RCL es un proyecto separado), evolución posible (una RCL se puede convertir en micro-frontend sin reescribir), testeo aislado (cada RCL tiene sus propios tests).

**Desventajas:** el deploy sigue siendo único (si una RCL cambia, se redeploya todo), requiere disciplina para mantener los límites entre módulos.

**Quién lo usa:** es el patrón estándar de ASP.NET Core Areas evolucionado. Muchos productos Microsoft internos usan este enfoque.

**Cuándo tiene sentido:** equipo de 2-10 devs, producto con múltiples módulos claros, necesidad de consistencia visual, intención de escalar en el futuro.

**Relación con el patrón 3:** la RCL es la pieza que transforma el patrón 2 en un camino viable hacia el patrón 3. Sin RCL, el patrón 2 es un callejón sin salida. Con RCL, el código ya está separado en módulos; el día que necesités deploy independiente, la separación ya está hecha.

### 6.5 Patrón 3: app shell + micro-frontends

Módulos frontend completamente independientes, cada uno con su propio ciclo de desarrollo y deploy, integrados bajo un shell compartido.

**Cómo funciona:** el shell provee la estructura exterior (navbar, sidebar, auth, routing). Los módulos se integran via Razor Class Libraries (en ecosistemas Blazor homogéneos), iframes (máximo aislamiento), Module Federation / Web Components (ecosistemas JavaScript), o sub-aplicaciones por ruta (como Mercado Libre).

**Variantes de integración:**

| Técnica | Aislamiento | Complejidad | Consistencia UX | Ideal para |
|---|---|---|---|---|
| RCL (Blazor) | Medio (compile-time) | Baja | Alta | Ecosistema Blazor puro |
| iframes | Alto (runtime) | Media | Media (hay que sincronizar estilos) | Módulos de terceros, mix de tecnologías |
| Module Federation (webpack 5) | Medio (runtime) | Alta | Media | Ecosistemas React/Vue/Angular |
| Web Components | Alto (runtime) | Media | Media | Componentes embebibles cross-framework |
| Sub-apps por ruta | Alto (runtime) | Media | Requiere design system | Aplicaciones grandes (Mercado Libre) |

**Quién lo usa:** Google Workspace (shell + apps independientes), Spotify (squads con frontends autónomos), Mercado Libre (sub-apps React por ruta con design system "Andes"), AWS Console (cada servicio es un módulo independiente), IKEA, Zalando.

**Cuándo tiene sentido:** múltiples equipos de frontend (5+ devs frontend), necesidad de deploy independiente real, módulos con ciclos de release diferentes, mix de tecnologías frontend.

**Cuándo NO tiene sentido:** equipo pequeño con stack homogéneo. En ese caso, el patrón 2.5 (monolito modular con RCL) da la misma autonomía de desarrollo sin la complejidad de orquestación.

### 6.6 Patrón 4: BFF (Backend for Frontend)

Una capa backend intermedia que adapta las APIs de los microservicios a las necesidades específicas de cada tipo de frontend.

**Cómo funciona:** en lugar de que cada frontend llame directamente a 5 microservicios para armar una pantalla, llama a un BFF que agrega los datos de los 5 servicios y devuelve exactamente lo que el frontend necesita en una sola llamada.

**Variantes:**

| Variante | Descripción | Cuándo usar |
|---|---|---|
| Un BFF por tipo de cliente | BFF-Web, BFF-Mobile, BFF-Admin | Cuando web y mobile necesitan datos muy diferentes |
| Un BFF único | Un solo BFF que sirve a todos los clientes | Cuando las necesidades son similares |
| GraphQL como BFF | Un servidor GraphQL que expone los microservicios | Cuando los clientes necesitan consultas flexibles |

**Ejemplo concreto:** la pantalla principal de la app móvil del tenant necesita mostrar últimos mensajes del chat + notificaciones de pago + estado del asistente IA. Sin BFF, la app haría 3 llamadas HTTP separadas. Con BFF, hace una sola a `GET /bff/mobile/dashboard` y recibe todo en una respuesta optimizada para mobile (datos compactos, imágenes en resolución apropiada).

**Quién lo usa:** Netflix (BFF por dispositivo: TV, mobile, web), SoundCloud, Spotify.

**Cuándo tiene sentido:** cuando tenés clientes con necesidades muy diferentes (web vs mobile vs watch), o cuando necesitás optimizar la cantidad de llamadas HTTP desde el frontend.

### 6.7 Patrón 5: API-first para consumo externo

Diseñar las APIs de los microservicios para que puedan ser consumidas directamente por cualquier tipo de cliente, sin acoplamiento a un frontend específico.

**Cómo funciona:** las APIs siguen estándares REST/OpenAPI, con documentación automática (Swagger), versionamiento, y SDKs generados automáticamente. Cualquier frontend (Blazor, React, Flutter, mobile nativo) puede consumirlas.

**Principios:**

- APIs RESTful con recursos bien definidos (`/api/v1/rooms/{id}/messages`)
- Documentación OpenAPI/Swagger generada desde el código
- Versionamiento via URL (`/api/v1/`, `/api/v2/`) o header
- Paginación, filtrado y ordenamiento estandarizados
- Respuestas Problem Details (RFC 7807) para errores
- SDKs auto-generados con NSwag o Kiota

**Cuándo tiene sentido:** siempre. Incluso si hoy solo tenés Blazor, diseñar APIs limpias permite que mañana un tenant integre desde su app React, un partner desarrolle un plugin, o vos mismo construyas una app mobile.

---

## 7. Extensibilidad del Admin Portal

### 7.1 Manifest-driven (descubrimiento dinámico)

**Nivel de esfuerzo para agregar un microservicio:** cero cambios en el portal.

El nuevo servicio implementa `AdminControllerBase`, expone `GET /admin/manifest` y `GET/PUT /admin/settings`. Un background worker en el portal (ManifestDiscoveryWorker) lo descubre automáticamente, muestra su health en el dashboard, y genera formularios de configuración a partir del schema del manifest.

Este patrón cubre el 80% de los casos: configuraciones key-value, toggles, selects, parametría por tenant.

### 7.2 RCL plugins (UI custom)

**Nivel de esfuerzo:** crear un proyecto RCL, implementar `IAdminModule`, agregar referencia al portal.

Para servicios que necesitan UI de administración más rica que formularios genéricos: dashboards con gráficos en tiempo real, monitores de conexiones, flujos de aprobación, editores visuales. La RCL aporta páginas custom al sidebar del portal.

### 7.3 Enfoque híbrido recomendado

Todos los microservicios implementan el manifest (nivel mínimo obligatorio). Solo los que necesitan UI rica agregan una RCL (nivel opcional). El portal muestra ambos tipos en el mismo sidebar: los que tienen RCL van a sus páginas custom, los que solo tienen manifest van al formulario genérico.

Flujo al agregar un microservicio nuevo:

1. Implementar `AdminControllerBase` → el portal lo descubre solo
2. Agregar ruta en `yarp.json` y endpoint en `ManifestDiscovery:Services`
3. (Opcional) Crear RCL con `IAdminModule` para UI custom
4. (Opcional) Agregar referencia de la RCL al portal `.csproj`

---

## 8. Consumo multi-cliente

El diseño API-first permite que los microservicios sean consumidos por cualquier tipo de cliente:

### 8.1 Blazor Interactive Server (principal)

El frontend principal. El código C# corre en el servidor, la UI se actualiza via SignalR. Ventajas: acceso directo a servicios del servidor, secretos nunca llegan al browser, sin carga de descarga de runtime al cliente.

Ideal para: panel de administración, dashboards internos, aplicaciones de negocio donde la latencia de red es aceptable.

### 8.2 SPAs (React, Angular, Vue)

Tenants que prefieran construir su propio frontend pueden consumir las APIs directamente. Se autentican con Authorization Code + PKCE contra el Identity Provider.

Ideal para: tenants con equipos de desarrollo propios, integraciones donde el tenant quiere control total de la UX.

### 8.3 Aplicaciones móviles (iOS, Android, MAUI)

Las APIs REST son consumibles desde cualquier HTTP client. Para mobile se recomienda un BFF que optimice las llamadas (menos requests, payloads más compactos).

Ideal para: experiencias mobile nativas, .NET MAUI para apps cross-platform.

### 8.4 Integraciones de terceros (webhooks, widgets)

- **Webhooks:** los microservicios notifican a sistemas externos cuando ocurren eventos. El tenant registra una URL y recibe POSTs con los eventos.
- **Widgets embebibles:** componentes de UI empaquetados como Web Components o JS snippets que el tenant inserta en su sitio. Ejemplo: `<script src="myplatform.com/widget/chat.js" data-tenant="xxx"></script>`
- **API pública documentada:** endpoints abiertos con API keys para que terceros construyan integraciones.

---

## 9. Despliegue

### 9.1 Docker y orquestación

Cada microservicio tiene un Dockerfile multi-stage (build con SDK, run con runtime). El `docker-compose.yml` orquesta todo el ecosistema localmente: SQL Server, RabbitMQ, Redis, Identity Server, Gateway, todos los microservicios, y el Admin Portal.

Para producción: Kubernetes (AKS, EKS) o Docker Swarm para escalamiento horizontal y self-healing.

### 9.2 IIS (Windows Server)

Cada API se publica como un sitio independiente en IIS con su propio Application Pool. Cada Application Pool corre en su propio proceso `w3wp.exe`, aislando los servicios entre sí.

---

## 10. Criterios de selección

### 10.1 Tabla de decisión: arquitectura backend

| Factor | Monolito | Monolito modular | Microservicios |
|---|---|---|---|
| Tamaño del equipo | 1-5 devs | 3-15 devs | 10+ devs |
| Dominios de negocio | 1-2 | 2-5 | 5+ |
| Deploy independiente | No necesario | Deseable | Requerido |
| Escalabilidad | Uniforme | Uniforme | Por servicio |
| Complejidad operativa | Baja | Media | Alta |
| Time to market (MVP) | Rápido | Medio | Lento |
| Evolución a largo plazo | Limitada | Buena | Máxima |

### 10.2 Tabla de decisión: estrategia de frontend

| Factor | Mini-fronts separados | Monolito puro | Monolito modular (RCL) | Micro-frontends |
|---|---|---|---|---|
| Devs frontend | — | 1-3 | 2-10 | 5+ |
| UX consistente | No | Sí | Sí | Requiere design system |
| Deploy independiente | Sí (pero UX rota) | No | No | Sí |
| Evolución futura | — | Callejón sin salida | Camino abierto al patrón 3 | Ya es el destino |
| Complejidad | Baja | Baja | Media | Alta |
| Stack homogéneo | — | Sí | Sí | No necesariamente |
| **Recomendado para esta plataforma** | No | Solo si es MVP | **Sí (punto de partida)** | Cuando los equipos lo requieran |

### 10.3 Tabla de decisión: autenticación

| Escenario | Flujo recomendado | Herramienta |
|---|---|---|
| Blazor Server (admin portal) | Authorization Code + PKCE | Duende IdentityServer / Keycloak |
| Blazor Server (tenant app) | Authorization Code + PKCE | Duende IdentityServer / Keycloak |
| SPA consumiendo APIs | Authorization Code + PKCE | Cualquier IdP compatible OAuth2 |
| App mobile | Authorization Code + PKCE + deep links | Cualquier IdP compatible OAuth2 |
| Servicio llamando a servicio | Client Credentials | Duende IdentityServer / Keycloak |
| Widget embebido | Token pre-generado por el tenant | API Key + JWT de corta duración |
| Webhook saliente | HMAC signature verification | Implementación propia |

### 10.4 Matriz de madurez evolutiva

Esta tabla muestra cómo evolucionar la plataforma a medida que crece el equipo y el producto:

| Etapa | Equipo | Backend | Frontend | Admin | Auth |
|---|---|---|---|---|---|
| **1. MVP** | 1-3 devs | Monolito modular (un solo deploy, separación por carpetas) | Blazor Server monolítico | Páginas dentro de la app principal | ROPC (migrar pronto) |
| **2. Producto** | 3-8 devs | Microservicios (extraer servicios uno a uno) | Blazor Server + RCLs por módulo | Admin Portal separado con manifest | Authorization Code + PKCE |
| **3. Escala** | 8-20 devs | Microservicios maduros + event bus | App shell + RCLs + posible BFF mobile | Admin Portal + RCL plugins | IdP centralizado + Client Credentials S2S |
| **4. Plataforma** | 20+ devs | Microservicios + API pública + webhooks | Micro-frontends reales donde se necesite | Admin Portal extensible + API admin | OAuth2 completo + API keys para terceros |

---

## 11. Guía para agentes IA

### 11.1 Roles y contexto

Esta sección define roles para que agentes IA puedan asistir en decisiones arquitectónicas y generación de código dentro de esta plataforma.

| Rol del agente | Descripción | Contexto que necesita |
|---|---|---|
| **Arquitecto de soluciones** | Diseña la estructura general, elige patrones, define contratos entre servicios. Toma decisiones de alto nivel sobre qué tecnología usar y cómo organizar el código. | Stack: .NET 10, Blazor Interactive Server, SQL Server, YARP, RabbitMQ + MassTransit. Patrones: Clean Architecture, CQRS con MediatR, multi-tenancy por claim JWT, manifest-driven admin. |
| **Desarrollador backend** | Implementa microservicios siguiendo Clean Architecture. Crea entidades, commands, queries, repositorios, controllers, health checks. | Estructura de 4 capas por servicio. Cada servicio hereda AdminControllerBase y expone manifest. FluentValidation para validación. AutoMapper para mappings. |
| **Desarrollador frontend Blazor** | Crea páginas y componentes Blazor Interactive Server. Trabaja con RCLs para modularidad. Implementa IAdminModule e IProductModule. | Blazor Server con render mode InteractiveServer. No WASM. Bootstrap o Fluent UI para estilos. SignalR para tiempo real. HttpClient tipados para consumir APIs. |
| **Desarrollador de integración** | Diseña contratos de API, implementa BFFs, configura webhooks, crea SDKs. Asegura que las APIs sean consumibles por cualquier cliente. | APIs RESTful, OpenAPI/Swagger, versionamiento, Problem Details RFC 7807, paginación estandarizada. |
| **DevOps / Infraestructura** | Configura Docker, Docker Compose, Kubernetes, IIS. Gestiona CI/CD, monitoring, health checks aggregados. | Dockerfile multi-stage, docker-compose con SQL Server + RabbitMQ + Redis. YARP como gateway. Health checks con AspNetCore.Diagnostics.HealthChecks. |
| **Especialista en seguridad** | Configura autenticación, autorización, gestión de secretos, rate limiting, CORS. | OAuth2 con Authorization Code + PKCE. JWT con claims de tenant. Azure Key Vault o HashiCorp Vault para secretos. Rate limiting per-tenant en el gateway. |

### 11.2 Tabla de selección de estructura por tipo de solicitud

Cuando un agente IA recibe una solicitud, debe usar esta tabla para determinar qué estructura aplicar:

| El usuario pide... | Rol a asumir | Estructura a usar | Archivos a generar |
|---|---|---|---|
| "Crear un nuevo microservicio de X" | Arquitecto + Backend | Clean Architecture 4 capas | Domain (Entities, Interfaces), Application (Commands, Queries, DTOs), Infrastructure (DbContext, Repositories, DI), API (Controller, AdminController, Program.cs, Dockerfile) |
| "Agregar una página de admin para X" | Frontend Blazor | RCL plugin para Admin Portal | `{X}AdminModule.cs` (implementa IAdminModule), `Pages/{X}Dashboard.razor`, `{X}AdminApiClient.cs` |
| "Agregar configuraciones al servicio X" | Backend | Manifest-driven settings | Agregar SettingDescriptors al manifest del AdminController existente, documentar en la API |
| "Integrar el servicio X con el servicio Y" | Integración | Eventos asincrónicos o HTTP síncrono | Contrato de evento en EventBus compartido, handler en el servicio receptor, publicación en el emisor |
| "Hacer que X sea accesible desde mobile" | Integración + Arquitecto | API-first + posible BFF | OpenAPI spec, BFF controller si se necesita agregación, SDK auto-generado |
| "Configurar el deploy de X" | DevOps | Docker + Compose o IIS | Dockerfile multi-stage, entrada en docker-compose.yml, ruta en yarp.json, endpoint en ManifestDiscovery |
| "Agregar autenticación a X" | Seguridad + Backend | JWT Bearer + tenant middleware | Configurar AddAuthentication en Program.cs, agregar AddMultiTenancy, definir policies de autorización |
| "Crear el frontend del chat para usuarios" | Frontend Blazor | RCL de producto | `Chat.UserFacing.RCL` con ChatProductModule.cs, Pages/ChatLobby.razor, Pages/ChatConversation.razor, Components/ |
| "Diseñar la base de datos de X" | Backend | EF Core Code-First | Entities en Domain, DbContext en Infrastructure con Fluent API configs, Query filters por tenant |
| "Agregar un widget embebible de X" | Frontend + Integración | Web Component o iframe | Componente Blazor standalone, JS loader script, documentación de integración para el tenant |

### 11.3 Prompts de referencia por rol

**Para que el agente asuma el rol de arquitecto:**

> Sos un arquitecto de software especializado en .NET y microservicios. La plataforma usa .NET 10, Blazor Interactive Server (no WASM), SQL Server, YARP como API Gateway, RabbitMQ + MassTransit para mensajería, y Duende IdentityServer para autenticación. Cada microservicio sigue Clean Architecture (Domain → Application → Infrastructure → API), usa MediatR para CQRS, FluentValidation, y expone un AdminManifest. El frontend es modular con Razor Class Libraries. Multi-tenancy se implementa via claim `tenant_id` en el JWT con query filters globales en EF Core.

**Para que el agente asuma el rol de desarrollador backend:**

> Sos un desarrollador backend .NET especializado en microservicios. Seguí la estructura Clean Architecture de 4 capas. El Domain no tiene dependencias externas. La Application usa MediatR (Commands/Queries), FluentValidation, y AutoMapper. La Infrastructure usa EF Core con SQL Server y query filters por TenantId. La API hereda AdminControllerBase para exponer el manifest y los settings CRUD. Registrá todos los servicios en DependencyInjection.cs. Agregá health checks para la base de datos y dependencias externas.

**Para que el agente asuma el rol de desarrollador frontend Blazor:**

> Sos un desarrollador Blazor especializado en Interactive Server (no WASM). Creás componentes y páginas que corren en el servidor con actualizaciones via SignalR. Usás Razor Class Libraries (RCL) para modularidad. Cada RCL de admin implementa IAdminModule. Cada RCL de producto implementa IProductModule. Usás HttpClient tipados para consumir APIs. Inyectás TenantContext para filtrar por tenant. El layout principal (AdminLayout.razor) carga módulos dinámicamente por assembly scanning.

---

## 12. Casos reales de la industria

### Mercado Libre

- **Arquitectura:** micro-frontends por ruta (vertical split). Cada flujo (búsqueda, detalle, carrito, checkout) es una aplicación React independiente con su propio deploy.
- **Stack:** Node + Express + React con Server Side Rendering.
- **Consistencia visual:** design system "Andes" (librería de componentes React compartida).
- **Infraestructura:** plataforma interna "Fury" para deploy y gestión.
- **Escala:** miles de desarrolladores organizados en squads autónomos.
- **Lección para nosotros:** su motivación principal es la autonomía de equipos, no la tecnología. Con un equipo chico, la misma autonomía se logra con RCLs sin la complejidad de multi-repo.

### Google Workspace

- **Arquitectura:** app shell + aplicaciones independientes. La barra superior (con el waffle menu de 9 puntos, avatar, cuenta) es el shell. Gmail, Calendar, Drive, Docs son apps completamente separadas.
- **Consistencia visual:** Material Design como design system unificado.
- **Auth:** SSO centralizado (Google Accounts).
- **Lección para nosotros:** demuestran que el patrón de app shell funciona a escala masiva, pero con cientos de ingenieros manteniendo la infraestructura de integración.

### Spotify

- **Arquitectura:** squads con frontends autónomos. Cada squad es dueño de una feature (reproducción, búsqueda, playlists, podcasts) con su propio frontend y backend.
- **Lección para nosotros:** la organización por squads es el driver, la arquitectura es consecuencia. No adoptes micro-frontends si no tenés los equipos que los justifiquen.

### AWS Console

- **Arquitectura:** cada servicio de AWS (EC2, S3, Lambda, etc.) tiene su propia UI dentro del console. El shell provee navegación y autenticación.
- **Lección para nosotros:** escala masiva de micro-frontends, pero cada servicio es mantenido por un equipo de decenas de personas. Es el destino final del patrón 3, al que se llega evolucionando.

### Atlassian (Jira/Confluence)

- **Arquitectura:** históricamente monolito (Java). Migrando gradualmente a micro-frontends con su framework "Atlassian Frontend Platform".
- **Lección para nosotros:** empezaron como monolito, les funcionó durante años, y migran módulo a módulo cuando lo necesitan. Es exactamente la evolución patrón 2 → 2.5 → 3 que estamos proponiendo.

---

## Apéndice: estructura de carpetas completa del monorepo

```
MyPlatform/
├── docker-compose.yml
├── MyPlatform.sln
├── README.md
├── docs/
│   ├── AGREGAR_NUEVO_MICROSERVICIO.md
│   └── ARCHITECTURE.md (este documento)
│
├── src/
│   ├── BuildingBlocks/
│   │   ├── MyPlatform.SharedKernel/
│   │   │   └── Admin/
│   │   │       ├── AdminManifest.cs
│   │   │       ├── AdminControllerBase.cs
│   │   │       ├── IAdminModule.cs
│   │   │       └── IProductModule.cs
│   │   ├── MyPlatform.Infrastructure.Common/
│   │   │   ├── MultiTenancy/
│   │   │   ├── Middleware/
│   │   │   └── Settings/
│   │   ├── MyPlatform.Auth.Shared/
│   │   └── MyPlatform.EventBus/
│   │
│   ├── Gateway/
│   │   └── MyPlatform.ApiGateway/
│   │
│   ├── Identity/
│   │   └── MyPlatform.Identity/
│   │
│   ├── Services/
│   │   ├── Chat/
│   │   │   ├── Chat.Domain/
│   │   │   ├── Chat.Application/
│   │   │   ├── Chat.Infrastructure/
│   │   │   ├── Chat.API/
│   │   │   ├── Chat.Admin.RCL/
│   │   │   └── Chat.UserFacing.RCL/
│   │   ├── AIAssistant/ (misma estructura)
│   │   ├── Payment/ (misma estructura)
│   │   ├── Forms/ (misma estructura)
│   │   └── Configuration/ (misma estructura)
│   │
│   └── Web/
│       ├── MyPlatform.AdminPortal/       ← Blazor Server (admin)
│       ├── MyPlatform.TenantApp/         ← Blazor Server (producto)
│       └── MyPlatform.EmbedWidgets/      ← Widgets embebibles
│
├── tests/
│   ├── Chat.UnitTests/
│   ├── Chat.IntegrationTests/
│   └── ...
│
└── deploy/
    ├── docker/
    ├── k8s/
    ├── iis/
    └── scripts/
```

---

*Documento generado como guía de referencia arquitectónica. Las decisiones finales deben adaptarse al contexto específico del equipo, el producto y los usuarios.*
