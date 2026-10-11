// Identificador y literal no ASCII a propósito: en UTF-8 compila igual en
// Windows y en Linux; en cp1252 sin BOM falla en Linux (caso disparador, N1).
using System.Reflection;
using Newtonsoft.Json;

var añoDeConstrucción = new { Datos.Saludo, Datos.VersionNormalizada, Datos.GeneradoCon };
Console.WriteLine(JsonConvert.SerializeObject(añoDeConstrucción));

using var recurso = Assembly.GetExecutingAssembly().GetManifestResourceStream("app.js")!;
using var lector = new StreamReader(recurso);
Console.WriteLine(lector.ReadLine());
