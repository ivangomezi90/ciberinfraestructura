using InventarioApp.Services;

namespace InventarioApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		builder.Services.AddSingleton<DbService>();
		builder.Services.AddSingleton<MainPage>();

		return builder.Build();
	}
}
