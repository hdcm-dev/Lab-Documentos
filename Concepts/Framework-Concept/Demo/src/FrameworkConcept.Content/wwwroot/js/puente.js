// Cliente JavaScript del puente (lado contenido).
//
// P3 — El cascarón registra UN solo método nativo, Call(sobre), con HybridWebView.SetInvokeJavaScriptTarget.
// Este archivo arma el sobre JSON-RPC 2.0 con «v» y lo entrega por el transporte de HybridWebView:
//   window.HybridWebView.InvokeDotNet('Call', [sobreComoTexto])
// que viaja como POST /__hwvInvokeDotNet al mismo origen (_framework/hybridwebview.js, condición C-2 de E01).
// El transporte es del cascarón: este archivo no lo reimplementa, sólo lo usa.
//
// Lo llama el cliente C# (Bridge/BridgeClient.cs) por JS interop; las páginas no lo usan directo.
//
// Superficie que el CASCARÓN usa de este archivo (para los eventos, sin respuesta):
//   InvokeJavaScriptAsync("puenteRecibirEvento", sobreEvento)   — sobreEvento: objeto o texto JSON
(function () {
    'use strict';

    let siguienteId = 1;
    let receptor = null;           // DotNetObjectReference de BridgeSession
    const eventosEnEspera = [];    // eventos que llegan antes de que el contenido se suscriba

    function hayPuente() {
        return !!(window.HybridWebView && typeof window.HybridWebView.InvokeDotNet === 'function');
    }

    // El cascarón puede devolver el sobre de respuesta como objeto o como texto JSON: se aceptan los dos.
    function comoObjeto(valor) {
        return typeof valor === 'string' ? JSON.parse(valor) : valor;
    }

    // En Android, hybridwebview.js (MAUI 10.0.20) manda el cuerpo del POST /__hwvInvokeDotNet en el encabezado
    // HTTP «X-Maui-Request-Body», porque el WebView de Android no expone el cuerpo. Un encabezado no lleva UTF-8:
    // «¿Qué» llegaba al cascarón como «�Qu�» (visto en el moto g42, E02). Por eso el sobre viaja en ASCII: todo
    // carácter fuera de ASCII va como escape JSON \uXXXX, que es el mismo texto para cualquier lector JSON.
    function comoAscii(texto) {
        return texto.replace(/[\u007f-￿]/g, function (c) {
            return '\\u' + c.charCodeAt(0).toString(16).padStart(4, '0');
        });
    }

    // Llama a un método de la tabla. Devuelve el sobre de respuesta como TEXTO JSON: el cliente C# lo lee
    // con las mismas opciones JSON que el cascarón (JsonDelContrato).
    async function llamar(v, metodo, parametrosJson) {
        if (!hayPuente()) {
            // BridgeClient reconoce esta marca y la convierte en «sin puente».
            throw new Error('SIN_PUENTE: no existe window.HybridWebView (el contenido no está dentro del cascarón).');
        }
        const sobre = {
            jsonrpc: '2.0',
            v: v,
            id: String(siguienteId++),
            method: metodo
        };
        if (parametrosJson) sobre.params = JSON.parse(parametrosJson);

        const respuesta = comoObjeto(await window.HybridWebView.InvokeDotNet('Call', [comoAscii(JSON.stringify(sobre))]));
        if (!respuesta || respuesta.id !== sobre.id) {
            throw new Error('El cascarón respondió un sobre que no corresponde al pedido ' + sobre.id + '.');
        }
        return JSON.stringify(respuesta);
    }

    function entregar(sobre) {
        // Un evento es una «notification» de JSON-RPC: lleva method y no lleva id.
        const parametros = sobre.params === undefined ? null : JSON.stringify(sobre.params);
        receptor.invokeMethodAsync('RecibirEvento', sobre.method, parametros)
            .catch(function (e) { console.error('puente: el contenido no pudo procesar ' + sobre.method, e); });
    }

    function recibirEvento(sobreEvento) {
        const sobre = comoObjeto(sobreEvento);
        if (!sobre || typeof sobre.method !== 'string') return;
        if (receptor) entregar(sobre);
        else eventosEnEspera.push(sobre);
    }

    function suscribir(referencia) {
        receptor = referencia;
        while (eventosEnEspera.length > 0) entregar(eventosEnEspera.shift());
    }

    // P1 — Métrica de arranque: «tiempo al primer render del contenido» (TPR) se mide desde esta marca.
    function marcarPrimerRender(version) {
        if (window.performance && performance.mark) performance.mark('primer-render');
        console.log('FC-MARCA primer-render contenido=' + version + ' perf.now=' + Math.round(performance.now()) + 'ms');
    }

    window.puente = {
        llamar: llamar,
        suscribir: suscribir,
        recibirEvento: recibirEvento,
        marcarPrimerRender: marcarPrimerRender
    };
    // Nombre sin punto para InvokeJavaScriptAsync del cascarón.
    window.puenteRecibirEvento = recibirEvento;
})();
