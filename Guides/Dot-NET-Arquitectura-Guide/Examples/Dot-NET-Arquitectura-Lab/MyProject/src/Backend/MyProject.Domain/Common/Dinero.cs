namespace MyProject.Domain.Common;

/// <summary>Value object: dos Dinero con el mismo monto y moneda son iguales.</summary>
public record Dinero(decimal Monto, string Moneda)
{
    public Dinero Sumar(Dinero otro)
    {
        if (Moneda != otro.Moneda)
            throw new DomainException("No se pueden sumar montos de monedas distintas.");
        return this with { Monto = Monto + otro.Monto };
    }
}
