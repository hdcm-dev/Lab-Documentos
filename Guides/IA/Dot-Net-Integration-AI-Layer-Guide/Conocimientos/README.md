# Conocimientos de la capa de IA en .NET

Conocimientos para agentes IA (y para personas) que integran una capa de IA en un sistema .NET, nuevo o
existente, trabajando como lo hizo el equipo que diseñó PushDispatch desde cero. Los libros son la fuente para
personas: la guía [`Dot-Net-Integration-AI-Layer-Guide.md`](../Dot-Net-Integration-AI-Layer-Guide.md) y la guía de
diseño conversacional [`Conversational-Design-Framework-Guide.md`](../Conversational-Design-Framework-Guide.md) (1.6.0), cuya base «Criterio y juicio» es la norma del turno. Un conocimiento destila el libro en imperativos con su porqué y su prueba,
remite a él por sección y nunca lo contradice: si choca, rige el libro.

| Id | Conocimiento | Para qué sirve | Cuándo se carga | Estado |
|---|---|---|---|---|
| K1 | [knowledge-AI-Layer-Integration-Designing-Specification.md](knowledge-AI-Layer-Integration-Designing-Specification.md) | Decidir si hace falta IA y especificarla: frontera en anillos, ARQ-1 a ARQ-6, `REQ-IA-nn` con prueba, OWASP | Antes de abrir una rama de código | 1.6 Vigente |
| K2 | [knowledge-AI-Layer-Console-Assistant-Building.md](knowledge-AI-Layer-Console-Assistant-Building.md) | Construir la consola en un panel Blazor: el código elige, el modelo redacta; corpus, destinos, votos, despliegue | Al implementar o tocar la consola | 1.7 Borrador |
| K3 | [knowledge-AI-Layer-Verification-And-Acceptance.md](knowledge-AI-Layer-Verification-And-Acceptance.md) | Aceptar con evidencia: batería ×3 ≥ 90 %, crítico ≥ 8, capturas, sitio real por rol, latencia | Antes de declarar cerrada una fase | 1.9 Vigente |
| K4 | [knowledge-AI-Layer-Integration-Working-Method.md](knowledge-AI-Layer-Integration-Working-Method.md) | Forma de trabajo: fases tipo, mesa con refutador, brecha, PRs por choque, registro de frentes, antipatrones E1–E9 | Al iniciar o retomar una integración | 1.5 Vigente |
| K5 | [knowledge-AI-Layer-Conversational-Design.md](knowledge-AI-Layer-Conversational-Design.md) | **Marco normativo del turno** (§M: principios T-1 a T-8, prueba de anillo, árbol, regla de aclaración, compuestas, rúbrica R1–R14) y formas de respuesta F1–F8 | Al decidir si un turno aclara, al escribir o leer respuestas, al juzgar un cambio | 1.2 Vigente |
| K6 | [knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md) | Detalle de K2: redactar el corpus RAG (frontmatter, fichas, sinónimos, recall@3, errores de ranking), crear funciones con `AIFunctionFactory` y resolver deeplinks del catálogo | Al escribir una página de ayuda, una función o un destino | 1.9 Borrador |
| K7 | [knowledge-AI-Layer-Testing-Strategy.md](knowledge-AI-Layer-Testing-Strategy.md) | Detalle de K3: la suite completa, de unitarias sin modelo y bUnit a adversas, mutación, E2E, batería ×3 y compuertas de CI | Al crear o cambiar pruebas, scripts de batería o la CI | 1.7 Vigente |
| K8 | [knowledge-AI-Layer-Chat-Console-Interface.md](knowledge-AI-Layer-Chat-Console-Interface.md) | La consola de chat como interfaz: anatomía, estados, destinos y gráficos, escritorio y teléfono, accesibilidad, historial, cambios sin guardar, voto, persistencia y observabilidad de conversaciones, variante multi-participante (propuesta) | Al diseñar o construir la superficie de la consola, su historial o su registro de conversaciones | 1.0 Borrador |

**Orden de lectura para un agente nuevo:** K4 §0 y §1.1 → K1 → K5 → K2 → K8 → K6 → K3 → K7. K4 gobierna; K5 atraviesa a
los demás: su §M es la norma del turno y K1, K2 y K6 remiten a ella en lugar de repetirla.

**Encadenamiento.** K6 detalla K2 (corpus, funciones, destinos), K8 dibuja lo que K2 calcula y K5 decide (la superficie de la consola) y K7 detalla K3 (cómo se escriben las pruebas que
producen la evidencia). K2 y K3 resumen y remiten; si un detalle de K6 o K7 pareciera chocar con su padre, rige el
padre y se corrige el hijo. Cada regla de K6 trae su prueba, que se escribe con K7.

**Convenciones.** Cabecera con alias, naturaleza, tema, consumidor, condición de carga, hereda de, sustituye,
compatible con, versión, estado y fecha. Anclas estables `kN-…`. Remisiones al libro como `[Guía §x]` con su ancla
`cN-k`. Los antipatrones se citan con prefijo (`K2-AP-n`, `K3-AP-n`, `K5-AP-n`, `K6-AP-n`, `K7-AP-n`, `K8-AP-n`) o por su `En` de K4 §8. Son
autocontenidos: los conocimientos del workspace de origen que se nombran como opcionales están resumidos en K2 §16
y K4 §14. Repo público: no se nombran el producto de referencia del mercado, clientes, servidores, direcciones,
usuarios, claves ni el modelo de IA.

**Última retroalimentación (2026-11-15, interfaz de la consola y juicio inicial y rondas 1 y 2).** Un relevamiento de
las tres consolas construidas (PushDispatch, Geometría y Movilidad Urbana) mostró que K1–K7 reproducían el motor y
el criterio pero no la interfaz: entra K8 1.0 Borrador (anatomía, estados, teléfono, accesibilidad, historial,
cambios sin guardar, persistencia y observabilidad con captura apagada por omisión, y la variante
multi-participante como propuesta rotulada). Las rondas del juicio de PushDispatch (3/28 → 10/28 → 11/28, vetos R12
y R3) dejan: matriz por rol escrita y almacenes de la conversación (K1 1.6), definición primero, mismo número en
todas las superficies, negativa sin guía ajena y receta hasta su confirmación (K5 1.2), seis defectos de anclaje y
recorte tras un cambio de corpus (K6 1.9), cambio de corpus o rol medido ×3 y PR con casos y pares a ciegas (K3
1.9), bisección de merges, catálogo de producción en la batería y prueba por veto (K7 1.7). K2 1.7 y K4 1.5 remiten
a K8. Guía de diseño conversacional 1.6.0 (lecciones 11 y 12).

**Retroalimentación anterior (2026-11-12, proveedores, consumo y MCP).** Dos mesas sobre la consola de PushDispatch
(la ronda del dueño del producto y la de proveedores de IA) y sus primeras rebanadas implementadas (fábrica de
clientes, registro de consumo, medición del proveedor de identidad, organizaciones del operador con catálogo de
intenciones). Entran: catálogo y contrato de intenciones con regla F/D/R fija y compuerta (K6 1.8, K7 R5.5),
negativa por cercanía y forma del gráfico como parámetro del contrato (K5 1.1), pastillas por cota de Wilson y
decorador de consumo (K2 1.6), proveedores como adaptadores con modelo por omisión por función, habilitación por
batería, límites de consumo con convenciones OpenTelemetry GenAI y MCP como servidor y como cliente (K1 1.5, K3
R2.11–R2.12, K7 R5.6–R5.7), y el plan por rebanadas con dependencias (K4 1.4). Guía de diseño conversacional 1.5.0.

**Retroalimentación previa (2026-11-12, marco del turno).** La consola de PushDispatch contestó «¿En qué cuenta?» a
«qué es una organización y dónde la creo», y el dueño del producto no vio mejora en las capturas: la aclaración
estaba regulada en tres lugares sin condicionarla a que el dato cambiara la respuesta, y no había un juicio de
mejora percibida. K5 pasa a 1.0 Vigente con el marco normativo (§M) destilado de la base de la guía de diseño
conversacional 1.4.0; K1 (1.4), K2 (1.5), K4 (1.3) y K6 (1.7) remiten a él. K6 sigue Borrador en cabecera, tabla e
historial (antes el historial estaba desordenado).

**Retroalimentación más antigua (2026-11-12).** Banco de diálogo sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria):
una aplicación a ciegas de K1–K7 contrastada con lo construido y con la rebanada EXP-0005 (chips que abren la
acción). Huecos cerrados: número hablado parseado y clase «número que cita la fórmula» (K7 R6.7), criterio del
enrutador frente al del corpus de fichas (K7 R6.10, K1 F-10), negativa que trae la ficha y rótulo del chip igual
al de la acción y según el rol (K5 §6), pertinencia por estado (K2 §5.7), límite de tasa como no medido (K3
R2.6b, K7 R9.12). Reglas reescritas: todo control trae su caso rojo (K7 R9.8), procedimiento en la voz (K5 §7.5,
K7 R9.11), p95 con modelo frente al corte (K3 R10.2b), acción sin parámetro (K6 §17.4); el id repetido K3 R2.6 se
separó (R2.2b). El banco no midió descubrimiento: el congelado ya traía las lecciones anteriores.
