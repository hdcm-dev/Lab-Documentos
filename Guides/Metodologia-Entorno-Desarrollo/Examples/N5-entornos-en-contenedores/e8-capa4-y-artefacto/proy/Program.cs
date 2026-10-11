using System;
class Programa {
    static void Main() {
        var texto = @"Atención:
señal";
        Console.WriteLine(texto + " " + DateTime.UnixEpoch.ToString("O"));
    }
}
