using FitnessActivityApp.Views;

namespace FitnessActivityApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new NavigationPage(new HomePage());
    }
}