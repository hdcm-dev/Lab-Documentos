// Recorrido del contenido en un navegador headless (Chromium), con el PUENTE SIMULADO.
// Lo lanza probar-en-navegador.sh dentro del contenedor de Playwright; no se corre a mano.
//
// Qué hace: levanta un servidor estático por variante (v1 y v2 del contenido publicado), sirve
// Testing/puente-simulado.js en la ruta _framework/hybridwebview.js y recorre las pantallas. Cada
// comprobación que falla se anota; al final sale con código 1 si hubo alguna. Cada captura deja su .txt de
// tres líneas: qué muestra, cómo se obtuvo y qué se comprobó.
//
// Variables: V1, V2 (carpetas wwwroot publicadas), SIM (puente-simulado.js), DEFINICIONES (carpeta
// definiciones/), SALIDA (dónde dejar capturas), PLAYWRIGHT_CORE (módulo playwright-core).
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_CORE);

const SALIDA = process.env.SALIDA;
const TIPOS = {
    '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css',
    '.json': 'application/json', '.wasm': 'application/wasm', '.dat': 'application/octet-stream', '.png': 'image/png'
};

// --- Servidor estático: como el servidor de contenido del cascarón, pero en HTTP común ----------------------
function servir(raiz, puerto) {
    const servidor = http.createServer((pedido, respuesta) => {
        const ruta = decodeURIComponent(new URL(pedido.url, 'http://x').pathname);
        let archivo;
        if (ruta === '/_framework/hybridwebview.js') archivo = process.env.SIM;           // el transporte simulado
        else if (ruta.startsWith('/sim/definiciones/')) archivo = path.join(process.env.DEFINICIONES, ruta.slice('/sim/definiciones/'.length));
        else archivo = path.join(raiz, ruta);
        // Rutas del router de Blazor (/localidades, /encuesta): se responde index.html.
        if (!fs.existsSync(archivo) || fs.statSync(archivo).isDirectory()) archivo = path.join(raiz, 'index.html');
        respuesta.writeHead(200, { 'Content-Type': TIPOS[path.extname(archivo)] || 'application/octet-stream', 'Cache-Control': 'no-store' });
        fs.createReadStream(archivo).pipe(respuesta);
    });
    return new Promise(listo => servidor.listen(puerto, '127.0.0.1', () => listo(servidor)));
}

// --- Comprobaciones y capturas ------------------------------------------------------------------------------
const fallas = [];
const comprobadas = [];
function comprobar(condicion, descripcion) {
    (condicion ? comprobadas : fallas).push(descripcion);
    console.log((condicion ? 'OK    ' : 'FALLA ') + descripcion);
    return condicion;
}

async function capturar(pagina, nombre, queMuestra, comprobado) {
    const archivo = path.join(SALIDA, `contenido-navegador-${nombre}.png`);
    await pagina.waitForTimeout(250);   // que terminen las transiciones de borde (.15s) antes de capturar
    await pagina.screenshot({ path: archivo, fullPage: true });
    fs.writeFileSync(archivo.replace(/\.png$/, '.txt'), [
        queMuestra,
        `Chromium headless (playwright-core ${require(process.env.PLAYWRIGHT_CORE + '/package.json').version}), puente SIMULADO (Testing/puente-simulado.js), ${new Date().toISOString()}; NO es el cascarón ni el teléfono.`,
        comprobado
    ].join('\n') + '\n');
    console.log('captura ' + archivo);
}

const navegador = await chromium.launch({ args: ['--no-sandbox'] });
const servidorV1 = await servir(process.env.V1, 5101);
const servidorV2 = await servir(process.env.V2, 5102);

// Cada escenario abre un contexto nuevo (almacenamiento limpio), con tamaño de teléfono.
let ultimaPagina = null;   // para diagnosticar si un escenario se corta a mitad
async function abrir(base, consulta) {
    const contexto = await navegador.newContext({ viewport: { width: 412, height: 915 }, deviceScaleFactor: 1, locale: 'es-AR' });
    const pagina = await contexto.newPage();
    ultimaPagina = pagina;
    pagina.errores = [];
    pagina.csp = [];
    pagina.on('pageerror', e => pagina.errores.push('pageerror: ' + e.message));
    pagina.on('console', m => {
        if (m.type() === 'error') pagina.errores.push('console: ' + m.text());
        if (m.text().includes('Content Security Policy')) pagina.csp.push(m.text());
    });
    await pagina.addInitScript(() => document.addEventListener('securitypolicyviolation',
        e => console.error('Content Security Policy violada: ' + e.violatedDirective + ' ' + e.blockedURI)));
    await pagina.goto(`${base}/${consulta}`, { waitUntil: 'load' });
    await pagina.waitForSelector('[data-testid=linea-de-estado], [data-testid=estado-puente]', { timeout: 60000 });
    return pagina;
}

const texto = (pagina, testid) => pagina.locator(`[data-testid="${testid}"]`).first().innerText();
const hay = (pagina, testid) => pagina.locator(`[data-testid="${testid}"]`).count().then(n => n > 0);
const sim = (pagina, expresion) => pagina.evaluate(expresion);
const V1 = 'http://127.0.0.1:5101';
const V2 = 'http://127.0.0.1:5102';

async function completarPaso1(pagina, nombre) {
    await pagina.fill('#pregunta-nombre', nombre);
    await pagina.fill('#pregunta-edad', '34');
    await pagina.selectOption('#pregunta-localidad', 'Corrientes');
    await pagina.click('[data-testid=boton-siguiente]');
    await pagina.waitForSelector('[data-testid=paso-2]');
}

async function completarPaso2(pagina) {
    await pagina.check('[data-testid=opcion-medios-colectivo]');
    await pagina.check('[data-testid=opcion-medios-bicicleta]');
    await pagina.selectOption('#pregunta-frecuencia', 'diaria');
    await pagina.click('[data-testid=boton-siguiente]');
    await pagina.waitForSelector('[data-testid=paso-3]');
}

async function completarPaso3(pagina) {
    await pagina.fill('#pregunta-distancia', '12.5');
    await pagina.fill('#pregunta-minutos', '40');
    await pagina.selectOption('#pregunta-motivo', 'trabajo');
}

try {
    // 1. Inicio con contenido v1 y definición v1 -------------------------------------------------------------
    {
        const p = await abrir(V1, '?reiniciar=1&definicion=1');
        await p.waitForSelector('[data-testid=inicio-version-definicion]:has-text("v1")');
        const ok = comprobar(await texto(p, 'inicio-version-contenido') === '1.0.0', 'v1: Inicio muestra contenido 1.0.0')
            & comprobar((await texto(p, 'inicio-version-definicion')).includes('definición v1'), 'v1: Inicio muestra definición v1')
            & comprobar(await texto(p, 'inicio-version-puente') === '1.2.0', 'v1: Inicio muestra puente 1.2.0')
            & comprobar(!(await hay(p, 'novedades')), 'v1: sin tarjeta de novedades');
        await p.waitForFunction(() => window.puenteSimulado.listo() !== null);
        const listo = await sim(p, () => window.puenteSimulado.listo());
        comprobar(listo === '1.0.0', `v1: avisó Listo(version) tras el primer render (${listo})`);
        const llamadas = await sim(p, () => window.puenteSimulado.llamadas.map(s => s.method));
        comprobar(llamadas[0] === 'bridge.handshake', `el primer sobre es bridge.handshake (${llamadas.slice(0, 3).join(', ')})`);
        const sobre = await sim(p, () => window.puenteSimulado.llamadas[0]);
        comprobar(sobre.jsonrpc === '2.0' && sobre.v === '1.2.0' && typeof sobre.id === 'string', 'sobre JSON-RPC 2.0 con v=1.2.0 e id');
        comprobar(await hay(p, 'estado-red'), 'network.status presente → se muestra el estado de red');
        comprobar(p.csp.length === 0, `sin violaciones de CSP (${p.csp.length})`);
        comprobar(p.errores.length === 0, `sin errores de consola (${p.errores.join(' | ')})`);
        await capturar(p, '01-inicio-v1', 'Inicio del contenido v1 (1.0.0) con la definición v1: versiones de contenido, definición, puente y capacidades.',
            `Comprobado: versiones 1.0.0 / v1 / 1.2.0, Listo(${listo}) recibido por el puente, sobre JSON-RPC 2.0 con v, CSP sin violaciones${ok ? '' : ' (hubo fallas, ver salida)'}.`);
        await p.context().close();
    }

    // 2. Localidades: siembra, alta, error de validación ---------------------------------------------------
    {
        const p = await abrir(V1, '?reiniciar=1');
        await p.click('a[href=localidades]');
        await p.waitForSelector('[data-testid=fila]');
        comprobar(await p.locator('[data-testid=fila]').count() === 2, 'Localidades: arranca con las 2 sembradas del caso base');
        await p.click('[data-testid=boton-guardar]');
        await p.waitForSelector('[data-testid=error-nombre]');
        comprobar(await hay(p, 'error-codigoPostal'), 'Localidades: guardar vacío muestra los errores de LocalidadService');
        await p.fill('#nombre', 'Rosario');
        await p.selectOption('#provincia', 'Santa Fe');
        await p.fill('#codigo-postal', '2000');
        await p.fill('#habitantes', '1348725');
        await p.click('[data-testid=boton-guardar]');
        await p.waitForSelector('[data-testid=fila]:nth-child(3)');
        comprobar((await texto(p, 'aviso')).includes('Se agregó la localidad Rosario'), 'Localidades: alta de Rosario por el puente');
        const docs = await sim(p, () => Object.keys(window.puenteSimulado.documentos().localidades || {}).length);
        comprobar(docs === 3, `Localidades: 3 documentos en la colección «localidades» del puente (${docs})`);
        comprobar(p.csp.length === 0 && p.errores.length === 0, 'Localidades: sin errores de consola ni CSP');
        await capturar(p, '02-localidades', 'ABM de localidades (caso base) sobre documents.* del puente: las 2 sembradas más Rosario.',
            'Comprobado: siembra de 2, errores de validación del caso base al guardar vacío, alta guardada como documento JSON (3 en la colección).');
        await p.context().close();
    }

    // 3. Encuesta v1: errores, tres pasos, registro con {encuestaId, versionDefinicion} ---------------------
    {
        const p = await abrir(V1, '?reiniciar=1&definicion=1');
        await p.click('a[href=encuesta]');
        await p.waitForSelector('[data-testid=paso-1]');
        await p.click('[data-testid=boton-siguiente]');
        await p.waitForSelector('[data-testid=error-nombre]');
        comprobar((await texto(p, 'error-edad')).includes('entre 16 y 110'), 'Encuesta: el paso 1 vacío muestra los errores de la definición');
        await capturar(p, '03-encuesta-errores', 'Encuesta dibujada desde la definición v1 (paso 1): errores al intentar avanzar vacío.',
            'Comprobado: el renderizador genérico dibuja el paso 1 y DefinicionValidator da los mensajes de la definición.');

        await completarPaso1(p, 'Ana Pérez');
        await completarPaso2(p);
        comprobar(await hay(p, 'capacidad-camara'), 'Encuesta: capacidad opcional camera.capture explicada en el último paso');
        await completarPaso3(p);
        await p.click('[data-testid=boton-siguiente]');
        await p.waitForSelector('[data-testid=resumen]');
        const fila = await sim(p, () => window.puenteSimulado.outbox()[0]);
        comprobar(fila && fila.encuestaId === 'encuesta-movilidad' && fila.versionDefinicion === 1,
            `Encuesta: encolada con {encuestaId, versionDefinicion} = {${fila?.encuestaId}, ${fila?.versionDefinicion}}`);
        comprobar(fila && fila.versionContenido === '1.0.0', `Encuesta: lleva versionContenido ${fila?.versionContenido}`);
        comprobar(fila && JSON.stringify(fila.valores.medios) === '["colectivo","bicicleta"]' && fila.valores.distancia === 12.5,
            `Encuesta: valores con su tipo (medios ${JSON.stringify(fila?.valores?.medios)}, distancia ${fila?.valores?.distancia})`);
        await p.waitForSelector('[data-testid=estado-outbox]:has-text("1")');
        comprobar(true, 'Encuesta: el evento outbox.changed actualiza «Por enviar: 1»');

        // Evento network.changed desde el cascarón (simulado).
        await sim(p, () => window.puenteSimulado.cambiarRed(false));
        await p.waitForSelector('[data-testid=estado-red]:has-text("Sin red")');
        comprobar(true, 'Encuesta: el evento network.changed cambia el estado a «Sin red»');
        comprobar(p.csp.length === 0 && p.errores.length === 0, 'Encuesta: sin errores de consola ni CSP');
        await capturar(p, '04-encuesta-registrada', 'Encuesta registrada con la definición v1, encolada por encuestas.enqueueRespuesta; red cortada por evento.',
            `Comprobado: fila del outbox con encuestaId=encuesta-movilidad, versionDefinicion=1, versionContenido=1.0.0 y clave ${fila?.claveIdempotencia}; eventos outbox.changed y network.changed.`);
        await p.context().close();
    }

    // 4. P13: encuesta empezada con v1, llega v2, se termina con v1; la próxima usa v2 ---------------------
    {
        const p = await abrir(V1, '?reiniciar=1&definicion=1');
        await p.click('a[href=encuesta]');
        await p.waitForSelector('[data-testid=paso-1]');
        await completarPaso1(p, 'Juan Gómez');          // al avanzar se guarda el borrador con su definición
        await p.goto(`${V1}/encuesta?definicion=2`, { waitUntil: 'load' });   // «llega» la definición v2
        await p.waitForSelector('[data-testid=aviso-version-nueva]');
        comprobar((await texto(p, 'encuesta-version')).includes('v1'), 'P13: la encuesta en curso sigue con la definición v1');
        comprobar(await hay(p, 'paso-2'), 'P13: se retoma en el paso 2 desde el borrador');
        await capturar(p, '05-encuesta-p13-en-curso', 'P13.2: encuesta empezada con la definición v1; llegó la v2 y la pantalla avisa que ésta termina con la v1.',
            'Comprobado: tras recargar con definición vigente v2, el borrador conserva su definición v1 y el paso.');
        await completarPaso2(p);
        await completarPaso3(p);
        await p.click('[data-testid=boton-siguiente]');
        await p.waitForSelector('[data-testid=resumen]');
        const fila = await sim(p, () => window.puenteSimulado.outbox()[0]);
        comprobar(fila?.versionDefinicion === 1, `P13: la respuesta empezada con v1 se envía con versionDefinicion=${fila?.versionDefinicion}`);

        await p.click('[data-testid=boton-reiniciar]');
        await p.waitForSelector('[data-testid=paso-1]');
        comprobar((await texto(p, 'encuesta-version')).includes('v2'), 'P13: la próxima encuesta empieza con la definición v2');
        await completarPaso1(p, 'Juan Gómez');
        await completarPaso2(p);
        comprobar(await hay(p, 'campo-comentario'), 'Definición v2: aparece la pregunta nueva «comentario» sin contenido nuevo');
        comprobar((await p.locator('label[for=pregunta-minutos]').innerText()).includes('por día'), 'Definición v2: etiqueta corregida de minutos');
        await capturar(p, '06-encuesta-v2-paso3', 'La próxima encuesta usa la definición v2: pregunta nueva «Comentario (opcional)» y etiqueta corregida, con el MISMO contenido 1.0.0.',
            'Comprobado: definición v2 dibujada por el renderizador genérico sin publicar contenido nuevo (P1).');
        await p.context().close();
    }

    // 5. Capacidades opcionales que faltan: se ocultan o se explican ------------------------------------------
    {
        const p = await abrir(V1, '?reiniciar=1&catalogo=ninguno');
        await p.waitForSelector('[data-testid="capacidad-device.info"]');
        comprobar(!(await hay(p, 'estado-red')), 'Sin network.status: el indicador de red se oculta');
        comprobar(!(await hay(p, 'inicio-dispositivo')), 'Sin device.info: no se llama ni se muestra el dispositivo');
        comprobar((await texto(p, 'capacidad-network.status')).includes('no trae'), 'Sin network.status: se explica por qué');
        const metodos = await sim(p, () => window.puenteSimulado.llamadas.map(s => s.method));
        comprobar(!metodos.includes('device.info') && !metodos.includes('network.status'), 'Capacidades faltantes: el contenido no las llama');
        await capturar(p, '07-capacidades-faltantes', 'Cascarón simulado con catálogo vacío: las capacidades opcionales se ocultan (red) o se explican (Inicio).',
            'Comprobado: sin llamadas a device.info ni network.status, sin indicador de red, motivo visible por capacidad.');
        await p.context().close();
    }

    // 6. Puente incompatible: no se muestran pantallas y NO se avisa Listo (el cascarón vuelve a previous) ----
    {
        const p = await abrir(V1, '?reiniciar=1&puente=2.0.0');
        comprobar((await texto(p, 'estado-puente')).includes('no es compatible'), 'Puente 2.0.0: el contenido dice que no es compatible');
        await p.waitForTimeout(500);
        comprobar(await sim(p, () => window.puenteSimulado.listo()) === null, 'Puente incompatible: NO avisa Listo (P2: rollback por marca)');
        await capturar(p, '08-puente-incompatible', 'El cascarón informa puente 2.0.0; el contenido (compatible >=1.1.0 <2.0.0) no muestra pantallas.',
            'Comprobado: mensaje de incompatibilidad y bridge.ready NO llamado, para que el próximo arranque vuelva a previous.');
        await p.context().close();
    }

    // 7. Sin puente: el contenido abierto fuera del cascarón ------------------------------------------------
    {
        const p = await abrir(V1, '?reiniciar=1&sinpuente=1');
        comprobar((await texto(p, 'estado-puente')).includes('sólo dentro de la app'), 'Sin puente: el contenido lo dice');
        await capturar(p, '09-sin-puente', 'El contenido abierto en un navegador común, sin window.HybridWebView.',
            'Comprobado: estado «sin puente» en lugar de pantallas que no podrían guardar.');
        await p.context().close();
    }

    // 8. Contenido v2 (1.1.0): el cambio visible ------------------------------------------------------------
    {
        const p = await abrir(V2, '?reiniciar=1&definicion=1');
        await p.waitForSelector('[data-testid=novedades]');
        comprobar(await texto(p, 'inicio-version-contenido') === '1.1.0', 'v2: Inicio muestra contenido 1.1.0');
        await p.waitForSelector('[data-testid=novedades-localidades]:has-text("2")');
        comprobar(true, 'v2: tarjeta de novedades con la cuenta de localidades (2)');
        await p.waitForFunction(() => window.puenteSimulado.listo() !== null);
        comprobar(await sim(p, () => window.puenteSimulado.listo()) === '1.1.0', 'v2: avisó Listo(1.1.0)');
        comprobar(p.csp.length === 0 && p.errores.length === 0, 'v2: sin errores de consola ni CSP');
        await capturar(p, '10-inicio-v2', 'Inicio del contenido v2 (1.1.0): cabecera de otro color y tarjeta «Novedades de esta versión».',
            'Comprobado: versión 1.1.0, tarjeta de novedades visible, Listo(1.1.0) recibido por el puente.');
        await p.context().close();
    }
} catch (e) {
    fallas.push('excepción: ' + e.message);
    console.log('FALLA excepción: ' + e.stack);
    if (ultimaPagina) {
        // Diagnóstico: dónde quedó la pantalla y qué dijo la consola. No es evidencia de la prueba.
        console.log('errores de la página: ' + ultimaPagina.errores.join(' | '));
        console.log('texto visible: ' + (await ultimaPagina.locator('body').innerText().catch(() => '?')).replace(/\s+/g, ' ').slice(0, 1500));
        await ultimaPagina.screenshot({ path: path.join(SALIDA, 'diagnostico-falla.png'), fullPage: true }).catch(() => {});
    }
} finally {
    await navegador.close();
    servidorV1.close();
    servidorV2.close();
}

// --- Folio 038: red combinada y política de acciones (constructor de red) -----------------------------------
// Aviso permanente con cada estado, «Enviar ahora» deshabilitado con motivo, encuesta pendiente sin red,
// reconexión automática (el outbox se vacía solo al volver «enLinea») y el doble cerrojo (AccionBloqueada).
// Abre su propio navegador y servidor: el de arriba ya se cerró en el finally.
{
    const navegadorRed = await chromium.launch({ args: ['--no-sandbox'] });
    const servidorRed = await servir(process.env.V1, 5111);
    const BASE = 'http://127.0.0.1:5111';
    let p = null;
    const abrirRed = async consulta => {
        const contexto = await navegadorRed.newContext({ viewport: { width: 412, height: 915 }, deviceScaleFactor: 1, locale: 'es-AR' });
        const pagina = await contexto.newPage();
        pagina.errores = [];
        pagina.on('pageerror', e => pagina.errores.push('pageerror: ' + e.message));
        pagina.on('console', m => { if (m.type() === 'error') pagina.errores.push('console: ' + m.text()); });
        await pagina.goto(`${BASE}/${consulta}`, { waitUntil: 'load' });
        await pagina.waitForSelector('[data-testid=linea-de-estado]', { timeout: 60000 });
        return pagina;
    };
    try {
        // R1. En línea: una línea discreta «En línea».
        p = await abrirRed('?reiniciar=1&definicion=1');
        await p.waitForSelector('[data-testid=aviso-red][data-estado=enLinea]');
        comprobar((await texto(p, 'aviso-red')).includes('En línea'), 'Red: en línea el aviso dice «En línea»');
        const metodos = await sim(p, () => window.puenteSimulado.llamadas.map(s => s.method));
        comprobar(metodos.includes('actions.policy'), 'Red: el contenido pide actions.policy al arrancar');

        // R2. Sin conexión: aviso permanente, encuesta registrada queda pendiente con su motivo.
        await sim(p, () => window.puenteSimulado.cambiarEstadoDeRed('sinRed'));
        await p.waitForSelector('[data-testid=aviso-red][data-estado=sinRed]');
        comprobar((await texto(p, 'aviso-red')).includes('Sin conexión'), 'Red: sin red el aviso dice «Sin conexión»');
        comprobar((await texto(p, 'aviso-red-motivo')).includes('se envía cuando vuelva'), 'Red: el aviso lleva el motivo del cascarón');
        await p.click('a[href=encuesta]');
        await p.waitForSelector('[data-testid=paso-1]');
        comprobar(await hay(p, 'aviso-red'), 'Red: el aviso sigue visible al cambiar de página (permanente)');
        await completarPaso1(p, 'Ana Sin Red');
        await completarPaso2(p);
        await completarPaso3(p);
        await p.click('[data-testid=boton-siguiente]');
        await p.waitForSelector('[data-testid=resumen]');
        comprobar((await texto(p, 'encuesta-pendiente')).includes('Sin conexión'), 'Red: la encuesta (Encolable) se registra sin red y dice que quedó pendiente');
        await p.waitForSelector('[data-testid=estado-outbox]:has-text("1")');
        const boton = p.locator('[data-testid=boton-enviar-ahora]');
        comprobar(await boton.isDisabled(), 'Red: «Enviar ahora» (SoloEnLinea) deshabilitado sin red');
        comprobar((await texto(p, 'motivo-outbox.enviarAhora')).includes('Sin conexión'), 'Red: «Enviar ahora» dice por qué está deshabilitado');
        await capturar(p, '11-red-sin-conexion', 'Sin conexión (estado sinRed): aviso permanente arriba, encuesta registrada «pendiente», «Enviar ahora» deshabilitado con motivo.',
            'Comprobado: aviso-red data-estado=sinRed con motivo, encuesta-pendiente, boton-enviar-ahora disabled y motivo-outbox.enviarAhora «Sin conexión».');

        // Doble cerrojo: aunque el contenido llamara igual, el puente rechaza con AccionBloqueada (-32005).
        const rechazo = await p.evaluate(async () => JSON.parse(await window.HybridWebView.InvokeDotNet('Call',
            [JSON.stringify({ jsonrpc: '2.0', v: '1.1.0', id: 'red-1', method: 'outbox.sendNow' })])));
        comprobar(rechazo.error?.code === -32005, `Red: outbox.sendNow sin red se rechaza con AccionBloqueada (${rechazo.error?.code} ${rechazo.error?.message})`);

        // R3. Hay red pero el servidor no responde.
        await sim(p, () => window.puenteSimulado.cambiarEstadoDeRed('redSinServidor'));
        await p.waitForSelector('[data-testid=aviso-red][data-estado=redSinServidor]');
        comprobar((await texto(p, 'aviso-red')).includes('Sin acceso al servidor'), 'Red: sin servidor el aviso dice «Sin acceso al servidor»');
        comprobar((await texto(p, 'motivo-outbox.enviarAhora')).includes('El servidor no responde'), 'Red: sin servidor «Enviar ahora» dice «El servidor no responde»');
        await capturar(p, '12-red-sin-servidor', 'Hay red pero el servidor no responde (redSinServidor): aviso «Sin acceso al servidor»; lo pendiente espera.',
            'Comprobado: aviso-red data-estado=redSinServidor, «Enviar ahora» deshabilitado con motivo «El servidor no responde».');

        // R4. Reconexión automática: vuelve «enLinea» y lo pendiente sale solo, sin tocar nada.
        await sim(p, () => window.puenteSimulado.cambiarEstadoDeRed('enLinea'));
        await p.waitForSelector('[data-testid=aviso-red][data-estado=enLinea]');
        await p.waitForSelector('[data-testid=estado-outbox]:has-text("Por enviar: 0")');
        comprobar(true, 'Red: al volver en línea el outbox se vacía solo («Por enviar: 0») y el aviso dice «En línea»');
        comprobar(!(await hay(p, 'boton-enviar-ahora')), 'Red: sin pendientes no se ofrece «Enviar ahora»');
        await capturar(p, '13-red-reconexion', 'Volvió la conexión (enLinea): el cascarón simulado vació el outbox solo y el contenido lo muestra.',
            'Comprobado: aviso-red data-estado=enLinea y «Por enviar: 0» sin ninguna acción de la persona.');

        // R5. Datos móviles.
        await sim(p, () => window.puenteSimulado.cambiarEstadoDeRed('enLineaMedida'));
        await p.waitForSelector('[data-testid=aviso-red][data-estado=enLineaMedida]');
        comprobar((await texto(p, 'aviso-red')).includes('datos móviles'), 'Red: en línea medida el aviso dice «datos móviles»');
        comprobar(p.errores.length === 0, `Red: sin errores de consola (${p.errores.join(' | ')})`);
        await p.context().close();

        // R6. En línea con un pendiente (encolado ya en línea: el simulador no tiene pasada propia):
        // «Enviar ahora» habilitado, sin motivo, y envía.
        p = await abrirRed('?reiniciar=1&definicion=1&estado=enLinea');
        await p.click('a[href=encuesta]');
        await p.waitForSelector('[data-testid=paso-1]');
        await completarPaso1(p, 'Juan En Línea');
        await completarPaso2(p);
        await completarPaso3(p);
        await p.click('[data-testid=boton-siguiente]');
        await p.waitForSelector('[data-testid=resumen]');
        comprobar(!(await hay(p, 'encuesta-pendiente')), 'Red: en línea la encuesta no dice «quedó pendiente»');
        await p.waitForSelector('[data-testid=boton-enviar-ahora]:not([disabled])');
        comprobar(!(await hay(p, 'motivo-outbox.enviarAhora')), 'Red: en línea «Enviar ahora» habilitado y sin motivo');
        await p.click('[data-testid=boton-enviar-ahora]');
        await p.waitForSelector('[data-testid=estado-outbox]:has-text("Por enviar: 0")');
        const envios = await sim(p, () => window.puenteSimulado.llamadas.filter(s => s.method === 'outbox.sendNow').length);
        comprobar(envios === 1, `Red: «Enviar ahora» llamó outbox.sendNow (${envios}) y lo pendiente salió («Por enviar: 0»)`);
        await p.context().close();
    } catch (e) {
        fallas.push('excepción (red): ' + e.message);
        console.log('FALLA excepción (red): ' + e.stack);
        if (p) {
            console.log('texto visible: ' + (await p.locator('body').innerText().catch(() => '?')).replace(/\s+/g, ' ').slice(0, 1500));
            await p.screenshot({ path: path.join(SALIDA, 'diagnostico-falla-red.png'), fullPage: true }).catch(() => {});
        }
    } finally {
        await navegadorRed.close();
        servidorRed.close();
    }
}

// --- Folio 038: sesión (constructor de autenticación) --------------------------------------------------------
// Puerta de sesión (DISENO §3.7) en modo dispositivo y oidc: ingreso (y su botón deshabilitado sin servidor),
// desbloqueo, vencida con banda de reingreso sin cortar el trabajo local, cierre que conserva pendientes, y que
// ningún sobre lleve tokens (P3). Abre su propio navegador y servidor.
{
    const navegadorSesion = await chromium.launch({ args: ['--no-sandbox'] });
    const servidorSesion = await servir(process.env.V1, 5112);
    const BASE = 'http://127.0.0.1:5112';
    let p = null;
    const abrirSesion = async consulta => {
        const contexto = await navegadorSesion.newContext({ viewport: { width: 412, height: 915 }, deviceScaleFactor: 1, locale: 'es-AR' });
        const pagina = await contexto.newPage();
        pagina.errores = [];
        pagina.on('pageerror', e => pagina.errores.push('pageerror: ' + e.message));
        pagina.on('console', m => { if (m.type() === 'error') pagina.errores.push('console: ' + m.text()); });
        await pagina.goto(`${BASE}/${consulta}`, { waitUntil: 'load' });
        await pagina.waitForSelector('[data-testid=linea-de-estado]', { timeout: 60000 });
        return pagina;
    };
    // P3 — Ninguna respuesta ni evento del puente lleva un token: sólo SesionInformada.
    const sinTokens = async pagina => !JSON.stringify(await sim(pagina, () => ({
        llamadas: window.puenteSimulado.llamadas, sesion: window.puenteSimulado.sesion()
    }))).match(/token/i);
    try {
        // S1. Modo dispositivo (APK sin -p:FcOidcAutoridad): la página como en A3, sin puerta ni usuario.
        p = await abrirSesion('?reiniciar=1&definicion=1&sesion=dispositivo&estado=enLinea');
        await p.waitForSelector('[data-testid=aviso-red]');
        comprobar(!(await hay(p, 'puerta')) && !(await hay(p, 'sesion-usuario')), 'Sesión: en modo dispositivo no hay puerta ni usuario (como A3)');
        await p.context().close();

        // S2. Oidc sin cuenta y sin conexión: «Ingresar» deshabilitado con motivo (sesion.ingresar es SoloEnLinea).
        p = await abrirSesion('?reiniciar=1&definicion=1&sesion=oidc-sinCuenta&estado=sinRed&ingreso=navegador');
        await p.waitForSelector('[data-testid=puerta][data-estado=sinCuenta]');
        comprobar(await p.locator('[data-testid=puerta-ingresar]').isDisabled(), 'Sesión: sin cuenta y sin red «Ingresar» está deshabilitado');
        comprobar((await texto(p, 'motivo-sesion.ingresar')).includes('Sin conexión'), 'Sesión: «Ingresar» dice por qué (Sin conexión)');
        comprobar(!(await hay(p, 'paso-1')) && !(await hay(p, 'sesion-usuario')), 'Sesión: sin cuenta la página no se muestra');
        await capturar(p, '21-sesion-sin-cuenta-sin-red', 'Sesión oidc sin cuenta y sin conexión: puerta «Ingresá con tu cuenta» con el botón deshabilitado y su motivo.',
            'Comprobado: puerta data-estado=sinCuenta, puerta-ingresar disabled, motivo-sesion.ingresar «Sin conexión».');

        // S3. Vuelve la red: se habilita; ingresar → activa, se ofrece la seguridad del teléfono.
        await sim(p, () => window.puenteSimulado.cambiarEstadoDeRed('enLinea'));
        await p.waitForSelector('[data-testid=puerta-ingresar]:not([disabled])');
        await p.click('[data-testid=puerta-ingresar]');
        await p.waitForSelector('[data-testid=sesion-usuario]');
        comprobar((await texto(p, 'sesion-usuario')).includes('encuestador'), 'Sesión: después de ingresar se ve el usuario');
        comprobar(await hay(p, 'sesion-ofrecer-desbloqueo'), 'Sesión: activa con desbloqueo disponible ofrece «usar la seguridad del teléfono»');
        await capturar(p, '22-sesion-activa-ofrece-desbloqueo', 'Sesión oidc activa recién ingresada: usuario arriba y la oferta «¿Usar la seguridad del teléfono para entrar?».',
            'Comprobado: auth.signIn simulado, sesion-usuario «encuestador», sesion-ofrecer-desbloqueo visible.');
        await p.click('[data-testid=sesion-activar-desbloqueo]');
        await p.waitForFunction(() => window.puenteSimulado.sesion().desbloqueoLocal === 'activado');
        comprobar(!(await hay(p, 'sesion-ofrecer-desbloqueo')), 'Sesión: activado el desbloqueo, la oferta desaparece');

        // S4. Se bloquea (arranque en frío o segundo plano): puerta «Desbloquear» → activa.
        await sim(p, () => window.puenteSimulado.cambiarSesion({ estado: 'bloqueada' }));
        await p.waitForSelector('[data-testid=puerta][data-estado=bloqueada]');
        comprobar(!(await hay(p, 'sesion-usuario')), 'Sesión: bloqueada no muestra la página');
        await capturar(p, '23-sesion-bloqueada', 'Sesión oidc bloqueada: puerta «Desbloqueá la app» (en el teléfono el diálogo es del sistema).',
            'Comprobado: puerta data-estado=bloqueada y la página oculta.');
        await p.click('[data-testid=puerta-desbloquear]');
        await p.waitForSelector('[data-testid=sesion-usuario]');
        comprobar(true, 'Sesión: «Desbloquear» llama auth.unlock y vuelve a la página');

        // S4b. D-03 = T1: sin bloqueo de pantalla el desbloqueo no se ofrece y se dice por qué, una vez.
        await sim(p, () => window.puenteSimulado.cambiarSesion({ desbloqueoLocal: 'noDisponible',
            motivo: 'Este teléfono no tiene bloqueo de pantalla: configurá un PIN, patrón o huella para usar la seguridad del teléfono.' }));
        await p.waitForSelector('[data-testid=sesion-desbloqueo-no-disponible]');
        comprobar((await texto(p, 'sesion-desbloqueo-no-disponible')).includes('no tiene bloqueo de pantalla')
            && !(await hay(p, 'sesion-ofrecer-desbloqueo')), 'Sesión: sin bloqueo de pantalla no se ofrece el desbloqueo y se dice por qué (D-03)');
        await p.click('[data-testid=sesion-desbloqueo-no-disponible] button');
        await p.waitForSelector('[data-testid=sesion-desbloqueo-no-disponible]', { state: 'detached' });
        await sim(p, () => window.puenteSimulado.cambiarSesion({ desbloqueoLocal: 'activado', motivo: undefined }));

        // S5. Vencida: se sigue trabajando (la encuesta se registra y queda pendiente), con banda de reingreso.
        await sim(p, () => window.puenteSimulado.cambiarSesion({ estado: 'vencida', motivo: 'Tu sesión venció: ingresá para enviar lo pendiente.' }));
        await p.waitForSelector('[data-testid=sesion-reingreso][data-estado=vencida]');
        await p.click('a[href=encuesta]');
        await p.waitForSelector('[data-testid=paso-1]');
        await completarPaso1(p, 'Eva Vencida');
        await completarPaso2(p);
        await completarPaso3(p);
        await p.click('[data-testid=boton-siguiente]');
        await p.waitForSelector('[data-testid=resumen]');
        comprobar((await texto(p, 'encuesta-pendiente')).includes('vuelvas a ingresar'), 'Sesión: vencida, la encuesta se registra y queda pendiente hasta reingresar');
        comprobar(await hay(p, 'puerta-reingresar'), 'Sesión: vencida muestra «Ingresar de nuevo» sin tapar la página');
        await capturar(p, '24-sesion-vencida', 'Sesión oidc vencida: banda «Tu sesión venció» con reingreso; la encuesta se registró igual y quedó pendiente.',
            'Comprobado: sesion-reingreso data-estado=vencida, encuesta-pendiente «…vuelvas a ingresar», puerta-reingresar visible.');

        // S6. Cerrar sesión: confirmación que dice que lo pendiente se conserva; pendientesConservados = 1.
        await p.click('[data-testid=sesion-cerrar]');
        comprobar((await p.locator('.fc-sesion__confirmar').innerText()).includes('se conserva'), 'Sesión: la confirmación de cierre dice que lo pendiente se conserva');
        await p.click('[data-testid=sesion-cerrar-confirmar]');
        await p.waitForSelector('[data-testid=puerta][data-estado=sinCuenta]');
        comprobar((await texto(p, 'sesion-pendientes-conservados')).includes('Quedó 1 pendiente'), 'Sesión: al cerrar, «Quedó 1 pendiente guardado»');
        const filas = await sim(p, () => window.puenteSimulado.outbox().length);
        comprobar(filas === 1, `Sesión: cerrar sesión no borra la fila pendiente (outbox ${filas})`);
        comprobar(await sinTokens(p), 'Sesión: ningún sobre ni la sesión informada llevan tokens (P3)');
        comprobar(p.errores.length === 0, `Sesión: sin errores de consola (${p.errores.join(' | ')})`);
        await capturar(p, '25-sesion-cerrada-conserva', 'Sesión cerrada: vuelve la puerta de ingreso y avisa que el pendiente quedó guardado para esa cuenta.',
            'Comprobado: auth.signOut simulado → sinCuenta, sesion-pendientes-conservados «1», outbox con 1 fila.');
        await p.context().close();

        // S7. Ingreso cancelado (la persona cierra el navegador): sigue sin cuenta y dice por qué.
        p = await abrirSesion('?reiniciar=1&definicion=1&sesion=oidc-sinCuenta&estado=enLinea&sesionFalla=cancelar&ingreso=navegador');
        await p.waitForSelector('[data-testid=puerta-ingresar]:not([disabled])');
        await p.click('[data-testid=puerta-ingresar]');
        await p.waitForSelector('[data-testid=sesion-error]');
        comprobar((await texto(p, 'sesion-error')).includes('Cerraste el navegador'), 'Sesión: ingreso cancelado (-32006) se explica y sigue sin cuenta');
        await p.context().close();

        // S8. Login tradicional (puente 1.2): formulario propio, clave incorrecta, ingreso, «Bloquear» y desbloqueo.
        p = await abrirSesion('?reiniciar=1&definicion=1&sesion=oidc-sinCuenta&estado=sinRed&ingreso=formulario');
        await p.waitForSelector('[data-testid=puerta-usuario]');
        comprobar(await hay(p, 'puerta-clave') && !(await hay(p, 'inicio-version-puente')), 'Login tradicional: formulario visible y la página sin montar');
        comprobar(await hay(p, 'puerta-tarjeta') && await hay(p, 'puerta-emblema'), 'Login tradicional: tarjeta con emblema');
        await capturar(p, '26a-login-vacio', 'Login tradicional rediseñado: tarjeta, campos con ícono, ojito y «Ingresar» (sin red: deshabilitado con su motivo).',
            'Comprobado: puerta-tarjeta, puerta-emblema, puerta-usuario, puerta-clave.');
        await p.fill('[data-testid=puerta-usuario]', 'encuestador');
        await p.fill('[data-testid=puerta-clave]', 'mal');
        comprobar(await p.locator('[data-testid=puerta-ingresar]').isDisabled(), 'Login tradicional: sin red «Ingresar» está deshabilitado');
        comprobar(await p.getAttribute('[data-testid=puerta-clave]', 'type') === 'password', 'Login tradicional: la clave va oculta');
        await p.click('[data-testid=puerta-mostrar-clave]');
        comprobar(await p.getAttribute('[data-testid=puerta-clave]', 'type') === 'text', 'Login tradicional: «Mostrar» deja ver la clave');
        comprobar(await p.getAttribute('[data-testid=puerta-mostrar-clave]', 'aria-pressed') === 'true'
            && await p.getAttribute('[data-testid=puerta-icono-ojo]', 'data-estado') === 'visible'
            && (await texto(p, 'puerta-clave-visible')).includes('visible'), 'Login tradicional: el ojito alterna aria-pressed, ícono y aviso');
        await capturar(p, '26b-login-ojito', 'Login tradicional: ojito activo, la clave se ve en monoespaciada.',
            'Comprobado: aria-pressed=true, puerta-icono-ojo data-estado=visible, puerta-clave-visible.');
        await sim(p, () => window.puenteSimulado.cambiarEstadoDeRed('enLinea'));
        await p.waitForSelector('[data-testid=puerta-ingresar]:not([disabled])');
        await p.fill('[data-testid=puerta-usuario]', '');
        await p.fill('[data-testid=puerta-clave]', '');
        const llamadasAntes = await sim(p, () => window.puenteSimulado.llamadas.length);
        await p.click('[data-testid=puerta-ingresar]');
        await p.waitForSelector('[data-testid=puerta-usuario-error]');
        comprobar(await hay(p, 'puerta-clave-error') && !(await hay(p, 'sesion-error'))
            && await sim(p, () => window.puenteSimulado.llamadas.length) === llamadasAntes
            && await p.evaluate(() => document.activeElement?.id) === 'puerta-usuario',
            'Login tradicional: vacío muestra el error junto al campo, enfoca el usuario y no llama a la sesión');
        await capturar(p, '26c-login-campos-vacios', 'Login tradicional: enviar vacío marca los dos campos y dice qué falta.',
            'Comprobado: puerta-usuario-error, puerta-clave-error, foco en el usuario, sin llamadas al puente.');
        await p.fill('[data-testid=puerta-usuario]', 'encuestador');
        await p.fill('[data-testid=puerta-clave]', 'mal');
        await p.click('[data-testid=puerta-ingresar]');
        await p.waitForSelector('[data-testid=sesion-error]');
        comprobar(await p.getAttribute('[data-testid=sesion-error]', 'role') === 'alert' && await p.inputValue('[data-testid=puerta-usuario]') === 'encuestador'
            && await p.evaluate(() => document.activeElement?.id) === 'puerta-clave', 'Login tradicional: el error tiene role=alert, conserva el usuario y enfoca la clave');
        await capturar(p, '26d-login-error', 'Login tradicional: credenciales rechazadas, banda de error y la clave vacía con foco.',
            'Comprobado: sesion-error role=alert, usuario conservado, foco en puerta-clave.');
        comprobar((await texto(p, 'sesion-error')).includes('incorrectos') && await p.inputValue('[data-testid=puerta-clave]') === '',
            'Login tradicional: clave incorrecta se explica y el campo de la clave queda vacío');
        await p.fill('[data-testid=puerta-clave]', 'demo-encuestador');
        await capturar(p, '26-login-tradicional', 'Login tradicional: formulario propio de la app (usuario y clave), sin navegador.',
            'Comprobado: puerta-usuario, puerta-clave (oculta/mostrar), «Ingresar» deshabilitado sin red, error legible con clave incorrecta.');
        await p.click('[data-testid=puerta-ingresar]');
        if (await hay(p, 'puerta-spinner')) await capturar(p, '26e-login-ingresando', 'Login tradicional: ingresando, el formulario queda y el botón muestra el spinner.',
            'Captura oportunista inmediatamente después del clic (el puente simulado responde enseguida).');
        await p.waitForSelector('[data-testid=sesion-usuario]');
        comprobar(!(await hay(p, 'sesion-bloquear')), 'Login tradicional: sin desbloqueo activado no hay «Bloquear»');
        await p.click('[data-testid=sesion-activar-desbloqueo]');
        await p.waitForSelector('[data-testid=sesion-bloquear]');
        await capturar(p, '27-login-tradicional-activa', 'Sesión activa con «Bloquear» junto a «Cerrar sesión».',
            'Comprobado: auth.signInWithPassword simulado, desbloqueo activado, sesion-bloquear visible.');
        await p.click('[data-testid=sesion-bloquear]');
        await p.waitForSelector('[data-testid=puerta][data-estado=bloqueada]');
        comprobar(true, 'Login tradicional: «Bloquear» llama auth.lock y la puerta pasa a bloqueada');
        await capturar(p, '28-login-tradicional-bloqueada', 'Login tradicional bloqueado: tarjeta con candado, leyenda de la seguridad del teléfono y «Desbloquear».',
            'Comprobado: puerta data-estado=bloqueada, puerta-desbloquear.');
        await p.click('[data-testid=puerta-desbloquear]');
        await p.waitForSelector('[data-testid=sesion-usuario]');
        comprobar(!JSON.stringify(await sim(p, () => window.puenteSimulado.sesion())).includes('demo-encuestador'),
            'Login tradicional: la clave no vuelve en la sesión informada');
        comprobar(p.errores.length === 0, `Login tradicional: sin errores de consola (${p.errores.join(' | ')})`);
        await p.context().close();
    } catch (e) {
        fallas.push('excepción (sesión): ' + e.message);
        console.log('FALLA excepción (sesión): ' + e.stack);
        if (p) {
            console.log('texto visible: ' + (await p.locator('body').innerText().catch(() => '?')).replace(/\s+/g, ' ').slice(0, 1500));
            await p.screenshot({ path: path.join(SALIDA, 'diagnostico-falla-sesion.png'), fullPage: true }).catch(() => {});
        }
    } finally {
        await navegadorSesion.close();
        servidorSesion.close();
    }
}

console.log(`\n${comprobadas.length} comprobaciones OK, ${fallas.length} fallas`);
for (const f of fallas) console.log('  FALLA ' + f);
process.exit(fallas.length === 0 ? 0 : 1);
