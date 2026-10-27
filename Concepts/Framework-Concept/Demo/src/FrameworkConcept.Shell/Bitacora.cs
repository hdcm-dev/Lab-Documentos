namespace FrameworkConcept.Shell;

/// <summary>
/// Registro a logcat con la etiqueta «FC» y el tiempo desde que arrancó el proceso, como en el ensayo E01.
/// Para leerlo: <c>adb logcat -s FC</c>. El tiempo sale de <c>Process.StartElapsedRealtime</c>, el mismo reloj con
/// que P1 mide el tiempo al primer render (TPR).
/// </summary>
public static class Bitacora
{
    public static long MsDesdeArranque()
    {
#if ANDROID
        return Android.OS.SystemClock.ElapsedRealtime() - Android.OS.Process.StartElapsedRealtime;
#else
        return Environment.TickCount64;
#endif
    }

    public static void Log(string mensaje)
    {
        var linea = $"t={MsDesdeArranque()}ms {mensaje}";
#if ANDROID
        Android.Util.Log.Info("FC", linea);
#else
        System.Diagnostics.Debug.WriteLine("FC " + linea);
#endif
    }

    public static void Error(string mensaje, Exception ex) => Log($"ERROR {mensaje}: {ex.GetType().Name}: {ex.Message}");
}
