// Nuevo. Prueba de arquitectura OBLIGATORIA (AC-5; ADR-11, H3-017): las capas son carpetas de un
// ensamblado, así que el compilador no impide una violación; esta prueba la reemplaza. Cada regla
// encuentra al menos un tipo en su namespace y falla si no encuentra ninguno.
//
// «Referencia» = firmas (base, interfaces, campos, propiedades, parámetros, retornos), atributos (su
// tipo y los typeof de sus argumentos) y también el cuerpo de los métodos: se recorre el IL y se
// resuelve cada token de tipo, método o campo. La quinta regla y la lectura de atributos son de
// REC-26; la sexta (Comun.AI y Comun.Pruebas) es de RL-23. Los casos rojos de MutacionesTests prueban
// que las reglas ven lo que dicen ver.
using System.Reflection;
using System.Reflection.Emit;
using MovilidadUrbana.Asistente.Infrastructure.AI.Chat;

namespace MovilidadUrbana.Asistente.Tests.Cableado;

[TestFixture]
public class ArquitecturaTests
{
    private const string Raiz = "MovilidadUrbana.Asistente";
    private static readonly Assembly Demo = typeof(HerramientasDelAsistente).Assembly;
    private static readonly string[] SdkDelProveedor = ["OllamaSharp", "OpenAI"];

    private static IReadOnlyList<Type> TiposDe(string prefijo, Func<Type, bool>? ademas = null) =>
        Demo.GetTypes().Where(t => (t.FullName ?? string.Empty).StartsWith(prefijo + ".", StringComparison.Ordinal) && (ademas?.Invoke(t) ?? true)).ToList();

    [Test]
    [Description("Domain/ no referencia Microsoft.Extensions.AI*, OllamaSharp ni OpenAI")]
    public void Domain_SinDependenciasDeIa()
    {
        var tipos = TiposDe($"{Raiz}.Domain");
        Assert.That(tipos, Is.Not.Empty, "al menos un tipo en Domain");
        AssertSinReferencias(tipos, a => a.StartsWith("Microsoft.Extensions.AI", StringComparison.Ordinal) || SdkDelProveedor.Contains(a));
    }

    [Test]
    [Description("Application/ no referencia …Infrastructure*, OllamaSharp ni OpenAI")]
    public void Application_SinInfrastructureNiSdk()
    {
        var tipos = TiposDe($"{Raiz}.Application");
        Assert.That(tipos, Is.Not.Empty, "al menos un tipo en Application");
        AssertSinReferencias(tipos, a => SdkDelProveedor.Contains(a), t => (t.FullName ?? string.Empty).StartsWith($"{Raiz}.Infrastructure", StringComparison.Ordinal));
    }

    [Test]
    [Description("El SDK del proveedor sólo en …Infrastructure.AI y Program")]
    public void SdkDelProveedor_SoloEnInfrastructureAIyProgram()
    {
        var fuera = Demo.GetTypes().Where(t =>
            !(t.FullName ?? string.Empty).StartsWith($"{Raiz}.Infrastructure.AI", StringComparison.Ordinal) &&
            !(t.FullName ?? string.Empty).StartsWith("Program", StringComparison.Ordinal)).ToList();
        Assert.That(fuera, Is.Not.Empty);
        Assert.That(TiposDe($"{Raiz}.Infrastructure.AI"), Is.Not.Empty, "al menos un tipo en Infrastructure.AI");
        AssertSinReferencias(fuera, a => SdkDelProveedor.Contains(a));
    }

    [Test]
    [Description("Ninguna clase de herramientas de …Infrastructure.AI recibe un I*Repository: toda escritura pasa por un caso de uso")]
    public void Herramientas_SinRepositorios()
    {
        var herramientas = TiposDe($"{Raiz}.Infrastructure.AI", t => t.Name.StartsWith("Herramientas", StringComparison.Ordinal) || t.Name == nameof(ResolutorDeAprobaciones) || t.Name == nameof(GuardaDeOrigen));
        Assert.That(herramientas, Is.Not.Empty, "al menos una clase de herramientas");
        AssertSinReferencias(herramientas, _ => false, t => t.IsInterface && t.Name.StartsWith('I') && t.Name.EndsWith("Repository", StringComparison.Ordinal));
    }

    [Test]
    [Description("Presentation (Components, Controllers, Contracts, Sessions, Concurrencia) no referencia …Infrastructure.AI, salvo Pages/Chat.razor; y la excepción sigue existiendo")]
    public void Presentation_EntraAInfrastructureAI_SoloPorChat()
    {
        var presentacion = TiposDePresentacion();
        Assert.That(presentacion.Where(t => !EsChat(t)), Is.Not.Empty, "al menos un tipo de Presentation fuera del chat");
        Assert.That(ViolacionesDePresentacion(presentacion.Where(t => !EsChat(t))), Is.Empty);
        Assert.That(ViolacionesDePresentacion(presentacion.Where(EsChat)), Is.Not.Empty,
            "Chat.razor ya no entra a Infrastructure.AI: la excepción quedó vieja y hay que sacarla de la regla");
    }

    [Test]
    [Description("…Comun.AI y …Comun.Pruebas (de Examples/_Comun) sólo en …Infrastructure.AI y Program; y la excepción sigue existiendo")]
    public void ComunAIyPruebas_SoloEnInfrastructureAIyProgram()
    {
        var fuera = Demo.GetTypes().Where(t => !EnInfrastructureAIoProgram(t)).ToList();
        Assert.That(fuera, Is.Not.Empty);
        Assert.That(ViolacionesDeComun(fuera), Is.Empty);
        Assert.That(ViolacionesDeComun(Demo.GetTypes().Where(EnInfrastructureAIoProgram)), Is.Not.Empty,
            "Infrastructure.AI y Program ya no usan Comun.AI ni Comun.Pruebas: la regla quedó vieja");
    }

    private static bool EnInfrastructureAIoProgram(Type t)
    {
        var nombre = t.FullName ?? string.Empty;
        return nombre.StartsWith($"{Raiz}.Infrastructure.AI", StringComparison.Ordinal) || nombre.StartsWith("Program", StringComparison.Ordinal);
    }

    /// <summary>Referencias a un tipo de …Comun.AI o …Comun.Pruebas, con el mismo inspector que las demás reglas.</summary>
    internal static IReadOnlyList<string> ViolacionesDeComun(IEnumerable<Type> tipos, bool leerCuerpo = true) =>
        Violaciones(tipos, _ => false, r =>
        {
            var n = r.FullName ?? string.Empty;
            return n.StartsWith($"{Raiz}.Comun.AI.", StringComparison.Ordinal) || n.StartsWith($"{Raiz}.Comun.Pruebas.", StringComparison.Ordinal);
        }, leerCuerpo);

    /// <summary>Capa de presentación de la demo: la página, los controllers, sus contratos y la entrada HTTP.</summary>
    internal static IReadOnlyList<Type> TiposDePresentacion() =>
        new[] { "Components", "Controllers", "Contracts", "Sessions", "Concurrencia" }.SelectMany(c => TiposDe($"{Raiz}.{c}")).ToList();

    /// <summary>La página de chat y los tipos que el compilador anida en ella (clausuras, máquinas de estado).</summary>
    internal static bool EsChat(Type t)
    {
        const string chat = $"{Raiz}.Components.Pages.Chat";
        var nombre = t.FullName ?? string.Empty;
        return nombre == chat || nombre.StartsWith(chat + "+", StringComparison.Ordinal);
    }

    /// <summary>Referencias a un tipo de …Infrastructure.AI, con el mismo inspector que las demás reglas.</summary>
    internal static IReadOnlyList<string> ViolacionesDePresentacion(IEnumerable<Type> tipos, bool leerCuerpo = true) =>
        Violaciones(tipos, _ => false, r => (r.FullName ?? string.Empty).StartsWith($"{Raiz}.Infrastructure.AI", StringComparison.Ordinal), leerCuerpo);

    private static void AssertSinReferencias(IEnumerable<Type> tipos, Func<string, bool> ensambladoProhibido, Func<Type, bool>? tipoProhibido = null) =>
        Assert.That(Violaciones(tipos, ensambladoProhibido, tipoProhibido), Is.Empty);

    private static IReadOnlyList<string> Violaciones(IEnumerable<Type> tipos, Func<string, bool> ensambladoProhibido, Func<Type, bool>? tipoProhibido, bool leerCuerpo = true)
    {
        var violaciones = new List<string>();
        foreach (var t in tipos)
        {
            foreach (var r in Referencias(t, leerCuerpo))
            {
                var ensamblado = r.Assembly.GetName().Name ?? string.Empty;
                if (ensambladoProhibido(ensamblado) || (tipoProhibido?.Invoke(r) ?? false)) violaciones.Add($"{t.FullName} → {r.FullName}");
            }
        }
        return violaciones.Distinct().ToList();
    }

    private const BindingFlags Todo = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static IEnumerable<Type> Referencias(Type t, bool leerCuerpo = true)
    {
        var refs = new HashSet<Type>();
        void Sumar(Type? x)
        {
            if (x is null || !refs.Add(x)) return;
            if (x.HasElementType) Sumar(x.GetElementType());
            if (x.IsGenericType) foreach (var a in x.GetGenericArguments()) Sumar(a);
        }
        void SumarAtributos(IEnumerable<CustomAttributeData> atributos)
        {
            foreach (var a in atributos)
            {
                Sumar(a.AttributeType);
                foreach (var arg in a.ConstructorArguments.Concat(a.NamedArguments.Select(n => n.TypedValue)))
                    if (arg.Value is Type comoValor) Sumar(comoValor);
            }
        }

        Sumar(t.BaseType);
        foreach (var i in t.GetInterfaces()) Sumar(i);
        SumarAtributos(t.GetCustomAttributesData());
        foreach (var f in t.GetFields(Todo)) { Sumar(f.FieldType); SumarAtributos(f.GetCustomAttributesData()); }
        foreach (var p in t.GetProperties(Todo)) { Sumar(p.PropertyType); SumarAtributos(p.GetCustomAttributesData()); }
        foreach (var m in t.GetMethods(Todo).Cast<MethodBase>().Concat(t.GetConstructors(Todo)))
        {
            foreach (var p in m.GetParameters()) { Sumar(p.ParameterType); SumarAtributos(p.GetCustomAttributesData()); }
            if (m is MethodInfo mi) Sumar(mi.ReturnType);
            SumarAtributos(m.GetCustomAttributesData());
            if (leerCuerpo) foreach (var x in TiposDelCuerpo(m)) Sumar(x);
        }
        return refs.Where(r => !r.IsGenericParameter);
    }

    private static readonly Dictionary<short, OpCode> OpCodesPorValor = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (OpCode)f.GetValue(null)!).ToDictionary(o => o.Value);

    /// <summary>Recorre el IL y resuelve los tokens de tipo, método y campo.</summary>
    private static IEnumerable<Type> TiposDelCuerpo(MethodBase metodo)
    {
        byte[]? il;
        try { il = metodo.GetMethodBody()?.GetILAsByteArray(); } catch (InvalidOperationException) { il = null; }
        if (il is null) yield break;
        var modulo = metodo.Module;
        var genT = metodo.DeclaringType?.IsGenericType == true ? metodo.DeclaringType.GetGenericArguments() : null;
        var genM = metodo.IsGenericMethod ? metodo.GetGenericArguments() : null;
        var i = 0;
        while (i < il.Length)
        {
            // Los códigos de dos bytes (0xFE xx) son negativos como short: OpCodes.Ceq.Value == unchecked((short)0xFE01).
            short valor = il[i] == 0xFE ? unchecked((short)(0xFE00 | il[i + 1])) : il[i];
            i += il[i] == 0xFE ? 2 : 1;
            if (!OpCodesPorValor.TryGetValue(valor, out var op))
            {
                yield break;
            }
            switch (op.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar: i += 1; break;
                case OperandType.InlineVar: i += 2; break;
                case OperandType.InlineI8 or OperandType.InlineR: i += 8; break;
                case OperandType.InlineSwitch: i += 4 + 4 * BitConverter.ToInt32(il, i); break;
                case OperandType.InlineType or OperandType.InlineMethod or OperandType.InlineField or OperandType.InlineTok:
                {
                    var token = BitConverter.ToInt32(il, i);
                    i += 4;
                    MemberInfo? miembro = null;
                    try { miembro = modulo.ResolveMember(token, genT, genM); } catch (ArgumentException) { } catch (BadImageFormatException) { }
                    switch (miembro)
                    {
                        case Type tipo: yield return tipo; break;
                        case MethodBase mb when mb.DeclaringType is not null:
                            yield return mb.DeclaringType;
                            if (mb is MethodInfo mi2) yield return mi2.ReturnType;
                            foreach (var p in mb.GetParameters()) yield return p.ParameterType;
                            break;
                        case FieldInfo fi:
                            if (fi.DeclaringType is not null) yield return fi.DeclaringType;
                            yield return fi.FieldType;
                            break;
                    }
                    break;
                }
                default: i += 4; break;
            }
        }
    }
}
