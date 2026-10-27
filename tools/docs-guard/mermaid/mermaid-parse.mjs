// docs-guard PR-4: valida la sintaxis de cada bloque ```mermaid con el parser oficial (mermaid.parse), sin navegador.
// Portado de la herramienta de C-VERIFICACION (expediente 0002). Uso: node mermaid-parse.mjs RUTA...
// Salida: una línea por bloque inválido; exit 1 si hay alguno, 2 si una ruta no existe.
import { JSDOM } from 'jsdom';
import { readFileSync, readdirSync, statSync, existsSync } from 'node:fs';
import { join, relative } from 'node:path';

const dom = new JSDOM('<!doctype html><html><body></body></html>');
globalThis.window = dom.window;
globalThis.document = dom.window.document;
for (const k of ['Element', 'HTMLElement', 'Node', 'SVGElement', 'DocumentFragment', 'DOMParser', 'MutationObserver']) {
  globalThis[k] = dom.window[k];
}
const { default: mermaid } = await import('mermaid');

const OMITIR = new Set(['bin', 'obj', 'node_modules', '.git', '.docs-guard']);
function* recorrer(d) {
  for (const n of readdirSync(d).sort()) {
    const p = join(d, n);
    if (statSync(p).isDirectory()) { if (!OMITIR.has(n)) yield* recorrer(p); } else if (p.endsWith('.md')) yield p;
  }
}
const anotar = process.env.GITHUB_ACTIONS === 'true';
let invalidos = 0, bloques = 0;
for (const arg of process.argv.slice(2)) {
  if (!existsSync(arg)) { console.error(`ERROR: no existe ${arg}`); process.exit(2); }
  for (const f of (statSync(arg).isDirectory() ? recorrer(arg) : [arg])) {
    const lineas = readFileSync(f, 'utf8').split('\n');
    let buf = null, inicio = 0;
    for (let i = 0; i < lineas.length; i++) {
      const l = lineas[i];
      if (buf === null && /^\s*```mermaid\s*$/.test(l)) { buf = []; inicio = i + 1; continue; }
      if (buf !== null && /^\s*```\s*$/.test(l)) {
        bloques++;
        try { await mermaid.parse(buf.join('\n')); } catch (e) {
          invalidos++;
          const msg = String(e.message || e).split('\n')[0].slice(0, 120);
          const ruta = relative(process.cwd(), f);
          console.log(anotar ? `::error file=${ruta},line=${inicio}::mermaid inválido: ${msg}` : `${ruta}:${inicio}: ERROR mermaid inválido: ${msg}`);
        }
        buf = null; continue;
      }
      if (buf !== null) buf.push(l);
    }
  }
}
console.log(`== PR-4 mermaid: ${invalidos ? 'ROJO' : 'verde'} (bloques=${bloques} invalidos=${invalidos})`);
process.exit(invalidos ? 1 : 0);
