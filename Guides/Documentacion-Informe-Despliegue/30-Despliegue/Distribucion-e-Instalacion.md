---
doc_id: TEM-DISTRIBUCION
doc_type: tema
title: Distribución e instalación
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [FAM-DESP, TEM-TOPOLOGIA, TEM-OPERACION, TEM-DECISIONES, MARCO-ESCENARIOS, MARCO-CONTEXTOS, MARCO-ACTORES, ANEXO-REFERENCIAS]
---

# Distribución e instalación — `TEM-DISTRIBUCION`

## Resumen ejecutivo

Sabido dónde corre cada componente, queda la pregunta de cómo llega hasta ahí: en qué se empaqueta, cómo se distribuye y qué hace falta para instalarlo. Es la segunda pregunta del despliegue y la más ligada a la plataforma: aquí es donde las decisiones concretas de .NET —publicar con o sin runtime, alojar Kestrel solo o detrás de un proxy, correr el proceso de fondo como servicio de Windows o como *daemon* systemd, empaquetar el escritorio con MSIX o ClickOnce— dejan de ser detalle de implementación y se vuelven material del informe, porque determinan qué tiene que existir en cada host antes de que el software arranque.

En `CTX-1` esta pregunta se responde en un párrafo: un artefacto, un host, listo. En `CTX-3` se convierte en el problema operativo dominante, porque instalar y actualizar software **en cada terminal** no es un acto único sino un procedimiento repetido que el informe debe describir como tal. El detalle paso a paso de esa instalación no vive aquí: vive en la [Installation Guide](../../Documentacion-Tecnica/50-Operativa/Installation-Guide.md) (`DOC-INSTALL`) y la [Deployment Guide](../../Documentacion-Tecnica/50-Operativa/Deployment-Guide.md) (`DOC-DEPLOY`) de la guía hermana. Lo que el informe hace es explicar el **modelo** de distribución y por qué se eligió, y remitir a esos documentos para la ejecución.

El lector central de este tema es `ACT-04`, el responsable de despliegue y operación, para quien la elección entre un despliegue dependiente del framework y uno autocontenido, o entre MSIX y un instalable clásico, no es teórica: es lo que va a tener que preparar, empujar y actualizar en cada máquina.

---

## Definición

### Qué es

La **distribución** es la forma en que un componente construido se empaqueta y se hace llegar al nodo donde va a correr; la **instalación** es lo que ocurre en ese nodo para dejarlo en condiciones de ejecutarse. Entre el código compilado y el proceso en ejecución hay una cadena de decisiones —qué se incluye en el paquete, qué se asume presente en el host, cómo se registra el arranque, cómo se actualiza— y cada una tiene consecuencias que el lector de un informe necesita entender.

En .NET esas decisiones tienen nombres precisos y documentación oficial (`N-09` a `N-17`), lo que permite describirlas sin ambigüedad. El punto de partida es el **modelo de publicación**, que `N-09` fija así:

| Modelo | Qué incluye el paquete | Qué exige del host | Cuándo conviene |
|---|---|---|---|
| **Dependiente del framework (FDD)** — por defecto | Solo la aplicación y sus dependencias | El runtime de .NET instalado | Hosts controlados con runtime gestionado; paquetes pequeños |
| **Autocontenido (SCD)** | La aplicación **más** el runtime de .NET | Nada de .NET | Equipos del cliente donde no se controla qué runtime hay |

Sobre esos dos modelos se combinan tres variantes, también de `N-09`: **single-file** empaqueta la publicación en un único ejecutable; **ReadyToRun** compila anticipadamente a código nativo para acelerar el arranque; **Native AOT** produce un binario nativo sin JIT ni metadata de runtime. No son un tercer modelo: son modificadores que se aplican sobre FDD o SCD.

La elección FDD/SCD es la primera que un informe de `CTX-3` debe declarar, y la razón está en la tabla: en una terminal del cliente no se controla qué runtime hay instalado, de modo que un despliegue autocontenido evita la clase entera de fallos «funcionaba en la máquina de al lado, en esta no».

### Qué problema resuelve

**Qué tiene que existir antes de arrancar.** Un componente no corre en el vacío: necesita un runtime, un servicio que lo lance, permisos, puertos. Describir el modelo de distribución es enumerar esas precondiciones, que es lo que un `ACT-04` necesita para preparar un host y lo que un integrador necesita para replicar el despliegue.

**Cómo se actualiza sin romper la operación.** Distribuir no es solo la primera instalación: es cada actualización posterior. En un centro de datos se resuelve con un pipeline; en el borde, con un mecanismo de actualización que llegue a cada terminal sin que alguien vaya físicamente a cada sala. La estrategia de actualización es parte del modelo de distribución y el informe la trata, no la deja implícita.

**Qué se rompe si el host no cumple.** Un despliegue FDD en un host sin el runtime correcto no arranca; un servicio de Windows sin permisos de cuenta no inicia tras el reinicio. Nombrar las precondiciones es también nombrar las causas de fallo de instalación, y eso ahorra diagnóstico.

### Qué NO es, y con qué se lo confunde

**No es el manual de instalación.** El informe describe el modelo y su justificación —«el escritorio se distribuye como paquete MSIX con actualización automática, para no depender de una visita a cada sala»—; el procedimiento paso a paso, con comandos y capturas, es `DOC-INSTALL`. Meter el manual dentro del informe lo infla y lo desactualiza, porque los pasos cambian más rápido que la arquitectura.

**No es la build.** Cómo se compila y se corren las pruebas pertenece al proceso de desarrollo, fuera del alcance de este informe. La distribución empieza donde termina la build: con el artefacto ya producido.

**No es «lo mismo que contenedores».** Empaquetar en una imagen de contenedor es *una* forma de distribuir, adecuada para servicios en infraestructura controlada (`CTX-4`), pero no para un escritorio que corre en la terminal de una sala. Confundir «distribución moderna» con «todo en contenedores» lleva a proponer Docker donde el problema es instalar un ejecutable en Windows.

**No es la topología.** [`TEM-TOPOLOGIA`](Topologia-y-Entornos.md) dice dónde corre cada cosa; este tema dice cómo llega ahí. Se apoyan mutuamente —no se puede describir la distribución sin saber los nodos— pero responden preguntas distintas.

---

## Las opciones de hosting y empaquetado en .NET

El informe elige, por componente, cómo se aloja el proceso. Las opciones verificadas y sus fuentes:

**Servicios web (backend y frontend).** Kestrel es el servidor HTTP de ASP.NET Core y puede exponerse **directamente** o **detrás de un proxy inverso** —IIS en Windows, Nginx en Linux— (`N-10`). El proxy inverso asume el borde: terminación TLS, balanceo, servido de estáticos, protección ante clientes lentos. En Linux, el patrón documentado es Kestrel detrás de Nginx, con una **unidad systemd** que arranca y supervisa el proceso (`N-14`); en Windows, además de IIS, una web app puede alojarse **como servicio de Windows** sin IIS, con arranque automático tras el reinicio (`N-11`).

**Servicios en segundo plano (el Worker de grabación).** Un proceso de larga duración sin interfaz se construye como **Worker Service**: la plantilla `dotnet new worker`, la clase base `BackgroundService` y el host de `Microsoft.Extensions.Hosting` (`N-12`). Para integrarlo con el gestor de servicios del sistema operativo, `N-13` provee `UseWindowsService()` —lo registra como servicio de Windows, con su *lifetime*, *content root* y *logging* adecuados— y `UseSystemd()` para el equivalente en Linux. Es exactamente el mecanismo por el que el Worker de audiencias corre como servicio de Windows en cada terminal.

**Contenedores (para el centro, si se contenedoriza).** El SDK de .NET produce imágenes de contenedor **sin Docker**: `dotnet publish -t:PublishContainer` genera una imagen OCI directamente (`N-15`). Docker o Podman solo hacen falta para *ejecutar* la imagen, no para construirla. Es la vía natural de distribución en `CTX-4`, y una opción para el backend del centro si se despliega contenedorizado.

**Escritorio (el programa del operador).** Una aplicación de escritorio Windows se distribuye con **MSIX** o **ClickOnce**, ambos con actualización automática (`N-16`); una aplicación **.NET MAUI** se publica empaquetada como MSIX o desempaquetada (`N-17`). La actualización automática es el rasgo decisivo en el borde: sin ella, cada nueva versión exige tocar cada terminal a mano.

Una advertencia de alcance, tomada de `ANEXO-REFERENCIAS`: la afirmación de que .NET ofrece imágenes base *chiseled* o *distroless* **no** está en `N-15` y no debe atribuírsele; pertenece a la página de imágenes base y no se verificó en esta revisión. Si el informe quiere apoyarse en eso, lo marca como por verificar.

---

## El problema de `CTX-3`: instalar y actualizar en cada terminal

En el borde, la distribución deja de ser un acto y pasa a ser un **procedimiento repetido**. `MARCO-CONTEXTOS` lo enuncia sin rodeos: el informe tiene que describir la instalación por terminal como un procedimiento, no como una nota al pie, y un informe de `CTX-3` que dedica tres párrafos al despliegue está mal calibrado. La razón es aritmética: catorce salas son catorce instalaciones del escritorio y catorce del Worker, más cada actualización futura multiplicada por catorce.

El informe no reproduce el procedimiento —eso es `DOC-INSTALL`—, pero sí describe su **forma** y sus decisiones:

- **Qué se instala en cada terminal**: el escritorio (paquete MSIX, `N-16`) y el Worker (servicio de Windows, `N-13`), ambos autocontenidos (`N-09` SCD) para no depender del runtime presente en la máquina.
- **Cómo se registra el arranque**: el Worker como servicio con arranque automático, de modo que tras un reinicio de la terminal vuelva a estar disponible sin intervención.
- **Cómo se actualiza**: la actualización automática de MSIX para el escritorio; para el Worker, el mecanismo que el equipo haya definido —y que el informe nombra, aunque el detalle esté en `DOC-DEPLOY`—.
- **Qué se hace con la instalación desde cero**: se remite a `DOC-INSTALL`, que contiene la guía completa de preparación de una terminal nueva.

Tratar esto como un procedimiento con decisiones, y no como un «se instala en cada máquina» al pasar, es lo que distingue un informe de `CTX-3` proporcionado de uno que subestima su propio despliegue.

---

## Distribución por componente del sistema de audiencias

La tabla siguiente es **sintética** e ilustra cómo un informe resume la distribución de cada componente. Los valores son de ejemplo coherente con `CTX-3`, no la configuración de un sistema real.

| Componente | Se aloja como | Modelo de publicación | Empaquetado / distribución | Actualización | Fuente |
|---|---|---|---|---|---|
| Escritorio del operador | App Windows (WPF) | Autocontenido (SCD) | MSIX por terminal | Automática (MSIX) | `N-09`, `N-16` |
| Worker de grabación | Servicio de Windows | Autocontenido (SCD) | Instalable por terminal; `UseWindowsService()` | Empujada por terminal | `N-09`, `N-12`, `N-13` |
| Backend | ASP.NET Core / Kestrel tras Nginx | Dependiente del framework (FDD) | Publicación en el servidor; unidad systemd | Pipeline al centro | `N-09`, `N-10`, `N-14` |
| Frontend administrativo | Blazor *interactive server* | Dependiente del framework (FDD) | Junto al backend en el centro | Pipeline al centro | `N-09`, `N-10` |
| Base de datos | PostgreSQL + Npgsql | — (motor gestionado) | Instalación gestionada en el centro | Ventana de mantenimiento | `N-18` |
| Servidor de archivos | Servicio FTP o tus | — | Instalación en el centro | Ventana de mantenimiento | `N-08`, `F-01` |

La tabla deja ver el patrón que un informe de `CTX-3` debe hacer visible: los componentes del **centro** usan FDD porque el runtime está controlado y el despliegue es un pipeline único; los componentes de la **terminal** usan SCD porque no se controla la máquina del cliente, y su actualización es un problema distribuido, no un pipeline. Esa asimetría —mismo lenguaje, dos modelos de distribución según dónde corre— es la observación de arquitectura que la tabla comunica de un vistazo.

---

## Aplicación por escenario

### `ESC-1` — Distribución prevista

Se decide y se registra el modelo de distribución antes de construir. Es el momento de elegir FDD o SCD por componente, el mecanismo de empaquetado del escritorio y la estrategia de actualización, y de anotar esas elecciones como decisiones de arquitectura con sus alternativas, porque después parecen inevitables. La trampa de `ESC-1` —sobredimensionar— aparece si se propone una cadena de distribución de nivel industrial para un sistema que todavía se instala a mano en dos salas de prueba.

### `ESC-2` — Distribución real

Se describe cómo se distribuye de verdad, con las divergencias respecto de lo planeado. Es común que el plan dijera «actualización automática en todas las terminales» y la realidad sea «se actualiza yendo sala por sala porque el mecanismo automático nunca se terminó». El informe de `ESC-2` describe la realidad: es lo que el lector necesita, y ocultarlo produce un documento elegante e inútil.

### `ESC-3` — Distribución en transición

El modelo de distribución cambia —de un instalable clásico a MSIX, de FDD a SCD, de FTP a tus en el canal de archivos— y el informe describe el estado de partida, el objetivo y la convivencia. Una migración de empaquetado suele implicar un período en que conviven terminales con el modelo viejo y el nuevo, y ese período es material del informe, no un detalle a resolver después.

### `ESC-4` — Distribución observada

Se reconstruye cómo se distribuye un sistema ajeno, y se detecta lo que falta. Un informe que describe los componentes pero calla cómo se instalan y actualizan deja sin respuesta la pregunta operativa más cara. Donde no hay informe, se infiere del sistema: un servicio de Windows registrado sugiere un Worker con `UseWindowsService()`; un paquete MSIX en el catálogo sugiere actualización automática. Se registra como inferencia hasta confirmarlo.

### Qué cambia según el contexto

| Contexto | Peso del tema | Decisión dominante | Riesgo |
|---|---|---|---|
| `CTX-1` Monolito | Bajo | FDD vs SCD del único artefacto | Inflar: describir un despliegue trivial como si fuera complejo |
| `CTX-2` Cliente-servidor | Medio | Hosting del backend: Kestrel solo o tras proxy | Omitir dónde termina TLS y cómo arranca el servicio tras reinicio |
| `CTX-3` Borde distribuido | Alto | Empaquetado y actualización **por terminal** | Tratar la instalación por terminal como una nota al pie |
| `CTX-4` Multiservicio | Alto | Contenedorización y orquestación | Describir cada imagen con igual detalle sin distinguir lo central |

El contraste entre `CTX-1` y `CTX-3` es el eje del tema. En el monolito, la distribución es un párrafo y estirarla es relleno; en el borde, es un procedimiento repetido y despacharla en un párrafo es un error de calibración. El mismo tema pesa lo opuesto según dónde corra el software.

---

## Ejemplos concretos

Los fragmentos son **sintéticos** y pertenecen al informe del sistema de audiencias.

### Fragmento — modelo de distribución del escritorio y el Worker

> **Distribución en las terminales.** El programa de escritorio se distribuye como paquete **MSIX** (`N-16`) con actualización automática habilitada: cuando se publica una versión nueva, cada terminal la recibe sin que un técnico deba visitarla, requisito impuesto por la dispersión geográfica de las salas. El Worker de grabación se instala como **servicio de Windows** mediante `UseWindowsService()` (`N-13`), con arranque automático, de modo que tras cortes de energía o reinicios de la terminal vuelva a estar operativo sin intervención. Ambos se publican **autocontenidos** (SCD, `N-09`) para no depender de la versión del runtime de .NET presente en cada equipo, que no está bajo control del operador. El procedimiento completo de preparación de una terminal nueva está en `DOC-INSTALL`.

La última frase es deliberada: el informe describe el modelo y la razón, y remite el paso a paso. Repetir aquí el procedimiento sería duplicar `DOC-INSTALL` y garantizar que las dos versiones diverjan.

### Fragmento — decisión de hosting del centro

> **Hosting del centro.** El backend corre sobre **Kestrel detrás de Nginx** en un servidor Linux (`N-10`, `N-14`); Nginx asume la terminación TLS y el servido de estáticos, y una unidad **systemd** arranca y supervisa el proceso, reiniciándolo si termina de forma anómala. Se optó por despliegue **dependiente del framework** (FDD, `N-09`) porque el runtime del servidor está gestionado por el equipo de operación, lo que mantiene los paquetes pequeños y la actualización del runtime independiente de la de la aplicación.

El fragmento nombra la fuente de cada decisión y su porqué. Un `ACT-04` que lo lea sabe qué preparar en el servidor y por qué el modelo del centro difiere del de las terminales, sin tener que preguntar.

---

## Preguntas guía

- ¿Elegí FDD o SCD por componente, y dije por qué? ¿Los componentes en equipos del cliente son autocontenidos?
- ¿Cómo se aloja cada servicio web: Kestrel solo o detrás de un proxy, y dónde termina TLS?
- ¿El proceso de fondo arranca solo tras un reinicio del host? ¿Con qué mecanismo —servicio de Windows, systemd—?
- En `CTX-3`, ¿describí la instalación por terminal como un procedimiento con decisiones, o la despaché en una frase?
- ¿Cómo se **actualiza** cada componente, no solo cómo se instala la primera vez?
- ¿Remití el paso a paso a `DOC-INSTALL` y `DOC-DEPLOY` en lugar de copiarlo dentro del informe?
- ¿Estoy proponiendo contenedores donde el problema es instalar un ejecutable en la terminal de una sala?

---

## Criterios de calidad

### Distribución bien descrita

Cada componente declara su modelo de publicación, su forma de alojamiento y su mecanismo de actualización, con la fuente `N-xx` que respalda cada opción. La asimetría entre los componentes del centro y los del borde está explicada, no dejada a que el lector la infiera. La instalación por terminal se trata con el peso que su repetición merece, y el detalle procedimental se remite a los documentos operativos por su ID. El lector sabe qué tiene que existir en cada host antes de que el software arranque.

### Distribución mal descrita y antipatrones

**La instalación al pasar.** «Se instala en cada terminal» sin describir el empaquetado, el arranque ni la actualización. Es el error de calibración característico de `CTX-3`: subestimar el problema más caro del despliegue.

**El modelo de publicación implícito.** No decir si el despliegue es FDD o SCD. El lector no sabe entonces si el host necesita el runtime, que es justo lo que determina si el software va a arrancar.

**El manual dentro del informe.** Copiar el procedimiento paso a paso de `DOC-INSTALL`. Infla el documento y garantiza que las dos versiones diverjan; el informe referencia, no duplica.

**Contenedores por defecto.** Proponer Docker para todo, incluido el escritorio que corre en Windows en la terminal de una sala. La contenedorización resuelve la distribución en infraestructura controlada, no la instalación de un ejecutable de escritorio.

**La actualización olvidada.** Describir la primera instalación y callar cómo se distribuyen las versiones siguientes. En el borde, la actualización es el problema recurrente, no la instalación inicial.

**Atribuir a una fuente lo que no dice.** Afirmar que .NET ofrece imágenes *chiseled* citando `N-15`, cuando esa fuente no lo dice. Si el dato no está verificado, se marca por verificar, no se le pone una cita que no lo respalda.

---

## Anexo — Ficha de distribución por componente

Se completa una fila por componente desplegable. La columna de justificación de cada elección es la que convierte la ficha en una decisión trazable y no en un inventario.

```yaml
distribucion:
  escenario: ESC-?
  contexto: CTX-?
  componentes:
    - nombre: ""
      corre_en: infraestructura_operador | equipo_cliente
      se_aloja_como: web_kestrel | web_tras_proxy | windows_service | systemd | contenedor | app_escritorio
      modelo_publicacion: fdd | scd
      variante: none | single_file | readytorun | native_aot
      justificacion_modelo: ""       # por qué FDD o SCD en este componente
      empaquetado: msix | clickonce | instalable | imagen_oci | publicacion_directa
      arranque_automatico: si | no
      actualizacion:
        mecanismo: automatica_msix | pipeline | empuje_por_terminal | manual | ventana_mantenimiento
        alcance: unico_host | por_terminal
      fuente: []                      # N-09, N-10, N-13, N-14, N-15, N-16, N-17...
  instalacion_por_terminal:          # solo si hay componentes en el borde
    es_procedimiento_repetido: si | no
    cantidad_de_terminales: ""
    detalle_remitido_a: DOC-INSTALL | DOC-DEPLOY
  afirmaciones_no_verificadas: []
```
