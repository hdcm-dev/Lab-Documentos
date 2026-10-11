namespace Contoso;
public static class Util
{
#if IMPOSTOR
    public const string Origen = "IMPOSTOR";
#else
    public const string Origen = "interno";
#endif
}
