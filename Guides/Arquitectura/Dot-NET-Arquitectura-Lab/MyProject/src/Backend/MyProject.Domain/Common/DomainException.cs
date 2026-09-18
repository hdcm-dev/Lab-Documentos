namespace MyProject.Domain.Common;

/// <summary>Se lanza cuando una operación violaría una regla del negocio.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
