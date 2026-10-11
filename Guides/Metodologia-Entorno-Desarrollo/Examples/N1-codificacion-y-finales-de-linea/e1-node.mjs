// E1-N1-05 — Cómo lee Node.js los mismos bytes: require(), fs, JSON.parse y TextDecoder.
// Se corre DENTRO de un contenedor node (ver README.md). Requiere haber corrido generar.py.
import { createRequire } from "node:module";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import path from "node:path";

const aqui = path.dirname(fileURLToPath(import.meta.url));
const dir = path.join(aqui, "work", "node");
const require = createRequire(import.meta.url);
const hex = (s) => [...s.slice(6, 9)].map((c) => c.codePointAt(0).toString(16).toUpperCase().padStart(4, "0")).join(" ");

console.log(`Node ${process.version}`);
for (const v of ["cp1252", "utf8", "utf8bom", "utf8-crlf"]) {
  const m = require(path.join(dir, `mod-${v}.js`));
  console.log(`require(mod-${v}.js) -> texto="${m.texto}" U+[${hex(m.texto)}] bloque.length=${m.bloque.length} (template literal)`);
}

// Identificador no ASCII en cp1252: el literal se corrompe sin error, el identificador no compila
try { require(path.join(dir, "mod-ident-cp1252.js")); console.log("require(mod-ident-cp1252.js) OK"); }
catch (e) { console.log(`require(mod-ident-cp1252.js) -> ${e.name}: ${e.message.split("\n")[0]}`); }

// fs.readFileSync con 'utf8' NO quita el BOM: el primer carácter es U+FEFF
const crudo = readFileSync(path.join(dir, "mod-utf8bom.js"), "utf8");
console.log(`fs.readFileSync(utf8bom,'utf8')[0] = U+${crudo.codePointAt(0).toString(16).toUpperCase()}`);

// JSON.parse con BOM
for (const f of ["config-utf8.json", "config-utf8bom.json"]) {
  const t = readFileSync(path.join(dir, f), "utf8");
  try { JSON.parse(t); console.log(`JSON.parse(${f}) OK`); }
  catch (e) { console.log(`JSON.parse(${f}) -> ${e.name}: ${e.message.split("\n")[0]}`); }
}
// require() de un .json con BOM
try { require(path.join(dir, "config-utf8bom.json")); console.log("require(config-utf8bom.json) OK"); }
catch (e) { console.log(`require(config-utf8bom.json) -> ${e.name}`); }

// TextDecoder: por defecto descarta el BOM; con ignoreBOM:true lo conserva; fatal:true detecta cp1252
const bytes = readFileSync(path.join(dir, "config-utf8bom.json"));
console.log(`TextDecoder() primer char = U+${new TextDecoder().decode(bytes).codePointAt(0).toString(16).toUpperCase()}`);
console.log(`TextDecoder({ignoreBOM:true}) primer char = U+${new TextDecoder("utf-8", { ignoreBOM: true }).decode(bytes).codePointAt(0).toString(16).toUpperCase()}`);
try { new TextDecoder("utf-8", { fatal: true }).decode(readFileSync(path.join(dir, "mod-cp1252.js"))); console.log("fatal:true cp1252 -> sin error"); }
catch (e) { console.log(`TextDecoder({fatal:true}) sobre cp1252 -> ${e.name} (${e.code})`); }
// import() ESM de un módulo con BOM: también se descarta
const esm = await import(path.join(dir, "esm-utf8bom.mjs"));
console.log(`import(esm-utf8bom.mjs) -> texto="${esm.texto}" primer char del literal OK=${esm.texto[0] === "A"}`);
