using System.Text.Json.Serialization;

namespace MyProject.Domain.Productos;

// Las clases de contraste del escenario `materializadores` (V09). Ninguna es un modelo a imitar: existen
// para que el laboratorio muestre qué hace cada materializador —EF Core, Dapper y el serializador— con la
// MISMA Entity de §3.3, y con las variantes de constructor que uno se encuentra al equivocarse.
// La Entity bajo prueba es `ProductoQueDevuelve`: constructor privado sin parámetros y setters privados.

// CONTRASTE 3 — sin constructor sin parámetros; los parámetros se llaman como las columnas, pero el tipo
// que el driver informa para la columna no es el de la propiedad. Dapper empareja por nombre Y por tipo.
public class ProductoConCtorDeNombres
{
    public ProductoConCtorDeNombres(string nombre, decimal precio) => (Nombre, Precio) = (nombre, precio);

    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
}

// CONTRASTE 4 — el mismo caso con el tipo que el driver informa (`real` de SQLite llega como `double`).
public class ProductoConCtorDeNombresYTipos
{
    public ProductoConCtorDeNombresYTipos(string nombre, double precio)
        => (Nombre, Precio, VecesQueCorrioElConstructorConParametros) =
           (nombre, (decimal)precio, VecesQueCorrioElConstructorConParametros + 1);

    public static int VecesQueCorrioElConstructorConParametros { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
}

// CONTRASTE 5 — la clase «cerrada a medias»: constructor público sin parámetros y setters privados.
// Es la combinación que parece proteger la Invariant y no protege nada: el serializador no se queja y
// devuelve el objeto en su valor por omisión (§7.4).
public class ProductoConSettersPrivados
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
}

// CONTRASTE 6 — constructor privado CON parámetros, anotado para el serializador. Funciona, y es
// justamente lo que §7.4 desaconseja: el atributo del serializador escribiendo dentro del dominio. Está
// acá para medir que el camino existe, no para copiarlo: lo que viaja es el contrato, no la Entity.
public class ProductoConJsonConstructor
{
    [JsonConstructor]
    private ProductoConJsonConstructor(Guid id, string nombre, decimal precio)
        => (Id, Nombre, Precio) = (id, nombre, precio);

    public Guid Id { get; }
    public string Nombre { get; }
    public decimal Precio { get; }
}

// CONTRASTE 7 — el caso POSITIVO del serializador, y el único que la guía recomienda: lo que viaja no es
// la Entity sino un objeto plano cuyo único constructor público tiene parámetros que se llaman como las
// propiedades. `ProductoResponse` es el record posicional del contrato (§6.3); `ProductoDeContrato` es la
// misma forma escrita como clase, con propiedades de sólo lectura, para que se vea que el serializador
// materializa por el constructor y no hay setter alguno que tocar.
public record ProductoResponse(Guid Id, string Nombre, decimal Precio);

public class ProductoDeContrato
{
    public ProductoDeContrato(Guid id, string nombre, decimal precio)
        => (Id, Nombre, Precio) = (id, nombre, precio);

    public Guid Id { get; }
    public string Nombre { get; }
    public decimal Precio { get; }
}

// CONTRASTE 8 — propiedades que ningún JSON de entrada puede escribir: una de sólo lectura y una
// calculada. Al SALIR, el serializador las publica igual.
public class ProductoDeSalida
{
    public string Nombre { get; private set; } = "Mate";
    public decimal Precio { get; private set; } = 3500m;
    public DateOnly Creado { get; } = new(2026, 9, 21);
    public decimal PrecioConIva => Math.Round(Precio * 1.21m, 2);
}
