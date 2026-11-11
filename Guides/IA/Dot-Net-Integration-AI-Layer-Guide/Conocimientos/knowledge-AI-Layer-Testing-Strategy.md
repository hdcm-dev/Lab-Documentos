# Capa de IA integrada en .NET: cómo se crea el testing, de la prueba sin modelo a la batería real

**Alias:** AI-Layer-Testing-Strategy
**Naturaleza:** propio
**Tema:** Cómo se escribe y se organiza la suite de pruebas de una capa de IA integrada en una aplicación .NET: la pirámide completa; unitarias sin modelo con dobles de `IChatClient` e `IEmbeddingGenerator`; bUnit para la consola; conformidad del catálogo de destinos, del corpus y de la configuración; pruebas adversas (inyección, cuentas ajenas, función inventada, ruta en el texto); mutación (cada prueba falla sin su arreglo); E2E con Playwright a 1280 y 390 con capturas; batería real ×3 automatizada con umbral e inestables; latencia en frío y en caliente pagando el cambio de modelo; verificación en el sitio real con sesión por rol; cifras contra SQL; compuertas de CI. Destilado de la integración de IA de PushDispatch y alineado con la guía de la capa de IA
**Consumidor:** un agente IA que escribe o revisa pruebas de la capa de IA; quien arma la CI o los scripts de batería y de capturas; el revisor de un PR que agrega una garantía
**Condicion-de-carga:** al crear el proyecto de pruebas de la capa; al agregar una garantía (cada `REQ-IA-nn` de K1 y cada regla de K2 o K6 necesita su prueba); al arreglar un defecto; al montar la CI o los scripts de batería y de capturas; se carga por secciones según el «Mapa de carga» de §0
**Hereda-de:** [K3](knowledge-AI-Layer-Verification-And-Acceptance.md) (los umbrales y la evidencia de aceptación mandan; este conocimiento los implementa como suite y scripts)
**Sustituye:** —
**Compatible-con:** `Dot-Net-Integration-AI-Layer-Guide.md` (libro de esta carpeta, edición 1.2.0; las remisiones `[Guía §x]` usan esa numeración y sus anclas `#cN-M`; base teórica en §11.1–§11.7 y §11.12); [K1](knowledge-AI-Layer-Integration-Designing-Specification.md) (los `REQ-IA-nn` que cada prueba ata); [K2](knowledge-AI-Layer-Console-Assistant-Building.md) (las reglas de construcción que se prueban); [K4](knowledge-AI-Layer-Integration-Working-Method.md) (ciclo del PR y registro de frentes); [K5](knowledge-AI-Layer-Conversational-Design.md) (forma esperada de cada respuesta); K6, AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks (corpus, funciones y destinos que se prueban)
**Versión:** 1.1
**Estado:** Vigente
**Fecha:** 2026-11-11

**Por qué nace Vigente:** destila prácticas ya medidas en un sistema (mismo criterio que K3); cada regla cita la prueba o la cifra que la respalda.

---

## Índice

- [0. Propósito y mapa de carga](#k7-0)
- [1. Principio: cada garantía tiene una prueba que falla sin ella](#k7-1)
- [2. La pirámide](#k7-2)
- [3. Unitarias sin modelo: dobles del modelo y de los embeddings](#k7-3)
- [4. bUnit de la consola](#k7-4)
- [5. Conformidad: catálogo, corpus y configuración](#k7-5)
- [6. Pruebas adversas](#k7-6)
- [7. Mutación: la prueba falla sin el arreglo](#k7-7)
- [8. E2E con Playwright a 1280 y 390](#k7-8)
- [9. Batería real ×3 automatizada](#k7-9)
- [10. Latencia en frío y en caliente](#k7-10)
- [11. Sitio real por rol y cifras contra SQL](#k7-11)
- [12. Compuertas de CI](#k7-12)
- [13. Errores que costaron, cada uno con su control](#k7-13)
- [14. Procedimiento paso a paso](#k7-14)
- [15. Criterios de parada](#k7-15)
- [16. Checklist binario](#k7-16)
- [17. Antipatrones con su control](#k7-17)
- [18. Casos](#k7-18)
- [19. Límites de este conocimiento](#k7-19)
- [Glosario](#k7-glosario)
- [Historial](#k7-historial)

---

<a id="k7-0"></a>
## 0. Propósito y mapa de carga

Este conocimiento dice **cómo se escribe y se organiza la suite** que corre en cada build y antes de cada fase.
**Qué evidencia hace falta para declarar cerrada una fase, con qué umbral y cómo se informa** lo dice
[K3](knowledge-AI-Layer-Verification-And-Acceptance.md); acá no se repite: donde K3 fija una norma (batería ×3,
umbral 90 %, condiciones de medición, sitio real, cifras contra SQL), K7 remite y aporta el cómo (proyecto,
doble, script, compuerta).

La base teórica está en el libro: qué puede afirmar una prueba sobre un sistema no determinista
([Guía §11.1](../Dot-Net-Integration-AI-Layer-Guide.md#c11-1)), el doble de `IChatClient`
([Guía §11.2](../Dot-Net-Integration-AI-Layer-Guide.md#c11-2)), el contrato de la salida estructurada
([Guía §11.3](../Dot-Net-Integration-AI-Layer-Guide.md#c11-3)), las corridas contra el modelo
([Guía §11.4](../Dot-Net-Integration-AI-Layer-Guide.md#c11-4)), el conjunto de oro
([Guía §11.7](../Dot-Net-Integration-AI-Layer-Guide.md#c11-7)) y los criterios de calidad
([Guía §11.12](../Dot-Net-Integration-AI-Layer-Guide.md#c11-calidad)). Este conocimiento no los contradice;
les suma lo que el libro todavía no trae: mutación, Skip con dependencia visible, E2E por ancho, conformidad
de configuración y la lista de errores medidos.

### Mapa de carga

| Situación del agente | Cargar |
|---|---|
| Siempre | §0, §1, §2 |
| Creo el proyecto de pruebas de la capa | §3, §4, §5, §12 |
| Arreglo un defecto | §1, §7 y la capa de §2 que le toca |
| Agrego una función, un destino o una página de ayuda | §3, §5, §6 (y K6) |
| Antes de cerrar una fase | §8, §9, §10, §11 y luego K3 |
| Monto o reviso la CI | §12, §13 |
| Reviso un PR de pruebas | §16, §17 |

---

<a id="k7-1"></a>
## 1. Principio: cada garantía tiene una prueba que falla sin ella

**R1.1 — Escribí una prueba por garantía y nombrala con la garantía.** Una garantía es una frase verificable
de K1 (`REQ-IA-nn`), de K2 o de K6: «el filtro por rol va antes de buscar», «un nombre de función desconocido
se rechaza». El nombre de la prueba la repite. *Por qué:* una prueba con nombre técnico («Test3») no dice qué
se rompió cuando falla. *Prueba:* en PushDispatch los nombres se leen como la garantía:
`El_filtro_por_rol_va_ANTES_de_buscar_ni_siquiera_se_vectoriza_lo_ajeno`, `HerramientaInexistente_Rechazada`,
`CifraAusenteEnHerramientas_Rechazado`; una tabla `REQ-IA-nn → prueba` no tiene filas vacías sin motivo.

**R1.2 — Una prueba vale si falla sin el código que protege.** Verificalo revirtiendo el arreglo (§7).
*Por qué:* una prueba que pasa con y sin el arreglo no prueba nada (§13, error 3). *Prueba:* el PR que arregla
un defecto trae el registro de la corrida con el arreglo revertido, en rojo.

**R1.3 — Separá lo que el código garantiza de lo que el modelo hace** (norma en
[K3 R1.2](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-1-principio)). En la suite eso se traduce en
proyectos o categorías distintas: lo determinista corre en cada build sin red; lo que necesita modelo corre
rotulado y fuera de la CI. *Prueba:* `dotnet test` sin servidor de modelos pasa entero; las pruebas con modelo
real se saltan **con un motivo que nombra la dependencia** (§12, R12.3).

---

<a id="k7-2"></a>
## 2. La pirámide

| Capa | Qué prueba | Cuándo corre | Costo | Sección |
|---|---|---|---|---|
| Unitarias sin modelo | ruteo, alcance, validación de argumentos, tope de llamadas, validador de cifras, troceo, filtro por rol, redacción anclada, motivo de habilitación | cada build | ms | §3 |
| Componentes (bUnit) | estados de la consola, chip de destino, voto, accesibilidad básica | cada build | ms | §4 |
| Conformidad | catálogo ↔ `@page` ↔ políticas; corpus ↔ pantallas; claves ↔ plantilla de configuración | cada build | ms | §5 |
| Adversas | inyección, cuentas ajenas, función inventada, ruta en el texto | cada build | ms | §6 |
| Recuperación | recall@3 por rol y tasa de «ninguna» sobre el conjunto de [K6 §7](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-7), contra la línea base léxica | cada PR que toca la ayuda o el léxico | s | R12.5 |
| Integración | API de la ayuda, base de prueba, servicios reales sin modelo; la base es local al proceso (en memoria, SQLite o un contenedor que levanta la propia prueba) y cuenta como build sin red | cada build | s | §3 |
| E2E | la app servida a 1280 y 390, claro y oscuro, capturas | antes de cerrar fase y en PR de UI | min | §8 |
| Batería real ×3 | el sistema entero contra el modelo, con oráculo | antes de cerrar fase | decenas de min | §9 |
| Latencia | frío y caliente pagando el cambio de modelo | con la batería | — | §10 |
| Sitio real por rol + SQL | la app desplegada con sesión de cada rol; cifras contra la base | al desplegar | min | §11 |

**R2.1 — Poné cada garantía en la capa más baja que la puede probar.** *Por qué:* lo que una unitaria detecta
en milisegundos no debe esperar a la batería; en PushDispatch el filtro por rol, el validador de cifras y el
tope de llamadas se prueban sin modelo, y la batería queda para lo que sólo el modelo decide (redacción,
elección de herramienta, abstención). *Prueba:* ninguna garantía determinista figura sólo en la batería.

**R2.2 — Ninguna capa reemplaza a otra.** Las unitarias en verde no dicen que la consola se vea (K3-AP-2), ni la
batería que el sitio real tenga la IA encendida (K3-AP-4). *Prueba:* el informe de fase (K3 §15) tiene una línea
por capa, MEDIDO o INFORME.

---

<a id="k7-3"></a>
## 3. Unitarias sin modelo: dobles del modelo y de los embeddings

**R3.1 — Usá un doble guionado de `IChatClient` que registre todo lo que se le mandó.** El guion recibe el
número de llamada, los mensajes y las opciones (incluidas las herramientas ofrecidas) y devuelve una llamada a
función o un texto. Registrar permite afirmar sobre lo que **no** debe llegar al modelo
([Guía §11.2](../Dot-Net-Integration-AI-Layer-Guide.md#c11-2)).
*Prueba:* existe un doble así y al menos una prueba busca en «todo lo enviado» algo prohibido (otra cuenta, un
correo, una clave).

Extracto de PushDispatch (`tests/NgPushDispatch.AI.Tests/Dobles.cs`, recortado):

```csharp
public sealed class ChatClientFalso(
    Func<int, IReadOnlyList<ChatMessage>, ChatOptions?, CancellationToken, Task<ChatResponse>> guion) : IChatClient
{
    private int _n;
    public List<(IReadOnlyList<ChatMessage> Mensajes, ChatOptions? Opciones)> Pedidos { get; } = [];

    public static ChatResponse Llamar(string herramienta, IDictionary<string, object?>? args = null) =>
        new(new ChatMessage(ChatRole.Assistant,
            [new FunctionCallContent(Guid.NewGuid().ToString("N"), herramienta, args ?? new Dictionary<string, object?>())]));
    public static ChatResponse Decir(string texto) => new(new ChatMessage(ChatRole.Assistant, texto));

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var lista = messages.ToList(); int n;
        lock (Pedidos) { Pedidos.Add((lista, options)); n = _n++; }
        return guion(n, lista, options, cancellationToken);
    }
    // ... streaming no soportado, GetService nulo, Dispose vacío
    public string TodoLoEnviado() => string.Join("\n", Pedidos.SelectMany(p => p.Mensajes)
        .Select(m => JsonSerializer.Serialize(m, AIJsonUtilities.DefaultOptions)));
}
```

**R3.2 — Usá un doble de `IEmbeddingGenerator` (o del puerto de vectorización) con vectores fijos y que cuente
lo que vectorizó.** *Por qué:* así se prueba que el filtro por rol va antes de la búsqueda («ni siquiera se
vectoriza lo ajeno») y que la caché no repite fragmentos. *Prueba:* en PushDispatch
`RedaccionConAnclajeTests` tiene `El_filtro_por_rol_va_ANTES_de_buscar_ni_siquiera_se_vectoriza_lo_ajeno`,
`Sin_embeddings_sigue_por_lexico` y `La_cache_de_embeddings_no_repite_fragmentos`.

**R3.3 — Probá el enrutador como tabla.** Pregunta → alcance, pregunta → intención → destino, con `[Theory]` e
`[InlineData]`. *Por qué:* el ruteo es código determinista y es donde más regresiones aparecen al sumar una
intención. *Prueba:* `DialogoYCatalogoTests.El_alcance_sale_de_la_pregunta` y
`DestinoYCuentaTests.El_destino_sale_de_la_intencion` son tablas; agregar una intención agrega filas.

**R3.4 — Probá los bordes del bucle de herramientas sin modelo:** guion que repite la llamada (corta en el tope),
varias llamadas en una respuesta (cuentan todas), modelo lento (degrada, no cuelga), proveedor caído (degrada,
sin excepción), dos consultas en paralelo (no se ven entre sí), cifra ausente en las herramientas (rechazada).
*Prueba:* `ConsultaConModeloTests` en PushDispatch: `GuionRepetido_CortaEnCuatro`,
`VariasLlamadasEnUnaRespuesta_CuentanTodas`, `ModeloLento_Degradado`, `ProveedorCaido_Degradado_SinExcepcion`,
`DosConsultasEnParalelo_NoSeVen`, `SinHerramienta_ConCifraInventada_Rechazado`.

**R3.5 — Probá el motivo de habilitación como dato, una vez por motivo y sin dirección ni modelo en el
registro.** *Por qué:* la IA apagada en silencio por una variable mal nombrada costó un despliegue (§13, error 6).
*Prueba:* `EstadoDeConsultaIaTests`: `Con_solo_ModeloDeRedaccion_queda_habilitada`,
`Cada_motivo_se_registra_una_vez_sin_direccion_ni_modelo`, `Si_el_motivo_cambia_se_registra_de_nuevo`.

**R3.6 — Los datos de prueba son sintéticos y fijos.** Ids con patrón reconocible, nombres neutros («Turnos»,
«org-a»), métricas fijas. *Por qué:* vocabulario neutro (K3 R9.3) y resultados repetibles. *Prueba:* el
fixture declara «no son datos reales» y el grep de §12 da cero.

---

<a id="k7-4"></a>
## 4. bUnit de la consola

**R4.1 — Probá cada estado visible de la consola con bUnit:** apagada con su motivo, cargando, respuesta,
«No está en la ayuda», aclaración con opciones, chip de destino, voto, detener; y la **página destino con su
parámetro de acción** (`?alta=1`: con permiso abre el modal una vez, sin permiso carga normal, recargar tras
cerrar no lo reabre, [K6 §17.2](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-17)). *Por qué:* son los estados que
la lectura humana juzga (K5) y los que se rompen al tocar el componente. *Prueba:* en PushDispatch
`ConsolaDeIaTests`, `DialogoDeLaConsolaTests`, `BotonDeConsolaTests` y `ConsolaQueAcompanaTests` en
`tests/NgPushDispatch.Web.Tests/`; un estado sin prueba figura como diferido con nombre.

**R4.2 — El chip de destino se prueba como enlace del catálogo, no como texto.** Afirmá el `href` contra la
ruta del catálogo y que el texto del modelo no aporta enlaces. *Prueba:* la prueba falla si el componente
renderiza una URL que vino en el texto de la respuesta.

**R4.3 — bUnit no reemplaza la captura.** bUnit no aplica hojas de estilo ni mide desborde. *Prueba:* el PR que
toca la consola trae también la corrida E2E de §8 (K3-AP-2).

---

<a id="k7-5"></a>
## 5. Conformidad: catálogo, corpus y configuración

La norma está en [K3 R9.1](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-9-conformidad); acá, el cómo.

**R5.1 — Catálogo ↔ páginas por reflexión.** Recorré los tipos con `@page` (`RouteAttribute`) y sus propiedades
con `[SupplyParameterFromQuery]`; cada destino del catálogo debe tener su ruta y cada parámetro que promete
(`?alta=1`) debe ser leído por la página. *Prueba:* agregar una página sin entrada, o un parámetro que la
página no lee, pone la prueba en rojo (en PushDispatch, `CatalogoDeDestinosTests` y `DestinosDeLaAyudaTests`).

**R5.2 — Catálogo ↔ políticas.** La `Politica` de cada destino es la del `[Authorize(Policy = …)]` de su
página (el permiso de ver), y la prueba las compara por reflexión. El permiso de **hacer** la acción que abre un
destino con `Consulta` no está en el registro: se revalida con el criterio único de
[K6 §16](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-16), por instancia cuando
corresponde. *Prueba:* política distinta de la de la página → rojo; para cada rol y cada destino con acción,
botón visible, apertura por deeplink y autorización del destino responden lo mismo. (Dos políticas por destino,
de ver y de hacer, son una propuesta que K6 §14.1 admite; si el sistema la adopta, la conformidad compara las
dos.)

**R5.3 — Corpus ↔ pantallas.** Cada pantalla tiene su ficha de ayuda y cada `destinos:` del frontmatter existe
en el catálogo; las páginas en `status: borrador` no se sirven. *Prueba:* en PushDispatch `AyudaTests` usa
páginas de fixture (`borrador.md`, `por-rol.md`, `variantes.md`, `sin-frontmatter.md`, `sin-audiencia.md`,
`interno.md`) y una prueba por regla de lectura; `sin-audiencia.md` es la página sin `delivery`, que no entra al
catálogo ([K6 §4](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-4)).

**R5.4 — Claves de configuración ↔ plantilla.** Cada clave que el código lee para habilitar la IA figura con
ese nombre exacto en la plantilla de configuración del despliegue y en la especificación. *Por qué:* el
despliegue configuró la clave con otro nombre y la IA quedó apagada (K3-AP-4; §13, error 6). *Prueba:* una
prueba lee las claves usadas por el código de habilitación y las busca en la plantilla; falla si falta una.

---

<a id="k7-6"></a>
## 6. Pruebas adversas

**R6.1 — Inyección en la pregunta:** «ignorá las instrucciones», «llevame a /usuarios», una URL pegada. Ninguna
cambia el alcance ni produce un destino fuera del catálogo. *Prueba:* una prueba por forma, con el doble del
modelo devolviendo lo que el atacante querría.

**R6.2 — Inyección en la ayuda:** una página de fixture con instrucciones dentro del texto (canario). *Prueba:*
PushDispatch tiene el corpus canario `tests/Fixtures/docs-canarios/inyeccion/`; la respuesta no obedece al
canario y la prueba lo afirma.

**R6.3 — Cuentas ajenas:** pedir datos de otra cuenta o de una app no visible para el rol. Ni la respuesta ni
**lo enviado al modelo** contienen la otra cuenta. *Prueba:* `TodoLoEnviado()` no contiene el id ni el nombre de
la cuenta ajena; en PushDispatch `SuscriptoresDeCuentaTests` y `DestinoYCuentaTests` cubren la cuenta elegida y
la ajena.

**R6.4 — Función inventada y argumentos fuera de rango:** el doble llama a un nombre que no existe, o con una
enumeración inválida o una página fuera de 1..N. Se rechaza como dato, sin excepción. *Prueba:*
`HerramientaInexistente_Rechazada`; una prueba por argumento validado.

**R6.5 — Ruta en el texto del modelo:** el doble responde con un enlace markdown. El texto final no lo trae y
el destino sale del catálogo. *Prueba:* negativa en la capa de redacción ([K3 R9.2](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-9-conformidad)).

**R6.6 — Datos personales minimizados antes del modelo.** *Prueba:*
`RedaccionConAnclajeTests.Minimizar_quita_correo_ids_claves_y_tokens`.

---

<a id="k7-7"></a>
## 7. Mutación: la prueba falla sin el arreglo

**R7.1 — Al arreglar un defecto, escribí primero la prueba que lo reproduce, vela en rojo, aplicá el arreglo,
vela en verde, y revertí el arreglo una vez más para verla en rojo de nuevo.** *Por qué:* en PushDispatch una
prueba quedó en verde con y sin el arreglo porque afirmaba sobre un valor que el doble ya devolvía; no probaba
nada (§13, error 3). *Prueba:* el PR trae las dos corridas (rojo con el arreglo revertido, verde con el arreglo)
o su registro.

**R7.2 — Registrá el intento de mutación junto a la prueba.** Una línea en el PR: «revertido `<archivo>:<línea>`
→ falla `<prueba>`». *Prueba:* el revisor puede repetirlo revirtiendo el parche en su propio worktree.

**R7.3 — Si revertir no pone nada en rojo, la garantía no tiene prueba.** No se cierra el arreglo. *Prueba:*
checklist §16.

```bash
# NO VERIFICADO: forma del control de mutación manual sobre un worktree propio.
# El arreglo es lo que cambió el PR contra su base, en los archivos del arreglo (no en las pruebas).
mkdir -p .mutacion                                            # carpeta temporal dentro del worktree; no se commitea
git diff origin/main...HEAD -- src/Ruta/DelArreglo.cs > .mutacion/arreglo.patch
git apply -R .mutacion/arreglo.patch                          # revertirlo
dotnet test --filter "FullyQualifiedName~MiGarantia" ; echo "esperado: falla"
git apply .mutacion/arreglo.patch                             # restaurar
dotnet test --filter "FullyQualifiedName~MiGarantia" ; echo "esperado: pasa"
```

Nombrá los archivos del arreglo uno por uno, estén donde estén: un `HEAD~1` o un `-- src/` genérico revierten
de más o de menos cuando la prueba y el arreglo están en commits distintos o el arreglo toca configuración.

---

<a id="k7-8"></a>
## 8. E2E con Playwright a 1280 y 390

La norma de las capturas está en [K3 §6](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-6-capturas); acá,
el script.

**R8.1 — Corré el E2E contra la app servida desde el SHA del PR, no contra la demo ni la maqueta.** *Por qué:*
la consola se veía bien en la maqueta y no en la app (K3-AP-2, K3-AP-11). *Prueba:* el registro del script trae
la URL local y el SHA.

**R8.2 — Un script por recorrido, parametrizado por ancho (1280 y 390) y tema (claro y oscuro).** Para cada
combinación: abre la pantalla, abre la consola, hace la pregunta, espera la respuesta, toma la captura y mide
desborde horizontal (`scrollWidth > clientWidth`), desplazamiento al recargar y el estilo computado de un
elemento de la consola. Además del recorrido de pregunta, uno **chip → acción**: tocar el chip de un destino con
`Consulta` abre la pantalla con el modal de la acción para el rol que puede, y sin modal para el que no.
*Prueba:* en PushDispatch los scripts de la demo E2E (un `correr.sh` y módulos por
recorrido: consultas, primera cuenta, detener) imprimen las tres medidas por captura.

**R8.3 — El script falla, no sólo captura.** Desborde > 0, desplazamiento > 0 o estilo sin aplicar terminan con
código distinto de cero. *Por qué:* una captura que nadie mira no detecta nada. *Prueba:* romper un estilo a
propósito hace fallar la corrida.

**R8.4 — Sesión por rol en el E2E.** El script inicia sesión con una cuenta de prueba por rol, raíz incluido
(K3-AP-3). *Prueba:* hay una captura por rol.

```js
// NO VERIFICADO: forma de la medición dentro del recorrido (Playwright)
for (const ancho of [1280, 390]) for (const tema of ['light', 'dark']) {
  await page.setViewportSize({ width: ancho, height: 900 });
  await page.emulateMedia({ colorScheme: tema });
  // ... abrir consola, preguntar, esperar respuesta
  const desborde = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
  if (desborde > 0) throw new Error(`desborde ${desborde}px a ${ancho} ${tema}`);
  await page.screenshot({ path: `capturas/${ancho}-${tema}.png`, fullPage: true });
}
```

---

<a id="k7-9"></a>
## 9. Batería real ×3 automatizada

La norma (×3, umbral 90 % sobre el mínimo, oráculo previo, M ≥ 20) está en
[K3 §2](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-2-bateria); acá, el script.

**R9.1 — La batería es un archivo de datos versionado y el veredicto lo calcula código.** Cada caso: rol,
anillo, tipo, pregunta, herramienta esperada, alcance, cifra con su fuente, destino o ninguno, forma esperada
(K5). El script lee el archivo, corre, guarda el registro crudo y recalcula el veredicto desde el registro.
*Prueba:* borrar el veredicto y recalcularlo desde el registro da lo mismo.

```jsonc
// NO VERIFICADO: forma de un caso; los nombres de campo son los que el script decide.
{
  "id": "C07", "rol": "recepcion", "anillo": 1, "tipo": "cifra",
  "pregunta": "¿Cuántos turnos libres hay mañana?",
  "herramienta": "turnos_libres", "argumentos": { "dia": "Manana" },
  "alcance": "sede-a",
  "cifras": [ { "valor": 12, "fuente": "turnos_libres.total" } ],
  "destino": "turnos.disponibilidad",
  "forma": "F1"
}
```

La cifra esperada se escribe con su fuente (función y campo) para que el código compare valor y origen; la forma
es el id de K5 (F1–F8), que el script contrasta con las marcas que deja la capa de redacción, no con el juicio de
quien lee.

**R9.6 — Congelá el oráculo: reloj inyectado y base sembrada.** Si alguna cifra depende de la fecha («libres
mañana», «enviados esta semana»), la batería corre con un `TimeProvider` fijado en la composición (fecha y zona
escritas en el registro) y contra una base sembrada con datos fijos, para que el oráculo escrito antes de correr
siga valiendo. El contraste con SQL del momento es para el sitio real (§11), no para la batería. *Prueba:* correr
la batería dos días distintos da el mismo veredicto; el registro trae la fecha fijada.

**R9.2 — Un solo comando corre las tres corridas y escribe un registro por corrida con su composición**
(SHA, versión del corpus, batería, modelo y servidor verificados antes del primer pedido, sin dirección).
*Prueba:* tres archivos con la misma composición; si cambió, el script se niega a sumar (K3 R2.6).

**R9.3 — Informá las inestables.** Un caso que falla en una o dos de las tres corridas es **inestable** y se
lista aparte con sus respuestas. *Por qué:* el mínimo puede pasar el 90 % con inestables escondidas, que son
las primeras en caer en producción. *Prueba:* el resumen tiene la columna «fallas en 3» por caso.

**R9.4 — La batería y la app usan la misma configuración.** Mismo pedido, mismo prompt, mismos límites de
recorte, mismo modelo de redacción y de embeddings, con la misma alternancia. *Por qué:* en PushDispatch la
crítica r6 encontró que la batería armaba el pedido distinto de la app y no pagaba el cambio de modelo; las
cifras no describían la app (§13, error 5). *Prueba:* la batería invoca el mismo servicio que la consola (no
una copia) y una prueba compara la configuración efectiva de ambos.

**R9.5 — La corrida con modelo real se salta en la CI con motivo explícito.** *Prueba:* en PushDispatch
`ModeloRealTests` usa `[SkippableFact]` con `Skip.If(…, "sin IA__Endpoint / IA__Modelo: la corrida rotulada no
corre en la CI")` (ver R12.3).

---

<a id="k7-10"></a>
## 10. Latencia en frío y en caliente

Norma en [K3 §3](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-3-condiciones) y
[K3 §10](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-10-latencia).

**R10.1 — La latencia sale del registro de la batería, no de una corrida aparte.** El script anota por pedido:
tiempo a la primera palabra, tiempo total, si fue el primero tras arrancar (frío), y qué modelos estaban en
memoria. *Prueba:* las cifras p50/p95 se recalculan desde el registro de §9.

**R10.2 — Forzá el frío.** Antes de la primera pregunta, descargá el modelo o reiniciá el servicio, y alterná
redacción y embeddings como la app. *Por qué:* medir con el modelo cargado dio latencias que la app no tenía
(K3-AP-7; §13, error 2). *Prueba:* la primera muestra de cada corrida está rotulada «frío» y es más lenta que
las siguientes; en PushDispatch `ModeloRealTests` separa la primera latencia de las calientes.

---

<a id="k7-11"></a>
## 11. Sitio real por rol y cifras contra SQL

Norma en [K3 §7](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-7-sitio-real) y
[K3 §8](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-8-cifras-sql).

**R11.1 — Script de humo del sitio real:** por cada rol, inicia sesión con una cuenta de prueba, lee el motivo
de habilitación expuesto por la app (sin dirección ni modelo), hace una pregunta respondible y abre el destino
ofrecido. *Prueba:* el registro tiene una fila por rol, raíz incluido, con motivo «habilitada».
El contrato del motivo es el de [K2 §10](knowledge-AI-Layer-Console-Assistant-Building.md#k2-10): una lista
cerrada (en PushDispatch `MotivoDeConsultaApagada`: `Ninguno` = habilitada, `FaltaEndpoint`, `FaltaModelo`,
`FaltaImplementacion`, `BanderaApagada`, `BanderaIlegible`), cuyo texto nombra la clave que falta y nunca su
valor; la superficie que lo expone (estado de la consola en el panel o registro al arrancar) la fija K1. Un
motivo fuera de la lista hace fallar el humo.

**R11.2 — Cifras contra SQL con una consulta independiente** del código de la herramienta (escrita a mano, no
llamando al repositorio de la app). *Por qué:* comparar la herramienta consigo misma no prueba nada. En
PushDispatch el contraste encontró la tasa de apertura que llegaba como fracción y se mostraba «0,2 %» en vez
de 15,6 % (360/2308). *Prueba:* el registro trae la consulta, su resultado y la cifra mostrada.

**R11.3 — La evidencia no publica secretos ni personas** (K3-AP-5, K3 R9.4). *Prueba:* grep de §12 sobre
registros y capturas.

---

<a id="k7-12"></a>
## 12. Compuertas de CI

**R12.1 — Compilá con `-warnaserror`** (o `TreatWarningsAsErrors`) los proyectos de la capa y de sus pruebas.
*Por qué:* un aviso de nulabilidad en un argumento de función es un defecto latente. *Prueba:* el workflow de
CI lo pasa y un aviso introducido rompe el build.

**R12.2 — Corré todas las capas deterministas (§3–§6) en cada PR, sin red hacia el modelo.** *Prueba:* la CI
no tiene la variable del endpoint y pasa. Un modelo de embeddings **local y en proceso** (cargado por la propia
prueba, sin servidor) se permite y el informe dice cuál corrió; un servidor de embeddings remoto cuenta como red
y no entra en la CI.

**R12.3 — `Skip` sólo con motivo que nombre la dependencia y con la dependencia visible en el informe.** Un
Skip sin motivo, o que se salta porque falta algo que debería estar, esconde una dependencia. *Por qué:* una
prueba saltada parece verde (§13, error 4). *Prueba:* grep de `Skip` en `tests/` devuelve sólo casos con texto
de motivo; el resumen de la CI cuenta las saltadas y el informe de fase las lista como INFORME.

**R12.5 — Compuerta de recuperación.** El PR que toca la ayuda, el léxico, los sinónimos o el umbral corre la
medición de [K6 §7](knowledge-AI-Layer-Retrieval-Corpus-Functions-And-Deeplinks.md#k6-7) completa, por rol, con
el léxico siempre y con embeddings si están en proceso (R12.2), y falla bajo el umbral de K1 (o recall@3 < 0,9 o
«ninguna» < 100 %). *Prueba:* agregar una página que roba preguntas de otra pone la compuerta en rojo.

**R12.4 — Vocabulario neutro y búsqueda de secretos.** Un paso de la CI busca nombres de organizaciones,
clientes y patrones de clave, correo y dirección sobre `tests/`, fixtures y evidencias. *Prueba:* en
PushDispatch la compuerta de vocabulario neutro de la CI rechazó un comentario y una prueba y obligó a
neutralizarlos (K3 R9.3).

---

<a id="k7-13"></a>
## 13. Errores que costaron, cada uno con su control

| # | Error | Qué pasó en PushDispatch | Control | Sección |
|---|---|---|---|---|
| 1 | Un 90 % informado sin repetir | 19/20 (95 %) en una corrida; ×3 dio 21/33 (64 %); con arreglos de troceo, recorte y control llegó a 27/30 ×3 y se aceptó con 36/36 en cada una de tres corridas | El script corre ×3 y se niega a resumir una sola | §9, K3-AP-6 |
| 2 | Medir con el modelo cargado | Latencias optimistas que la app no tenía | Frío forzado y alternancia de modelos en el script | §10, K3-AP-7 |
| 3 | Una prueba que pasaba sin probar nada | Afirmaba sobre lo que el doble ya devolvía; verde con y sin el arreglo | Mutación: revertir y ver rojo | §7 |
| 4 | Skip que esconde una dependencia | La prueba saltada se leía como verde y la dependencia faltante no aparecía en ningún informe | Skip con motivo y conteo de saltadas en el informe | §12 |
| 5 | Demo y batería con configuraciones distintas | La batería armaba el pedido distinto de la app y no pagaba el cambio de modelo | La batería invoca el mismo servicio; prueba de configuración efectiva | §9, K3-AP-11 |
| 6 | El despliegue sin la variable que el código exigía | La clave del modelo de redacción se configuró con otro nombre; la IA quedó apagada sin error visible | Conformidad clave ↔ plantilla; motivo leído en el sitio real | §5, §11, K3-AP-4 |

---

<a id="k7-14"></a>
## 14. Procedimiento paso a paso

1. Listá las garantías: `REQ-IA-nn` de K1 y reglas de K2 y K6 que toca el cambio.
2. Para cada una, elegí la capa más baja que la prueba (§2) y escribí la prueba con su nombre (R1.1).
3. Si es un defecto: prueba en rojo, arreglo, verde, mutación (§7).
4. Si agregaste destino, función, página de ayuda o clave: sumá la fila de conformidad o adversa (§5, §6).
5. Corré `dotnet test -warnaserror` sin modelo; todo verde, Skips con motivo.
6. Si el cambio toca la consola: E2E a 1280 y 390, claro y oscuro, por rol (§8).
7. Antes de cerrar la fase: batería ×3 con el mismo servicio, frío forzado, inestables listadas (§9, §10).
8. Al desplegar: humo por rol y cifras contra SQL (§11).
9. Pasá los registros a K3 para la aceptación y el informe.

---

<a id="k7-15"></a>
## 15. Criterios de parada

**La suite está lista para el PR** cuando: cada garantía del cambio tiene prueba nombrada; los defectos
arreglados tienen su mutación registrada; la CI pasa sin modelo con `-warnaserror`; los Skips tienen motivo; el
grep de vocabulario y secretos da cero.

**La suite está lista para la aceptación de K3** cuando además: E2E sin desborde ni desplazamiento en las
cuatro combinaciones y por rol; tres registros de batería con la misma composición, inestables listadas;
latencia fría y caliente desde esos registros; humo del sitio real por rol y contraste SQL.

**Escalá a la mesa** si una garantía no se puede probar en ninguna capa (señal de diseño, no de prueba), si
una prueba exige ajustar la batería para pasar (K3-AP-10) o si la configuración de la app y la de la batería no
se pueden unificar.

---

<a id="k7-16"></a>
## 16. Checklist binario

- [ ] Cada garantía del cambio tiene una prueba cuyo nombre la enuncia.
- [ ] Ninguna garantía determinista está sólo en la batería.
- [ ] Hay doble de `IChatClient` que registra lo enviado, y una prueba busca en lo enviado algo prohibido.
- [ ] Hay doble de embeddings y una prueba de que lo ajeno al rol no se vectoriza.
- [ ] El enrutador se prueba como tabla.
- [ ] Tope de llamadas, función inexistente, argumento inválido, proveedor caído y cifra sin fuente tienen prueba.
- [ ] Cada estado visible de la consola tiene prueba bUnit o diferido con nombre.
- [ ] Conformidad catálogo ↔ `@page` ↔ `[SupplyParameterFromQuery]` ↔ políticas en verde.
- [ ] Conformidad corpus ↔ pantallas y `status: borrador` no servido.
- [ ] Recall@3 por rol y «ninguna» medidos en el PR que toca la ayuda (R12.5).
- [ ] La batería corre con reloj fijado y base sembrada si alguna cifra depende de la fecha (R9.6).
- [ ] Conformidad claves de configuración ↔ plantilla de despliegue.
- [ ] Adversas: inyección en pregunta, canario en la ayuda, cuenta ajena, ruta en el texto.
- [ ] Cada defecto arreglado tiene registro de mutación (rojo con el arreglo revertido).
- [ ] La CI compila con `-warnaserror` y corre sin modelo.
- [ ] Todo `Skip` tiene motivo que nombra la dependencia y aparece en el informe.
- [ ] Grep de vocabulario neutro y de secretos da cero.
- [ ] E2E contra la app del SHA a 1280 y 390, claro y oscuro, por rol, y el script falla ante desborde.
- [ ] La batería invoca el mismo servicio y la misma configuración que la app.
- [ ] Tres registros de batería con composición idéntica e inestables listadas.
- [ ] Latencia fría y caliente recalculable desde los registros.
- [ ] Humo del sitio real por rol y cifras contra una consulta SQL independiente.

---

<a id="k7-17"></a>
## 17. Antipatrones con su control

Cuando el error es el mismo que uno de K3, la fuente es K3 y acá sólo se remite.

| Id | Antipatrón | Control |
|---|---|---|
| <a id="k7-ap-1"></a>K7-AP-1 | Prueba que pasa con y sin el arreglo | Mutación registrada (§7) |
| <a id="k7-ap-2"></a>K7-AP-2 | `Skip` sin motivo o que esconde una dependencia | R12.3; conteo de saltadas en el informe |
| <a id="k7-ap-3"></a>K7-AP-3 | Garantía determinista probada sólo con el modelo | R2.1; lo determinista en unitarias |
| <a id="k7-ap-4"></a>K7-AP-4 | Doble del modelo que no registra lo enviado | R3.1; prueba de «lo que no debe llegar» |
| <a id="k7-ap-5"></a>K7-AP-5 | Batería con su propia copia del pedido | R9.4; mismo servicio que la app (ver K3-AP-11) |
| <a id="k7-ap-6"></a>K7-AP-6 | E2E que sólo captura y nunca falla | R8.3 |
| <a id="k7-ap-7"></a>K7-AP-7 | Cifra comparada contra la misma herramienta | R11.2; consulta SQL escrita a mano |
| <a id="k7-ap-8"></a>K7-AP-8 | Clave leída por el código ausente en la plantilla | R5.4 (ver K3-AP-4) |
| — | Un 90 % sin repetir | ver [K3-AP-6](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-ap-6) |
| — | Medir con el modelo cargado | ver [K3-AP-7](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-ap-7) |
| — | Ajustar la batería para que pase | ver [K3-AP-10](knowledge-AI-Layer-Verification-And-Acceptance.md#k3-ap-10) |

---

<a id="k7-18"></a>
## 18. Casos

**PushDispatch (producto diseñado desde cero; caso de origen).** Suite de la consola de IA del panel de
control, organizada por capa: `tests/NgPushDispatch.AI.Tests/` (doble `ChatClientFalso` y `ConsultaFalsa` en
`Dobles.cs`; `ConsultaConModeloTests` con tope de llamadas, función inexistente, cifra sin fuente, proveedor
caído y paralelismo; `EstadoDeConsultaIaTests` con el motivo de habilitación; `ModeloRealTests` saltable con
motivo); `tests/NgPushDispatch.Utils.Tests/Consola/` (`DialogoYCatalogoTests` y `DestinoYCuentaTests` como
tablas de alcance y destino; `SuscriptoresDeCuentaTests` con cuenta elegida y ajena; `RedaccionConAnclajeTests`
con troceo, filtro por rol previo, caché y minimización; `PlantillaDeRespaldoTests` con la sección de la
pregunta ganando a la introducción); `tests/NgPushDispatch.Ayuda.Tests/` con páginas de fixture por regla de
lectura; corpus canario de inyección en `tests/Fixtures/`; bUnit en `tests/NgPushDispatch.Web.Tests/`. Fuera de
la suite: scripts de batería ×3 y de capturas Playwright por recorrido, y el humo del sitio real por rol, que
encontró la IA apagada por un nombre de clave distinto y la plantilla de respaldo equivocada (de ahí
`EstadoDeConsultaIaTests` y `PlantillaDeRespaldoTests`). Los seis errores del §13 salen de este caso.

**Movilidad Urbana (demo del libro).** Muestra la base: dobles de `IChatClient` en las pruebas de los ejemplos
(`Examples/_Comun/MovilidadUrbana.Asistente.Comun.Pruebas/`, `Examples/01-Chat-Y-Streaming/Pruebas/`,
`Examples/09-Caso-De-Uso-Como-Herramienta/Pruebas/`), cableado sin red y corridas con registro
([Guía §11.9](../Dot-Net-Integration-AI-Layer-Guide.md#c11-ejemplos)). No tiene panel ni roles: §4, §8 y §11
exigen un sistema con pantalla y sesiones.

**Geometría (laboratorio didáctico).** Sirve para ensayar §3, §7 y §9 a escala chica: el valor esperado se
calcula sin modelo, así que el oráculo de la batería y la mutación se escriben enteros en código.

---

<a id="k7-19"></a>
## 19. Límites de este conocimiento

- No fija umbrales ni la forma del informe de aceptación: los fija [K3](knowledge-AI-Layer-Verification-And-Acceptance.md).
- No cubre bibliotecas de evaluación ni LLM juez: [Guía §11.5](../Dot-Net-Integration-AI-Layer-Guide.md#c11-5), [§11.6](../Dot-Net-Integration-AI-Layer-Guide.md#c11-6).
- La mutación descripta es manual; no cubre herramientas de mutación automática.
- Los snippets rotulados NO VERIFICADO muestran la forma, no son código de los ejemplos del libro.
- Probado en un solo sistema con panel; los casos de Movilidad Urbana y Geometría ilustran capas, no la suite entera.

---

<a id="k7-glosario"></a>
## Glosario

- **Garantía:** frase verificable de la especificación o de un conocimiento que el código debe cumplir siempre.
- **Doble guionado:** implementación de prueba de `IChatClient` que devuelve lo que dice un guion y registra lo recibido.
- **Conformidad:** prueba sin modelo que ata dos artefactos que cambian en PRs distintos (catálogo y páginas, corpus y pantallas, claves y plantilla).
- **Adversa:** prueba que simula a quien quiere que el sistema haga lo que no debe.
- **Mutación:** revertir el arreglo para comprobar que la prueba falla sin él.
- **Inestable:** caso de la batería que falla en una o dos de tres corridas.
- **Humo del sitio real:** recorrido mínimo por rol sobre la app desplegada.

---

<a id="k7-historial"></a>
## Historial

| Versión | Fecha | Cambio |
|---|---|---|
| 1.0 | 2026-11-11 | Creación. Detalle de testing que K3 §9 y K2 dejaban en general; K3 conserva la norma de aceptación. |
| 1.1 | 2026-11-11 | Tras la prueba a ciegas: `status: borrador` (no `estado:`), igual que K6; R5.2 con una sola política (ver) y el hacer por el criterio de K6 §16; fila y compuerta de recuperación (R12.5); embeddings en proceso permitidos en la CI; criterio de la capa de integración; página destino con parámetro en bUnit y recorrido chip → acción en E2E; caso de batería de ejemplo y R9.6 (reloj inyectado y base sembrada); contrato del motivo de habilitación; mutación contra `origin/main...HEAD` en una carpeta del worktree. |
