using FitnessActivityApp.Models;
using FitnessActivityApp.ViewModels;

namespace FitnessActivityApp.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage()
    {
        InitializeComponent();

        _viewModel = new HistoryViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadSessionsAsync();
    }

    private async void OnSessionSelected(object sender, SelectionChangedEventArgs e)
    {
        ActivitySession? selectedSession = e.CurrentSelection.FirstOrDefault() as ActivitySession;

        if (selectedSession == null)
            return;

        ((CollectionView)sender).SelectedItem = null;

        await Navigation.PushAsync(new ActivityDetailsPage(selectedSession.Id));
    }
}