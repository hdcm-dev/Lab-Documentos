namespace MyProject.Domain.Productos;

// Conjunto cerrado SIN comportamiento: un enum. Los valores son explícitos porque son los que quedan
// escritos en la base: reordenar los nombres no debe reinterpretar las filas ya guardadas.
public enum EstadoProducto
{
    Borrador = 1,
    Publicado = 2,
    Retirado = 3
}
