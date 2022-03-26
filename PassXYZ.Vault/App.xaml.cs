namespace PassXYZ.Vault;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

    protected override Window CreateWindow(IActivationState activationState)
    {
        return new PxWindow(new MainPage());
    }
}
