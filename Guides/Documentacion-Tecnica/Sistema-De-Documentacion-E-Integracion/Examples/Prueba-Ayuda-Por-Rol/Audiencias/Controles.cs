using System.Text;

namespace Audiencias;

// Los controles de la resolución 399 (§5.2.2) que esta prueba ejercita. Cada uno devuelve sus fallas; vacío es verde.
public static class Controles
{
    // E-1: enlace de una página a otra que alguno de sus receptores no recibe (sube en la jerarquía expandida).
    public static List<string> CruceDeAudiencia(Catalogo c)
    {
        var fallas = new List<string>();
        foreach (var (o, d) in c.Enlaces())
        {
            if (!c.Paginas.TryGetValue(d, out var t)) { fallas.Add($"enlace roto: {o} → {d}"); continue; }
            var sin = c.Receptores(c.Paginas[o]).Except(c.Receptores(t)).Order().ToList();
            if (sin.Count > 0) fallas.Add($"cruce-de-audiencia: {o} → {d}; {string.Join(", ", sin)} no la recibe");
        }
        return fallas;
    }

    // E-6: en un repositorio público, ninguna fuente de nivel mayor que «producto».
    public static List<string> NivelVsRepo(Catalogo c) =>
        c.Visibilidad != "publico" ? [] :
        c.Paginas.Values.Where(p => Array.IndexOf(Catalogo.Niveles, c.NivelDe(p)) > 1)
            .Select(p => $"nivel-vs-repo: {p.Slug} es «{c.NivelDe(p)}» en un repositorio «{c.Visibilidad}»").ToList();

    // E-3: ninguna cadena no pública en el paquete, ni en texto ni en recursos compilados (UTF-8 y UTF-16).
    public static List<string> PaqueteSinFuga(string dir, IEnumerable<string> prohibidas)
    {
        var fallas = new List<string>();
        foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Order())
        {
            var bytes = File.ReadAllBytes(f);
            foreach (var p in prohibidas)
                if (Contiene(bytes, Encoding.UTF8.GetBytes(p)) || Contiene(bytes, Encoding.Unicode.GetBytes(p)))
                    fallas.Add($"paquete-sin-fuga: «{p}» en {Path.GetRelativePath(dir, f)}");
        }
        return fallas;
    }

    static bool Contiene(byte[] h, byte[] n) => h.AsSpan().IndexOf(n) >= 0;
}
