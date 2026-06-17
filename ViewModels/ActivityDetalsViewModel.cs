using System.Collections.ObjectModel;
using FitnessActivityApp.Models;
using FitnessActivityApp.Services;

namespace FitnessActivityApp.ViewModels;

public class ActivityDetailsViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService = new();

    private ActivitySession? _session;
    private string _titleText = "Activity details";
    private string _mainStatsText = string.Empty;
    private string _speedStatsText = string.Empty;
    private string _patternText = string.Empty;

    public ObservableCollection<ActivitySample> Samples { get; } = new();

    public ActivitySession? Session
    {
        get => _session;
        set => SetProperty(ref _session, value);
    }

    public string TitleText
    {
        get => _titleText;
        set => SetProperty(ref _titleText, value);
    }

    public string MainStatsText
    {
        get => _mainStatsText;
        set => SetProperty(ref _mainStatsText, value);
    }

    public string SpeedStatsText
    {
        get => _speedStatsText;
        set => SetProperty(ref _speedStatsText, value);
    }

    public string PatternText
    {
        get => _patternText;
        set => SetProperty(ref _patternText, value);
    }

    public async Task LoadAsync(int sessionId)
    {
        Session = await _databaseService.GetSessionByIdAsync(sessionId);

        if (Session == null)
            return;

        List<ActivitySample> samples = await _databaseService.GetSamplesForSessionAsync(sessionId);

        Samples.Clear();

        foreach (ActivitySample sample in samples)
        {
            Samples.Add(sample);
        }

        TimeSpan duration = TimeSpan.FromSeconds(Session.DurationSeconds);

        TitleText = $"{Session.ActivityType} - {Session.StartTime:dd.MM.yyyy HH:mm}";

        string stepsText = Session.ActivityType == ActivityType.Cycling.ToString()
            ? "N/A"
            : $"{Session.Steps:N0}";

        MainStatsText =
            $"Distance: {Session.DistanceMeters / 1000.0:0.00} km\n" +
            $"Duration: {duration:hh\\:mm\\:ss}\n" +
            $"Steps: {stepsText}\n" +
            $"Calories: {Session.Calories:0} kcal";

        SpeedStatsText =
            $"Average speed: {Session.AverageSpeedKmh:0.0} km/h\n" +
            $"Max speed: {Session.MaxSpeedKmh:0.0} km/h\n" +
            $"GPS samples: {Samples.Count}";

        PatternText = BuildPatternText(samples, Session.ActivityType);
    }

    private static string BuildPatternText(List<ActivitySample> samples, string activityType)
    {
        if (samples.Count == 0)
            return "Nema dovoljno uzoraka za pattern prikaz.";

        double maxSpeed = samples.Max(x => x.SpeedKmh);
        double minSpeed = samples.Min(x => x.SpeedKmh);
        double avgSpeed = samples.Average(x => x.SpeedKmh);

        if (activityType == ActivityType.Cycling.ToString())
        {
            return
                $"Pattern vožnje:\n" +
                $"Min sample speed: {minSpeed:0.0} km/h\n" +
                $"Avg sample speed: {avgSpeed:0.0} km/h\n" +
                $"Max sample speed: {maxSpeed:0.0} km/h\n" +
                $"Broj zabeleženih GPS uzoraka: {samples.Count}";
        }

        double maxCadence = samples.Max(x => x.CadenceStepsPerMinute);
        double avgCadence = samples.Average(x => x.CadenceStepsPerMinute);
        int finalSteps = samples.Max(x => x.Steps);

        return
            $"Pattern hoda/trčanja:\n" +
            $"Min sample speed: {minSpeed:0.0} km/h\n" +
            $"Avg sample speed: {avgSpeed:0.0} km/h\n" +
            $"Max sample speed: {maxSpeed:0.0} km/h\n" +
            $"Final steps: {finalSteps:N0}\n" +
            $"Avg cadence: {avgCadence:0} steps/min\n" +
            $"Max cadence: {maxCadence:0} steps/min\n" +
            $"Broj zabeleženih GPS uzoraka: {samples.Count}";
    }
}