using MyProject.Domain.Common;

namespace MyProject.Domain.Productos;

// Conjunto cerrado CON regla: un Value Object. Constructor privado + Factory Function, la misma mecánica
// de la Entity aplicada a un valor. Es el único de los tres —enum, cadena, Value Object— que no puede
// contener un valor imposible: no hay forma de escribir `new Moneda("XYZ")` desde ningún lado.
public sealed record Moneda
{
    private static readonly string[] Admitidas = ["ARS", "USD", "EUR"];

    private Moneda(string codigo) => Codigo = codigo;

    public string Codigo { get; }

    // Camino de constitución: valida.
    public static DomainResult<Moneda> Create(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return DomainResult<Moneda>.Rechazar("MONEDA_REQUERIDA");

        var normalizado = codigo.Trim().ToUpperInvariant();
        return Admitidas.Contains(normalizado)
            ? DomainResult<Moneda>.Aplicar(new Moneda(normalizado))
            : DomainResult<Moneda>.Rechazar("MONEDA_NO_ADMITIDA");
    }

    // Camino de materialización: NO valida. Lo usa el conversor de EF Core para reconstruir lo que la base
    // ya tiene; un dato que ya está guardado no se puede rechazar leyéndolo. (Criterio de esta guía.)
    public static Moneda Materializar(string codigo) => new(codigo);

    public override string ToString() => Codigo;
}

// El contraste: un record posicional sin regla, como el `Dinero` de §3.3. El constructor es público y
// admite cualquier cadena; `with` además la cambia sin pasar por ninguna validación.
public record MonedaSinRegla(string Codigo);
