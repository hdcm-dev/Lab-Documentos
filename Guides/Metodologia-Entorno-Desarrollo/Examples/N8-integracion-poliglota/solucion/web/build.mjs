// Build mínimo del frontend: escribe dist/app.js. Registra con qué Node se construyó.
import { mkdirSync, writeFileSync } from "node:fs";
import isOdd from "is-odd";

mkdirSync("dist", { recursive: true });
const contenido =
  `// construido con node ${process.version}\n` +
  `export const tresEsImpar = ${isOdd(3)};\n`;
// Codificación explícita: nunca depender del default de la plataforma.
writeFileSync("dist/app.js", contenido, { encoding: "utf8" });
console.log(`web: dist/app.js generado con node ${process.version}`);
