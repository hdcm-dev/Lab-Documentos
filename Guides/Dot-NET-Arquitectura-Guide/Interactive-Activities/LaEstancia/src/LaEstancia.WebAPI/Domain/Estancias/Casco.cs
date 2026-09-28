
namespace LaEstancia.WebAPI.Domain;

public class Casco
{
  public string? Administrador {get;set;}    

  private Casco()
  {}

  public static Casco Create()
  {
     var casco= new Casco();         
     casco.Administrador="No Definido";
     return casco;
  }
}