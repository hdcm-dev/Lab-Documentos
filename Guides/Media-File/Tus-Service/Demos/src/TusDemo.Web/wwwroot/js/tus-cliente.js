// Cliente tus mínimo para el navegador, escrito a mano para la guía.
// Implementa la parte del protocolo tus 1.0.0 que usa la demo: crear (POST), consultar el avance (HEAD)
// y enviar por trozos (PATCH). En un producto conviene la biblioteca tus-js-client.

const VERSION_TUS = "1.0.0";
let controlador = null; // AbortController de la suba en curso; permite pausar.

// Base64 de un texto UTF-8 (los valores de Upload-Metadata van en Base64).
function base64(texto) {
  const bytes = new TextEncoder().encode(texto);
  let binario = "";
  for (const b of bytes) binario += String.fromCharCode(b);
  return btoa(binario);
}

// Clave con la que se recuerda la URL de una suba para retomarla después.
function huella(archivo) {
  return `tus::${archivo.name}::${archivo.size}::${archivo.lastModified}`;
}

async function consultarAvance(url, cabeceras) {
  const r = await fetch(url, { method: "HEAD", headers: cabeceras });
  if (!r.ok) return null; // 404/410: la suba ya no existe (por ejemplo, venció)
  return Number(r.headers.get("Upload-Offset"));
}

async function trabajar(archivo, { endpoint, clave, trozo }, dotnet) {
  const cabeceras = { "Tus-Resumable": VERSION_TUS, "X-Clave-Demo": clave };
  const llave = huella(archivo);
  let url = localStorage.getItem(llave);
  let offset = 0;

  // 1) ¿Hay una suba anterior de este mismo archivo? Preguntar cuánto llegó.
  if (url) {
    const avance = await consultarAvance(url, cabeceras);
    if (avance === null) { localStorage.removeItem(llave); url = null; }
    else offset = avance;
  }

  // 2) Si no hay, crear una suba nueva con su largo y sus metadatos.
  if (!url) {
    const r = await fetch(endpoint, {
      method: "POST",
      headers: {
        ...cabeceras,
        "Upload-Length": String(archivo.size),
        "Upload-Metadata": `filename ${base64(archivo.name)},filetype ${base64(archivo.type || "application/octet-stream")}`,
      },
    });
    if (r.status !== 201) throw new Error(`Crear: ${r.status} ${await r.text()}`);
    url = new URL(r.headers.get("Location"), endpoint).href; // Location puede ser relativa
    localStorage.setItem(llave, url); // guardar ANTES del primer trozo
  }

  // 3) Enviar trozo por trozo desde el offset que el servidor confirmó.
  controlador = new AbortController();
  await dotnet.invokeMethodAsync("Avance", offset, archivo.size);
  while (offset < archivo.size) {
    const pedazo = archivo.slice(offset, offset + trozo);
    const r = await fetch(url, {
      method: "PATCH",
      headers: { ...cabeceras, "Upload-Offset": String(offset), "Content-Type": "application/offset+octet-stream" },
      body: pedazo,
      signal: controlador.signal,
    });
    if (r.status === 409) { // el servidor tiene otro offset: preguntar y seguir desde ahí
      offset = await consultarAvance(url, cabeceras);
      continue;
    }
    if (r.status !== 204) throw new Error(`Enviar: ${r.status} ${await r.text()}`);
    offset = Number(r.headers.get("Upload-Offset"));
    await dotnet.invokeMethodAsync("Avance", offset, archivo.size);
  }

  localStorage.removeItem(llave); // terminada: ya no hay nada que retomar
  return url.split("/").pop();    // el id de la suba es el último segmento de la URL
}

// Arranca la suba y vuelve enseguida. El final se avisa con Terminado o Fallo.
export function subir(selector, opciones, dotnet) {
  const archivo = selector.files[0];
  if (!archivo) {
    dotnet.invokeMethodAsync("Fallo", "Error", "Elegí un archivo primero.");
    return;
  }
  trabajar(archivo, opciones, dotnet)
    .then((id) => dotnet.invokeMethodAsync("Terminado", id))
    .catch((e) => dotnet.invokeMethodAsync("Fallo", e.name ?? "Error", e.message ?? String(e)));
}

export function pausar() {
  controlador?.abort();
}

// La descarga necesita la cabecera de autenticación, así que no alcanza con un enlace:
// se pide con fetch y se entrega al navegador como un archivo.
export async function descargar(url, clave, nombre) {
  const r = await fetch(url, { headers: { "X-Clave-Demo": clave } });
  if (!r.ok) throw new Error(`Descargar: ${r.status}`);
  const blob = await r.blob();
  const enlace = document.createElement("a");
  enlace.href = URL.createObjectURL(blob);
  enlace.download = nombre;
  enlace.click();
  URL.revokeObjectURL(enlace.href);
}
