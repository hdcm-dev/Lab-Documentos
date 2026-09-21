namespace MyProject.Domain.Common;

// Igual que MyProject/src/Backend/MyProject.Domain/Common/DomainException.cs: la estrategia de rechazo
// que la guía muestra en §3.3. Esta variante la compara con la otra, DomainResult, sobre el mismo Producto.
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
