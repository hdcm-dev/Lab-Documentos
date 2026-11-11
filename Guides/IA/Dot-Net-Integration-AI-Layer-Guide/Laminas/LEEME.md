# Láminas de la guía de capa de IA

## LD-01 · Aplicación conversacional — arquitectura de referencia

Una sola hoja que da el panorama completo de una aplicación conversacional, desde el usuario con su rol hasta el proveedor de IA. Recorre el turno: interfaz, orquestador, mecanismo, redacción controlada y forma de la respuesta. Muestra además lo que lo gobierna: límites, monitoreo, MCP, seguridad OWASP LLM y el ciclo de diseño. Al pie lleva catorce preguntas conceptuales, cada una con su respuesta breve y las partes a las que se refiere.

- **Definición:** `LD-01-arquitectura-conversacional.def.json`. Tiene 29 partes numeradas en orden, 8 zonas, 36 conexiones rotuladas y la tabla de preguntas.
- **Fuentes:** `../Dot-Net-Integration-AI-Layer-Guide.md`, `../Conversational-Design-Framework-Guide.md` y `../Conocimientos/` (K1–K7), tomadas de origin/main `1403c468e87c16408106ebf80cd6b6d6de1b5e01`.
- **Alcance:** la lámina es genérica. No nombra productos, proveedores ni modelos concretos, ni datos de despliegue.

### Zonas

| Zona | Partes |
|---|---|
| Z1 Usuario | 1 |
| Z2 Interfaz conversacional | 2–5 |
| Z3 Orquestador del turno | 6–9 |
| Z4 Mecanismos | 10–13 |
| Z5 Datos y conocimiento | 14–18 |
| Z6 Respuesta | 19–21 |
| Z7 Proveedores de IA | 22–24 |
| Z8 Gobierno transversal | 25–29 |

## Cómo se genera

1. Toda corrección se hace en el `.def.json`. El SVG nunca se edita a mano (paso 5 del conocimiento).
2. Un compositor lee la definición y escribe el SVG maestro vectorial en A3 apaisado. Usa una sola tipografía sans-serif y un tono de gris por zona. Las conexiones se dibujan con tres trazos: continuo para el turno, a trazos para el gobierno y punteado para el ciclo de diseño. Ese compositor todavía no existe.
3. El SVG se exporta a JPG con calidad 95. Si falta un conversor (cairosvg, rsvg-convert), se usa un contenedor efímero; no se instala nada en el host.
4. La verificación revisa cuatro cosas: la numeración es secuencial y coincide con las zonas, toda conexión y toda pregunta apunta a partes que existen, cada respuesta tiene 25 palabras o menos, y los textos no se solapan.

Por ahora sólo están la definición y la comprobación de su consistencia. Falta el compositor y, con él, el SVG y el JPG.

## Conocimiento aplicado

`Knowledge-Estilo-ilustracion-Lamina.md`, que hereda de `Knowledge-Estilo-ilustracion-tecnica.md` (repo Research.IA-PlanB, `Research.IA-PlanB.Documentacion/Conocimientos/`). La lámina es de tipo **LD**: un diagrama vectorial sin geometría de pieza. Ese conocimiento está en estado `BORRADOR` y nunca se aplicó a un LD, así que esta es su primera aplicación `[PROPUESTA SIN ENSAYAR]`.

## Apartamientos declarados

| # | Regla | Apartamiento | Motivo |
|---|---|---|---|
| A1 | §4: leyenda fija «≈: valor vigente del modelo…» | Se reemplaza por una leyenda propia: la lámina es esquemática y no está a escala, no lleva cifras y explica qué significa cada trazo | No hay medidas ni modelo físico. Una marca `≈` sin cifras no tendría a qué aplicarse |
| A2 | §2: «sin cajas de texto; una tabla de juntas es la única excepción» | Las preguntas conceptuales van como **tabla** (#, pregunta, respuesta breve, partes), no como cajas. La excepción se extiende de «juntas» a «preguntas» | Lo pide el PO, y la tabla es la forma que el conocimiento ya admite |
| A3 | §2: entre 3 y 8 cotas | No hay cotas | Un LD no tiene dimensiones |
| A4 | §5, pasos 1, 3 y 7 (modelo único, render y cámara, revisión contra fotos) | No aplican. La fuente única son las guías, en un SHA fijo | Sin modelo 3D ni fotos. El conocimiento ya prevé que un LD sea vectorial |
| A5 | §5: compositor en `Datasheets/_Comun/` | El compositor, cuando exista, va en esta carpeta o en `../tools/` | Este repo no tiene `Datasheets/` |
| A6 | §3: un tono de gris por parte | Un tono de gris por zona | Con 29 partes, un gris por parte no se distingue. La zona es la unidad que se lee |
| A7 | §2: «Parte N: nombre» con línea guía | Cada parte lleva su número dentro de la caja, sin líneas guía | No hay pieza a la que apuntar |
| A8 | §6: el rótulo lleva el hash del modelo | El rótulo lleva el SHA de las guías fuente | El «modelo» de esta lámina es el texto de las guías |
