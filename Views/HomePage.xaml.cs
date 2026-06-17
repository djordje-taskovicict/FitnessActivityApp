using FitnessActivityApp.Services;
using FitnessActivityApp.ViewModels;
using FitnessActivityApp.Views;


namespace FitnessActivityApp.Views;

public partial class HomePage : ContentPage
{
    private bool _hasCheckedInitialSetup;

    public HomePage()
    {
        InitializeComponent();

        BindingContext = new HomeViewModel(Dispatcher);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasCheckedInitialSetup)
            return;

        _hasCheckedInitialSetup = true;

        if (!SettingsService.IsSetupCompleted()) //First opening of the app requires setup of a profile
        {
            await DisplayAlertAsync(
                "Setup required",
                "Pre korišćenja aplikacije moraš da uneseš osnovne podatke.",
                "OK");

            await Navigation.PushAsync(new SettingsPage(isRequiredSetup: true));
        }
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HistoryPage());
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}