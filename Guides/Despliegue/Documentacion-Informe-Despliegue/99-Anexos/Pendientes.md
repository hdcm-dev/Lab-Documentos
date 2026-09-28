---
doc_id: ANEXO-PENDIENTES
doc_type: anexo
title: Pendientes y estado del avance
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [ANEXO-REFERENCIAS, MARCO-CONVENCIONES, GUIA-INDICE]
---

# Pendientes y estado del avance — `ANEXO-PENDIENTES`

## Resumen ejecutivo

Registro del estado de construcción de la guía y de lo que queda abierto, para que el trabajo sea retomable ante un corte. Distingue tres cosas: qué está terminado, qué fuentes quedaron sin verificar de primera mano y no deben citarse como hechas, y qué extensiones tienen sentido pero no se abordaron. No es una lista de deseos: cada pendiente tiene ubicación propuesta y criterio de cierre.

---

## Estado del cuerpo documental

| Bloque | Documentos | Estado |
|--------|-----------:|--------|
| Marco de referencia | 4 | Completo |
| Mapa conceptual | 1 | Completo |
| Familia 10 · Naturaleza del informe | 4 | Completo |
| Familia 20 · Arquitectura | 4 | Completo |
| Familia 30 · Despliegue | 4 | Completo |
| Familia 40 · Requisitos | 3 | Completo |
| Familia 50 · Redacción | 4 | Completo |
| Anexos | 5 | Completo |
| Índice (README) | 1 | Completo |

El recuento de familias incluye el README de cada familia. La revisión de consistencia contra el mapa se registra en la sección siguiente.

---

## Fuentes sin verificar de primera mano

Trasladadas de [`ANEXO-REFERENCIAS`](Referencias.md#7-fuentes-no-verificadas-de-primera-mano) para tenerlas juntas. No se citan como hechos hasta verificarlas contra el documento primario.

| Fuente | Qué falta verificar | Cómo cerrarlo |
|--------|---------------------|---------------|
| Normas ISO `N-01`..`N-06` | Texto normativo interno de cláusulas; solo se leyó portada, prólogo, alcance e índice de los PDF de muestra | Acceder al texto completo de pago y verificar cada cita de cláusula |
| `N-07` UML, cláusula 19 | La semántica de nodos y artefactos se parafraseó del índice | Leer la cláusula 19 del PDF de la OMG |
| `G-04` SAD de RUP | La estructura de secciones exacta es convención variable | Contrastar contra una fuente RUP primaria si se necesita precisión |
| `N-02` procesos | La cifra «seis procesos, 37 actividades» es de literatura secundaria | Verificar contra el texto de la norma |
| Imágenes base *chiseled*/*distroless* de .NET | No están en `N-15` | Verificar la página de imágenes base de .NET antes de citar |
| `F-02` draft IETF resumable upload | El número de revisión del draft cambia con el tiempo | Reverificar la revisión vigente al citar |

---

## Convenciones sin fijar

- **Numeración de secciones del informe.** La [plantilla](Plantilla-del-Informe.md) propone trece secciones; no se fijó si en informes cortos conviene fusionar 5–6 (componentes y vistas) o 7–8 (despliegue y operación). Criterio de cierre: decidir un umbral de tamaño por debajo del cual se fusionan, y documentarlo en [`TEM-ESTRUCTURA`](../50-Redaccion/Estructura-del-Documento.md).
- **Prefijos de requisitos.** La guía usa `RF-` y `RNF-`. No se fijó cómo se relacionan con los identificadores del [SRS](../../Documentacion-Tecnica/20-Analisis/SRS.md) de la guía hermana si ambos coexisten en un proyecto. Criterio de cierre: definir si el informe reusa los IDs del SRS o mantiene los propios con una tabla de correspondencia.

---

## Extensiones posibles no abordadas

Temas con lugar en el marco pero fuera del alcance de esta versión. Se listan con la familia donde entrarían.

- **Vista de seguridad en el informe** (`FAM-ARQ`). Cómo integrar la postura de seguridad —límites de confianza, datos sensibles— sin duplicar la [arquitectura de seguridad](../../Documentacion-Tecnica/30-Arquitectura/Arquitectura-de-Seguridad.md) de la guía hermana. Hoy se referencia; podría desarrollarse el ángulo de síntesis.
- **Vista de datos en el informe** (`FAM-ARQ`). Qué del [modelo de datos](../../Documentacion-Tecnica/40-Diseno/Modelo-de-Datos.md) merece entrar en un informe de solución y a qué nivel.
- **Costos y capacidad** (`FAM-DESP`). Dimensionamiento e infraestructura como parte de la vista de despliegue, más allá de la topología.
- **Presentación oral del informe** (`FAM-RED`). Cómo se defiende un informe de solución ante un comité técnico, que es un uso frecuente y con criterios propios.

---

## Registro de revisión de consistencia

| Fecha | Qué se revisó | Resultado |
|-------|---------------|-----------|
| 2026-07-21 | Construcción inicial de la guía: marco, mapa, cinco familias, anexos e índice | Ver [informe de la revisión final en el README](../README.md#estado-y-verificación) |

Lo que la revisión verifica: que todo `doc_id` del mapa exista, que no haya enlaces rotos, que cada afirmación normativa cite una fila de `ANEXO-REFERENCIAS`, que los diagramas Mermaid sean válidos, y que ningún documento reescriba contenido que corresponde a la guía hermana.
