# Ejemplos de la guía: cómo se corren

Cada carpeta `NN-Nombre/` es una consola `net10.0` (o un proyecto de pruebas) que **se compila sola desde su carpeta** y usa lo compartido de `_Comun/` por `ProjectReference` relativa. Todo .NET corre en contenedor (`mcr.microsoft.com/dotnet/sdk:10.0`); en el host alcanzan bash, docker, jq y GNU grep.

## Qué hay acá

| Ruta | Qué es |
|---|---|
| `Directory.Packages.props` | Las versiones de **todos** los paquetes, en un solo lugar, con gestión central y pinning de transitivos. Un `.csproj` nombra el paquete sin versión. |
| `Directory.Build.props` / `.targets` | `Nullable`, `ImplicitUsings`, intermedios fuera del árbol cuando corre el arnés, y el SHA-256 de `Directory.Packages.props` grabado en cada ensamblado (campo `paquetes_hash` del registro). |
| `.gitignore` | `bin/`, `obj/`, `registros/`, `datos/`, `*.db`. |
| `_Comun/MovilidadUrbana.Dominio/` | Dominio y casos de uso de Movilidad Urbana, copiados del caso base (procedencia abajo). |
| `_Comun/MovilidadUrbana.Persistencia/` | EF Core SQLite, repositorios acotados a la sesión, `SessionSeeder` y la siembra de encuestas. |
| `_Comun/MovilidadUrbana.Asistente.Comun/` | Capa única de opciones del proveedor, fábrica de clientes, cliente guionado (`--fake`), cliente espía, embeddings estables (FNV-1a), guarda de truncado, predicado de salto y escritor del registro R-LLM. |
| `_Comun/MovilidadUrbana.Asistente.Comun.Pruebas/` | Pruebas de cableado de `_Comun` (sin red). |
| `_Comun/Humo/` | Consola mínima que cumple el contrato de ejemplo. Sirve de plantilla. |
| `../tools/` | `verificar.sh` (el arnés), `recalcular-oraculos.sh`, las compuertas (`compuerta-ac7.sh`, `compuerta-ac8.sh`, `compuerta-rpriv.sh`, `compuerta-cabeceras.sh`, `compuerta-intervalos.py`, `compuerta-referencias.sh`), el comparador `comparar-ac9.py`, `contar.sh`, `verificar-markdown.py` y `diff-dominio.sh`. |
| `../Compuertas/` | El formato de las listas de las compuertas (que viven fuera del repo, en `COMPUERTAS_DIR`) y las previews admitidas por AC-6. |

## Cómo se corre

Desde la carpeta de la guía (`/LAB/Lab-Documentos/Lab-Documentos/Guides/IA/Dot-Net-Integration-AI-Layer-Guide/`):

```bash
tools/verificar.sh build                       # restore, build -warnaserror, pruebas sin servidor, consolas --fake --verificar
tools/verificar.sh correr 01-Chat-Y-Streaming  # un ejemplo con --fake (compila sólo ese)
tools/verificar.sh correr 01-Chat-Y-Streaming --con-modelo   # el mismo, contra el modelo de su manifiesto
tools/verificar.sh --con-modelo                # las cuatro fases en orden: build → segundo servidor 3a → servidor de chat → segundo servidor 3b
tools/verificar.sh extension [<carpeta>...]    # las extensiones de medición (TURNO-2), contra el servidor de chat
tools/verificar.sh estabilidad                 # las repeticiones de estabilidad (TURNO-1), contra el servidor de chat
tools/verificar.sh embeddings                  # la fase de embeddings (3a), contra el segundo servidor
tools/verificar.sh extension --fake            # cualquiera de las tres vías en seco: turno build, sin red
tools/verificar.sh medidor                     # uso de cada recurso y tope restante del servidor de chat
tools/verificar.sh tasa 8 10                   # 8/10 con su intervalo Clopper-Pearson al 95 %
```

Sin servidor de modelos, cada ejemplo corre con `--fake`: un cliente guionado responde lo que el ejemplo necesita para probar sus garantías de código. Las pruebas que necesitan modelo llevan `[Category("LLM")]` y se saltan con un mensaje si el servidor no responde, si el modelo no está o si no declara la capacidad que hace falta.

Para compilar a mano un solo ejemplo, sin el arnés:

```bash
docker run --rm -u 1000:1000 -e HOME=/tmp -v "$PWD/Examples":/src -w /src/NN-Nombre \
  "${SDK_IMAGEN:-mcr.microsoft.com/dotnet/sdk:10.0}" dotnet run -- --fake
```

(sin `IA_LAYER_ARTEFACTOS`, `bin/` y `obj/` quedan en la carpeta del ejemplo; el `.gitignore` los excluye).

**Contra el modelo, fuera del arnés** (Anexo E.5 de la guía). Con otro Ollama u otro modelo, el arnés no corre, y es a propósito: así tus cifras no se mezclan con las de la guía. Para correr igual, usá el mismo `docker run` sin `--fake` (por ejemplo, `-- --verificar`), pasando por nombre las variables del servidor (`-e OLLAMA_ENDPOINT -e OLLAMA_MODEL -e OLLAMA_SERVIDOR -e OLLAMA_NUM_CTX …`), como hace `correr_con_modelo` en `tools/verificar.sh`. La corrida no compila (`dotnet run --no-build`) si antes no corriste el build. Lo que salga lleva tu rótulo (modelo, digest, versión, fecha) y no se compara con el de la guía.

## Variables

El código lee todo del entorno; ningún archivo del repo tiene una dirección.

**Vocabulario.** Como en el Anexo E.3 de la guía: el **servidor de chat** es el que atiende `OLLAMA_ENDPOINT` (papel `modelo-compartido` en el arnés) y el **segundo servidor** es el de `OLLAMA_PROPIO_*` (papel `modelo-propio`). Los identificadores (`modelo-compartido`, `modelo-propio`, el valor `compartido` de `OLLAMA_SERVIDOR`, la fase `lan`) conservan su nombre histórico porque los citan el código y capturas fijadas por hash.

| Variable | Default | Para qué |
|---|---|---|
| `OLLAMA_ENDPOINT` | `http://localhost:11434` | servidor de chat |
| `OLLAMA_MODEL` | `qwen3.5:4b` | modelo de chat |
| `OLLAMA_SERVIDOR` | `propio` en el código; **sin defecto** en el arnés con modelo | `compartido` · `propio` · `nube`; va al campo `servidor` del registro |
| `OLLAMA_NUM_CTX` | `8192` (origen `default`) | ventana configurada. En las fases con modelo la fija el arnés en 8192, carga el modelo con ese valor dentro del turno y lo comprueba en `/api/ps` (origen `arnes-ps-tras-carga`); con `compartido` y sin ella, la corrida se rechaza |
| `OLLAMA_PROPIO_ENDPOINT` | `http://localhost:11434` | segundo servidor (el nombre de la variable es histórico): chat de 02 y 05, juez de 15 y embeddings |
| `OLLAMA_PROPIO_MODEL` | `qwen3.5:4b` | modelo de chat del segundo servidor (`ConfiguracionDelModelo.cs`) |
| `OLLAMA_EMBED_ENDPOINT` | el de `OLLAMA_PROPIO_ENDPOINT` (nunca el de chat) | servidor de embeddings |
| `OLLAMA_EMBED_MODEL` | `qwen3-embedding:0.6b` | modelo de embeddings |
| `EMB_PLAN` | `EMB-B` | `EMB-A` · `EMB-B` · `EMB-C` (sin modelo de embeddings) |
| `RLLM_DIR` | `./registros` en el código; en el arnés, **obligatoria y fuera del árbol** con modelo, y `${TMPDIR:-/tmp}/guia-ia-registros` en build y en seco | registro R-LLM y `turnos.jsonl` |
| `RLLM_MAQUINA` | `guion` con `--fake`; **sin defecto** en el arnés con modelo | un **rol** (`estacion-1`, `compartido`, `propio`), nunca el nombre del equipo; va al campo `maquina` del registro |
| `TURNO_CMD` | vacío | envoltorio de turno (abajo) |

Variables sólo del arnés: `OLLAMA_PROPIO_URL_CMD` (comando que imprime el endpoint del Ollama propio; se ejecuta dentro del turno), `OLLAMA_PROPIO_SALUD_CMD` (si falla antes del primer turno, rige EMB-C y se registra), `RLLM_MAQUINA_PROPIO` (el rol de la máquina del Ollama propio, por ejemplo `estacion-2`; nunca el nombre del equipo), `TOPE_COMPARTIDO_S` (tope del servidor compartido, 14.400 s), `RLLM_UMBRAL_MS_POR_TOKEN` y `RLLM_CARACTERES_POR_TOKEN` (los calibra la sonda de `num_ctx`), `TURNO_REGISTRO` (registro del gestor de turnos, para el medidor), `TURNO_MAPA` (TSV `papel<TAB>recurso del gestor`, fuera del repo, para que el medidor traduzca los papeles), `TURNO_DESDE` (inicio del ciclo: el medidor ignora lo anterior), `VOLUMEN_NUGET`, `VOLUMEN_ARTEFACTOS` y `ETIQUETA` (volúmenes y etiqueta de docker; defectos neutros `nuget`, `artefactos` y `proyecto=guia-ia`) y `COMPUERTAS_DIR` (las listas de las compuertas, fuera del repo).

**Sin defecto en los modos con modelo.** `fase`, `--con-modelo`, `correr … --con-modelo` y las vías sin `--fake` exigen `OLLAMA_SERVIDOR`, `RLLM_MAQUINA`, `RLLM_MAQUINA_PROPIO` y `RLLM_DIR` (fuera del árbol de la guía); los que tocan el servidor de chat exigen además `OLLAMA_VERSION_ESPERADA`, `DIGEST_4B_ESPERADO` y `DIGEST_EMB_ESPERADO` (`-` si la fase no usa embeddings de ese servidor). Si falta una, el arnés sale con 2 y lo dice, antes de pedir el turno y sin llamar a ningún servidor. Sólo `build` y las corridas en seco usan el rótulo neutro `sin-servidor` y la máquina `guion`.

## Turnos

`verificar.sh` no conoce ningún gestor: si `TURNO_CMD` está definida, cada fase corre como `$TURNO_CMD <recurso> -- <comando>`; si no, corre sin turno y lo dice. Los recursos son **papeles**, no equipos: `build`, `modelo-propio` y `modelo-compartido`; el envoltorio del laboratorio los traduce a los recursos de su gestor. Se piden **de a uno**, en este orden: `build` → `modelo-propio` (fase 3a) → `modelo-compartido` → `modelo-propio` (fase 3b). Ninguna fase pide un segundo recurso mientras tiene uno. Cada fase agrega una línea a `RLLM_DIR/turnos.jsonl` (`turno_id`, `recurso`, `fase`, `pedido_en`, `concedido_en`, `liberado_en`, `duracion_s`, `servidor`, `maquina`); antes de cada ejemplo contra el servidor de chat, el arnés le pasa como `--plazo-s` lo que queda del tope. El tope es para un servidor **ajeno**, que comparten otros: con un Ollama propio y sin gestor no hace falta. La fase del servidor de chat se sigue llamando `lan` (`tools/verificar.sh fase lan`): es un nombre histórico, que no se cambia porque lo citan capturas fijadas por hash.

**Medidor.** El tope restante es el tope menos lo usado. Si `TURNO_REGISTRO` apunta al registro del gestor, lo usado sale de ahí. El registro es TSV append-only: fecha ISO-8601, recurso, dueño, evento y motivo. El medidor suma cada tramo `concedido` → `liberado`, `vencida`, `huerfana` o `revocado` del mismo recurso y dueño, y una concesión abierta cuenta hasta ahora. Así entran también las corridas hechas a mano, fuera del arnés. De `turnos.jsonl` se suman sólo las fases que corrieron sin turno, porque las demás ya están en el gestor. Sin `TURNO_REGISTRO`, se suma sólo `turnos.jsonl`, y el medidor subestima lo que se corrió fuera del arnés. `tools/verificar.sh medidor` muestra las dos fuentes y el resultado por recurso. Los tramos `vencida` y `huerfana` se cierran cuando el gestor los recupera, que es después del vencimiento real: el error queda del lado seguro del tope.

En la fase del servidor de chat, dentro del turno y antes de la primera medición, el arnés (1) lee `/api/version` y `/api/tags` y compara la versión y los digests con `OLLAMA_VERSION_ESPERADA`, `DIGEST_4B_ESPERADO` y `DIGEST_EMB_ESPERADO`: deja una fila `servidor-compartido` en `turnos.jsonl` y, si algo no coincide, no mide («no medido»); (2) lee `/api/ps`: si lista **otro** modelo, aborta sin llamar, para no recargar un servidor que usan otros; (3) carga el modelo con `num_ctx` 8192 (una llamada de calentamiento), vuelve a leer `/api/ps` y registra lo que informa (fila `num-ctx`); si no es 8192, no mide. La fase del segundo servidor carga su modelo de chat igual. Al cerrar cada fase, el arnés descarga los modelos que usó (`keep_alive` 0).

## Vías: extensión, embeddings y estabilidad

Algunas piezas tienen una corrida más que la del ejemplo (plan §E.1 y §E.5). Cada una se declara en `ejemplo.json` con su clave:

| Clave | Quién la declara | Recurso y momento | Qué corre |
|---|---|---|---|
| `extension` | 03, 04, 08 | `modelo-compartido`, TURNO-2, después de los ejemplos de ese turno | las 8 entradas de medición (`--verificar --extension`) |
| `estabilidad` | 07 | `modelo-compartido`, TURNO-1, después de los ejemplos de ese turno | 3 repeticiones de una entrada, que no cuentan para la tasa (`--verificar --estabilidad`) |
| `embeddings` | 07 | `modelo-propio`, fase 3a, después de los ejemplos de esa fase | vectoriza el corpus y las preguntas y los deja en `RLLM_DIR/embeddings/` (`--precomputar-embeddings`) |

```json
"extension": { "turno": "TURNO-2", "pesimista_s": 640,
               "comando": "dotnet run --project 03-Salida-Estructurada -- --verificar --extension" }
```

- El arnés toma los argumentos de la consola de `comando`: lo que sigue a ` -- `, sin `--fake`. El recurso es el de la vía (`recurso`) o, si no lo dice, el del manifiesto.
- `pesimista_s` es el pesimista de §E.5. Contra el servidor de chat rige la **regla de cola**: una vía cuyo pesimista no entra entero en el tope restante no se empieza y queda «no ejecutado contra el modelo».
- `tools/verificar.sh fase lan` corre las vías de cada turno. `fase 3a` corre la de embeddings. `tools/verificar.sh <vía> [<carpeta>...]` corre una vía sola, con su propio turno.
- Con `--fake`, la vía corre **en seco**: un turno `build`, compilación con `-warnaserror` y la consola con `--fake` y los argumentos de la vía, con los tres endpoints en un puerto cerrado. El registro va aparte, a `RLLM_DIR/seco/`, para que un archivo del guion nunca quede donde lo lee una corrida que cuenta. En seco, los embeddings de 07 salen del generador estable con `modelo` «guion». La corrida de chat de 07 rechaza un archivo así.
- La demo no entra: sus embeddings (PD-2) se corren con `tools/demo.sh embeddings`, en el turno `modelo-propio`.

## El contrato de cada ejemplo

1. **Carpeta** `Examples/NN-Nombre/` con un único `.csproj` (`net10.0`), su `README.md` (qué muestra, objetivo de aprendizaje, comando, salida esperada) y su `ejemplo.json`:

   ```json
   { "recurso": "ninguno | modelo-propio | modelo-compartido",
     "turno": "TURNO-1 | TURNO-2 | TURNO-3",
     "fase": "3a | 3b",
     "entradas": [ { "ruta": "entradas.v1.json", "sha256": "<sha256 del archivo>" } ],
     "comando": "dotnet run --project NN-Nombre -- --fake --verificar",
     "extension | estabilidad | embeddings": { "turno": "…", "fase": "…", "recurso": "…", "pesimista_s": 0, "comando": "…" } }
   ```

   `fase` sólo vale para `modelo-propio` (por defecto `3a`). Las vías son opcionales (sección anterior). Si el SHA-256 de una entrada no coincide, el arnés no corre el ejemplo: cambiar una entrada congelada crea `v2`.
2. **Referencias** relativas a `_Comun`: `../_Comun/MovilidadUrbana.Dominio/MovilidadUrbana.Dominio.csproj`, `../_Comun/MovilidadUrbana.Persistencia/…` (si usa la base) y `../_Comun/MovilidadUrbana.Asistente.Comun/…`. Paquetes **sin** `Version`; un paquete nuevo se agrega a `Directory.Packages.props`.
3. **Cliente**: siempre por `FabricaDeClientes.CrearChatOllama(...)` o `FabricaDeClientes.Envolver(cliente, config, adaptador)`, también en `--fake` y para el juez y MAF. Nadie construye su propio `OllamaApiClient` ni fija razonamiento o `num_ctx` por su cuenta.
4. **Argumentos**: `ArgumentosDeEjemplo.Leer(args)` (`--fake`, `--verificar`, `--plazo-s`). Con `--verificar`, sale con 0 sólo si se cumplen todos los invariantes de código. Al vencer el plazo, termina la llamada en curso y marca las entradas que faltan `excluida` con motivo «plazo».
5. **Registro R-LLM**: un `EscritorDeRegistro` por corrida (escribe `RLLM_DIR/<ejemplo>.jsonl`); por cada llamada, `registro.Escribir(registro.Llamada(...))`, y por cada entrada un `RegistroDeEntrada` con el nombre del oráculo y `pasa`, `falla` o `excluida` con su motivo. Una llamada con `truncado_posible = true` excluye la entrada. El oráculo se escribe también como `../tools/oraculos/<nombre>.jq`, para que `recalcular-oraculos.sh` lo recalcule desde el registro.
6. **Datos**: sólo Movilidad Urbana. Lo que no sale del caso base se rotula «escrito para el ejemplo», usa valores de `Catalogos` y nombres «Persona 01…20». Un nombre de localidad nuevo en un registro se agrega a `localidades-permitidas.txt` en `COMPUERTAS_DIR` (formato en `../Compuertas/README.md`). **Alcance**: la regla vale para lo que escribe el autor (entradas, oro, corpus, siembras, guiones de `--fake`). La salida del modelo no se corrige ni se filtra para cumplirla: si el modelo inventa una localidad o una provincia fuera de `Catalogos`, es una conducta que el ejemplo muestra (las reglas del dominio la rechazan), no un dato del ejemplo. La salida sí pasa la compuerta AC-7: si un nombre inventado cae en un campo «Nombre» del registro y la compuerta lo marca, se revisa que no sea de una persona y recién entonces se agrega a la lista, rotulado como salida del modelo.

## Compuertas

```bash
COMPUERTAS_DIR=<carpeta> tools/compuerta-ac7.sh Examples   # direcciones privadas, términos privados y nombres en los registros
COMPUERTAS_DIR=<carpeta> tools/compuerta-ac8.sh Examples   # dominios y proyectos ajenos
COMPUERTAS_DIR=<carpeta> tools/compuerta-rpriv.sh .        # R-PRIV con patrones (también corre en el build)
tools/compuerta-cabeceras.sh "$RLLM_DIR"                   # ninguna cabecera ni credencial serializada en los cuerpos capturados
tools/recalcular-oraculos.sh "$RLLM_DIR"/*.jsonl           # cada oráculo, recalculado desde el registro
tools/diff-dominio.sh                                      # Domain/ de _Comun idéntico al de la demo
```

Las listas viven **fuera** del repo, en `COMPUERTAS_DIR`, cada una con su archivo `.positivos` al lado; `../Compuertas/README.md` explica su formato y cómo las arma un clon limpio. Las compuertas fallan cerradas (código 2) si falta la variable, el archivo o sus líneas, o si la autoprueba no marca todos los positivos. Informan `ruta:línea`, nunca el texto encontrado.

## Procedencia

El dominio y la persistencia son copia de `Lab-E2E.WebBlazor/src/MovilidadUrbana.ApiWeb/` en el commit `8fb4c3a`. Cada archivo copiado empieza con `// Procedencia: <ruta> @ 8fb4c3a` y, si cambió, `// Delta: <columna> — <motivo>`; los archivos sin copia dicen «Nuevo». Cambios frente a la copia:

| Cambio | Columna | Motivo |
|---|---|---|
| Namespace `MovilidadUrbana.ApiWeb` → `MovilidadUrbana.Asistente` | Modificado por otro motivo | una sola raíz para la demo y los ejemplos |
| `LocalidadRules.ProvinciaValida`: clave exacta de `Catalogos.Provincias` (antes, «no vacío») | Modificado a causa de la IA | un modelo que propone texto puede devolver «Corrientes Capital» |
| `EncuestaRules.MedioValido`, `MediosValidos`, `FrecuenciaValida`, `MotivoValido`; `ValidarPaso` los usa | Modificado a causa de la IA | antes `["avion"]` pasaba la validación y se registraba como lista vacía |
| `LocalidadService.GetByNombreYProvinciaAsync` (error si es ambiguo) y `SaveAsync` informa el id creado (`Result.Id`) | Modificado a causa de la IA | la baja por herramienta resuelve la localidad en la sesión y el alta informa lo creado |
| `IEncuestaRepository.GetAllAsync` y `ResultadosDeEncuestaService` | Agregado por la IA | el caso base no tiene consulta de resultados |
| Siembra de 20 encuestas por sesión (`SiembraDeEncuestas`), «escrito para el ejemplo» | Agregado por la IA | estímulo determinista para los ejemplos de resultados |
| EF Core Sqlite 10.0.11 → 10.0.14; sin `GenerateDocumentationFile` ni `NoWarn` | Modificado por otro motivo | versiones centrales; el build no admite supresiones |

Cifras que salen de la siembra: estímulo del mismo autor.
