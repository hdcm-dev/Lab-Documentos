
namespace LaEstancia.WebAPI.Domain.Estancias;

class Estancia
{
    public string Nombre {get;set;}

    private Estancia()
    {}

    public static Create(string nombre, Campo campo)
    {
        var nuevo=new Estancia();
        nuevo.Nombre=nombre;
        nuevo.AgregarCampo(campo);
    }
}