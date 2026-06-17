using FitnessActivityApp.Services;
using FitnessActivityApp.ViewModels;


namespace FitnessActivityApp.Views;

public partial class SettingsPage : ContentPage
{
    private readonly bool _isRequiredSetup;
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(bool isRequiredSetup = false)
    {
        InitializeComponent();

        _isRequiredSetup = isRequiredSetup;

        _viewModel = new SettingsViewModel();
        BindingContext = _viewModel;

        if (_isRequiredSetup)
        {
            NavigationPage.SetHasBackButton(this, false);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (_isRequiredSetup && !SettingsService.IsSetupCompleted())
            return true;

        return base.OnBackButtonPressed();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        bool saved = _viewModel.TrySaveSettings(out string errorMessage);

        if (!saved)
        {
            await DisplayAlertAsync("Greška", errorMessage, "OK");
            return;
        }

        await DisplayAlertAsync("Sačuvano", "Podešavanja su uspešno sačuvana.", "OK");

        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }
}