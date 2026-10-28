# InventarioApp (.NET MAUI + MySQL)

CRUD de `tblInventario` sobre la base `inventario` (ver `../inventario_schema.sql`).

## Requisitos
- .NET SDK 9 + workload MAUI: `dotnet workload install maui` (o ajusta el `TargetFramework` del `.csproj` a tu SDK)
- MySQL local con el esquema cargado:
  `mysql -u root -p < ../inventario_schema.sql`

## Configurar la conexión
Edite `Resources/Raw/database.json` con el servidor, puerto, base, usuario y contraseña. Ese archivo es local y está ignorado por Git; use `database.example.json` como referencia. La app carga estos valores y se conecta al abrirse.

Las credenciales empaquetadas dentro de una app móvil se pueden extraer. Esta conexión directa es adecuada para desarrollo local; para producción, use una API entre la app y MySQL.

## Ejecutar (macOS)
```bash
dotnet build -t:Run -f net9.0-maccatalyst
```

## Estructura
- `Services/DbService.cs`: conexión (MySqlConnector) y consultas parametrizadas.
- `Models/`: `Producto` (CatProducto) e `ItemInventario` (tblInventario JOIN CatProducto).
- `MainPage.xaml(.cs)`: alta, edición, baja y búsqueda.