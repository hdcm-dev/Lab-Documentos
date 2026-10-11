using System.Text;
System.Console.WriteLine($"Encoding.Default           : {Encoding.Default.WebName}");
System.Console.WriteLine($"GetEncoding(0) sin provider: {Encoding.GetEncoding(0).WebName}");
try { Encoding.GetEncoding(1252); System.Console.WriteLine("1252 sin provider: OK"); } catch (System.Exception e) { System.Console.WriteLine($"1252 sin provider: {e.GetType().Name}"); }
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
System.Console.WriteLine($"GetEncoding(0) con provider: {Encoding.GetEncoding(0).WebName}");
System.Console.WriteLine($"GetEncoding(1252) con provider: {Encoding.GetEncoding(1252).WebName}");
