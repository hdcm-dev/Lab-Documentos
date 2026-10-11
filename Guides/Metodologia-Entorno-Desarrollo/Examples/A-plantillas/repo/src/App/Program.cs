// Identificador y literal no ASCII: compilan igual en Windows y en Linux
// porque el archivo está en UTF-8 válido (guía §4).
var señal = "Atención: lectura fuera de rango";
System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new { señal }));
