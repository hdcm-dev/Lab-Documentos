// Pieza sintética: depende de un paquete real (ms) para que el SBOM tenga algo que listar.
import ms from "ms";
export const espera = (texto) => ms(texto);
