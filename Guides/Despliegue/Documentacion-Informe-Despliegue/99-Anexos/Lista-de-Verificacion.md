---
doc_id: ANEXO-CHECK
doc_type: anexo
title: Lista de verificación del informe
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Informe de solución: arquitectura, despliegue y requisitos en .NET
last_review: 2026-07-21
audience: [humano, agente]
traces: [ANEXO-PLANTILLA, TEM-ERRORES, TEM-RNF, MARCO-ESCENARIOS, MARCO-CONTEXTOS]
---

# Lista de verificación del informe — `ANEXO-CHECK`

## Resumen ejecutivo

Lista para revisar un informe de solución antes de entregarlo, o para evaluar uno ajeno en `ESC-4`. Está organizada en tres cortes: una revisión general que aplica a todo informe, una por sección de la [plantilla](Plantilla-del-Informe.md), y una condicional según el escenario y el contexto. No sustituye al criterio: una casilla marcada no garantiza calidad, pero una sin marcar señala un hueco concreto. Los ítems se ordenan de más a menos determinante dentro de cada bloque.

Como toda lista de verificación, su valor está en los ítems que uno preferiría saltear. El campo que más informes reprueba —`cada RNF tiene métrica`— es también el que más se omite en la autoevaluación.

---

## Revisión general

| # | Verificación | Por qué importa |
|---|---|---|
| G1 | El resumen ejecutivo se entiende sin leer el resto | `ACT-06` decide con eso solo |
| G2 | Está declarado el destinatario y qué decisión habilita el informe | Sin eso, todo pesa igual y nada sirve a la decisión |
| G3 | El alcance dice qué queda **excluido** y por qué | Evita el reclamo por lo que no estaba | 
| G4 | Cada afirmación normativa cita su fuente y su nivel de autoridad | Distingue norma de plantilla de costumbre |
| G5 | Las siglas se expanden en su primer uso | El lector no comparte el contexto del autor |
| G6 | Los diagramas dicen lo mismo que el texto | Un diagrama que contradice al texto siembra desconfianza |
| G7 | Los diagramas están como código (Mermaid), regenerables | Diffeables y mantenibles |
| G8 | No se reescribe lo que ya está en otro documento; se referencia | Única fuente de verdad |

---

## Revisión por sección

Cada fila remite a la sección de la [plantilla](Plantilla-del-Informe.md).

| Sección | Verificación clave |
|---|---|
| 1 · Resumen ejecutivo | Se entiende solo, sin jerga; dice qué se decide |
| 2 · Introducción y alcance | Incluye/excluye explícito; referencias a SAD/SRS si existen |
| 3 · Contexto y objetivos | *Stakeholders* y *concerns* nombrados; diagrama de contexto no se mete adentro del sistema |
| 4 · Visión de arquitectura | Estilo justificado por un *concern*; diagrama al nivel de zoom correcto |
| 5 · Vista de componentes | Cada componente con responsabilidad en una frase y límites; no es el árbol de carpetas |
| 6 · Vistas y comportamiento | El flujo elegido revela algo que el diagrama estático no |
| 7 · Vista de despliegue | Nodo↔componente, protocolo y puerto; modelo de publicación declarado |
| 8 · Operación y resiliencia | Describe el peor caso; el comportamiento degradado es diseño, no accidente |
| 9 · Requisitos funcionales | Cada RF significativo trazado a un mecanismo; RF de `ACT-05`, no inventados |
| 10 · Requisitos no funcionales | Cada RNF con métrica y con su característica de `N-04` 25010:2023 |
| 11 · Decisiones | Alternativas y costos registrados, no solo beneficios |
| 12 · Riesgos y pendientes | Presente; sin riesgos escondidos por incomodidad |
| 13 · Anexos | Glosario, referencias y diagramas fuente |

---

## Revisión condicional por escenario

| Escenario | Verificación obligatoria |
|---|---|
| `ESC-1` En diseño | Cada afirmación marca su estado: decidido / propuesto / por confirmar. Los supuestos a validar están en la sección de pendientes |
| `ESC-2` Construida | Las divergencias conocidas entre diseño y realidad están declaradas. Cada afirmación se puede confrontar con el sistema en ejecución |
| `ESC-3` En evolución | Hay estado actual **y** objetivo. El costo, el riesgo y el período de convivencia de la migración están declarados |
| `ESC-4` Evaluación | Se separó lo demostrado de lo afirmado. Las preguntas sin responder quedaron listadas con su nivel de confianza |

---

## Revisión condicional por contexto

| Contexto | Verificación obligatoria |
|---|---|
| `CTX-1` Monolito | La arquitectura describe componentes y responsabilidades, no la estructura de carpetas |
| `CTX-2` Cliente-servidor | Los contratos entre nodos y el punto donde termina TLS están descritos |
| `CTX-3` Borde distribuido | La instalación por terminal está como procedimiento; la operación degradada, en el centro del informe, no en una nota |
| `CTX-4` Multiservicio | Los componentes tienen jerarquía —no todo con el mismo detalle—; la consistencia de datos distribuidos está tratada |

---

## Bloque para agentes

Un agente que valide un informe puede recorrer estos campos y marcar el resultado. La ausencia de un campo obligatorio para el escenario o contexto es un hallazgo.

```yaml
verificacion:
  general:
    resumen_se_entiende_solo: si | no
    destinatario_y_decision_declarados: si | no
    alcance_excluye_explicito: si | no
    afirmaciones_normativas_con_fuente: si | no
    diagramas_coinciden_con_texto: si | no
    no_duplica_otros_documentos: si | no
  por_seccion:
    componentes_no_es_arbol_de_carpetas: si | no
    despliegue_declara_modelo_publicacion: si | no
    cada_RF_trazado_a_mecanismo: si | no
    cada_RNF_con_metrica: si | no
    cada_RNF_nombra_caracteristica_25010: si | no
    decisiones_con_alternativas_y_costo: si | no
    seccion_riesgos_presente: si | no
  condicional:
    escenario: ESC-?
    contexto: CTX-?
    esc2_divergencias_declaradas: si | no | na
    esc3_estado_actual_y_objetivo: si | no | na
    ctx3_instalacion_por_terminal_descrita: si | no | na
    ctx3_operacion_degradada_central: si | no | na
  hallazgos: []          # ítems en "no" que constituyen huecos
```

El par `cada_RNF_con_metrica` / `cada_RNF_nombra_caracteristica_25010` es el corazón de la evaluación de un informe de solución. Un requisito no funcional sin métrica no se puede verificar; uno sin característica de referencia no se puede ubicar en el mapa de calidad y suele esconder que falta otro.
