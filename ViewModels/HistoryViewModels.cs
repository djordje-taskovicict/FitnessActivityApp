using FitnessActivityApp.Models;
using FitnessActivityApp.Services;
using FitnessActivityApp.ViewModels;
using System.Collections.ObjectModel;

namespace FitnessActivityApp.ViewModels;

public class HistoryViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService = new();

    private bool _isBusy;
    private string _emptyText = "Još nema arhiviranih aktivnosti.";

    public ObservableCollection<ActivitySession> Sessions { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string EmptyText
    {
        get => _emptyText;
        set => SetProperty(ref _emptyText, value);
    }

    public async Task LoadSessionsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            Sessions.Clear();

            List<ActivitySession> sessions = await _databaseService.GetArchivedSessionsAsync();

            foreach (ActivitySession session in sessions)
            {
                Sessions.Add(session);
            }

            EmptyText = Sessions.Count == 0
                ? "Još nema arhiviranih aktivnosti."
                : string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }
}