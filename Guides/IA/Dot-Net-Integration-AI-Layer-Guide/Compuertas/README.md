# Compuertas: qué queda en el repo y qué vive afuera

Las compuertas de privacidad y de dominio (AC-7, AC-8 y la compuerta R-PRIV del build) leen sus listas de una carpeta **fuera del repo**, que se pasa en la variable `COMPUERTAS_DIR`. El repo guarda sólo el formato (este archivo) y `ac6.excepciones`, que no tiene nada privado. Las listas no se versionan porque nombran justo lo que no puede aparecer: equipos, usuarios, recursos del laboratorio, proyectos ajenos. Una compuerta con `COMPUERTAS_DIR` vacía, inexistente o dentro de la carpeta de la guía **falla cerrada** (código 2), salvo la R-PRIV del build, que corre sus clases genéricas y avisa.

## Qué archivos lleva `COMPUERTAS_DIR`

| Archivo | Lo usa | Formato |
|---|---|---|
| `r-priv.lista` | AC-7 (2) y R-PRIV (identificadores) | un término por línea; `#` comenta. AC-7 lo busca como texto fijo sin distinguir mayúsculas; R-PRIV, por palabra entera |
| `r-priv.positivos` | AC-7 y R-PRIV (autoprueba) | una línea por caso; cada una tiene que contener un término de la lista |
| `r-priv.historicos` | AC-7 y R-PRIV | un valor por línea, **por valor exacto**: un valor histórico de un archivo congelado que no se puede cambiar sin crear una `v2`. Se quita de la línea antes de buscar |
| `rpriv.patrones` | R-PRIV (clases privadas) | `clase<TAB>expresión regular extendida`, una por línea |
| `localidades-permitidas.txt` | AC-7 (3) | un nombre de localidad por línea, con un comentario que diga de dónde sale (siembra del caso base, prueba con procedencia o salida del modelo rotulada) |
| `rd-po.lista`, `rd-po.positivos` | AC-8 (c) | nombres de otros proyectos, uno por línea, y sus positivos |
| `rd-po-4.negativos` | AC-8 (autoprueba) | líneas que el barrido **no** puede marcar |
| `rd-po-4.positivos` | AC-8 (autoprueba) | exactamente cinco líneas que el barrido tiene que marcar |
| `rd-po-4.excepciones` | AC-8 | `ruta:línea motivo`, con la ruta relativa a la carpeta de la guía |

## Cómo las arma un clon limpio

1. Crear una carpeta fuera del repo (por ejemplo, junto a los registros de las corridas) y exportarla: `export COMPUERTAS_DIR=<esa carpeta>`.
2. `r-priv.lista`: los nombres de **tus** equipos y usuarios, los nombres de tus contenedores y volúmenes, los de los recursos de tu gestor de turnos y los de cualquier proyecto privado que no deba aparecer. Un `r-priv.positivos` con una línea por término.
3. `rpriv.patrones`: las familias de nombres que no son un término fijo (por ejemplo, el prefijo de tus contenedores seguido de cualquier sufijo). Si no tenés, el archivo puede no existir.
4. `localidades-permitidas.txt`: las localidades sembradas del caso base (`SessionSeeder`) y las de las pruebas con procedencia; después, las que aparezcan en tus registros, cada una revisada a mano (Examples/README, regla de «Datos»).
5. `rd-po.lista` y los tres `rd-po-4.*`: el vocabulario de los dominios ajenos que el caso único no puede mezclar, con sus autopruebas.
6. Correr `tools/compuerta-ac7.sh Examples`, `tools/compuerta-ac8.sh Examples` y `tools/compuerta-rpriv.sh .`: las tres tienen que dar 0 y su autoprueba, completa.

Ninguno de esos archivos se copia al repo ni a la bitácora. Las compuertas informan `ruta:línea`, nunca el texto que encontraron.
