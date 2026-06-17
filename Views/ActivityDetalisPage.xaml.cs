using FitnessActivityApp.ViewModels;

namespace FitnessActivityApp.Views;

public partial class ActivityDetailsPage : ContentPage
{
    private readonly int _sessionId;
    private readonly ActivityDetailsViewModel _viewModel;

    public ActivityDetailsPage(int sessionId)
    {
        InitializeComponent();

        _sessionId = sessionId;

        _viewModel = new ActivityDetailsViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync(_sessionId);
    }
}