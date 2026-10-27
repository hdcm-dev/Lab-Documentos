// PUENTE SIMULADO — sólo para probar el contenido en un navegador común, sin el cascarón.
//
// En la app, _framework/hybridwebview.js lo sirve el HybridWebView del cascarón y define
// window.HybridWebView.InvokeDotNet. En la prueba en navegador, el servidor de prueba sirve ESTE archivo en
// esa misma ruta: el contenido no se entera de la diferencia. Responde los métodos de la tabla del puente
// (Core/Contracts/Puente/TablaDeMetodos.cs) con datos en memoria del navegador.
//
// No es el cascarón: no firma, no tiene SQLite ni outbox real, no envía nada. Sirve para ver que el contenido
// arma bien los sobres, dibuja las definiciones y reacciona a capacidades faltantes.
//
// Se configura con la dirección de la página (se recuerda en sessionStorage):
//   ?definicion=1|2        versión vigente de encuesta-movilidad (archivo definiciones/encuesta-movilidad/vN.json)
//   ?puente=1.1.0          versión del puente que informa el handshake
//   ?catalogo=a,b          ids del catálogo (por defecto device.info,network.status,auth.session; «ninguno» = vacío)
//   ?red=0|1               conectividad que informa network.status
//   ?estado=enLinea        estado combinado de red (folio 038): sinRed, redSinServidor, enLinea, enLineaMedida
//   ?sesion=dispositivo    modo y estado de la sesión (folio 038): dispositivo, o oidc-<estado> (oidc-sinCuenta, oidc-activa…)
//   ?sesionFalla=cancelar  auth.signIn y auth.unlock contestan AutenticacionCancelada (-32006)
//   ?sinpuente=1           no define window.HybridWebView (el contenido abierto fuera del cascarón)
//   ?reiniciar=1           borra los datos guardados
// Y expone window.puenteSimulado para que la prueba mire y provoque cosas (eventos, outbox, llamadas).
(function () {
    'use strict';

    const CLAVE_CONFIG = 'puente-simulado.config';
    const CLAVE_DATOS = 'puente-simulado.datos';

    // --- Configuración -----------------------------------------------------------------------------------
    const consulta = new URLSearchParams(location.search);
    const config = Object.assign(
        { definicion: '1', puente: '1.1.0', catalogo: 'device.info,network.status,auth.session', red: '1', estado: '', sinpuente: '0', sesion: 'dispositivo', sesionFalla: '' },
        JSON.parse(sessionStorage.getItem(CLAVE_CONFIG) || '{}'));
    for (const clave of Object.keys(config)) {
        if (consulta.has(clave)) config[clave] = consulta.get(clave);
    }
    sessionStorage.setItem(CLAVE_CONFIG, JSON.stringify(config));
    if (consulta.has('reiniciar')) localStorage.removeItem(CLAVE_DATOS);

    // --- Datos (documentos y outbox), en localStorage para sobrevivir a una recarga -----------------------
    const datos = JSON.parse(localStorage.getItem(CLAVE_DATOS) || '{"documentos":{},"outbox":[]}');
    const guardar = () => localStorage.setItem(CLAVE_DATOS, JSON.stringify(datos));
    const llamadas = [];       // cada sobre recibido, para que la prueba lo mire
    let listo = null;          // la versión que avisó bridge.ready

    // Versión de cada entrada como en Core/Contracts/Puente/CatalogoDeCapacidades.cs (puente 1.1).
    const versionDeEntrada = { 'device.info': '1.0.0', 'network.status': '1.1.0', 'auth.session': '1.0.0' };
    const catalogo = config.catalogo === 'ninguno' ? [] : config.catalogo.split(',').filter(Boolean)
        .map(id => ({ id: id, version: versionDeEntrada[id] || '1.0.0' }));
    const tiene = id => catalogo.some(e => e.id === id);

    // --- Métodos -----------------------------------------------------------------------------------------
    class ErrorDelPuente extends Error {
        constructor(code, message) { super(message); this.code = code; }
    }

    const coleccion = nombre => (datos.documentos[nombre] = datos.documentos[nombre] || {});

    const metodos = {
        'bridge.handshake': () => ({
            versionPuente: config.puente,
            catalogo: catalogo,
            huellaContrato: 'simulado00000000000000000000000000000000000000000000000000000000'
        }),
        'bridge.ready': p => { listo = p.version; return { aceptado: true }; },

        'documents.get': p => {
            const d = coleccion(p.coleccion)[p.id];
            return d ? { documento: d } : {};
        },
        'documents.list': p => ({ documentos: Object.values(coleccion(p.coleccion)) }),
        'documents.put': p => {
            const actual = coleccion(p.coleccion)[p.id];
            if (p.versionEsperada !== undefined && (!actual || actual.version !== p.versionEsperada)) {
                throw new ErrorDelPuente(-32004, 'El documento cambió: versión esperada ' + p.versionEsperada + '.');
            }
            const version = actual ? actual.version + 1 : 1;
            coleccion(p.coleccion)[p.id] = { coleccion: p.coleccion, id: p.id, versionDeForma: p.versionDeForma, version: version, json: p.json };
            guardar();
            return { version: version };
        },
        'documents.delete': p => {
            const habia = !!coleccion(p.coleccion)[p.id];
            delete coleccion(p.coleccion)[p.id];
            guardar();
            return { borrado: habia };
        },

        'encuestas.getDefinicion': async p => {
            if (p.encuestaId !== 'encuesta-movilidad') throw new ErrorDelPuente(-32602, 'Encuesta desconocida: ' + p.encuestaId);
            const r = await fetch('sim/definiciones/encuesta-movilidad/v' + config.definicion + '.json');
            return { definicion: await r.json() };
        },
        'encuestas.enqueueRespuesta': p => {
            const e = p.envio;
            if (!e || !e.claveIdempotencia || !e.encuestaId || !(e.versionDefinicion >= 1) || !e.versionContenido) {
                throw new ErrorDelPuente(-32602, 'Falta claveIdempotencia, encuestaId, versionDefinicion o versionContenido.');
            }
            const repetida = datos.outbox.some(f => f.claveIdempotencia === e.claveIdempotencia);
            if (!repetida) { datos.outbox.push(e); guardar(); }
            emitir('outbox.changed', { pendientes: datos.outbox.length, rechazadas: 0 });
            return { claveIdempotencia: e.claveIdempotencia, encolada: !repetida };
        },
        'outbox.status': () => ({ pendientes: datos.outbox.length, rechazadas: 0 }),

        'device.info': () => ({ fabricante: 'Simulador', modelo: 'Navegador', plataforma: 'Web', versionSistema: navigator.userAgent.match(/Chrome\/[\d.]+/)?.[0] || '?' }),
        'network.status': () => Object.assign({ conectado: config.red === '1', estado: estadoDeRed() }, motivoDeRed() ? { motivo: motivoDeRed() } : {}),

        // --- Folio 038: red combinada y acciones (sección del constructor de red) ---------------------------
        // Evalúa como el cascarón (EvaluadorDeAcciones, DISENO §4.3) con el estado de red y la sesión simulados.
        'actions.policy': () => ({ acciones: politica().map(evaluar) }),
        'actions.check': p => {
            const a = politica().find(x => x.accion === p.accion);
            if (!a) throw new ErrorDelPuente(-32602, 'Acción desconocida: ' + p.accion);
            return { evaluacion: evaluar(a) };
        },
        // «Enviar ahora»: la guardia (más abajo) ya la dejó pasar; «envía» todo lo pendiente.
        'outbox.sendNow': () => ({ enviadas: vaciarOutbox(), pendientes: datos.outbox.length, rechazadas: 0 }),

        // --- Folio 038: sesión (sección del constructor de autenticación) -----------------------------------
        // Simulación mínima del arquitecto: modo dispositivo, activa. Nunca devuelve tokens (P3).
        // Como Shell/Auth/SesionDelUsuario: el navegador del sistema y el diálogo del sistema no existen acá; el ingreso
        // y el desbloqueo salen bien al instante, salvo que la prueba pida cancelarlos (?sesionFalla=cancelar).
        // Nunca devuelve tokens (P3): sólo SesionInformada.
        'auth.status': () => ({ sesion: sesionSimulada() }),
        'auth.signIn': () => {
            exigirOidc();
            if (config.sesionFalla === 'cancelar') throw new ErrorDelPuente(-32006, 'Cerraste el navegador sin terminar el ingreso.');
            return { sesion: cambiarSesion({ estado: 'activa', usuario: 'encuestador', motivo: undefined }) };
        },
        'auth.enableUnlock': () => {
            exigirOidc();
            return { sesion: cambiarSesion({ desbloqueoLocal: 'activado' }) };
        },
        'auth.unlock': () => {
            exigirOidc();
            if (sesionSimulada().estado !== 'bloqueada') return { sesion: sesionSimulada() };
            if (config.sesionFalla === 'cancelar') throw new ErrorDelPuente(-32006, 'Cancelaste el desbloqueo.');
            return { sesion: cambiarSesion({ estado: 'activa', motivo: undefined }) };
        },
        'auth.signOut': () => {
            exigirOidc();
            // P4 — Cerrar sesión no borra pendientes: quedan con su dueño.
            const pendientesConservados = datos.outbox.length;
            return { sesion: cambiarSesion({ estado: 'sinCuenta', usuario: undefined, desbloqueoLocal: 'disponible' }), pendientesConservados };
        }
    };
    const capacidadDe = {
        'device.info': 'device.info', 'network.status': 'network.status',
        'auth.status': 'auth.session', 'auth.signIn': 'auth.session', 'auth.enableUnlock': 'auth.session',
        'auth.unlock': 'auth.session', 'auth.signOut': 'auth.session'
    };

    // Folio 038 (red) — Doble cerrojo, como DespachadorDelPuente: un método que ejecuta una acción no permitida se
    // rechaza con AccionBloqueada (-32005) aunque el contenido no haya deshabilitado el botón.
    const metodosDeAccion = { 'documents.put': 'localidades.guardar', 'documents.delete': 'localidades.guardar',
        'encuestas.enqueueRespuesta': 'encuesta.enviar', 'outbox.sendNow': 'outbox.enviarAhora', 'auth.signIn': 'sesion.ingresar' };
    for (const [metodo, accion] of Object.entries(metodosDeAccion)) {
        const ejecutar = metodos[metodo];
        metodos[metodo] = p => {
            const e = evaluar(politica().find(x => x.accion === accion));
            if (!e.permitida) throw new ErrorDelPuente(-32005, e.motivo);
            return ejecutar(p);
        };
    }

    // Estado combinado: el de ?estado=, o el que se deduce de ?red= (como el esqueleto del cascarón).
    function estadoDeRed() { return config.estado || (config.red === '1' ? 'enLinea' : 'sinRed'); }
    // TransicionesDeRed.MotivoDe (DISENO §2.1): el texto que el cascarón manda en «motivo».
    function motivoDeRed() {
        return { sinRed: 'Sin conexión. Lo que hagas se guarda y se envía cuando vuelva.',
            redSinServidor: 'Hay red pero el servidor no responde. Reintentando…', enLineaMedida: 'Datos móviles' }[estadoDeRed()];
    }

    // Core/Red/PoliticaDeAcciones.cs, copiada a mano: si cambia allá, cambia acá.
    function politica() {
        return [
            { accion: 'localidades.guardar', clase: 'soloLocal' },
            { accion: 'encuesta.enviar', clase: 'encolable' },
            { accion: 'outbox.enviarAhora', clase: 'soloEnLinea' },
            { accion: 'sesion.ingresar', clase: 'soloEnLinea' }
        ];
    }
    // Core/Red/PoliticaDeAcciones.cs → EvaluadorDeAcciones.Evaluar, copiado a mano (DISENO §4.3).
    function evaluar(a) {
        const red = estadoDeRed();
        const s = sesionSimulada();
        const dispositivo = s.modo === 'dispositivo';
        const entradaAbierta = dispositivo || ['activa', 'vencida', 'revocada'].includes(s.estado);
        const sesionValida = dispositivo || s.estado === 'activa';
        const conServidor = red === 'enLinea' || red === 'enLineaMedida';
        const sinServidor = red === 'sinRed' ? 'Sin conexión' : 'El servidor no responde';
        const sinEntrada = s.estado === 'bloqueada' ? 'Desbloqueá la app'
            : s.estado === 'ingresando' ? 'Completá el ingreso en el navegador' : 'Ingresá con tu cuenta';
        const r = (permitida, quedaPendiente, motivo) =>
            Object.assign({ accion: a.accion, clase: a.clase, permitida: permitida, quedaPendiente: quedaPendiente }, motivo ? { motivo: motivo } : {});

        if (a.accion === 'sesion.ingresar') {
            if (dispositivo) return r(false, false, 'Esta app no usa cuenta');
            if (s.estado === 'activa') return r(false, false, 'Ya ingresaste');
            if (!entradaAbierta && s.estado !== 'sinCuenta') return r(false, false, sinEntrada);
            return conServidor ? r(true, false) : r(false, false, sinServidor);
        }
        if (!entradaAbierta) return r(false, false, sinEntrada);
        if (a.clase === 'soloLocal') return r(true, false);
        if (a.clase === 'encolable') {
            if (red === 'sinRed') return r(true, true, 'Sin conexión: se guarda y se envía cuando vuelva');
            if (red === 'redSinServidor') return r(true, true, 'El servidor no responde: se guarda y se envía cuando responda');
            if (!sesionValida) return r(true, true, 'Se guarda; se envía cuando vuelvas a ingresar');
            return r(true, false);
        }
        if (!conServidor) return r(false, false, sinServidor);
        if (!sesionValida) return r(false, false, 'Tu sesión venció: ingresá de nuevo');
        if (a.clase === 'soloWiFi' && red === 'enLineaMedida') return r(false, false, 'Sólo con WiFi');
        return r(true, false);
    }

    // Lo que hace el sincronizador del cascarón al enviar: la fila sale y el conteo baja. Devuelve cuántas salieron.
    function vaciarOutbox() {
        const salieron = datos.outbox.length;
        datos.outbox.length = 0;
        guardar();
        if (salieron > 0) emitir('outbox.changed', { pendientes: 0, rechazadas: 0 });
        return salieron;
    }

    // Cambió la red: network.changed, actions.changed y, si volvió el servidor con sesión válida, la reconexión
    // automática del cascarón (SincronizadorDelOutbox): lo pendiente sale solo, sin que nadie toque nada.
    function avisarCambioDeRed(antes) {
        const ahora = estadoDeRed();
        emitir('network.changed', Object.assign({ conectado: config.red === '1', estado: ahora }, motivoDeRed() ? { motivo: motivoDeRed() } : {}));
        emitir('actions.changed', { acciones: politica().map(evaluar) });
        const conServidor = e => e === 'enLinea' || e === 'enLineaMedida';
        const s = sesionSimulada();
        if (!conServidor(antes) && conServidor(ahora) && (s.modo === 'dispositivo' || s.estado === 'activa')) {
            setTimeout(vaciarOutbox, 300);
        }
    }

    // --- Folio 038: sesión (sección del constructor de autenticación) -----------------------------------------
    // ?sesion=dispositivo | oidc-<estado> fija el estado inicial; después cambia con auth.* y puenteSimulado.cambiarSesion.
    let sesion = null;
    function sesionSimulada() {
        if (sesion) return sesion;
        if (config.sesion === 'dispositivo') return (sesion = { modo: 'dispositivo', estado: 'activa', desbloqueoLocal: 'noDisponible' });
        const estado = config.sesion.replace(/^oidc-/, '');
        const motivos = { vencida: 'Tu sesión venció: ingresá para enviar lo pendiente.', revocada: 'El servidor cerró tu sesión: ingresá de nuevo para enviar lo pendiente.' };
        return (sesion = {
            modo: 'oidc', estado: estado, usuario: estado === 'sinCuenta' ? undefined : 'encuestador',
            desbloqueoLocal: estado === 'bloqueada' ? 'activado' : 'disponible', motivo: motivos[estado]
        });
    }

    function exigirOidc() {
        if (sesionSimulada().modo !== 'oidc') throw new ErrorDelPuente(-32007, 'Esta app no usa cuenta.');
    }

    // Como SesionDelUsuario.Publicar: auth.changed y, como la guardia escucha la sesión, actions.changed.
    function cambiarSesion(cambios) {
        sesion = Object.assign({}, sesionSimulada(), cambios);
        for (const k of Object.keys(sesion)) if (sesion[k] === undefined) delete sesion[k];
        emitir('auth.changed', { sesion: sesion });
        emitir('actions.changed', { acciones: politica().map(evaluar) });
        return sesion;
    }

    async function call(sobreTexto) {
        const sobre = JSON.parse(sobreTexto);
        llamadas.push(sobre);
        const responder = cuerpo => JSON.stringify(Object.assign({ jsonrpc: '2.0', v: config.puente, id: sobre.id }, cuerpo));
        try {
            if (sobre.jsonrpc !== '2.0' || typeof sobre.method !== 'string' || typeof sobre.id !== 'string') {
                throw new ErrorDelPuente(-32600, 'Sobre inválido.');
            }
            if (String(sobre.v).split('.')[0] !== config.puente.split('.')[0]) {
                throw new ErrorDelPuente(-32001, 'El contenido espera el puente ' + sobre.v + ' y este es ' + config.puente + '.');
            }
            const metodo = metodos[sobre.method];
            if (!metodo) throw new ErrorDelPuente(-32601, 'Método inexistente: ' + sobre.method);
            if (capacidadDe[sobre.method] && !tiene(capacidadDe[sobre.method])) {
                throw new ErrorDelPuente(-32002, 'Capacidad no disponible: ' + sobre.method);
            }
            return responder({ result: await metodo(sobre.params || {}) });
        } catch (e) {
            return responder({ error: { code: e.code || -32603, message: e.message } });
        }
    }

    function emitir(metodo, params) {
        // Como el cascarón con InvokeJavaScriptAsync: un sobre sin id.
        setTimeout(() => window.puenteRecibirEvento && window.puenteRecibirEvento({ jsonrpc: '2.0', v: config.puente, method: metodo, params: params }), 0);
    }

    if (config.sinpuente !== '1') {
        window.HybridWebView = {
            // Firma de HybridWebView: InvokeDotNet(nombreDelMétodo, [parámetros]). El cascarón sólo registra «Call».
            InvokeDotNet: async (nombre, parametros) => {
                if (nombre !== 'Call') throw new Error('El cascarón sólo expone Call(sobre); se pidió ' + nombre + '.');
                return call(parametros[0]);
            }
        };
    }

    window.puenteSimulado = {
        config: config,
        llamadas: llamadas,
        outbox: () => datos.outbox,
        documentos: () => datos.documentos,
        listo: () => listo,
        emitir: emitir,
        cambiarRed: conectado => {
            const antes = estadoDeRed();
            config.red = conectado ? '1' : '0'; config.estado = '';
            avisarCambioDeRed(antes);
        },
        // Folio 038: la sesión cambia sin que el contenido la pida (p. ej. {estado: 'vencida'} o {estado: 'bloqueada'}).
        cambiarSesion: cambios => cambiarSesion(cambios),
        sesion: () => sesionSimulada(),
        // Folio 038: estado combinado explícito (p. ej. 'redSinServidor': hay red pero el servidor no responde).
        cambiarEstadoDeRed: estado => {
            const antes = estadoDeRed();
            config.estado = estado; config.red = estado === 'sinRed' ? '0' : '1';
            avisarCambioDeRed(antes);
        }
    };
    console.log('puente simulado: ' + JSON.stringify(config));
})();
