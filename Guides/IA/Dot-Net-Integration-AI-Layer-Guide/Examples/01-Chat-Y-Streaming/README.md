# 01 · Chat y streaming

**Nivel:** básico · **Guía:** capítulo 6, §6.1–§6.2 · **Objetivo de aprendizaje:** OA-D1 (consumir un modelo con `IChatClient`).

## Qué muestra

- `IChatClient` con el adaptador OllamaSharp, pedido **sólo** a la fábrica de la capa única (`FabricaDeClientes.CrearChatOllama`). La aplicación (`Application/AsistenteDeAyuda.cs`) no sabe qué adaptador hay debajo.
- **Instrucciones** en `ChatOptions.Instructions`, armadas desde `LocalidadPolicy` y `EncuestaPolicy`: si un requisito cambia en las reglas del dominio, cambia lo que recibe el modelo.
- **Historial explícito**: el modelo no recuerda nada. La segunda pregunta («¿Y qué me pide la encuesta…?») sólo se entiende porque la vuelta reenvía pregunta 1, respuesta 1 y pregunta 2.
- **Streaming**: `GetStreamingResponseAsync` escribe cada fragmento apenas llega, y `ToChatResponse()` arma la respuesta completa para el historial y el registro.
- **Razonamiento apagado por la capa de opciones**, no por el ejemplo: `qwen3.5:4b` razona por defecto y, con un presupuesto de salida chico, devuelve `Text` vacío (Sondeo §2). La **variante negativa** lo reproduce con un cliente simulado (`Infrastructure/ModeloQueRazonaPorDefecto.cs`): sin la capa, `Text` vacío y `finish=length`; con la capa, texto y `finish=stop`.

## Cómo se corre

Desde la carpeta de la guía:

```bash
tools/verificar.sh correr 01-Chat-Y-Streaming                # --fake --verificar, sin servidor
tools/verificar.sh correr 01-Chat-Y-Streaming --con-modelo   # contra el servidor de su ejemplo.json (compila antes con «build»)
```

Las pruebas viven en el mismo proyecto (`Pruebas/`): `dotnet test --filter "Category!=LLM"` corre las de cableado sin red; la de `[Category("LLM")]` se salta con el motivo del predicado si no hay modelo.

## Entradas

`entradas.v1.json` (escrito para el ejemplo; congelado por SHA-256 en `ejemplo.json`): dos preguntas, una sobre el alta de una localidad y otra sobre la encuesta, con los campos que cada una espera. Es una **demostración**: no hay tasa.

## Qué observar

Salida con `--fake` (guion de dos respuestas en 5 fragmentos cada una):

```text
> [e01] ¿Qué datos tengo que cargar para dar de alta una localidad?
Para el alta cargá el nombre, la provincia, el código postal y los habitantes.
  oráculo campos-pedidos: pasa (nombra todos los campos; finish=stop); fragmentos: 5; razonamiento: None; num_ctx: 8192 (default)
> [e02] ¿Y qué me pide la encuesta de movilidad?
...
variante negativa (simulada): sin la capa → Text «», finish=length; con la capa → «Nombre, provincia, código postal y habitantes.», finish=stop
```

Con el modelo, las respuestas cambian de redacción; lo que se mira es su forma: nombran los campos y terminan por `stop`, con `razonamiento: None` y el `num_ctx` que fijó el arnés.

- **Oráculo `campos-pedidos`** (`tools/oraculos/campos-pedidos.jq`, recalculable desde el registro): la respuesta nombra cada campo que pide la pregunta (sin mayúsculas ni tildes; «cp» como palabra entera) y la llamada terminó por `stop`.
- **Invariantes de código** (`--verificar` sale con 1 si alguno falla): toda llamada sale con razonamiento `None` y `num_ctx`; las instrucciones van en las opciones y no en el historial; la segunda vuelta reenvía la conversación entera. Con `--fake`, además: 5 fragmentos por respuesta, oráculo en `pasa` y la variante negativa como se describe arriba.

## En este laboratorio

La procedencia de las corridas anteriores a la versión 0.12.0 del libro está en su Anexo H.

**Corrida vigente** (igual al `estado` de `ejemplo.json`): corrida del 2026-10-29 (R-01, estación 1, qwen3.5:4b, Ollama 0.40.11, digest d8b0f5e9760c, num_ctx 8192): 2/2 entradas distintas pasan no-vacia y campos-pedidos, sin tasa; la cancelación contra el modelo no está medida (no hay pieza que cancele con modelo).

La variante negativa no se corre contra el servidor: costaría llamadas fuera del presupuesto de la demostración y, sin la capa, la llamada saldría sin `num_ctx` y podría recargar el modelo de otros clientes.

## El código, paso a paso

Los fragmentos son los que el libro comenta en el capítulo 6; `// …` marca un recorte.

`Examples/01-Chat-Y-Streaming/Application/AsistenteDeAyuda.cs`
```csharp
private readonly List<ChatMessage> _historial = [];

// …
public async Task<VueltaDeChat> PreguntarAsync(string pregunta, Action<string> alFragmento, CancellationToken cancellationToken = default)
{
    _historial.Add(new ChatMessage(ChatRole.User, pregunta));
    var enviados = _historial.ToList();
    var opciones = new ChatOptions { Instructions = Instrucciones, MaxOutputTokens = TopeDeSalida };

    var updates = new List<ChatResponseUpdate>();
    var fragmentos = new List<string>();
    try
    {
        await foreach (var update in cliente.GetStreamingResponseAsync(enviados, opciones, cancellationToken))
        {
            updates.Add(update);
            if (!string.IsNullOrEmpty(update.Text))
            {
                fragmentos.Add(update.Text);
                alFragmento(update.Text);
            }
        }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // Un proveedor corta lanzando la excepción…
    }

    // …y otro termina la enumeración sin lanzar: se mira el token en los dos casos.
    if (cancellationToken.IsCancellationRequested)
    {
        // El parcial se devuelve, pero ni él ni la pregunta quedan en el historial.
        _historial.RemoveAt(_historial.Count - 1);
        return new VueltaDeChat(enviados, opciones, updates.ToChatResponse(), fragmentos, Cancelada: true);
    }

    var respuesta = updates.ToChatResponse();
    // …
    _historial.AddRange(respuesta.Messages);
    return new VueltaDeChat(enviados, opciones, respuesta, fragmentos);
}
```

Qué mirar: `enviados` copia el historial entero antes de cada llamada y `_historial.AddRange` suma la respuesta para la vuelta siguiente; el token se mira después del `foreach`, porque la enumeración puede terminar sin excepción (§6.2 del libro).

**Salida con el modelo.** Observado en el laboratorio: `qwen3.5:4b` (digest `d8b0f5e9760c`), Ollama 0.40.11, estación 1, `num_ctx` 8.192, 2026-10-29; 2 entradas distintas, 2 de 2 pasan el oráculo `campos-pedidos` (recuento, sin tasa). Respuesta a «¿Qué datos tengo que cargar para dar de alta una localidad?»:

```text
Para dar de alta una localidad debes cargar su **nombre** (entre 3 y 60 caracteres, único por provincia), la **provincia** correspondiente, el **código postal** (4 dígitos) y el número de **habitantes**.
```
