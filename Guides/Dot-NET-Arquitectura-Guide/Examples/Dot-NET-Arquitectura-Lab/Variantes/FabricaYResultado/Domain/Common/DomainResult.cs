using System.Diagnostics.CodeAnalysis;

namespace MyProject.Domain.Common;

// La segunda estrategia de rechazo: la regla incumplida es un VALOR que se devuelve, no una excepción
// que se lanza. readonly record struct: no asigna en el montículo y compara por datos, como un Value Object.
public readonly record struct DomainResult
{
    private DomainResult(bool aplicado, string? codigo)
    {
        Aplicado = aplicado;
        Codigo = codigo;
    }

    public bool Aplicado { get; }

    // Código del vocabulario del dominio, no texto de presentación: el contrato de error también es contrato.
    public string? Codigo { get; }

    public static DomainResult Aplicar() => new(true, null);

    public static DomainResult Rechazar(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);   // defecto de programación: esto sí es excepción
        return new DomainResult(false, codigo);
    }
}

// La variante con carga útil. Value es nulo cuando el resultado es un rechazo, y el compilador NO puede
// deducirlo de Aplicado: por eso el llamador escribe `resultado.Value!`. TryGetValue evita ese `!`.
public readonly record struct DomainResult<TValue>
{
    private DomainResult(bool aplicado, TValue? value, string? codigo)
    {
        Aplicado = aplicado;
        Value = value;
        Codigo = codigo;
    }

    public bool Aplicado { get; }

    public TValue? Value { get; }

    public string? Codigo { get; }

    public static DomainResult<TValue> Aplicar(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new DomainResult<TValue>(true, value, null);
    }

    public static DomainResult<TValue> Rechazar(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return new DomainResult<TValue>(false, default, codigo);
    }

    // [NotNullWhen] le enseña al análisis de nulabilidad lo que Aplicado por sí solo no le dice.
    public bool TryGetValue([NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? codigo)
    {
        value = Value;
        codigo = Codigo;
        return Aplicado;
    }
}
