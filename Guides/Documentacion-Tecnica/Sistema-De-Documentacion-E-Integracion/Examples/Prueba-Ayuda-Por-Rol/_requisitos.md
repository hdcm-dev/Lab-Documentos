---
doc_id: DEMO-PAR-01
doc_type: rrd
title: Registro de requerimientos documentales de la prueba de ayuda por rol
description: Qué página de ayuda pide cada rol del panel web de la prueba, a qué caso de uso de la unidad Web traza y cómo se comprueba.
status: borrador
origin: ia-assisted
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-11-01
audience: [analista, po, desarrollo, agente]
traces: [TEM-SDEI-05]
---

# Registro de requerimientos documentales de la prueba de ayuda por rol

## Índice

1. [Qué es este registro](#1-qué-es-este-registro)
2. [Requerimientos](#2-requerimientos)
3. [Cómo se verifica](#3-cómo-se-verifica)

## 1. Qué es este registro

Es el RRD de las siete páginas en castellano de [`Web/Ayuda/es/`](Web/Ayuda/es/). La prueba es un ejemplo de
`ESC-SDEI-3` (panel con roles): cada página tiene su fila en este registro y traza al caso de uso de la unidad que contiene
la pantalla, que en Lab-Geometria es la unidad Web (`GeometriaFactory-Web`), leída en el SHA `5f07b12`. El vínculo
con la fila va en la columna «Pieza» de este registro y no en `traces` de la página: el esquema liviano de las páginas
de ayuda (`tools/docs-guard/esquemas/ayuda.schema.json`) sólo admite trazas con versión, y las filas `RD-` no la
tienen. Las filas
usan el rango `RD-21` en adelante porque los `RD-` son únicos en todo el repositorio y la mini demo ya usa `RD-01` a
`RD-04`. Las páginas de [`Consola/Ayuda/`](Consola/Ayuda/) no tienen fila: son el banco de prueba del validador, con
defectos plantados.

| Término | Significado |
| --- | --- |
| Audiencia | Rol del panel que lee la página: alumno, administrador, o los dos |
| Fuente | El caso de uso de la unidad Web con su versión en `5f07b12` |
| Decisión | La tomó la mesa del expediente 0003 por delegación del PO, al decidir cómo se corrige el ejemplo |

## 2. Requerimientos

| ID | Audiencia | Tarea | Pieza | Fuente | Criterio | Decisión | Decide y fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| RD-21 | alumno y administrador | Ingresar al laboratorio con correo y contraseña | CL-06, [`es/comun/ingresar.md`](Web/Ayuda/es/comun/ingresar.md) | `CU-10002@1.5` | Dado el caso de uso de la pantalla, cuando la persona lee la ayuda, entonces los pasos coinciden con su flujo; instrumento: `traces-resolve` con el SDD como fuente y relectura declarada en el pull request | aceptado | mesa del expediente 0003, 2026-11-01 |
| RD-22 | alumno y administrador | Cambiar la contraseña propia, también cuando el cambio es obligatorio | CL-06, [`es/comun/cambiar-la-contrasena.md`](Web/Ayuda/es/comun/cambiar-la-contrasena.md) | `CU-10003@1.5` | Dado el caso de uso de la pantalla, cuando la persona lee la ayuda, entonces los pasos coinciden con su flujo; instrumento: `traces-resolve` con el SDD como fuente y relectura declarada en el pull request | aceptado | mesa del expediente 0003, 2026-11-01 |
| RD-23 | alumno | Enviar un trabajo, con la previsualización optativa, y leer las observaciones | CL-06, [`es/alumno/enviar-un-trabajo.md`](Web/Ayuda/es/alumno/enviar-un-trabajo.md) | `CU-10005@1.2` | Dado el caso de uso de la pantalla, cuando la persona lee la ayuda, entonces los pasos coinciden con su flujo; instrumento: `traces-resolve` con el SDD como fuente y relectura declarada en el pull request | aceptado | mesa del expediente 0003, 2026-11-01 |
| RD-24 | alumno | Consultar los trabajos propios y abrir uno | CL-06, [`es/alumno/consultar-mis-trabajos.md`](Web/Ayuda/es/alumno/consultar-mis-trabajos.md) | `CU-10006@1.1`, `CU-10007@1.3` | Dado el caso de uso de la pantalla, cuando la persona lee la ayuda, entonces los pasos coinciden con su flujo; instrumento: `traces-resolve` con el SDD como fuente y relectura declarada en el pull request | aceptado | mesa del expediente 0003, 2026-11-01 |
| RD-25 | administrador | Habilitar, bloquear, rehabilitar y dar de baja cuentas, sabiendo que la baja borra los trabajos | CL-06, [`es/administrador/gobernar-las-cuentas.md`](Web/Ayuda/es/administrador/gobernar-las-cuentas.md) | `CU-10004@1.9` | Dado el caso de uso de la pantalla, cuando la persona lee la ayuda, entonces los pasos coinciden con su flujo y la ayuda avisa lo que no se puede deshacer; instrumento: `traces-resolve` con el SDD como fuente y relectura declarada en el pull request | aceptado | mesa del expediente 0003, 2026-11-01 |
| RD-26 | administrador | Resetear la contraseña de un alumno | CL-06, [`es/administrador/resetear-una-contrasena.md`](Web/Ayuda/es/administrador/resetear-una-contrasena.md) | `CU-10004@1.9` | Dado el caso de uso de la pantalla, cuando la persona lee la ayuda, entonces los pasos coinciden con su flujo; instrumento: `traces-resolve` con el SDD como fuente y relectura declarada en el pull request | aceptado | mesa del expediente 0003, 2026-11-01 |
| RD-27 | administrador | Dar desenlace a un trabajo pendiente | CL-06, [`es/administrador/dar-desenlace.md`](Web/Ayuda/es/administrador/dar-desenlace.md) | `CU-10009@1.1` | Dado el caso de uso de la pantalla, cuando la persona lee la ayuda, entonces los pasos coinciden con su flujo; instrumento: `traces-resolve` con el SDD como fuente y relectura declarada en el pull request | aceptado | mesa del expediente 0003, 2026-11-01 |

## 3. Cómo se verifica

En la compuerta de este repositorio, cada fila pasa el esquema del RRD y cada traza al caso de uso queda como aviso
«no verificable»: la carpeta no tiene `source.lock`, porque todavía no hay un trabajo de la CI que baje Lab-Geometria
en ese SHA y lo pase por `--fuente` ([`TEM-05`](../../TEM-05-Ayuda-Al-Usuario-Final.md#33-ayuda-por-rol-con-un-cruce-que-nadie-había-plantado)).
A mano, con el SDD de la unidad en `5f07b12` como fuente, las ocho trazas a casos de uso resuelven:

```bash
git clone --quiet --filter=blob:none https://github.com/hdcm-dev/Lab-Geometria.git <clon>
mkdir -p <sdd> && git -C <clon> archive 5f07b12 SDD/Docs/Unidades-Entrega | tar -x -C <sdd>
find <sdd> -type d -name _legacy -prune -exec rm -rf {} +
python3 -I tools/docs-guard/dg.py trazas <esta carpeta>/Web/Ayuda/es --raiz . --fuente <sdd>
```

Volver al [README de la prueba](README.md).
