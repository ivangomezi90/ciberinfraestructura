using InventarioApp.Models;
using MySqlConnector;

namespace InventarioApp.Services;

/// <summary>
/// Acceso a la base de datos "inventario" (ver inventario_schema.sql):
/// CatProducto(ID, Descripcion) y tblInventario(ID, FechaAlta, IDProducto, Cantidad).
/// Todas las consultas están parametrizadas.
/// </summary>
public class DbService
{
	private string? _connectionString;

	private string ConnectionString => _connectionString
		?? throw new InvalidOperationException("La configuración de la base de datos no se ha cargado.");

	public void Configure(DatabaseSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings);

		_connectionString = new MySqlConnectionStringBuilder
		{
			Server = settings.Server,
			Port = settings.Port,
			Database = settings.Database,
			UserID = settings.User,
			Password = settings.Password,
			ConnectionTimeout = 5,
			CharacterSet = "utf8mb4"
		}.ConnectionString;
	}

	private async Task<MySqlConnection> OpenAsync()
	{
		var conn = new MySqlConnection(ConnectionString);
		await conn.OpenAsync();
		return conn;
	}

	/// <summary>Abre la conexión y devuelve la versión del servidor.</summary>
	public async Task<string> ProbarConexionAsync()
	{
		await using var conn = await OpenAsync();
		return conn.ServerVersion;
	}

	public async Task<Producto> ObtenerOCrearProductoAsync(string descripcion)
	{
		descripcion = descripcion.Trim();
		if (string.IsNullOrWhiteSpace(descripcion))
			throw new ArgumentException("La descripción del producto es obligatoria.", nameof(descripcion));

		await using var conn = await OpenAsync();
		await using (var lookup = new MySqlCommand(
			"SELECT ID FROM CatProducto WHERE Descripcion = @descripcion LIMIT 1", conn))
		{
			lookup.Parameters.AddWithValue("@descripcion", descripcion);
			var existingId = await lookup.ExecuteScalarAsync();
			if (existingId != null && existingId != DBNull.Value)
				return new Producto { Id = Convert.ToInt32(existingId), Descripcion = descripcion };
		}

		await using (var insert = new MySqlCommand(
			"INSERT INTO CatProducto (Descripcion) VALUES (@descripcion)", conn))
		{
			insert.Parameters.AddWithValue("@descripcion", descripcion);
			await insert.ExecuteNonQueryAsync();
		}

		await using var getId = new MySqlCommand("SELECT LAST_INSERT_ID()", conn);
		var newId = Convert.ToInt32(await getId.ExecuteScalarAsync());
		return new Producto { Id = newId, Descripcion = descripcion };
	}

	public async Task<List<ItemInventario>> GetInventarioAsync(string? filtro = null)
	{
		var lista = new List<ItemInventario>();
		await using var conn = await OpenAsync();
		await using var cmd = new MySqlCommand(@"
			SELECT i.ID, i.FechaAlta, i.IDProducto, p.Descripcion, i.Cantidad
			FROM tblInventario i
			JOIN CatProducto p ON p.ID = i.IDProducto
			WHERE @filtro IS NULL OR p.Descripcion LIKE CONCAT('%', @filtro, '%')
			ORDER BY i.ID", conn);
		cmd.Parameters.AddWithValue("@filtro", string.IsNullOrWhiteSpace(filtro) ? DBNull.Value : filtro.Trim());
		await using var r = await cmd.ExecuteReaderAsync();
		while (await r.ReadAsync())
		{
			lista.Add(new ItemInventario
			{
				Id = r.GetInt32("ID"),
				FechaAlta = r.GetDateTime("FechaAlta"),
				IdProducto = r.GetInt32("IDProducto"),
				Producto = r.GetString("Descripcion"),
				Cantidad = r.GetInt32("Cantidad")
			});
		}
		return lista;
	}

	public async Task<int> AgregarAsync(int idProducto, int cantidad)
	{
		await using var conn = await OpenAsync();
		await using var cmd = new MySqlCommand(
			"INSERT INTO tblInventario (IDProducto, Cantidad) VALUES (@prod, @cant)", conn);
		cmd.Parameters.AddWithValue("@prod", idProducto);
		cmd.Parameters.AddWithValue("@cant", cantidad);
		return await cmd.ExecuteNonQueryAsync();
	}

	public async Task<int> ActualizarAsync(int id, int idProducto, int cantidad)
	{
		await using var conn = await OpenAsync();
		await using var cmd = new MySqlCommand(
			"UPDATE tblInventario SET IDProducto = @prod, Cantidad = @cant WHERE ID = @id", conn);
		cmd.Parameters.AddWithValue("@prod", idProducto);
		cmd.Parameters.AddWithValue("@cant", cantidad);
		cmd.Parameters.AddWithValue("@id", id);
		return await cmd.ExecuteNonQueryAsync();
	}

	public async Task<int> EliminarAsync(int id)
	{
		await using var conn = await OpenAsync();
		await using var cmd = new MySqlCommand("DELETE FROM tblInventario WHERE ID = @id", conn);
		cmd.Parameters.AddWithValue("@id", id);
		return await cmd.ExecuteNonQueryAsync();
	}
}
