using UIKit;

namespace FrameworkConcept.Shell;

/// <summary>Punto de entrada de iOS: UIKit arranca con <see cref="AppDelegate"/> como delegado de la app.</summary>
public static class Program
{
    private static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
}
