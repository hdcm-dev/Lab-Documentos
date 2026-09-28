namespace LaEstancia.WebAPI.Domain.Campos;

class Campo
{
    public string Identificador {get; private set;}=string.Empty;
    public double SuperficieTotal {get;set;}
    public int CantidadParcelas {get;set;}

    private List<Parcela> parcelas {get;set;}

    private Campo(){}

    public static Campo(string identificador, double superficie)
    {
        var nuevo=new Campo();
        nuevo.Identificador=identificador;
    }
}