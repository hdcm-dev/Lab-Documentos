// Nuevo (ejemplo 17). Un ILoggerFactory mínimo que guarda lo que escribe UseLogging, para mirar qué
// sale en los logs según el nivel. En una app real va el logging del host; acá alcanza con esto.
using Microsoft.Extensions.Logging;

namespace MovilidadUrbana.Asistente.Infrastructure;

public sealed record RenglonDeLog(LogLevel Nivel, string Categoria, string Texto);

public sealed class LogsEnMemoria(LogLevel nivelMinimo) : ILoggerFactory
{
    public List<RenglonDeLog> Renglones { get; } = [];

    public LogLevel NivelMinimo { get; } = nivelMinimo;

    public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

    public void AddProvider(ILoggerProvider provider) { }

    public void Dispose() { }

    private sealed class Logger(LogsEnMemoria dueno, string categoria) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= dueno.NivelMinimo;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            lock (dueno.Renglones)
            {
                dueno.Renglones.Add(new RenglonDeLog(logLevel, categoria, formatter(state, exception)));
            }
        }
    }
}
