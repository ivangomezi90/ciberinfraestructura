namespace InventarioApp.Models;

/// <summary>Fila de CatProducto.</summary>
public class Producto
{
	public int Id { get; set; }
	public string Descripcion { get; set; } = string.Empty;

	public override string ToString() => Descripcion;
}
