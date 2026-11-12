# Conocimientos de la capa de IA en .NET

Conocimientos para agentes IA (y para personas) que integran una capa de IA en un sistema .NET, nuevo o
existente, trabajando como lo hizo el equipo que diseñó PushDispatch desde cero. Los libros son la fuente para
personas: la guía [`Dot-Net-Integration-AI-Layer-Guide.md`](../Dot-Net-Integration-AI-Layer-Guide.md) y la guía de
diseño conversacional [`Conversational-Design-Framework-Guide.md`](../Conversational-Design-Framework-Guide.md) (1.4.0), cuya base «Criterio y juicio» es la norma del turno. Un conocimiento destila el libro en imperativos con su porqué y su prueba,
remite a él por sección y nunca lo contradice: si choca, rige el libro.

| Id | Conocimiento | Para qué sirve | Cuándo se carga | Estado |
|---|---|---|---|---|
| K1 | [knowledge-AI-Layer-Integration-Designing-Specification.md](knowledge-AI-Layer-Integration-Designing-Specification.md) | Decidir si hace falta IA y especificarla: frontera en anillos, ARQ-1 a ARQ-6, `REQ-IA-nn` con prueba, OWASP | Antes de abrir una rama de código | 1.4 Vigente |
| K2 | [knowledge-AI-Layer-Console-Assistant-Building.md](knowledge-AI-Layer-Console-Assistant-Building.md) | Construir la consola en un panel Blazor: el código elige, el modelo redacta; corpus, destinos, votos, despliegue | Al implementar o tocar la consola | 1.5 Borrador |
| K3 | [knowledge-AI-Layer-Verification-And-Acceptance.md](knowledge-AI-Layer-Verification-And-Acceptance.md) | Aceptar con evidencia: batería ×3 ≥ 90 %, crítico ≥ 8, capturas, sitio real por rol, latencia | Antes de declarar cerrada una fase | 1.7 Vigente |
| K4 | [knowledge-AI-Layer-Integration-Working-Method.md](knowledge-AI-Layer-Integration-Working-Method.md) | Forma de trabajo: fases tipo, mesa con refutador, brecha, PRs por choque, registro de frentes, antipatrones E1–E9 | Al iniciar o retomar una integración | 1.3 Vigente |
| K5 | [knowledge-AI-Layer-Conversational-Design.md](knowledge-AI-Layer-Conversational-Design.md) | **Marco normativo del turno** (§M: principios T-1 a T-8, prueba de anillo, árbol, regla de aclaración, compuestas, rúbrica R1–R14) y formas de respuesta F1–F8 | Al decidir si un turno aclara, al escribir o leer respuestas, al juzgar un cambio | 1.0 Vigente |
| K6 | [knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md) | Detalle de K2: redactar el corpus RAG (frontmatter, fichas, sinónimos, recall@3, errores de ranking), crear funciones con `AIFunctionFactory` y resolver deeplinks del catálogo | Al escribir una página de ayuda, una función o un destino | 1.7 Borrador |
| K7 | [knowledge-AI-Layer-Testing-Strategy.md](knowledge-AI-Layer-Testing-Strategy.md) | Detalle de K3: la suite completa, de unitarias sin modelo y bUnit a adversas, mutación, E2E, batería ×3 y compuertas de CI | Al crear o cambiar pruebas, scripts de batería o la CI | 1.5 Vigente |

**Orden de lectura para un agente nuevo:** K4 §0 y §1.1 → K1 → K5 → K2 → K6 → K3 → K7. K4 gobierna; K5 atraviesa a
los demás: su §M es la norma del turno y K1, K2 y K6 remiten a ella en lugar de repetirla.

**Encadenamiento.** K6 detalla K2 (corpus, funciones, destinos) y K7 detalla K3 (cómo se escriben las pruebas que
producen la evidencia). K2 y K3 resumen y remiten; si un detalle de K6 o K7 pareciera chocar con su padre, rige el
padre y se corrige el hijo. Cada regla de K6 trae su prueba, que se escribe con K7.

**Convenciones.** Cabecera con alias, naturaleza, tema, consumidor, condición de carga, hereda de, sustituye,
compatible con, versión, estado y fecha. Anclas estables `kN-…`. Remisiones al libro como `[Guía §x]` con su ancla
`cN-k`. Los antipatrones se citan con prefijo (`K2-AP-n`, `K3-AP-n`, `K5-AP-n`, `K6-AP-n`, `K7-AP-n`) o por su `En` de K4 §8. Son
autocontenidos: los conocimientos del workspace de origen que se nombran como opcionales están resumidos en K2 §16
y K4 §14. Repo público: no se nombran el producto de referencia del mercado, clientes, servidores, direcciones,
usuarios, claves ni el modelo de IA.

**Última retroalimentación (2026-11-12, marco del turno).** La consola de PushDispatch contestó «¿En qué cuenta?» a
«qué es una organización y dónde la creo», y el dueño del producto no vio mejora en las capturas: la aclaración
estaba regulada en tres lugares sin condicionarla a que el dato cambiara la respuesta, y no había un juicio de
mejora percibida. K5 pasa a 1.0 Vigente con el marco normativo (§M) destilado de la base de la guía de diseño
conversacional 1.4.0; K1 (1.4), K2 (1.5), K4 (1.3) y K6 (1.7) remiten a él. K6 sigue Borrador en cabecera, tabla e
historial (antes el historial estaba desordenado).

**Retroalimentación anterior (2026-11-12).** Banco de diálogo sobre [Lab-Geometria](https://github.com/hdcm-dev/Lab-Geometria):
una aplicación a ciegas de K1–K7 contrastada con lo construido y con la rebanada EXP-0005 (chips que abren la
acción). Huecos cerrados: número hablado parseado y clase «número que cita la fórmula» (K7 R6.7), criterio del
enrutador frente al del corpus de fichas (K7 R6.10, K1 F-10), negativa que trae la ficha y rótulo del chip igual
al de la acción y según el rol (K5 §6), pertinencia por estado (K2 §5.7), límite de tasa como no medido (K3
R2.6b, K7 R9.12). Reglas reescritas: todo control trae su caso rojo (K7 R9.8), procedimiento en la voz (K5 §7.5,
K7 R9.11), p95 con modelo frente al corte (K3 R10.2b), acción sin parámetro (K6 §17.4); el id repetido K3 R2.6 se
separó (R2.2b). El banco no midió descubrimiento: el congelado ya traía las lecciones anteriores.
