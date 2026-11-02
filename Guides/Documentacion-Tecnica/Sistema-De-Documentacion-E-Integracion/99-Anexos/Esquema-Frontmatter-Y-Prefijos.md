---
doc_id: ANX-SDEI-ESQUEMA
doc_type: anexo
title: Anexo — esquema de frontmatter y registro de prefijos
description: Contrato de frontmatter del cuerpo (diez campos más description), trazas, registro cerrado de prefijos y forma del registro de requerimientos.
status: borrador
origin: ia-assisted
confidence: alta
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-02
audience: [humano, agente, desarrollo, analista]
traces: [MARCO-SDEI, MAPA-SDEI, ANX-SDEI-GLOSARIO]
---

# Anexo — esquema de frontmatter y registro de prefijos

Este anexo es el contrato que cumple todo documento del cuerpo y que conviene adoptar en los proyectos que el cuerpo
documenta. Le dice a una persona qué escribir en la cabecera de una página y a un agente cómo leerla sin parsear la
prosa. Lo verifica la compuerta `docs-guard` en el grupo PR-2 (esquema) y en el PR-5 (trazas).

## Índice

1. [Definiciones](#1-definiciones)
2. [Los diez campos más description](#2-los-diez-campos-más-description)
3. [README del cuerpo y páginas de ayuda](#3-readme-del-cuerpo-y-páginas-de-ayuda)
4. [Trazas](#4-trazas)
5. [Registro de prefijos](#5-registro-de-prefijos)
6. [Registro de requerimientos documentales](#6-registro-de-requerimientos-documentales)
7. [Un ejemplo completo y sus errores](#7-un-ejemplo-completo-y-sus-errores)
8. [Dónde vive la fuente de verdad](#8-dónde-vive-la-fuente-de-verdad)

## 1. Definiciones

| Término | Definición |
| --- | --- |
| Frontmatter | Bloque YAML entre dos líneas `---` al principio del archivo. La compuerta entiende un subconjunto: `clave: valor`, `clave: [a, b]` y listas con `- elemento` |
| Vocabulario cerrado | Lista fija de valores admitidos para un campo; un valor fuera de la lista pone la compuerta en rojo |
| Cuerpo nuevo | Carpeta con un `README.md` cuyo frontmatter tiene `corpus_id`. Dentro de ella, todo archivo que no sea un README tiene que tener frontmatter |
| Prefijo | La parte de un identificador antes del primer guion (`TEM` en `TEM-01`). Sale de un registro cerrado |

## 2. Los diez campos más description

| Campo | Obligatorio | Valores | Para qué sirve |
| --- | --- | --- | --- |
| `doc_id` | Sí | Prefijo registrado, guion y sufijo en mayúsculas (`TEM-01`, `ANX-SDEI-CQ`) | Identidad estable: las trazas y las citas entre repositorios apuntan a él, no a la ruta |
| `doc_type` | Sí | `tema`, `marco-de-referencia`, `familia`, `mapa`, `anexo`, `anexo-referencias`, `indice`, `guia-practica`, `guia-de-estudio`, `operativa`, `study-note`, `rrd`, `proceso`, `ficha`, `registro-audiencias` | Decide qué estructura se espera y qué controles se relajan (en `anexo-referencias`, AI-4 sólo avisa) |
| `title` | Sí | Texto, 3 caracteres o más | Título para índices y canales; el H1 visible puede ser más corto |
| `status` | Sí | `borrador`, `vigente`, `obsoleto` | Ciclo de vida; pasar a `vigente` es acto de una persona |
| `origin` | Sí | `human`, `ia-assisted`, `ia-generated` | Quién produjo el texto; gatilla la revisión según origen (`CQ-14`) |
| `confidence` | Si `origin` no es `human` | `alta`, `media`, `baja` | Cuánto confía quien redactó en lo que afirma |
| `owner` | Sí | Texto | Responsable de mantenerlo |
| `last_review` | Sí | Fecha `AAAA-MM-DD` | Frescura: el carril programado avisa pasados 365 días |
| `audience` | Sí | Lista con uno o más de: `humano`, `agente`, `estudiante`, `docente`, `desarrollo`, `qa`, `devops`, `po`, `analista`, `persona-usuaria` | A quién se dirige; ordena las rutas de lectura. Es de **orientación**: no da ni quita acceso. `agente` queda en el vocabulario sólo como valor heredado (ver abajo) |
| `traces` | Sí (puede ser `[]`) | Lista de `ID` o `ID@MAYOR.MENOR`, sin repetidos | De qué depende el documento; cada traza tiene que resolver |
| `description` | Sí | Texto de 10 a 160 caracteres | Resumen para índices y agentes. La revisa una persona en el pull request que pasa el documento a vigente |
| `delivery` | No | Lista de audiencias declaradas en el registro de audiencias (`AUD-`) | Regla de entrega: qué audiencias reciben la página. Es el campo que protege |

**Audiencia y entrega.** `audience` orienta y `delivery` entrega. La clave `delivery` está en inglés, como las demás, y
no se parece a `audience` para que un error de tipeo no cambie la orientación por la entrega. Sus valores están cerrados
al registro de audiencias, una pieza con prefijo `AUD-` y `doc_type: registro-audiencias` que tiene una fila por
audiencia con sus dimensiones (rol, tipo de alcance, superficie y nivel de acceso) y la jerarquía con `incluye:`; la
matriz audiencia × página se deriva y no se escribe a mano. No hay migración: lo heredado se conserva, y un documento
sin `delivery` o con `agente` en `audience` sólo avisará. El agente es un lector, no una audiencia, y hereda la de su
usuario. Los controles `audiencia-vocabulario` (un valor de `delivery` fuera del registro) y `delivery-clave-cercana`
(`audiencias:`, `audiences:` y variantes) nacen en «avisa» y todavía no están implementados en `docs-guard`: entran con
el expediente de herramienta, cada uno con su par rojo/verde. Hoy el esquema sólo comprueba la forma de `delivery`.

## 3. README del cuerpo y páginas de ayuda

Dos formas tienen su propio esquema:

| Forma | Cómo se reconoce | Campos obligatorios |
| --- | --- | --- |
| README de un cuerpo | `README.md` con `corpus_id` | `corpus_id` (mayúsculas y guiones), `version` (SemVer, admite sufijo `-borrador`), `title`, `description`. Puede sumar los diez campos, como el [README de este cuerpo](../README.md) |
| Página de ayuda al usuario final | Tiene `roles` o `translation_of` y no tiene `doc_id` | `title` y, según el caso, `roles` o `translation_of`. Opcionales: `delivery` (como arriba) y `contexto: <app>/<id>`, el identificador de contexto de la pantalla o del campo que pide la página; el mapa pantalla → página se deriva de esos campos y el control `contexto-resuelve` (página sin `contexto` que la pida o `contexto` sin página) entra en «avisa» con el expediente de herramienta. El esquema completo lo trae el ejemplo de la ayuda por rol |

`corpus_id` y `version` viven sólo en el README: un tema no repite la versión del cuerpo. Única excepción: el
conocimiento de `Conocimientos/` lleva `version`, que repite su `**Versión:**` ([Marco, §5](../00-Marco.md#5-convenciones)).

## 4. Trazas

Una traza dice de qué otro identificador depende un documento. Tiene dos niveles:

| Nivel | Dónde | Forma | Ejemplo |
| --- | --- | --- | --- |
| 1 | En el frontmatter de la página | `traces: [ID]` o `traces: [ID@MAYOR.MENOR]` | `traces: [RN-LOC-01@1.1]` (la ayuda de la mini demo) |
| 2 | En la ficha del ID | `ruta#Símbolo@SHA` de dónde se declara, se verifica y se aplica en el código | `src/MovilidadUrbana.MAUI/Domain/Rules/LocalidadRules.cs#LocalidadRules.LargoMaximoDelNombre@8fb4c3a` (la ficha `RN-LOC-01`; el símbolo va como `Clase.Miembro`) |

El registro contra el que se resuelven se genera solo: lo forman los `doc_id` de todo el repositorio y los
encabezados que empiezan con un identificador (por ejemplo, el encabezado `### ESC-SDEI-2 — App cliente sin roles`
del [Marco](../00-Marco.md#2-escenarios) declara `ESC-SDEI-2`). Con `@MAYOR.MENOR`, la declaración tiene que tener
esa versión.

```mermaid
flowchart LR
  PG["Página"] -->|declara| P["traces: [ID#64;1.0]"] --> R{"¿ID en el registro?"}
  R -->|no, prefijo interno| ROJO["Rojo"]
  R -->|no, prefijo externo sin source.lock| AV["Aviso: no verificable"]
  R -->|no, prefijo externo con source.lock| ROJO
  R -->|sí, una vez| V{"¿Versión igual?"}
  R -->|sí, más de una vez| ROJO
  V -->|sí o sin versión| OK["Verde"]
  V -->|no| ROJO
```

*Diagrama: cómo decide el control `traces-resolve`. Un ID declarado dos veces es ambiguo y también da rojo.*

## 5. Registro de prefijos

El registro es cerrado: un prefijo nuevo entra por un pull request que lo agrega. **Interno** quiere decir que el ID se
declara en este repositorio; si no resuelve, bloquea. **Externo** quiere decir que se declara en el sistema que se
documenta y se resuelve contra el SHA de su `source.lock`.

| Prefijo | Ámbito | Qué identifica |
| --- | --- | --- |
| `ESC` | interno | Escenario del marco |
| `CTX` | interno | Contexto del marco |
| `ACT` | interno | Actor del marco |
| `DOC` | interno | Documento temático por tipo de documentación |
| `FAM` | interno | Familia documental |
| `MET` | interno | Método ágil |
| `ARQ` | interno | Modelo de arquitectura |
| `MARCO` | interno | Documento del marco de referencia |
| `MAPA` | interno | Mapa conceptual |
| `ANX` | interno | Anexo |
| `IDX` | interno | Índice |
| `FC` | interno | Documento del Framework Concept (`FC-TEM-nn`, `FC-GUIA-INDICE`, `FC-MARCO-REFERENCIA`, `FC-ANEXO-99`), espacio propio para no chocar con los cuerpos de `Guides/` |
| `AUD` | interno | Registro de audiencias (`doc_type: registro-audiencias`) |
| `TEM` | interno | Tema del cuerpo |
| `CQ` | interno | Criterio de calidad |
| `PE` | interno | Patrón de ejemplo |
| `OBJ` | interno | Objetivo de aprendizaje |
| `EJR` | interno | Ejercicio |
| `RD` | interno | Requerimiento documental |
| `VER` | interno | Ítem de verificación |
| `GP` | interno | Guía práctica de un ejemplo |
| `DEMO` | interno | Pieza de una demo de `Examples/` (mini demo MovilidadUrbana: `DEMO-MU-nn`) |
| `RF` | externo | Requisito funcional |
| `RNF` | externo | Requisito no funcional |
| `RN` | externo | Regla de negocio |
| `CU` | externo | Caso de uso |
| `ADR` | externo | Decisión de arquitectura |
| `TC` | externo | Caso de prueba |
| `RSK` | externo | Riesgo |
| `FLU` | externo | Flujo de usuario |
| `NB` | externo | Necesidad de negocio |

Las referencias bibliográficas usan anclas `ref-apellido-año` dentro del [anexo de referencias](Referencias.md); no
son identificadores del registro.

**Convención de este cuerpo [criterio propio].** Los identificadores del marco y de los anexos llevan el `corpus_id`
como segundo segmento (`ESC-SDEI-1`, `ANX-SDEI-CQ`), porque el registro es único para todo el repositorio y otras
guías ya declaran `ESC-1` o `MARCO-CONVENCIONES`. Sin ese segmento, una traza a `ESC-1` sería ambigua.

## 6. Registro de requerimientos documentales

Un archivo `_requisitos.md` o con `doc_type: rrd` tiene una tabla con estas columnas, en este orden:

| Columna | Regla |
| --- | --- |
| `ID` | `RD-nn` |
| `Audiencia` | Una sola |
| `Tarea` | Lo que esa audiencia necesita lograr |
| `Pieza` | Clase y archivo |
| `Criterio` | Forma Dado/Cuando/Entonces |
| `Decisión` | `aceptado`, `rechazado`, `diferido` o `pendiente` |

Ejemplo de fila válida: la primera del RRD de la mini demo
([`docs/_requisitos.md`](../Examples/MovilidadUrbana-Mini-Demo/docs/_requisitos.md)), recortada como en `TEM-02`:

| ID | Audiencia | Tarea | Pieza | Criterio | Decisión |
| --- | --- | --- | --- | --- | --- |
| RD-01 | persona usuaria | Dar de alta una localidad sin que el nombre se rechace ni quede distinto de lo esperado | CL-06, `docs/ayuda/alta-de-localidad.md` | Dado el límite de cada regla, cuando la persona lee la ayuda, entonces cada número coincide con el de la ficha; instrumento: `traces-resolve` | aceptado |

## 7. Un ejemplo completo y sus errores

Frontmatter válido de un tema, el de `TEM-03` de este cuerpo, con la lista de `audience` y de `traces` recortada
[Fragmento ilustrativo: frontmatter de `TEM-03`, l. 1-13, con `audience` y `traces` recortadas]:

```yaml
---
doc_id: TEM-SDEI-03
doc_type: tema
title: Tema 03 — Topología de repositorios y versionado
description: "Dónde vive cada pieza documental respecto del código y contra qué versión vale: commit, SHA fijado o etiqueta, con la regla para elegir."
status: borrador
origin: ia-assisted
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-10-28
audience: [humano, agente, estudiante]
traces: [MARCO-SDEI, MAPA-SDEI, TEM-SDEI-02]
---
```

| Si cambiás… | La compuerta dice | Grupo |
| --- | --- | --- |
| `status: aprobado` | `esquema-cuerpo`: valor fuera del vocabulario | PR-2 |
| Borrás `confidence` con `origin: ia-assisted` | `esquema-cuerpo`: falta `confidence` | PR-2 |
| `doc_id: TOP-03` | `prefijo`: «TOP-» no está en el registro | PR-2 |
| `traces: [TEM-SDEI-99]` | `traza-no-resuelve` | PR-5 |
| Borrás el bloque entero | `sin-frontmatter`: todo documento del cuerpo abre con frontmatter | PR-2 |

## 8. Dónde vive la fuente de verdad

Las tablas de este anexo explican el contrato. Los datos que la compuerta lee están en el repositorio:

| Dato | Archivo |
| --- | --- |
| Esquema de un documento con `doc_id` | [`cuerpo.schema.json`](../../../../tools/docs-guard/esquemas/cuerpo.schema.json) |
| Esquema del README de un cuerpo | [`readme-cuerpo.schema.json`](../../../../tools/docs-guard/esquemas/readme-cuerpo.schema.json) |
| Esquema de una página de ayuda | [`ayuda.schema.json`](../../../../tools/docs-guard/esquemas/ayuda.schema.json) |
| Registro de prefijos | [`prefijos.txt`](../../../../tools/docs-guard/prefijos.txt) |

Si una tabla de este anexo y su archivo no coinciden, manda el archivo, y la diferencia se corrige en este anexo en el
mismo pull request que cambió el archivo.

Volver al [Mapa](../01-Mapa.md#6-anexos).
