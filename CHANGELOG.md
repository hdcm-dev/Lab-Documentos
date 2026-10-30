# Changelog

Cambios relevantes de las guías del repositorio. El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

## 2026-10-30 (Framework Concept: el login es una página del contenido)

### Concepts/Framework-Concept/Demo — decisión del PO

#### Cambiado

- **La cara del ingreso y del desbloqueo pasa al contenido dinámico** (`Shell/Auth/PresentadorDeSesion.cs`, `ConfiguracionDelCascaron.cs`, `FrameworkConcept.Shell.csproj`, `DISENO-RED-Y-AUTENTICACION.md`): la puerta de sesión del contenido (`Content/Auth/PuertaDeSesion.razor`) ya no queda tapada por las pantallas XAML del cascarón y se actualiza sin tienda. El canje, los tokens y el diálogo del sistema siguen en el cascarón (`auth.session`). Nueva propiedad `-p:FcPantallasDeSesion=Contenido|Cascaron` (por defecto `Contenido`). Verificado en el moto g42.

## 2026-10-29 (Framework Concept: TPR del g42 medido por el cascarón)

### Concepts/Framework-Concept/ — expediente 0001, folio 049

#### Cambiado

- **Marca `primer-render` por el cascarón** (`Shell/Pages/ContenidoPage.xaml.cs`, `Tools/LectorDeTpr.cs`, `LectorDeTprTests`): al llegar «Listo», el cascarón lee del WebView la hora de la marca y la escribe con la etiqueta `FC` y el `tpr=` contado desde el inicio del proceso. El lector usa ese valor cuando está. No cambia el contrato del puente.
- **TPR en el moto g42** (`99-Anexos.md` §6, `Developer-Guide.md`, `01-Actualizacion-Sin-Tienda.md`, `Demo/README.md`): pasa de **NO OBTENIDO** a **NO CUMPLE**. Con el APK Release: 6 874 a 7 309 ms en régimen y 9 152 ms en el primer arranque; el umbral es 5 000 ms. Se abre la condición de revisión (2) de P1.

## 2026-10-29 (Framework Concept: estado del tramo A5)

### Concepts/Framework-Concept/ — expediente 0001, folio 048

#### Cambiado

- **Tramo A5** (`03` §7, `06` y §9, `Developer-Guide.md`, `99-Anexos.md` §6): la guía decía que no había corrida del workflow con `run_id` y que `main` no estaba protegida. Las dos cosas ya están, verificadas por la API. La corrida completa con compuerta y APK es la `40847315104`, en `hpdev-runner-dev`; la omisión declarada (PR sin `Demo/`), la `40738280836`; y `main` exige `fw-ok` (estricto y también para administradores). Sigue pendiente la prueba de un PR en rojo con M-13.

## 2026-10-29 (Framework Concept: runner de hpdev, Keycloak y TPR en el g42)

### Concepts/Framework-Concept/ y el comentario de framework-concept-demo.yml — expediente 0001, folios 046 y 047

#### Cambiado

- **TPR en el moto g42** (`99-Anexos.md` §6, `Developer-Guide.md`, `01-Actualizacion-Sin-Tienda.md`, `Demo/README.md`): pasa de «sin medir» a **NO OBTENIDO**, con las cotas medidas el 2026-10-28 (folio 047). El contenido llega a «puente Listo» de 7,3 a 7,7 s, pero la marca `primer-render` no llega al logcat porque el WebView del teléfono no reenvía la consola: el TPR queda acotado entre 4,4 y 7,7 s. La condición (2) de P1 sigue abierta.
- **Runner**: el glosario, la fila `run_id` del anexo, `03` §8, `06` §8.2 y el comentario del workflow nombran `hpdev-runner-dev`. Ese runner lleva la misma etiqueta `[self-hosted, i7infra-dev]`, que no cambia, y su entorno se verificó: SDK 10.0.403, maui-android 10.0.110, sin `docker.sock` ni KVM. `i7infra-runner-dev` volvió con la misma etiqueta: el campo `runner_name` dice cuál corrió.
- **Keycloak** (`Demo/README.md`, `Demo/infra/keycloak/keycloak.sh`): la IP de ejemplo se describe por su función («la máquina que sirve Keycloak»), no por la máquina del laboratorio.

## 2026-10-28 (Framework Concept: IDs en su propio espacio)

### Concepts/Framework-Concept/ y tools/docs-guard/prefijos.txt — expediente 0001

#### Cambiado

- **IDs del Framework Concept** con prefijo propio: `TEM-01`…`TEM-08`, `GUIA-INDICE`, `MARCO-REFERENCIA` y `ANEXO-99` pasan a `FC-TEM-01`…`FC-TEM-08`, `FC-GUIA-INDICE`, `FC-MARCO-REFERENCIA` y `FC-ANEXO-99` (`doc_id`, `traces` y citas en el texto, 215 apariciones en 12 archivos). Chocaban con los IDs del cuerpo SDEI y de otras guías, y el control `id-duplicado` de docs-guard (PR #31) los marcaba en rojo. Se quita de `00-Marco.md` la traza a `MAPA-CONCEPTUAL`, que el Framework Concept no declara.
- **`tools/docs-guard/prefijos.txt`**: registra el prefijo `FC` (interno).

## 2026-10-28 (Framework Concept: enlaces del folio 042 en el anexo)

### Concepts/Framework-Concept/ — expediente 0001, constancia 043

#### Añadido

- **`99-Anexos.md`**, tabla «Enlaces a los folios y ensayos citados»: fila del **folio 042** (la guía lo cita en cuatro lugares y no tenía enlace) y enlace «TPR en emulador» a `E03-Red-Y-Autenticacion/P1-TPR-EN-EMULADOR.md` en la fila E03. Es lo único del PR #14 que `main` no tenía; el resto de ese PR quedó superado por el folio 042 y se cierra sin fusionar.

## 2026-10-28 (compuerta E1-G: frontmatter que un lector YAML acepta)

### tools/docs-guard/ y scripts/ci-local.sh — expediente 0002 de DocAsCode, compuerta E1-G

#### Añadido

- **`dg.py esquema`** (grupo `PR-2`): regla `frontmatter-no-yaml`. Un valor sin comillas que contiene «: » no es YAML válido y un lector estricto (un generador de sitio, un agente) rechaza todo el frontmatter. Bloquea en el cuerpo nuevo y avisa en lo heredado. Fixtures `esquema-yaml/rojo` y `verde` en la autoprueba.

#### Cambiado

- **`README.md`** del cuerpo `SDEI`, **`TEM-03-Topologia-Y-Versionado.md`** y **`TEM-06-Compuertas-Y-CI.md`**: `description` entre comillas. El README además actualiza la fila «Estado»: los siete temas ya están escritos.
- **`fixtures/url-fecha/obsoleto/a.md`**: `description` entre comillas.

## 2026-10-28 (compuerta E1-G: reparaciones de cierre del escalón 1)

### .github/workflows/ y punteros de lectura — expediente 0002 de DocAsCode, compuerta E1-G

#### Cambiado

- **`docs-guard.yml`**: corre también en `push` a `main` y por `workflow_dispatch`, para que la corrida sobre el commit de merge quede registrada (criterio G-b).
- **`framework-concept-demo.yml`**: `actions/checkout` fijada por SHA (v4.2.2), como el resto de los workflows.
- **`README.md`** raíz y **`Guides/README.md`**: la ruta de lectura apunta al README del cuerpo `SDEI` en lugar de la guía de un solo archivo, que está `obsoleta`. El README raíz nombra `AGENTS.md` como entrada para agentes.
- **`README.md`** del cuerpo `SDEI`: el diagrama de rutas ya no marca los temas «en redacción»; los siete tienen archivo.
- **`00-Marco.md`** (definición de traza), **`01-Mapa.md`** §3 y **`README.md`** del cuerpo (ruta «intervenir»): aclaraciones que pidió la prueba de lectura de E1-G: qué comprueba una traza sin versión, que `traces-resolve` no depende de `corpus_id`, qué hacer con una página desactualizada que no traza a ninguna ficha y por qué ruta entra alguien que recién llega.

## 2026-10-27 (la guía de un solo archivo pasa a obsoleta; `url-fecha` ampliado)

### Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/ — cuerpo `SDEI`, expediente 0002 de DocAsCode, unidad E1-U4f

#### Cambiado

- **`Sistema-Documentador-Tecnico-Guide.md`** (`DOC-SDT-0001`): `status: obsoleto`. Suma un aviso con su reemplazo (el cuerpo `SDEI`) y una tabla de destinos «sección vieja → `doc_id` nuevo» con las 47 secciones, el ancla de destino y la cobertura (completa o parcial, con lo que no se trasladó). El texto original no cambia; se archiva en la versión siguiente del cuerpo.
- **`README.md`** del cuerpo y **`01-Mapa.md`**: la ruta «aprender» ya no pasa por la guía de un solo archivo; su fila de la tabla de contenido dice `obsoleto` y enlaza la tabla de destinos.
- **`TEM-06-Compuertas-Y-CI.md`** y **`99-Anexos/Criterios-De-Calidad.md`** (`CQ-11`): `url-fecha` está en `PR-7` y bloquea en el cuerpo nuevo.
- **`TEM-07-Marco-SDD.md`**: la cita de las reglas del marco SDD lleva fecha de consulta.

### tools/docs-guard/ y scripts/ci-local.sh — unidad E1-U4f (E-F7)

#### Añadido

- **`dg.py url-fecha`** (grupo `PR-7`): toda URL y toda cita por dominio en un code span lleva, en su párrafo o fila, la fecha de consulta o la cita enlazada al anexo de referencias. Bloquea en el cuerpo nuevo salvo en documentos `obsoleto`; avisa en lo heredado. Fixtures `url-fecha/rojo`, `verde` y `obsoleto` en la autoprueba.

## 2026-10-27 (temas 04 a 06 del cuerpo del sistema documentador)

### Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/ — cuerpo `SDEI`, expediente 0002 de DocAsCode, unidad E1-U4c

#### Añadido

- **`TEM-04-Doble-Audiencia.md`** (`TEM-SDEI-04`): una fuente para personas y agentes; canales por escenario y contexto, `AGENTS.md` como puntero, índice derivado del frontmatter y la compuerta anti-inyección, con ensayo reproducible.
- **`TEM-05-Ayuda-Al-Usuario-Final.md`** (`TEM-SDEI-05`): ayuda derivada de la especificación, traza con versión, divergencias declaradas y rol como contexto que se resuelve en el servidor, con ensayo de `traces-resolve` sobre la mini demo.
- **`TEM-06-Compuertas-Y-CI.md`** (`TEM-SDEI-06`): los dos carriles, la regla de bloqueo de cinco condiciones, pares rojo/verde, seguridad del runner y plantilla para dar de alta un control.
- **`99-Anexos/Referencias.md`**: entrada `ref-gitlab-testing-sf` (GitLab, *Documentation testing*).

#### Cambiado

- **`README.md`** del cuerpo: las filas de `TEM-04` a `TEM-06` de la tabla de contenido enlazan a sus archivos.
- **`TEM-03-Topologia-Y-Versionado.md`** y **`TEM-07-Marco-SDD.md`**: sus pies enlazan a `TEM-04` y a `TEM-06` (antes «en redacción»). Los temas 04 a 06 usan los seis encabezados sin número, igual que 01 a 03 (VER-T07).
- **`00-Marco.md`** §6, **`PE-01`** y **`EJR-SDEI-04`**: el caso 60/80 es el largo del nombre de una localidad (`RN-LOC-01`), como dice la ficha verificada contra el código, no el de un comentario.

## 2026-10-27 (TEM-07 y proceso «Cómo intervenir»)

### Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/ — expediente 0002 de DocAsCode, unidad E1-U4d

#### Añadido

- **`TEM-07-Marco-SDD.md`** (`TEM-SDEI-07`, borrador hasta el escalón 2): integración del sistema documentador con un marco SDD, con los seis encabezados del tema en la forma de los temas 01 a 03, la frontera «se cita / se agrega / decide una persona» por clase de pieza y un ensayo de `traces-resolve` contra el SDD de Lab-Geometria en `5f07b12` (las versiones congeladas en `_legacy/` vuelven ambiguo el registro).
- **`Examples/Como-Intervenir.md`** (`GP-SDEI-INTERVENIR`): plantilla del proceso en cinco pasos con bloques `entradas` / `salidas` / `validaciones`; el paso «validar» corre `scripts/ci-local.sh`.
- **`Examples/Como-Intervenir-Lab-Documentos.md`** (`GP-SDEI-INTERVENIR-LD`): la instancia de una página para este repositorio, con las guías prácticas de los ejemplos.
- **`99-Anexos/Referencias.md`**: GitHub (2026) Spec Kit, Kiro (s. f.) y Nygard (2011).

#### Cambiado

- **`README.md`** y **`01-Mapa.md`** del cuerpo: filas de la tabla de contenido y enlaces de la ruta «intervenir».

## 2026-10-27 (temas 01 a 03 del cuerpo del sistema documentador)

### Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/ — cuerpo `SDEI`, expediente 0002 de DocAsCode, unidad E1-U4b

#### Añadido

- **`TEM-01-Sistema-Documentador.md`** (`TEM-SDEI-01`): qué es un sistema documentador, arquitectura de referencia, el portal de un proveedor de notificaciones como caso de industria, adopciones públicas con fecha (docs as code, referencia desde OpenAPI, lint de prosa) y un inventario hecho por comando.
- **`TEM-02-Ciclo-Documental.md`** (`TEM-SDEI-02`): las fases 1a a 5 y el control de deriva, el registro de requerimientos documentales con criterio Dado/Cuando/Entonces, estados del documento y la plantilla de cláusula.
- **`TEM-03-Topologia-Y-Versionado.md`** (`TEM-SDEI-03`): las cuatro topologías, las anclas de versión, la regla del mismo pull request y adopciones públicas con fecha (documentación junto al código, rama por versión, copias versionadas).
- **`99-Anexos/Referencias.md`** §7: SemVer, Keep a Changelog, Diátaxis, GivenWhenThen y doce repositorios públicos citados por commit.

#### Cambiado

- **`README.md`** del cuerpo: la tabla de contenido enlaza los tres temas. **`01-Mapa.md`**: su siguiente documento es el tema 01.

## 2026-10-27 (AGENTS.md de la raíz)

### Raíz, scripts/ y tools/docs-guard/ — expediente 0002 de DocAsCode, unidad E1-U4e

#### Añadido

- **`AGENTS.md`**: puntero para agentes de código y puerta de la ruta «agente» del cuerpo `SDEI` (D27). Dice qué es el repositorio, qué leer y en qué orden (README del cuerpo, convenciones del Marco, esquema de frontmatter, glosario, Mapa), cómo validar con `scripts/ci-local.sh`, qué puede proponer un agente y qué es clase H, y enlaza los anexos de referencia. Cambiarlo es clase H.
- **`tools/docs-guard/excepciones.txt`**: primera excepción con huella (AK-3): `AGENTS.md` queda fuera de AI-3 mientras su SHA-256 no cambie.

#### Cambiado

- **`scripts/ci-local.sh`**: los grupos `enlaces`, `ai` y `estructura` revisan también el `AGENTS.md` de la raíz del repositorio que se verifica, si existe (CL-03); si falta, `enlaces` avisa.
- **README del cuerpo `SDEI`** y **README de la raíz**: enlazan `AGENTS.md`; la caja de la ruta «agente» ya no figura «en redacción».

## 2026-10-27 (mini demo MovilidadUrbana)

### Guides/, .github/ y tools/docs-guard/ — expediente 0002 de DocAsCode, unidad E1-U3

#### Añadido

- **`Examples/MovilidadUrbana-Mini-Demo/`** en la guía del sistema documentador: documentación de MovilidadUrbana.MAUI escrita desde este repositorio, con el código fijado por SHA en `source.lock` (Lab-E2E.WebBlazor, `8fb4c3a`). RRD, fichas `RN-LOC-01` y `RN-LOC-02` con anclas `ruta#Símbolo@SHA`, ayuda al usuario (CL-06), técnica (CL-07), el contrato OpenAPI extraído en ejecución y un README con el tutorial.
- **`.github/workflows/mini-demo-movilidad.yml`** y **`verificar.sh`**: bajan el SHA con `git archive`, compilan y arrancan la ApiWeb, extraen `GET /openapi/v1.json` y comprueban contrato (bloquea), contrato con una operación plantada (tiene que dar rojo), trazas de nivel 1 y 2 (bloquea), la ficha RN-LOC-01 sin corregir (tiene que dar rojo) y la deriva contra `main` (avisa).
- **`tools/docs-guard/`**: anclas de nivel 2 en `dg.py trazas --fuente` (y `--deriva`); IDs `RD-nn` del RRD en el registro; prefijo `DEMO`; fixtures `anclas/` (par rojo/verde y la negativa real `real-rn-loc-01/`).

#### Cambiado

- **`dg.py contratos`**: un contrato con `source.lock` que declara `workflow:` lo compara ese workflow; `docs-guard` ya no lo marca como «sin extracción».

## 2026-10-27 (esqueleto del cuerpo del sistema documentador)

### Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/ — cuerpo `SDEI` 2.0.0-borrador, expediente 0002 de DocAsCode, unidad E1-U4a

#### Añadido

- **`README.md`** del cuerpo: `corpus_id: SDEI`, `version: 2.0.0-borrador`, objetivos de aprendizaje `OBJ-SDEI-01` a `05`, las tres rutas de lectura (aprender, intervenir, agente) y la tabla de contenido completa. Al declarar `corpus_id`, la carpeta pasa a ser cuerpo nuevo para `docs-guard`: frontmatter obligatorio y `reflint` bloqueante.
- **`00-Marco.md`**: los cuatro escenarios `ESC-SDEI-1` a `4` (corpus de guías, app sin roles, panel con roles, servicio con paquetes), los contextos `CTX-SDEI-1` a `4`, los actores `ACT-SDEI-1` a `6` y las convenciones del cuerpo.
- **`01-Mapa.md`**: tablas «estoy acá → qué aplico» por escenario, contexto, artefacto (catálogo CL-01 a CL-14) y actor, e índice de anexos.
- **`99-Anexos/`**: esquema de frontmatter y registro de prefijos; glosario; referencias APA 7 con anclas `ref-…` y fecha de consulta; criterios de calidad `CQ-01` a `CQ-14`; patrones de ejemplos `PE-01` a `PE-12`; lista de verificación `VER-*`; «Cómo decidir» con la escala anclada y una matriz trabajada; ejercicios `EJR-SDEI-01` a `05` con claves.
- **`tools/docs-guard/prefijos.txt`**: prefijo `GP` (guía práctica de un ejemplo).

#### Cambiado

- **`Examples/Lanzar-Example.md`**: suma frontmatter (`GP-SDEI-LANZAR`), que el cuerpo nuevo exige. El texto no cambia.

## 2026-10-27 (docs-guard completo: grupos PR-1 a PR-7 y carril programado)

### .github/, scripts/ y tools/docs-guard/ — expediente 0002 de DocAsCode, unidad E1-U2

#### Añadido

- **`tools/docs-guard/`**: los controles del carril PR, portados de los prototipos de las comisiones y reescritos sin dependencias (`dg.py`, Python 3 con biblioteca estándar; mermaid por lockfile). PR-1 relativos que salen del repositorio; PR-2 esquema de frontmatter (diez campos más `description`), registro cerrado de prefijos y esquema del RRD; PR-3 anti-inyección AI-1 a AI-5 con excepciones con huella; PR-5 `traces-resolve`; PR-6 diff del contrato OpenAPI; PR-7 piezas fijadas con su SHA-256 y `reflint`. Ver `tools/docs-guard/README.md`.
- **`tools/docs-guard/fixtures/`**: un par rojo/verde por control que bloquea y el fixture permanente AK-7, que tiene que dar rojo en cada corrida. El paso `autoprueba` los corre en cada PR.
- **`tools/docs-guard/dominios.txt`**: lista de dominios permitidos (AK-4), clase P con aprobación explícita. **`prefijos.txt`**, **`excepciones.txt`**, **`esquemas/`** y **`arbol.tsv`** (tabla repo → ruta del carril programado).
- **`.github/workflows/docs-enlaces-semanal.yml`** y **`tools/docs-guard/semanal.sh`**: carril programado, sólo aviso. Arma el árbol del workspace con los repos públicos (clon anónimo), corre `lychee --root-dir` con red, el atraso de `base_commit` y `source.lock`, la deriva de piezas fijadas y la frescura de `last_review`, y abre o actualiza un issue. Declara `issues: write` sólo para eso.

#### Cambiado

- **`.github/workflows/docs-guard.yml`**: un solo job con un paso por grupo (PR-1 a PR-7, autoprueba y avisos) y un agregador estricto al final. Cada grupo corre aunque otro falle, para que el PR vea todos sus defectos en una corrida. Las herramientas se toman de `scripts/` y `tools/docs-guard/`.
- **`scripts/ci-local.sh`**: los mismos pasos, en el mismo orden; `todo` corre todos los grupos y agrega. Los pasos `herramientas`, `enlaces` y `estructura` conservan el nombre.
- **Guía del sistema documentador**: suma `description` al frontmatter, para que la línea de base del esquema sea 0.

## 2026-10-27 (medición del TPR de la demo)

### Concepts/Framework-Concept/ — expediente 0001 de Framework Concept, folio 042

#### Corregido

- **Medición del TPR (P1)**: la falta de la marca `primer-render` en el cierre era la pantalla apagada del emulador (el WebView no carga sin la actividad al frente), no la marca. `Demo/scripts/guion.sh tpr` mide con la pantalla encendida y sin bloqueo, y `FrameworkConcept.Tools tpr` calcula el TPR por proceso y explica una corrida sin marca. El cascarón deja escrito a los 10 s si el WebView no pidió el contenido. `Gate.Tests/Medicion/` lo cubre (4 pruebas).
- **Guía**: `99-Anexos.md` §6, `Developer-Guide.md` §8, `01-Actualizacion-Sin-Tienda.md` §8 y el README de la demo con la causa, la medición del emulador (9 641–12 451 ms, no juzga el umbral) y el TPR del moto g42, que sigue sin medir.

## 2026-10-27 (saneamiento de la guía del sistema documentador)

### Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion — expediente 0002 de DocAsCode, unidad E1-U1

#### Corregido

- **Enlaces**: los ocho enlaces de la guía que apuntaban a `../Documentacion-Tecnica/` (ruta duplicada desde la reorganización por categoría) ahora apuntan a `../Creacion-Guias-Tecnicas/`. La compuerta ya no los excluye.
- **Frontmatter**: `status: borrador`, `origin: ia-generated`, `confidence: alta` y `audience: [humano, agente]`, según las convenciones del corpus; `owner: fernandofilipuzzi` como en las otras guías; `DOC-ADR` ya no se repite en `traces`.
- **Citas**: la de `ACT-10` pasa a ser literal; la de Docusaurus cita la fuente fijada por commit y no la rama `main`; la enumeración de atributos DITA se ajusta a la página citada (`@props` en lugar de `@product`).
- **`llms.txt`**: deja de llamarse «adoptada»; es una propuesta publicada en `llmstxt.org`.
- **Prueba de concepto**: recuento de páginas (7 fuentes + 2 traducciones; el log cuenta pares), nombres que difieren del árbol propuesto, `302` observado frente al `401` previsto, y los dos mecanismos de `translation_of`.
- **Contrato del frontmatter de la ayuda**: una sola tabla (7.3), con `owner` y `last_review`.
- **`Examples/Lanzar-Example.md`**: rutas del ejemplo (`$P` y la invocación) con la carpeta actual.

#### Añadido

- **`README.md`** y **`Guides/README.md`**, que estaban vacíos: ruta de lectura mínima hasta la guía.

### scripts/

- **`docs-guard.lychee.toml`**: quedan sólo las exclusiones permanentes y con motivo: el defecto plantado de la consola y los dos enlaces de respaldo de cultura de `Web/Ayuda/en/`.

## 2026-10-27 (compuerta de documentación y delegación del merge)

### .github/ y scripts/ — `docs-guard` (expediente 0002 de DocAsCode, unidad E1-U0a)

#### Añadido

- **`.github/workflows/docs-guard.yml`**: compuerta mínima de documentación. Corre en `pull_request` y como workflow reutilizable (`workflow_call`) en el runner propio `[self-hosted, i7infra-dev]`, con `timeout-minutes: 10`. Verifica la guía del sistema documentador (`Guides/Documentacion-Tecnica/Sistema-De-Documentacion-E-Integracion/`) con `lychee --offline --include-fragments` (enlaces internos y anclas) y `markdownlint-cli2` con reglas sólo estructurales. Seguridad: `permissions: contents: read`, sin `pull_request_target`, actions fijadas por SHA y un PR desde un fork no corre en el runner propio.
- **`scripts/ci-local.sh`**: los mismos tres pasos, en el mismo orden, para correr la compuerta antes de empujar.
- **`scripts/docs-guard.lychee.toml`** y **`scripts/docs-guard.markdownlint-cli2.jsonc`**: la configuración de ambos. La de lychee excluye, con nombre, los ocho enlaces rotos de la línea de base: siete los sanea la unidad siguiente (E1-U1) y uno es el defecto plantado de la prueba de concepto, que se conserva.

#### Delegación del merge (Conformacion-Pull-Request-Automatico 2.0)

- **Fecha:** 2026-10-27. **Decidió:** el responsable del producto (PO). **Asiento:** folio 130 §2 del expediente 0002 de DocAsCode (`130-respuestas-del-po-y-asiento.md`, en `hdcm-dev/Lab-Documentos.Documentacion`).
- **Testimonio del PO (fila L2, literal):** «esto surgió porque despues de un avance surgia preguntas y confirmaciones tontas … Mi objetivo es que se enriquesca de tal forma el conocimiento analitico que los subagentes con este conocimiento puedan orquestar de forma automatica todo el proceso de desarrollo».
- **Alcance:** los PR del expediente 0002 en este repositorio, incluido el primer merge (esta unidad), por pedido explícito del PO (folio 130 §3).
- **Guarda:** techo de espera 120 minutos; intervalo de relectura 30 segundos; estado `clean` y al menos un control de la cabeza, todos en éxito. El techo duplica el de la delegación anterior porque el runner `i7infra-dev` es único y lo comparten otros proyectos.
- **Método:** commit de merge atado al SHA de la cabeza, nunca `squash`; la rama remota se borra tras leer que el merge dio.
- **Excepción:** un cuestionamiento particular del PO sobre la unidad la vuelve manual; también lo irreversible con consecuencia externa (configuración de la organización, publicación de paquetes ajenos al caso, compras), que sale al PO con sugerencias y valor por defecto.

## 2026-10-26 (guía nueva)

### Concepts/Framework-Concept/ — v1.0.0 (nueva, borrador)

#### Añadido

- **`Developer-Guide.md`** (`GUIA-INDICE`, único índice): guía de estudio para apps .NET MAUI con pantallas Blazor WebAssembly que se actualizan desde un servidor propio sin pasar por las tiendas, con el marco (`00-Marco.md`), siete temas (`01` a `07`), la cohesión (`08-Cohesion-Global.md`) y los anexos (`99-Anexos.md`). Sale del expediente 0001 de Framework Concept (tramo A4 del folio 037).
- **`Demo/`**: la demo compilable que ilustra la guía (cascarón MAUI, contenido Blazor WASM, herramientas de firma, receptor de prueba y tres suites), con su compuerta en `.github/workflows/framework-concept-demo.yml` (check `fw-ok`, runner propio).
- `.github/pull_request_template.md`: plantilla de PR.
- `README.md` como portada; `Framework-APP.md` es el borrador del PO y queda fuera del conjunto.

#### Delegación del merge (Conformacion-Pull-Request-Automatico 2.0)

- **Fecha:** 2026-10-25. **Decidió:** el responsable del producto (PO). **Asiento:** folio 007 del expediente 0001 de Framework Concept (`PROMPTs/Concepts/01-Framework-Concept/OUTPUTs/Expedientes/0001-Framework-Concept-Maui-Autoactualizable/actuaciones/007-providencia-delegacion-del-merge.md`, en `hdcm-dev/Lab-Documentos.Documentacion`).
- **Alcance:** los PR del expediente 0001 en `hdcm-dev/Lab-Documentos` y `hdcm-dev/Lab-Documentos.Documentacion`.
- **Guarda:** techo de espera 60 minutos; intervalo de relectura 30 segundos; estado `clean` y controles de la cabeza en éxito (al menos uno).
- **Método:** commit de merge atado al SHA de la cabeza, nunca `squash`; la rama remota se borra tras leer que el merge dio.
- **Excepción:** un cuestionamiento particular del PO sobre la unidad la vuelve manual.

## 2026-09-21 (reorganización y guía nueva)

### Guides/Git-Flow-Guides/ — reubicación

#### Cambiado

- `Guides/GitFlow-Practice-Guide/` y `Guides/GitHubFlow-Practice-Guide/` pasan a `Guides/Git-Flow-Guides/`, que las agrupa como las dos variantes del mismo tema. Es una mudanza pura: los cuatro archivos no cambian un carácter y ningún documento del repositorio citaba las rutas viejas.

### Guides/Sistema-Documentador-Tecnico-Guide/ — v1.0.0 (nueva)

#### Añadido

- **`Sistema-Documentador-Tecnico-Guide.md`** (759 líneas, `DOC-SDT-0001`, estado `draft`): apunte que responde si un portal de documentación como el de OneSignal nace de Markdown renderizado, y qué hace falta para montar uno propio. Verificado contra el sitio: lo genera Mintlify desde MDX más OpenAPI, y el fuente de cada página se obtiene agregando `.md` a la URL (§1). Sobre esa evidencia desarrolla el modelo *docs as code* (§2), las piezas de un sistema documentador (§3), audiencias y privilegios con la distinción entre papel de usuario y confidencialidad (§5), el caso de un panel .NET con Lab-Geometria —mismo repositorio o auxiliar, coherencia de ramas, quién renderiza— (§6), la documentación como fase del ciclo de desarrollo con sus estados, sus tres formas de envejecer y el deprecado (§8), preguntas guía para diseñar uno propio (§9), glosario (§11) y registro de evidencias (§12).
- **§7 «Ayuda por rol en Lab-Geometria»**: propuesta completa —arquitectura, contrato del frontmatter, derivación desde la especificación, dónde se decide el papel, idiomas y validaciones en CI—, con prueba de concepto ejecutada.
- **`Examples/Prueba-Ayuda-Por-Rol/`**: la prueba de concepto, en devcontainer reproducible (`dotnet:1-10.0`, puerto 5190). `Consola/` es el motor aislado —catálogo, filtro por papel, `llms.txt` y validador— y termina con `Validador: 3 fallas` y `[exit=1]` **a propósito**: dos defectos plantados y un tercero que encontró el validador por su cuenta. `Web/` es un panel Blazor .NET 10 con ingreso simulado por papel, ayuda contextual, `404` por rol que no filtra el título en el HTML, fuente `.md` servido, `llms.txt` recortado a la sesión y dos idiomas. El contenido deriva de los casos de uso CU-00022, CU-00023, CU-00024, CU-00026, CU-00028 y CU-00029 (v1.1) de Lab-Geometria, declarados en `traces`. Siete capturas del 2026-09-19.
- **`Examples/Lanzar-Example.md`**: tool-prompt para levantar la prueba, exponerla por un túnel de Cloudflare y entregar la URL, con las variantes `--bajar` y `--estado`.

## 2026-09-21 (mesa de editores, ciclo 4)

### Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md — v2.4.0

Ciclo abierto por una instrucción del Product Owner: «no dudes en incluir citas … y si podés explicaciones de esas citas, con algún snippet de código que materialice esa idea». Norma que queda escrita en §0.5: **toda cita de un mecanismo viene con el texto de la fuente, qué implica para quien diseña y el código que lo materializa**; una cita va sola sólo cuando habla del método de la guía. Ninguna fuente se recorta por presupuesto: lo que cede es prosa propia.

#### Corregido

- **Atribuciones.** *Service Layer* es de **Randy Stafford** (PoEAA **p. 133**, no 135) y así se lo nombra en §4.1, §4.3 y §9.6; *Repository*, de **Edward Hieatt y Rob Mee** (§4.1 lo dice ahora, como ya hacía §4.4); el **decorador** de §4.9 remite a Gamma et al.; «composition root» se atribuye a **Mark Seemann**, con su definición textual y entrada nueva en el Anexo E.
- **§9.6 paso 7**: se reemplaza la cita de Evans 2003 de segunda mano por el «Therefore» de *Layered Architecture* (Evans, 2015, p. 10), que además manda lo que la sección necesita —aislar el dominio de la lógica de aplicación—, y se materializa con el `Handle` compilado de §4.10. La entrada `ref-evans-2003` se retira: ya no se cita nada de ese libro.
- **§3.8 y Anexo A**: V06 regenerada. Los cinco números pasan a 4.559,2 ms / 296,0 B contra 30,8 ms / 0,0 B, factor **148x**, y 764,2 contra 848,1 ms en las altas válidas; la derivada «4,3 µs» pasa a 4,6 µs. La deuda declarada de V06 queda saldada: las seis capturas se regeneran juntas.
- **§0.3** deja de prometer una plantilla que tres secciones no cumplen; **§7.2 e** nombra al *Presentation Model* como padre del ViewModel.

#### Cambiado

- **Una sola clasificación de reglas.** La misma regla se clasificaba en cuatro tablas (§3.2, §3.6 y dos en §3.7). Ahora §3.7 es la única que clasifica reglas una por una —con las tres del producto sin precio absorbidas—, §3.2 decide si la regla es del negocio y §3.6 de qué tipo es; las dos remiten.
- **§4.10 se parte en dos**: el *Use Case* con dos *Entities* queda en §4.10 y «¿Cuándo `SaveChanges` sube del Repository al Handler?» pasa a ser §4.11, con la pregunta de cierre en §4.12. Trece remisiones actualizadas.

#### Añadido

- **Citas que faltaban, con su explicación y su código**: Evans, *Layered Architecture* p. 10, para la regla de dependencia (§2.1, materializada con `IProductoRepository`/`ProductoRepository` y el CS0234 de L07); Evans, *Repositories* p. 17 («the entities and value objects become mere data containers») para el *Anemic Domain Model* (§3.5); la definición de *Service Layer* de Stafford (§4.1); *Data Mapper* como el nombre de lo que §5.4 describe; la *trust region* de Wirfs-Brock como respaldo conceptual del criterio de no validar al materializar (§5.4); *Document Message* de Hohpe y Woolf para el Response DTO (§7.2 d); *Presentation Model* (§7.2 e); y los niveles de objetivo de Cockburn en el enunciado de §4.10.
- **Dos secciones que citaban sin mostrar código**: §3.6, con las nueve citas del Business Rules Group, recibe un bloque compilado donde se ven los tres tipos —*Structural Assertion*, *Action Assertion*, *Derivation*— con su tabla «línea → concepto»; §4.7, con las cinco de CQS, contrasta `PrecioDeVenta()` contra `Desactivar()` y nombra la excepción consciente de `Handle`.
- **§5.6**: el cuerpo `ProblemDetails` **medido** de L21, y el contraste en una línea con el `code` que agregaría un dominio por resultado. **§3.8**: la señal de cuándo el predicado se saca del objeto (el criterio que tiene que valer en memoria y en la base). **§6.3**: se nombra al serializador materializando el `record` posicional por su constructor, que §5.4 afirma y L22 ya tenía capturado.
- **§9.6**: los pasos 5 y 6 de la escalera pasan a tener código (interfaz, doble de prueba y el `PostAsJsonAsync` del cliente), cerrando PE-08 del ciclo 1. **Anexo B**: tres preguntas nuevas (cómo rechaza el dominio, el catálogo de códigos como contrato, el camino de materialización y los conjuntos cerrados). **§9.3**: la señal «el borde necesita distinguir motivos de rechazo».
- **Anexo A**: queda dicho qué **no** construye el laboratorio por decisión —un segundo ABM compilado y una variante de cliente—, con su fundamento.
- Anexo E: Seemann (2011) y Fowler (2004c) *Presentation Model*.

## 2026-09-21 (mesa de editores, ciclo 3)

### Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md — v2.3.0

Tercer ciclo de la mesa, sobre las tres preguntas del Product Owner: quién propuso la refactorización del constructor a la fábrica, qué es exactamente una *Invariant* y qué son Dapper y el serializador. La guía crece 60 líneas y 2 bloques, el tope del ciclo; el volumen vuelve a ir al laboratorio.

#### Añadido

- **§3.6**: tabla de las tres piezas que sostienen una *Invariant* —el *Factory Method* la establece, el setter privado impide romperla, cada método que toca el dato la reverifica— con la columna «si falta», y el cierre como **Criterio de esta guía**.
- **§3.7**: tabla de siete ejemplos y contraejemplos clasificados con las tres condiciones del §3.1 como filtro, incluidos los cuatro que **no** son *Invariant* (DNI repetido, precio de venta, longitud de columna, autorización) y por qué fallan.
- **§4.10**: para qué sirve el borde del *Aggregate*, con «Define properties and invariants for the aggregate as a whole…» (Evans, 2015); «un pedido confirmado tiene al menos un ítem» como *Invariant* del agregado, que `ItemPedido` no puede verificar y `Pedido` sí.
- **§5.4**: tabla de los cuatro materializadores —`Create`, EF Core, Dapper, `System.Text.Json`— con para qué sirve cada uno, cómo construye y qué pasa sin constructor sin parámetros, y un bloque compilado que reúne las cuatro líneas del mismo programa. Dapper aporta lo que EF Core no muestra: falla en silencio si la columna no se llama como la propiedad, y sin seguimiento de cambios el *Repository* y la *Unit of Work* vuelven a ser código propio (V09).
- **§5.1, §6.1 y Anexo D**: definiciones de **ORM**, **micro-ORM** y **serializador** —la guía usaba «ORM» ocho veces sin expandirlo— con sus tres filas de glosario.
- **§7.4**: bloque compilado del modo de falla medido del serializador y la medición que lo acompaña (`NO lanzó: el objeto llegó entero en su valor por omisión`); con el constructor privado lanza `NotSupportedException` y con `[JsonConstructor]` materializa sin tocar un setter.
- **`Variantes/FabricaYResultado/`**: escenario `materializadores` (`ProductosDeMaterializacion.cs` y dos escenarios nuevos del Demo), con **Dapper 2.1.86** como primer y único paquete de terceros del laboratorio; captura **V09**, listada en el Anexo A, y fila nueva en el Anexo C con su licencia verificada en el `.nuspec`. Compila en Debug y Release con 0 advertencias. V09 cubre los dos lados del serializador: los tres modos de falla y el caso positivo —un `record` posicional y una clase de sólo lectura materializadas por constructor, con cero setters públicos disponibles—.
- Anexo E: Bloch (2018) *Effective Java*, 3.ª ed., Dapper (2026) y Microsoft (2026o) *Use immutable types and properties*, que es la fuente de la regla del constructor que usa `System.Text.Json` (uno solo con parámetros, o `[JsonConstructor]` cuando hay varios).

#### Cambiado

- **§3.1**, viñeta *Factory Method*: la procedencia queda completa —catalogada desde la 1.ª edición de *Refactoring* (1999) como *Replace Constructor with Factory Method* y renombrada en la 2.ª (2018)— y Bloch entra como convergencia posterior e independiente («Consider static factory methods instead of constructors»), no como origen.
- **§3.1**, viñeta *Invariant*, y su fila del glosario: la definición suma la tercera condición que la vuelve criterio de diseño —si se rompe, el objeto no debería existir, y por eso se verifica al construirlo—, junto con las dos que ya tenía.
- **§7.4**: «setters públicos y constructor sin parámetros» era una condición suficiente presentada como necesaria; ahora dice «un camino de escritura que el serializador alcance: setters públicos, **o** un constructor cuyos parámetros se llamen como las propiedades».
- No se cita la frase de Evans de 2003 sobre las invariantes «whenever data changes»: no está en la *DDD Reference* 2015, que es la fuente que la guía declara para Evans.

#### Deuda declarada

- **V06 no se regeneró** al ampliar la variante y §3.8 quedó citando cinco números de una corrida anterior. *(Saldada en v2.4.0: V06 se regeneró y los números se actualizaron.)*

## 2026-09-20 (mesa de editores, ciclo 2)

### Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md — v2.2.0

Segundo ciclo de la mesa (comisiones Fowler y .NET), sobre el pedido del Product Owner acerca de las fábricas estáticas con constructor privado, las guardas, los resultados tipados y los valores. `MyProject/`, `lab.sh`, `Conceptos-DI/` y las capturas L00–L24 siguen sin cambios; la guía crece 147 líneas y 3 bloques de código, dentro del tope del ciclo.

#### Añadido

- **§3.8 «¿Se rechaza lanzando o devolviendo?»** (sección nueva; la pregunta de cierre del capítulo pasa a §3.9). Las dos formas de rechazar con criterio para elegir: tabla de cuatro preguntas, las dos citas de Fowler (2014) —lo esperable no va por excepción; el contexto decide—, el bloque compilado de `Create` devolviendo `DomainResult<T>` con `TryGetValue`, y las dos mediciones que forman el par: 4.357,0 ms/296,0 B contra 30,6 ms/0,0 B por millón de rechazos (**142x**, V06) y el resultado que se puede descartar sin un solo aviso ni siquiera con `AnalysisMode=All` (V04), frente a la excepción que corta el proceso. Se nombra la *Notification* de Fowler (2004b) como la forma que junta **todos** los motivos —un resultado de un solo código no lo es— y la consulta sin efecto que informa el motivo sin mutar (CQS y *Side-Effect-Free Function*), que es el `isValidForCheckIn` que §3.7 ya predica; no es *Specification* ni *Guard Clause*.
- **§3.9**: el conjunto cerrado que de verdad está cerrado. El `enum` admite `(EstadoProducto)99`, se relee desde una columna entera con `IsDefined = False` sin lanzar y viaja como número en JSON; el *Value Object* con constructor privado y fábrica no admite un valor imposible (bloque compilado de `Moneda`, con `MonedaSinRegla` como contraste y V08 como medición). El umbral «un valor se vuelve tipo cuando tiene una regla» se declara más estricto que Evans y que PoEAA, y §7.1 remite a esa marca en lugar de afirmarlo en seco.
- **§4.2**: la línea de pensamiento entidad → intención → hecho consumado, con el **Diagrama 4.1** (mermaid) y las dos preguntas que la cruzan sin ser etapas: «¿qué valores admite?» y «¿qué contesta cuando no puede?». El hecho consumado se distingue en sus dos formas: *Domain Event* si lo que importa es que ocurrió, *Entity* si tiene identidad y ciclo de vida (Evans, 2015). §7.1 remite al diagrama.
- **§5.4**: el constructor privado como **costura del ORM**, con la documentación de EF Core que lo respalda (Microsoft, 2026m) y la medición del ciclo completo (constructor elegido, dos ejecuciones —alta y relectura—, setters privados escritos igual; V05); qué pasa sin ese constructor (`InvalidOperationException: No suitable constructor was found`, V07); por qué el camino de materialización **no valida**, con sus tres razones y el desvío para el escenario E-C, declarado **Criterio de esta guía**; y cómo se persiste un `enum` (texto, o entero con valores explícitos; nunca entero sin ellos).
- **§7.2 d**: el contrato de error también es contrato —códigos del vocabulario del dominio, sin texto de presentación (Fowler, 2004b)—, con dos filas nuevas del cuadro ✅/❌ y la advertencia sobre el `const string`: los compiladores propagan las constantes, así que cambiar un código obliga a recompilar a quien lo consume (Microsoft, 2026n).
- **`Examples/Dot-NET-Arquitectura-Lab/Variantes/FabricaYResultado/`** (especialista .NET): el mismo `Producto` con las dos estrategias de rechazo, `DomainResult`/`DomainResult<T>` como `readonly record struct` con `TryGetValue`, el `enum` con valores explícitos, `Moneda` como *Value Object* con fábrica, las dos entidades de contraste que EF Core no puede materializar y el mapeo desde afuera con `HasConversion`. Compila en Debug y Release con 0 advertencias; capturas **V04–V08**, listadas en el Anexo A.
- Anexo E: Fowler (2014) *Replacing Throwing Exceptions with Notification in Validations*, Fowler (2004b) *Notification*, Fowler (s. f.) catálogo de *Refactoring* (*Replace Constructor with Factory Function* y *Replace Nested Conditional with Guard Clauses*), Evans y Fowler (1997) *Specifications*, Microsoft (2026m) *Entity types with constructors* y Microsoft (2026n) *The `const` keyword*. Anexo D: cinco entradas nuevas (conjunto cerrado, *Domain Event*, *Factory Function*, *Notification*, resultado de dominio).

#### Cambiado

- **§3.1**: la viñeta *Factory Method* dice «único camino **desde afuera de la clase**» —adentro, `Create` usa el constructor privado por inicializador— y admite más de una fábrica, una por acto de constitución; se cita el «Therefore» completo de Evans, con la frontera a partir de la cual la fábrica se muda a un objeto aparte (colaboradores, varias piezas, clases concretas), y se nombra la refactorización catalogada. La viñeta *Value Object* enumera lo que **no** es el patrón aunque viva en una carpeta `Values/`: el `enum`, el catálogo de constantes y la función de normalización.
- **§3.3**: una nota aclara que el constructor privado sin parámetros no es un segundo camino de alta —no sabe recibir datos—, sino el camino del motor de datos; si alguna vez hay dos constructores, el de parámetros es el de constitución.
- **§3.4**: fila nueva ❌ con la segunda razón del setter privado, medida: una propiedad `{ get; }` compila, no avisa y **desaparece del modelo**, así que el dato no se guarda ni se relee (V07).
- **§5.6**: qué cambia en el borde cuando el dominio devuelve resultado en lugar de lanzar —no hay excepción que traducir, el código de condición se mapea a estado y viaja en un miembro de extensión del `ProblemDetails`, y el texto de presentación deja de vivir en `Domain`—.

## 2026-09-19 (mesa de editores)

### Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md — v2.1.0

Reedición por la mesa de editores (editor, pedagogía, Fowler, .NET, refutador; bitácora de la mesa en el repositorio de documentación del laboratorio). Objetivo: que cada concepto tenga definición, explicación, código y la asociación explícita entre los dos. `MyProject/`, `lab.sh`, `Conceptos-DI/` y las capturas L00–L24 no cambian.

#### Cambiado

- **Fuentes y precisión técnica (R-4).** Las citas de PoEAA llevan capítulo y página cuando son del libro (§3.5, §4.1, §9.6: pp. 111, 119, 133, 135, 137; *Service Layer* atribuido a Randy Stafford) y el Anexo E distingue el libro del catálogo en línea. *Use Case*/Handler tienen fuente: *Service Layer* en su forma *operation script* y «cada Transaction Script en su propia clase con el patrón Command»; §9.6 paso 7 ya no dice que el *Service Layer* «desaparece» sino que cambia de forma. «DTO (Fowler, 2002)» queda reservado para `Contracts` (§5.1, Anexo D vuelven a «entre procesos»); §4.3 y §7.4 dejan de llamar DTO al Command y a la Query; el modelo de lectura se declara **Criterio de esta guía** con *LocalDTO* (Fowler, 2004) citado. *Factory Method* se apoya en la *Factory* de Evans (2015), con GoF como salvedad; *Entity* y *Value Object* citan a Evans y al catálogo; `Dinero` se identifica como el núcleo del patrón *Money*. §3.5 cita la duplicación entre transacciones y la advertencia de que el corte no se cuantifica (p. 111). Correcciones de .NET: la igualdad de un `record` es miembro a miembro y una colección se compara por referencia (§4.3, §3.1); `[Index]` vive en `Microsoft.EntityFrameworkCore.Abstractions` (§5.4, §7.4); `[FromServices]` se infiere desde ASP.NET Core 7 y la guía lo escribe por legibilidad (§5.2, Microsoft 2026l); `decimal?` produce una columna opcional por convención (§3.7); `IProductoRepository.AddAsync` usa `DbSet.Add` adentro (§5.5). §2.2 pasa a la marca nueva **[Compilado sin captura]** de §0.5.
- **`AsNoTracking` y *Unit of Work* (R-3).** §5.5 muestra `ProductoRepository` y declara `AsNoTracking()` como apartamiento: correcto para las Queries, no para un Command que obtiene, cambia y confirma; la variante lo mide (V03: 1 fila con seguimiento, 0 sin). §4.10 agrega la pregunta «¿Cuándo `SaveChanges` sube del Repository al Handler?» con las dos firmas compiladas lado a lado, distingue «confirmaciones» (llamadas a `SaveChangesAsync`) de `Pedido.Confirmar()`, nombra *Success/Minimal Guarantee* como las *Postconditions* del *Use Case* y explicita por qué 2a es `DomainException`. §9.6: `Id` vale `0` hasta `SaveChanges` y el índice único llega como `DbUpdateException` → 500.
- **Definiciones con remisión (R-1).** §1.1, §3.1, §4.1, §5.1 y §6.1 abren diciendo en qué bloque se ven todos sus términos y cada viñeta termina con «→ §n.m, `Archivo.cs`» y la firma cuando cabe. Debajo de los bloques **[Compilado]** de §2.5, §3.3, §4.5, §4.6, §4.10, §5.2, §5.4, §5.5, §5.6 y §6.3 hay una tabla «línea o miembro → concepto → dónde se define» (≤ 4 filas). §0.3 describe la plantilla por parte y la Parte III como tabla → preguntas → criterio y ejercicios.
- **Forma (R-6).** Marcas **[Compilado]** con archivo y rango de líneas; los dos bloques que citaban una carpeta separan sus archivos con `// — Archivo.cs —`; §2.2 restituye el comentario original `// composition root: …`; diagramas numerados por capítulo (2.1, 2.2, 2.3, 5.1, 6.1, 7.1, 9.1); §7.1 pasa a llamarse «Los siete objetos y su pregunta»; §9.6 pierde la marca «Respuesta» (su título no es pregunta); Booch queda como paréntesis; §3.7 nombra la *validación contextual*; §7.2 e corrige «el único objeto» por «uno de los tres». Glosario: columna «Código» (sección y archivo del bloque), los cuatro «2.» pasan a «2.1», once entradas nuevas (ciclo de vida, DAO, `DelegatingHandler`, `HttpClient`, `internal`, *Main Success Scenario*, *Minimal Guarantee*, *Money*, *primary constructor*, prueba del papel, *Table Data Gateway*) y *Service Layer* → 4.1, 4.3, 9.6. §9.7 «El criterio, en una línea» pasa a §9.8.

#### Añadido

- **Archivos del laboratorio que nunca se mostraban (R-2).** Bloques **[Compilado]** de `Program.cs` (composition root, §2.5, con la tabla de ciclos de vida Singleton/Scoped/Transient y el dato de que `InMemoryProductoRepository` es Singleton en L13), `ProductoTests.cs` (la *Postcondition* de `Create` y la igualdad de `Dinero`, §3.3), `ObtenerProductosQuery` + `Handle` + `ProductoDto.From` (§4.5), `FakeProductoRepository` (§4.6), `GetById` con `ActionResult<T>` y el `404` (§5.2), `ProductoRepository` (§5.5), `IUnitOfWork` como la interfaz de servicio técnico del laboratorio e `ItemPedido` con su constructor `internal` (§4.10); `IEmailService`/`ICurrentUserService` quedan como ejemplos hipotéticos. §0.5 reescribe la fila «Tipo nombrado sin código»: los tipos que dan nombre a una definición se muestran; los intermedios viven en `lab.sh` con su línea.
- **Variante `Variantes/Pedidos/` extendida a `Infrastructure`** (EF Core sobre SQLite en memoria; especialista .NET): `AppDbContext : DbContext, IUnitOfWork` sin adaptador, `ProductoRepository` con seguimiento y `PedidoRepository` que solo marcan, `PedidoConfiguration` con `OwnsMany`, el *Use Case* de modificación `CambiarPrecioProducto` y `ProductoRepositorySinSeguimiento` como contraste; el Demo registra los handlers con lifetimes reales y abre un scope por *Use Case*. V01/V02 regeneradas (salida de §4.10 idéntica) y **V03** nueva (`cambiar-precio`), citadas en §4.10 y §5.5; Anexo A las lista.
- **El lector ejercita (R-5).** §9.7 «Ejercicios de transferencia»: los turnos de un consultorio y la regla del stock que §4.10 dejaba abierta, con respuesta diferida al final. Dos contraejemplos en código **[Fragmento ilustrativo: contraejemplo]**: el Handler con el `if` del precio (§4.3) y el handler que solo reenvía (§4.11). §4.2 reescribe el «antes» con el cuerpo real de `RegistrarPedido` y remite al Handler compilado de §4.10 y al paso 7 de §9.6. En las siete secciones con paso ⚠ (§1.4, §1.5, §2.3, §3.4, §4.6, §5.5, §5.6) una línea «Antes de ejecutar, anotá» precede a la respuesta. §2.2 avisa que el `if` del precio está en `AltaDeProducto` a propósito; §6.3 explica por qué el cliente de consola está exento del ❌ de §6.2; la fila «varias instancias» de §3.6 remite a §9.6.
- **Objetos de cliente (R-7).** Fragmentos ilustrativos de `IProductoApiService` (§6.2), `ProductoListItemViewModel` (§7.2 e) y `ProductoFormModel` con `ToRequest()` (§7.2 f).
- Anexo E: Fowler (2004) *LocalDTO*; Microsoft (2026l) *What's new in ASP.NET Core in .NET 7*.

## 2026-09-19

### Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md — v2.0.2

#### Cambiado

- El laboratorio pasa de `Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Lab/` a `Guides/Dot-NET-Arquitectura-Guide/Examples/Dot-NET-Arquitectura-Lab/`; la guía actualiza sus cinco referencias (convenciones de §0, L00 y anexo A). `lab.sh` no depende de su ubicación y las capturas quedan como se registraron.
- Terminología: los términos de la literatura se escriben en inglés y sin traducir, en cursiva la primera vez (*Entity*, *Value Object*, *Business Rule*, *Invariant*, *Precondition*, *Factory Method*, *Use Case*, *Repository*, *Unit of Work*, *Aggregate*…). Se aplica en §3.1 y §4.1 (definiciones), en los títulos de §3.2, §3.5, §4.9, §5.2, §5.4 y §5.5 (con sus anclas y remisiones), en el resto de la prosa técnica, en el diagrama 6 y en el glosario, cuyas entradas pasan a ser el término en inglés con su equivalente en castellano.
- §0.4: «el precio no puede ser cero ni negativo» se presenta como decisión de esta tienda para el laboratorio («un producto se da de alta con su precio»), y anuncia la variante de §3.7. El laboratorio no se regenera: L09–L21 siguen valiendo.
- §3.2: la prueba del papel se corrige (el cuaderno del depósito sí anota mercadería sin precio; lo que el mostrador no hace es venderla); se agrega la pregunta «¿vale siempre o solo antes de una acción?» (ahora son cuatro) y la fila «no se vende un producto sin precio». §3.1 separa los ejemplos de *Business Rule* e *Invariant* y define *Precondition* y *Postcondition*.
- §4: se renumera por las secciones nuevas (§4.2→§4.5, §4.3→§4.6, §4.4→§4.8, §4.5→§4.9, §4.6→§4.11), y §3.6→§3.8 y §9.6→§9.7, con sus remisiones en el texto, §7.1, anexo A y glosario.
- §0.5: las marcas *Salida registrada* y **[Compilado]** admiten las variantes (`Variantes/…`, `Variantes/capturas/Vnn-….txt`). Índice, §9.5 y anexo A remiten a lo nuevo.

#### Añadido

- §3.2 «¿Cómo se reconoce una regla de negocio?»: definición (Martin, 2017, cap. 20), tres preguntas en orden (¿vale en papel?, ¿vale llegue por donde llegue el dato?, ¿el error se entiende en términos del negocio?), tabla de condiciones que lo son y que no lo son con su lugar, y la distinción con las reglas específicas de la aplicación de Martin (2012), que la guía trata como pasos del caso de uso. Se suma la definición a §3.1, la entrada al glosario y la referencia Martin (2017); la invariante pasa a definirse como una regla de negocio sobre un solo objeto. §3.2–§3.5 se renumeran a §3.3–§3.6 (con sus remisiones en §7.1, anexo A y glosario).
- §2.2 «Los tres conceptos en un programa de consola»: regla de dependencia, inversión e inyección en tres pasos (la regla nombra el detalle → la regla declara la interfaz → el contenedor hace los `new`), con un diagrama que separa la dependencia de código de la llamada en ejecución y una tabla de qué responde cada concepto y cómo puede darse sin los otros. El código está en `Examples/Dot-NET-Arquitectura-Lab/Conceptos-DI/` (Paso0–Paso2; compilados con SDK 10.0.400, cero advertencias). §2.2–§2.7 se renumeran a §2.3–§2.8 con sus remisiones; la marca «Fragmento ilustrativo» de §0.5 admite programas aparte.
- §3.6 «¿Dónde vive una *Business Rule* en un programa orientado a objetos?»: los tres tipos del Business Rules Group (2000) —*Structural Assertion*, *Action Assertion*, *Derivation*—, el objeto y el *Message* (Booch, 1994; Kay, 1998), *Design by Contract* (Meyer, 1988; Eiffel Software, s. f.; Evans, 2015), *Responsibility* (Wirfs-Brock, 2006) y una tabla de síntesis tipo de regla → dónde vive, con ejemplos de la tienda.
- §3.7 «¿Vale siempre o solo antes de una acción? El producto sin precio»: la objeción del producto que llega sin manifiesto de precios; *Invariant* frente a *Precondition* y la validación contextual (Fowler, 2005b); variante compilada `Precio decimal?` + `AsignarPrecio` + `PrecioDeVenta` en `Examples/Dot-NET-Arquitectura-Lab/Variantes/ProductoSinPrecio/`, con su salida V01.
- §4.2 «¿Cómo se pasa de un *Use Case* a un Command y un Handler?»: el método de servicio `VentasService.Vender` partido en firma (Command) y cuerpo (Handler), la frase «tengo un producto y quiero venderlo» descompuesta en piezas y el orden en que conviene pensarlas.
- §4.3 «¿Cómo se reconoce un Command y un Handler?»: señales de cada uno, prueba rápida, con qué se confunden (Request DTO, servicio, método de la *Entity*) y la analogía de la orden de trabajo.
- §4.4 «¿Cómo se reconoce un *Repository*?»: definición (Fowler, 2002; Evans, 2015), cinco preguntas de criterio con su respuesta (objetos y no tablas, uno por *Aggregate Root*, dónde vive, sin reglas, cuándo hace falta con EF Core) y con qué se confunde.
- §4.7 «¿De dónde vienen los nombres Command y Query?»: *Command-Query Separation* (Meyer, 1988, vía Fowler, 2005a), el patrón Command (Gamma et al., 1994) y *Command Message* (Hohpe y Woolf, 2003); remite a §4.8 (CQRS) sin repetirlo.
- §4.10 «Un *Use Case* con dos *Entities*: RegistrarPedido»: `Producto` y `Pedido` como *Entities*, `Pedido` como *Aggregate Root* con `ItemPedido` (que copia el precio del día), «vender» como *Use Case* (Martin, 2012), enunciado completo al estilo de Cockburn (2001) con extensiones enlazadas al código, y `RegistrarPedidoHandler` con `IUnitOfWork` y una sola confirmación. Código compilado en `Examples/Dot-NET-Arquitectura-Lab/Variantes/Pedidos/` sobre el `Producto` de la variante sin precio, con su salida V02.
- §7.4 «¿Cómo se traduce el vocabulario de siempre al de esta guía?»: tabla de equivalencias (DTO → Command/Query y Request/Response, método del servicio → Handler, clase plana de `Models/` → *Entity*, *Value Object* sin cambios, `DbContext` → *Repository*), los tres papeles que cumple una clase plana mientras no hay reglas y por qué chocan cuando aparecen, y la distinción entre las dos particiones: la del transporte, que sí crea una clase nueva, y la de la persistencia, que se resuelve sacando las anotaciones del ORM a la Fluent API (§5.4) y solo crea un *Persistence Model* con esquema ajeno (§7.2 g). §7.4 y §7.5 se renumeran a §7.5 y §7.6, con sus remisiones en §7.3, §9.1 y el anexo B; §4.3 y §5.4 remiten a la sección nueva.
- §4.10: `IUnitOfWork` declara `Task<int> SaveChangesAsync(CancellationToken ct = default)`, la firma de `DbContext.SaveChangesAsync`, de modo que `AppDbContext` la implementa sin adaptador; la prosa lo dice así. El código de la variante y la captura V02 se regeneraron: la salida es idéntica.
- §9.6 «La escalera en código: del dato a los *Use Cases*»: un ABM de personas en siete pasos, cada uno con su señal, la pieza que agrega, el delta de código (fragmento ilustrativo) y lo que dice Fowler (*Transaction Script*, *Service Layer*, *Anemic Domain Model*, *Repository* y *Unit of Work* dentro del `DbContext`), con remisiones a §3, §4, §5 y §6 en lugar de repetirlos.
- `Examples/Dot-NET-Arquitectura-Lab/Variantes/`: `ProductoSinPrecio/` (Domain + Demo), `Pedidos/` (Domain + Application + Demo), `global.json` (10.0.400, `latestFeature`), `variantes.sh` y `capturas/` V01–V02 (compilados con SDK 10.0.400 en `mcr.microsoft.com/dotnet/sdk:10.0`, cero advertencias). No forman parte de `MyProject/` ni de `lab.sh`; el anexo A las lista aparte.
- Anexo E: Booch (1994), Business Rules Group (2000), Cockburn (2001), Eiffel Software (s. f.), Evans (2003, 2015), Fowler (2003, 2005a, 2005b), Gamma et al. (1994), Hohpe y Woolf (2003), Kay (1998), Meyer (1988) y Wirfs-Brock (2006). Anexo D: 18 entradas nuevas (*Action Assertion*, *Aggregate*, *Aggregate Root*, *Anemic Domain Model*, *Command Message*, *Command-Query Separation*, *Derivation*, *Design by Contract*, *Domain Model*, *Extension*, *Message*, *Precondition*, *Postcondition*, *Primary Actor*, *Responsibility*, *Service Layer*, *Structural Assertion*, validación contextual).

## 2026-09-18 (reedición)

### Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md — v2.0.1

#### Cambiado

- Reedición íntegra como guía de estudio y de consulta de criterios (1425 líneas): §0 cómo usarla, problema conductor y escenarios E-A..E-D; Parte I fundamentos (solución, proyecto, referencia; regla de dependencia); Parte II construcción capa por capa (Domain, Application, Infrastructure y WebAPI, clientes); Parte III criterio (§7 «Cada objeto responde una pregunta» con las siete respuestas explicativas, §8 estructura física y nombres, §9 del problema a la estructura); anexos A–E (hoja de ruta del laboratorio, lista de verificación, versiones y licencias fechadas, glosario, referencias autor-fecha).
- Cada capítulo sigue el patrón definiciones → decisiones como pregunta con respuesta en una línea → práctica → cierre «¿cuándo no?».
- Toda salida mostrada proviene de una ejecución registrada; todo bloque de C# está rotulado [Compilado] o [Fragmento ilustrativo]; las afirmaciones volátiles llevan fuente y fecha de consulta.
- Base tecnológica: .NET 10 LTS, EF Core 10, `.slnx`, plantilla `webapi` sin Swashbuckle. MediatR y AutoMapper salen del ejemplo (handlers propios y mapeo manual) y pasan a §9.4 como caso de evaluación de dependencias (licencia dual desde 13.0/15.0; vulnerabilidad alta en AutoMapper 14.0.0).
- Estructura física `src/Backend`, `src/Clients`, `src/Contracts`; `Contracts` nace con el primer cliente .NET; el controller traduce entre contrato y mensajes (el Command deja de ser el cuerpo HTTP); `IProductoRepository` en Domain; regla de referencias del front condicionada por escenario.
- Correcciones técnicas sobre la versión anterior: `CreatedAtAction` apunta a una acción existente; `Add` + `SaveChangesAsync` en el repositorio; Blazor Hybrid descrito según la documentación (nativo + Web View, sin WebAssembly); configuración de WASM en `wwwroot` rotulada como visible al usuario; sin afirmar que Identity emita JWT.

#### Añadido

- `Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Lab/`: `lab.sh` (25 pasos L00–L24 en `mcr.microsoft.com/dotnet/sdk:10.0`), `capturas/` (salidas literales con encabezado de fecha, imagen, digest y SDK), `aserciones.log` (74 PASS, 1 FAIL provocado a propósito) y `MyProject/` (la solución final que compila con cero advertencias).

## 2026-09-18

### Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md

#### Añadido

- Entrada **Clean Architecture** en el glosario: diagrama de anillos concéntricos, tabla de qué sabe e ignora cada anillo y nota de inversión de dependencias.
- Aclaración de que los anillos expresan **dependencia, no ubicación**: `Domain`, `Application` e `Infrastructure` son proyectos y espacios de nombres hermanos, y la cebolla se ve en sus referencias (`Infrastructure → Application → Domain`).

#### Cambiado

- Los diagramas ASCII (flujo de MediatR, visión general de la solución y otros) pasan a Mermaid.
- La interfaz del repositorio (`IProductoRepository`) vive en `Domain` en toda la guía; antes el glosario y la nota de inversión la ubicaban en `Application`, en contradicción con el árbol del proyecto `Domain`. Las interfaces de servicios técnicos (`IEmailService`, `ICurrentUserService`) quedan en `Application`.
- Convención de nombres: los términos de arquitectura, capas y patrones van en inglés y los conceptos del dominio del problema en español.
  - Anillos: `Entities (Domain)`, `Use Cases (Application)`, `Interface Adapters`, `Frameworks & Drivers`; grupos de la solución: `Presentation`, `Core`.
  - Operaciones estándar del patrón en inglés: repositorio `GetByIdAsync` / `AddAsync` / `RemoveAsync`; controller `GetAll` / `Create`; servicios de API del front `GetAllAsync` / `GetByIdAsync` / `CreateAsync` / `DeleteAsync`; factory `Producto.Create`.
  - Páginas de scaffold: `Details.razor`, `Form.razor`, `Home.razor`.
  - `ProductoListaViewModel` y `ProductoListaItemViewModel` se unifican en `ProductoListItemViewModel`, bajo `ViewModels/`.
  - Los casos de uso siguen en español porque expresan intenciones del usuario: `CrearProductoCommand`, `ObtenerProductosQuery`.

### Guides/references/arquitectura

#### Eliminado

- `guia-arquitectura-.net.md`, `guia-arquitectura-microservicios.md` y `guia-manejo-recursos.md`: eran copias idénticas de `Guides/Dot-NET-Arquitectura-Guide/Dot-NET-Arquitectura-Guide.md`, `Microservicios-Guide.md` y `Manejo-Archivos-Guide.md`.
