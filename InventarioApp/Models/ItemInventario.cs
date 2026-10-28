namespace InventarioApp.Models;

/// <summary>Fila de tblInventario con la descripción del producto (JOIN con CatProducto).</summary>
public class ItemInventario
{
	public int Id { get; set; }
	public DateTime FechaAlta { get; set; }
	public int IdProducto { get; set; }
	public string Producto { get; set; } = string.Empty;
	public int Cantidad { get; set; }

	public string FechaTexto => FechaAlta.ToString("yyyy-MM-dd HH:mm");
}
