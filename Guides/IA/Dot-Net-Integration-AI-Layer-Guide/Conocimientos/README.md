# Conocimientos de la capa de IA en .NET

Conocimientos para agentes IA (y para personas) que integran una capa de IA en un sistema .NET, nuevo o
existente, trabajando como lo hizo el equipo que diseñó PushDispatch desde cero. Los libros son la fuente para
personas: la guía [`Dot-Net-Integration-AI-Layer-Guide.md`](../Dot-Net-Integration-AI-Layer-Guide.md) y la guía de
diseño conversacional (en redacción). Un conocimiento destila el libro en imperativos con su porqué y su prueba,
remite a él por sección y nunca lo contradice: si choca, rige el libro.

| Id | Conocimiento | Para qué sirve | Cuándo se carga | Estado |
|---|---|---|---|---|
| K1 | [knowledge-AI-Layer-Integration-Designing-Specification.md](knowledge-AI-Layer-Integration-Designing-Specification.md) | Decidir si hace falta IA y especificarla: frontera en anillos, ARQ-1 a ARQ-6, `REQ-IA-nn` con prueba, OWASP | Antes de abrir una rama de código | 1.2 Vigente |
| K2 | [knowledge-AI-Layer-Console-Assistant-Building.md](knowledge-AI-Layer-Console-Assistant-Building.md) | Construir la consola en un panel Blazor: el código elige, el modelo redacta; corpus, destinos, votos, despliegue | Al implementar o tocar la consola | 1.3 Borrador |
| K3 | [knowledge-AI-Layer-Verification-And-Acceptance.md](knowledge-AI-Layer-Verification-And-Acceptance.md) | Aceptar con evidencia: batería ×3 ≥ 90 %, crítico ≥ 8, capturas, sitio real por rol, latencia | Antes de declarar cerrada una fase | 1.5 Vigente |
| K4 | [knowledge-AI-Layer-Integration-Working-Method.md](knowledge-AI-Layer-Integration-Working-Method.md) | Forma de trabajo: fases tipo, mesa con refutador, brecha, PRs por choque, registro de frentes, antipatrones E1–E9 | Al iniciar o retomar una integración | 1.1 Vigente |
| K5 | [knowledge-AI-Layer-Conversational-Design.md](knowledge-AI-Layer-Conversational-Design.md) | Formas de respuesta F1–F8: cifra con nota, desglose, aclaración, abstención, negativa útil, chip de destino | Al escribir o leer respuestas del asistente | 0.11 Borrador provisorio |
| K6 | [knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md) | Detalle de K2: redactar el corpus RAG (frontmatter, fichas, sinónimos, recall@3, errores de ranking), crear funciones con `AIFunctionFactory` y resolver deeplinks del catálogo | Al escribir una página de ayuda, una función o un destino | 1.4 Borrador |
| K7 | [knowledge-AI-Layer-Testing-Strategy.md](knowledge-AI-Layer-Testing-Strategy.md) | Detalle de K3: la suite completa, de unitarias sin modelo y bUnit a adversas, mutación, E2E, batería ×3 y compuertas de CI | Al crear o cambiar pruebas, scripts de batería o la CI | 1.3 Vigente |

**Orden de lectura para un agente nuevo:** K4 §0 y §1.1 → K1 → K5 → K2 → K6 → K3 → K7. K4 gobierna; K5 atraviesa a
los demás.

**Encadenamiento.** K6 detalla K2 (corpus, funciones, destinos) y K7 detalla K3 (cómo se escriben las pruebas que
producen la evidencia). K2 y K3 resumen y remiten; si un detalle de K6 o K7 pareciera chocar con su padre, rige el
padre y se corrige el hijo. Cada regla de K6 trae su prueba, que se escribe con K7.

**Convenciones.** Cabecera con alias, naturaleza, tema, consumidor, condición de carga, hereda de, sustituye,
compatible con, versión, estado y fecha. Anclas estables `kN-…`. Remisiones al libro como `[Guía §x]` con su ancla
`cN-k`. Los antipatrones se citan con prefijo (`K2-AP-n`, `K3-AP-n`, `K5-AP-n`, `K6-AP-n`, `K7-AP-n`) o por su `En` de K4 §8. Son
autocontenidos: los conocimientos del workspace de origen que se nombran como opcionales están resumidos en K2 §16
y K4 §14. Repo público: no se nombran el producto de referencia del mercado, clientes, servidores, direcciones,
usuarios, claves ni el modelo de IA.
