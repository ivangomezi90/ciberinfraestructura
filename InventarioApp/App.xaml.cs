namespace InventarioApp;

public partial class App : Application
{
	private readonly MainPage _mainPage;

	public App(MainPage mainPage)
	{
		InitializeComponent();
		_mainPage = mainPage;
	}

	protected override Window CreateWindow(IActivationState? activationState)
		=> new Window(_mainPage) { Title = "Inventario", Width = 900, Height = 800 };
}
