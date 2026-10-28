using InventarioApp.Models;
using InventarioApp.Services;

namespace InventarioApp;

public partial class MainPage : ContentPage
{
	private const int ProductoPersonalizadoId = -1;
	private static readonly string[] NombresCatalogo =
	[
		"AMD Ryzen 5 7600", "AMD Ryzen 7 7800X3D", "AMD Ryzen 9 7950X3D", "Intel Core i5-14600K", "Intel Core i7-14700K",
		"Intel Core i9-14900K", "GeForce RTX 4060", "GeForce RTX 4070 SUPER", "GeForce RTX 4080 SUPER", "GeForce RTX 4090",
		"Radeon RX 7600", "Radeon RX 7700 XT", "Radeon RX 7800 XT", "Radeon RX 7900 XT", "Radeon RX 7900 XTX",
		"Placa madre B650 AM5", "Placa madre X670E AM5", "Placa madre B760 LGA1700", "Placa madre Z790 LGA1700", "Memoria DDR5 16 GB",
		"Memoria DDR5 32 GB", "Memoria DDR5 64 GB", "Memoria DDR4 32 GB", "SSD NVMe 1 TB", "SSD NVMe 2 TB",
		"SSD NVMe 4 TB", "SSD SATA 1 TB", "Disco duro 2 TB", "Fuente 650 W 80+ Gold", "Fuente 750 W 80+ Gold",
		"Fuente 850 W 80+ Gold", "Fuente 1000 W 80+ Gold", "Gabinete Mid Tower", "Gabinete Full Tower", "Disipador por aire para CPU",
		"Refrigeración líquida AIO 240 mm", "Refrigeración líquida AIO 360 mm", "Pasta térmica", "Monitor 24 pulgadas 144 Hz", "Monitor 27 pulgadas 165 Hz",
		"Monitor 27 pulgadas 240 Hz", "Monitor 32 pulgadas 4K 144 Hz", "Teclado mecánico TKL", "Teclado mecánico completo", "Mouse gamer",
		"Mousepad gamer XL", "Audífonos gamer", "Micrófono USB", "Webcam", "Control para PC"
	];
	private readonly List<Producto> _catalogoProductos;
	private readonly Producto _opcionPersonalizada;
	private readonly DbService _db;
	private ItemInventario? _seleccionado;

	public MainPage(DbService db)
	{
		InitializeComponent();
		_db = db;
		_catalogoProductos = NombresCatalogo
			.Select(descripcion => new Producto { Descripcion = descripcion })
			.ToList();
		_opcionPersonalizada = new Producto
		{
			Id = ProductoPersonalizadoId,
			Descripcion = "Otro (especificar)"
		};
		_catalogoProductos.Add(_opcionPersonalizada);
		ProductoPicker.ItemsSource = _catalogoProductos;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await ConnectAsync();
	}

	private void SetStatus(string texto, bool ok)
	{
		StatusLabel.Text = texto;
		StatusLabel.TextColor = ok ? Colors.Green : Colors.Red;
	}

	private void SetCrudEnabled(bool conectado)
	{
		AddBtn.IsEnabled = conectado;
		UpdateBtn.IsEnabled = conectado && _seleccionado != null;
		DeleteBtn.IsEnabled = conectado && _seleccionado != null;
	}

	private async Task ConnectAsync()
	{
		try
		{
			var settings = await DatabaseSettings.LoadAsync();
			_db.Configure(settings);
			var version = await _db.ProbarConexionAsync();
			SetStatus($"Conectado a MySQL {version} ({settings.Database})", true);
			await RecargarAsync();
			SetCrudEnabled(true);
		}
		catch (Exception ex)
		{
			SetCrudEnabled(false);
			SetStatus($"No se pudo conectar: {ex.Message}", false);
		}
	}

	private async Task RecargarAsync()
	{
		InventarioView.ItemsSource = await _db.GetInventarioAsync(SearchBar.Text);
		_seleccionado = null;
		InventarioView.SelectedItem = null;
		SetCrudEnabled(true);
	}

	private bool TryLeerFormulario(out Producto producto, out int cantidad)
	{
		var seleccionado = ProductoPicker.SelectedItem as Producto;
		producto = new Producto();
		cantidad = 0;

		if (seleccionado == null)
		{
			DisplayAlert("Validación", "Selecciona un producto.", "OK");
			return false;
		}

		if (seleccionado.Id == ProductoPersonalizadoId)
		{
			var descripcion = ProductoPersonalizadoEntry.Text?.Trim();
			if (string.IsNullOrWhiteSpace(descripcion))
			{
				DisplayAlert("Validación", "Escribe el nombre del producto.", "OK");
				return false;
			}
			producto.Descripcion = descripcion;
		}
		else
		{
			producto.Descripcion = seleccionado.Descripcion;
		}

		if (!int.TryParse(CantidadEntry.Text, out cantidad) || cantidad <= 0)
		{
			DisplayAlert("Validación", "La cantidad debe ser un entero mayor que 0.", "OK");
			return false;
		}
		return true;
	}

	private async void OnAddClicked(object? sender, EventArgs e)
	{
		if (!TryLeerFormulario(out var producto, out var cantidad)) return;
		try
		{
			var productoGuardado = await _db.ObtenerOCrearProductoAsync(producto.Descripcion);
			await _db.AgregarAsync(productoGuardado.Id, cantidad);
			CantidadEntry.Text = string.Empty;
			await RecargarAsync();
			SetStatus($"Se agregó {cantidad} x {productoGuardado.Descripcion}.", true);
		}
		catch (Exception ex) { SetStatus($"Error al agregar: {ex.Message}", false); }
	}

	private async void OnUpdateClicked(object? sender, EventArgs e)
	{
		if (_seleccionado == null) return;
		if (!TryLeerFormulario(out var producto, out var cantidad)) return;
		try
		{
			var productoGuardado = await _db.ObtenerOCrearProductoAsync(producto.Descripcion);
			await _db.ActualizarAsync(_seleccionado.Id, productoGuardado.Id, cantidad);
			await RecargarAsync();
			SetStatus($"Registro {_seleccionado?.Id} actualizado.", true);
		}
		catch (Exception ex) { SetStatus($"Error al actualizar: {ex.Message}", false); }
	}

	private async void OnDeleteClicked(object? sender, EventArgs e)
	{
		if (_seleccionado == null) return;
		var id = _seleccionado.Id;
		if (!await DisplayAlert("Eliminar", $"¿Eliminar el registro {id} ({_seleccionado.Producto})?", "Sí", "No"))
			return;
		try
		{
			await _db.EliminarAsync(id);
			CantidadEntry.Text = string.Empty;
			await RecargarAsync();
			SetStatus($"Registro {id} eliminado.", true);
		}
		catch (Exception ex) { SetStatus($"Error al eliminar: {ex.Message}", false); }
	}

	private void OnProductoSelectionChanged(object? sender, EventArgs e)
	{
		var esPersonalizado = (ProductoPicker.SelectedItem as Producto)?.Id == ProductoPersonalizadoId;
		ProductoPersonalizadoEntry.IsVisible = esPersonalizado;
		if (!esPersonalizado)
			ProductoPersonalizadoEntry.Text = string.Empty;
	}

	private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		_seleccionado = e.CurrentSelection.FirstOrDefault() as ItemInventario;
		if (_seleccionado != null)
		{
			CantidadEntry.Text = _seleccionado.Cantidad.ToString();
			var productoCatalogado = _catalogoProductos.FirstOrDefault(producto =>
				producto.Id != ProductoPersonalizadoId &&
				string.Equals(producto.Descripcion, _seleccionado.Producto, StringComparison.OrdinalIgnoreCase));
			if (productoCatalogado != null)
			{
				ProductoPicker.SelectedItem = productoCatalogado;
			}
			else
			{
				ProductoPicker.SelectedItem = _opcionPersonalizada;
				ProductoPersonalizadoEntry.Text = _seleccionado.Producto;
			}
		}
		SetCrudEnabled(AddBtn.IsEnabled);
	}

	private async void OnSearch(object? sender, EventArgs e) => await BuscarAsync();

	private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
	{
		if (AddBtn.IsEnabled) await BuscarAsync();
	}

	private async Task BuscarAsync()
	{
		try { await RecargarAsync(); }
		catch (Exception ex) { SetStatus($"Error al buscar: {ex.Message}", false); }
	}
}
