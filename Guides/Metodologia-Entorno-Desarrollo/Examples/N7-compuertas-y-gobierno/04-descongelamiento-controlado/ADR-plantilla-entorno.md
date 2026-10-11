# ADR-NNNN — <verbo + qué se descongela> (p. ej. «Subir el SDK de .NET a la banda 10.0.4xx»)

- **Estado**: Propuesto | Aceptado | Reemplazado por ADR-MMMM
- **Fecha**: AAAA-MM-DD
- **Decide**: <rol dueño de la política de entorno, p. ej. líder técnico>
- **Confianza**: alta | media | baja

## Contexto
Qué está congelado hoy (archivo y versión), qué lo obliga a moverse
(fin de soporte, vulnerabilidad, característica necesaria) y desde cuándo.

## Opciones consideradas
1. No moverse (qué riesgo se acepta y hasta cuándo).
2. Mover sólo <X>.
3. Mover <X> y <Y> juntos.

## Decisión
La opción elegida y el compromiso que acepta.

## Superficie del cambio (todo en el mismo PR)
| Archivo | Antes | Después |
|---|---|---|
| global.json | | |
| imagen de CI / devcontainer (digest) | | |
| lock files regenerados | | |

## Verificación
Salida de la compuerta en la imagen nueva (VERDE) y en la vieja (ROJO esperado).

## Consecuencias
Qué tienen que hacer las estaciones (instalar el SDK nuevo, reconstruir el
devcontainer) y cómo se vuelve atrás (revertir el PR completo).
