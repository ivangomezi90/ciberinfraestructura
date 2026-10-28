using System.Text.Json;
using Microsoft.Maui.Storage;

namespace InventarioApp.Services;

public sealed class DatabaseSettings
{
	public string Server { get; init; } = "localhost";
	public uint Port { get; init; } = 3306;
	public string Database { get; init; } = "inventario";
	public string User { get; init; } = "root";
	public string Password { get; init; } = string.Empty;

	public static async Task<DatabaseSettings> LoadAsync()
	{
		await using var stream = await FileSystem.OpenAppPackageFileAsync("database.json");
		return await JsonSerializer.DeserializeAsync<DatabaseSettings>(
			stream,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
			?? throw new InvalidDataException("El archivo database.json está vacío o no es válido.");
	}
}