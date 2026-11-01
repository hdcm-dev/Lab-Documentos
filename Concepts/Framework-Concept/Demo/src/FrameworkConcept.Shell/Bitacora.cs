namespace FrameworkConcept.Shell;

/// <summary>
/// Registro con la etiqueta «FC» y el tiempo desde que arrancó el proceso, como en el ensayo E01.
///
///   Android: a logcat. Para leerlo: <c>adb logcat -s FC</c>. El tiempo sale de <c>Process.StartElapsedRealtime</c>, el
///            mismo reloj con que P1 mide el tiempo al primer render (TPR).
///   iOS:     al registro unificado (os_log, subsistema «com.frameworkconcept.demo», categoría «FC») y a la salida
///            estándar, que muestra <c>xcrun simctl launch --console-pty booted com.frameworkconcept.demo</c>. Para el
///            registro: <c>xcrun simctl spawn booted log stream --level info --predicate 'subsystem == "com.frameworkconcept.demo"'</c>
///            (sin <c>--level info</c>, stream muestra sólo el nivel default y las líneas FC, que van como Info, no salen).
///            El tiempo sale de la hora de inicio que el núcleo guarda para el proceso (sysctl KERN_PROC_PID,
///            <c>p_starttime</c>): cuenta desde exec, antes de main, como el reloj de Android cuenta desde el fork del
///            zygote. Ver InicioDelProceso, más abajo.
/// </summary>
public static class Bitacora
{
#if IOS
    private static readonly CoreFoundation.OSLog Registro = new("com.frameworkconcept.demo", "FC");
#endif

    public static long MsDesdeArranque()
    {
#if ANDROID
        return Android.OS.SystemClock.ElapsedRealtime() - Android.OS.Process.StartElapsedRealtime;
#elif IOS
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - InicioDelProceso.EpocaMs;
#else
        return Environment.TickCount64;
#endif
    }

    public static void Log(string mensaje)
    {
        var linea = $"t={MsDesdeArranque()}ms {mensaje}";
#if ANDROID
        Android.Util.Log.Info("FC", linea);
#elif IOS
        Registro.Log(CoreFoundation.OSLogLevel.Info, linea);
        Console.WriteLine("FC " + linea);
#else
        System.Diagnostics.Debug.WriteLine("FC " + linea);
#endif
    }

    public static void Error(string mensaje, Exception ex) => Log($"ERROR {mensaje}: {ex.GetType().Name}: {ex.Message}");

#if IOS
    /// <summary>
    /// P1 — La hora (época, ms) en que el núcleo creó este proceso: <c>kinfo_proc.kp_proc.p_starttime</c>, que es lo
    /// primero de la estructura (un <c>timeval</c>: segundos en 64 bits y microsegundos en 32). Se lee una vez. Si el
    /// sysctl fallara se usa la hora de la primera lectura: el TPR saldría corto, y la bitácora lo dice.
    /// </summary>
    private static class InicioDelProceso
    {
        public static readonly long EpocaMs = Leer();

        private const int CtlKern = 1, KernProc = 14, KernProcPid = 1;
        private const int TamanoDeKinfoProc = 648;   // sizeof(struct kinfo_proc) en arm64 y x86_64

        [System.Runtime.InteropServices.DllImport(ObjCRuntime.Constants.libSystemLibrary)]
        private static extern int sysctl(int[] nombre, uint largoDelNombre, byte[] viejo, ref nint largoDelViejo, IntPtr nuevo, nint largoDelNuevo);

        private static long Leer()
        {
            var nombre = new[] { CtlKern, KernProc, KernProcPid, Environment.ProcessId };
            var datos = new byte[TamanoDeKinfoProc];
            nint largo = datos.Length;
            if (sysctl(nombre, (uint)nombre.Length, datos, ref largo, IntPtr.Zero, 0) == 0 && largo >= 16)
            {
                var segundos = BitConverter.ToInt64(datos, 0);
                var microsegundos = BitConverter.ToInt32(datos, 8);
                return segundos * 1000 + microsegundos / 1000;
            }
            var ahora = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Registro.Log(CoreFoundation.OSLogLevel.Error, "no se pudo leer el inicio del proceso (sysctl); el TPR cuenta desde ahora");
            return ahora;
        }
    }
#endif
}
