// Cliente tus mínimo para el navegador, escrito a mano para la guía.
// Implementa la parte del protocolo tus 1.0.0 que usa la demo: crear (POST), consultar el avance (HEAD)
// y enviar por trozos (PATCH). En un producto conviene la biblioteca tus-js-client.

const VERSION_TUS = "1.0.0";
const MAX_CONFLICTOS = 5; // 409 seguidos que se toleran antes de abandonar
let controlador = null;   // AbortController de la suba en curso; permite pausar.
let enCurso = false;      // evita dos subas a la vez (doble clic)

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

function esperarMs(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

// Avisa a .NET sin dejar promesas rechazadas sueltas (por ejemplo, si el circuito ya se cerró).
function avisar(dotnet, metodo, ...args) {
  return dotnet.invokeMethodAsync(metodo, ...args).catch(() => {});
}

// Devuelve el offset que tiene el servidor, o null si la suba ya no existe (404, 410) o no es
// de este usuario (403): sólo en esos casos se puede olvidar la URL guardada. Cualquier otro
// código (por ejemplo un 500 o un 503 pasajero) es un error y no borra nada.
async function consultarAvance(url, cabeceras, signal) {
  const r = await fetch(url, { method: "HEAD", headers: cabeceras, signal });
  if (r.status === 404 || r.status === 410 || r.status === 403) return null;
  if (!r.ok) throw new Error(`Consultar: ${r.status}`);
  return Number(r.headers.get("Upload-Offset"));
}

async function trabajar(archivo, { endpoint, clave, trozo }, dotnet) {
  // El controlador se crea antes del primer pedido: pausar corta también el HEAD y el POST.
  controlador = new AbortController();
  const signal = controlador.signal;
  const cabeceras = { "Tus-Resumable": VERSION_TUS, "X-Clave-Demo": clave };
  const llave = huella(archivo);
  let url = localStorage.getItem(llave);
  let offset = 0;

  // 1) ¿Hay una suba anterior de este mismo archivo? Preguntar cuánto llegó.
  if (url) {
    const avance = await consultarAvance(url, cabeceras, signal);
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
      signal,
    });
    if (r.status !== 201) throw new Error(`Crear: ${r.status} ${await r.text()}`);
    url = new URL(r.headers.get("Location"), endpoint).href; // Location puede ser relativa
    localStorage.setItem(llave, url); // guardar ANTES del primer trozo
  }

  // 3) Enviar trozo por trozo desde el offset que el servidor confirmó.
  await dotnet.invokeMethodAsync("Avance", offset, archivo.size);
  let conflictos = 0;
  while (offset < archivo.size) {
    const pedazo = archivo.slice(offset, offset + trozo);
    const r = await fetch(url, {
      method: "PATCH",
      headers: { ...cabeceras, "Upload-Offset": String(offset), "Content-Type": "application/offset+octet-stream" },
      body: pedazo,
      signal,
    });
    if (r.status === 409) { // el servidor tiene otro offset (o la suba está ocupada): esperar, preguntar y seguir
      conflictos++;
      if (conflictos >= MAX_CONFLICTOS) throw new Error(`Enviar: ${conflictos} respuestas 409 seguidas`);
      await esperarMs(500 * conflictos);
      const avance = await consultarAvance(url, cabeceras, signal);
      if (avance === null) throw new Error("La suba ya no existe");
      offset = avance;
      continue;
    }
    if (r.status !== 204) throw new Error(`Enviar: ${r.status} ${await r.text()}`);
    conflictos = 0;
    offset = Number(r.headers.get("Upload-Offset"));
    await dotnet.invokeMethodAsync("Avance", offset, archivo.size);
  }

  localStorage.removeItem(llave); // terminada: ya no hay nada que retomar
  return url.split("/").pop();    // el id de la suba es el último segmento de la URL
}

async function correr(archivo, opciones, dotnet) {
  let id = null;
  let error = null;
  try {
    id = await trabajar(archivo, opciones, dotnet);
  } catch (e) {
    error = e;
  } finally {
    // Se libera antes de avisar a .NET, así el próximo clic ya encuentra la bandera limpia.
    enCurso = false;
    controlador = null;
  }
  if (error) await avisar(dotnet, "Fallo", error.name ?? "Error", error.message ?? String(error));
  else await avisar(dotnet, "Terminado", id);
}

// Arranca la suba y vuelve enseguida. El final se avisa con Terminado o Fallo.
export function subir(selector, opciones, dotnet) {
  if (enCurso) {
    avisar(dotnet, "Fallo", "EnCurso", "Ya hay una suba en curso.");
    return;
  }
  const archivo = selector.files[0];
  if (!archivo) {
    avisar(dotnet, "Fallo", "Error", "Elegí un archivo primero.");
    return;
  }
  enCurso = true;
  correr(archivo, opciones, dotnet);
}

export function pausar() {
  controlador?.abort();
}

async function bajar(url, clave, nombre) {
  const r = await fetch(url, { headers: { "X-Clave-Demo": clave } });
  if (!r.ok) throw new Error(`Descargar: ${r.status}`);
  const blob = await r.blob();
  const enlace = document.createElement("a");
  enlace.href = URL.createObjectURL(blob);
  enlace.download = nombre;
  enlace.click();
  URL.revokeObjectURL(enlace.href);
}

// La descarga necesita la cabecera de autenticación, así que no alcanza con un enlace:
// se pide con fetch y se entrega al navegador como un archivo. Arranca y vuelve enseguida
// (.NET no espera el fetch); si falla, se avisa con DescargaFallo.
export function descargar(url, clave, nombre, dotnet) {
  bajar(url, clave, nombre)
    .catch((e) => avisar(dotnet, "DescargaFallo", e?.message ?? String(e)));
}
