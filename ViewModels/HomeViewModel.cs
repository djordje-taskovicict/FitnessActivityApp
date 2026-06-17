using System.Collections.ObjectModel;
using System.Windows.Input;
using FitnessActivityApp.Models;
using FitnessActivityApp.Services;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Dispatching;

namespace FitnessActivityApp.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IDispatcher _dispatcher;
    private readonly LocationService _locationService = new();
    private readonly DatabaseService _databaseService = new();
    private readonly StepCounterService _stepCounterService = new();

    private IDispatcherTimer? _uiTimer;
    private IDispatcherTimer? _locationTimer;

    private readonly List<ActivitySample> _activitySamples = new();

    private Location? _lastLocation;
    private DateTime? _lastLocationTime;

    private bool _isActivityRunning;
    private bool _isUsingRealStepCounter;

    private bool _canStartActivity;
    private bool _canStopActivity;

    private string _selectedActivityType = ActivityType.Walking.ToString();
    private string _statusText = "Ready.";

    private DateTime? _startTime;
    private DateTime? _endTime;

    private double _durationSeconds;
    private double _distanceMeters;
    private double _currentSpeedKmh;
    private double _averageSpeedKmh;
    private double _maxSpeedKmh;
    private int _steps;
    private double _calories;
    private double _currentCadenceStepsPerMinute;

    private string _durationText = "00:00:00";
    private string _distanceText = "0.00 km";
    private string _currentSpeedText = "0.0 km/h";
    private string _averageSpeedText = "0.0 km/h";
    private string _maxSpeedText = "0.0 km/h";
    private string _stepsText = "0";
    private string _caloriesText = "0 kcal";

    public ObservableCollection<string> ActivityTypes { get; } = new();

    public ICommand NewActivityCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    public HomeViewModel(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        ActivityTypes.Add(ActivityType.Walking.ToString());
        ActivityTypes.Add(ActivityType.Running.ToString());
        ActivityTypes.Add(ActivityType.Cycling.ToString());

        SelectedActivityType = ActivityType.Walking.ToString();

        NewActivityCommand = new Command(async () => await NewActivityAsync());
        StartCommand = new Command(async () => await StartActivityAsync());
        StopCommand = new Command(async () => await StopActivityAsync());

        ResetActivity();
    }

    public bool IsActivityRunning
    {
        get => _isActivityRunning;
        set => SetProperty(ref _isActivityRunning, value);
    }

    public bool CanStartActivity
    {
        get => _canStartActivity;
        set => SetProperty(ref _canStartActivity, value);
    }

    public bool CanStopActivity
    {
        get => _canStopActivity;
        set => SetProperty(ref _canStopActivity, value);
    }

    public string SelectedActivityType
    {
        get => _selectedActivityType;
        set => SetProperty(ref _selectedActivityType, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public DateTime? StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public DateTime? EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    public double DurationSeconds
    {
        get => _durationSeconds;
        set => SetProperty(ref _durationSeconds, value);
    }

    public double DistanceMeters
    {
        get => _distanceMeters;
        set => SetProperty(ref _distanceMeters, value);
    }

    public double CurrentSpeedKmh
    {
        get => _currentSpeedKmh;
        set => SetProperty(ref _currentSpeedKmh, value);
    }

    public double AverageSpeedKmh
    {
        get => _averageSpeedKmh;
        set => SetProperty(ref _averageSpeedKmh, value);
    }

    public double MaxSpeedKmh
    {
        get => _maxSpeedKmh;
        set => SetProperty(ref _maxSpeedKmh, value);
    }

    public int Steps
    {
        get => _steps;
        set => SetProperty(ref _steps, value);
    }

    public double Calories
    {
        get => _calories;
        set => SetProperty(ref _calories, value);
    }

    public double CurrentCadenceStepsPerMinute
    {
        get => _currentCadenceStepsPerMinute;
        set => SetProperty(ref _currentCadenceStepsPerMinute, value);
    }

    public string DurationText
    {
        get => _durationText;
        set => SetProperty(ref _durationText, value);
    }

    public string DistanceText
    {
        get => _distanceText;
        set => SetProperty(ref _distanceText, value);
    }

    public string CurrentSpeedText
    {
        get => _currentSpeedText;
        set => SetProperty(ref _currentSpeedText, value);
    }

    public string AverageSpeedText
    {
        get => _averageSpeedText;
        set => SetProperty(ref _averageSpeedText, value);
    }

    public string MaxSpeedText
    {
        get => _maxSpeedText;
        set => SetProperty(ref _maxSpeedText, value);
    }

    public string StepsText
    {
        get => _stepsText;
        set => SetProperty(ref _stepsText, value);
    }

    public string CaloriesText
    {
        get => _caloriesText;
        set => SetProperty(ref _caloriesText, value);
    }

    private async Task NewActivityAsync()
    {
        if (IsActivityRunning)
            return;

        ResetActivity();

        CanStartActivity = true;
        CanStopActivity = false;

        StatusText = "New activity is ready.";

        await Task.CompletedTask;
    }

    private async Task StartActivityAsync()
    {
        if (IsActivityRunning || !CanStartActivity)
            return;

        bool hasPermission = await _locationService.RequestLocationPermissionAsync();

        if (!hasPermission)
        {
            StatusText = "Location permission is required.";
            return;
        }

        ResetActivity();

        StartTime = DateTime.Now;
        EndTime = null;

        await TryStartStepCounterAsync();

        IsActivityRunning = true;

        CanStartActivity = false;
        CanStopActivity = true;

        if (SelectedActivityType == ActivityType.Cycling.ToString())
        {
            StatusText = "Activity is running.";
        }
        else if (_isUsingRealStepCounter)
        {
            StatusText = "Activity is running. Real step counter active.";
        }
        else
        {
            StatusText = "Activity is running. Step estimate fallback active.";
        }

        StartTimers();
    }

    private async Task TryStartStepCounterAsync()
    {
        _isUsingRealStepCounter = false;

        _stepCounterService.Stop();
        _stepCounterService.Reset();

        if (SelectedActivityType == ActivityType.Cycling.ToString())
            return;

        bool permissionGranted = await _stepCounterService.RequestPermissionAsync();

        if (!permissionGranted)
            return;

        if (!_stepCounterService.IsAvailable)
            return;

        bool started = _stepCounterService.Start();

        _isUsingRealStepCounter = started;
    }

    private async Task StopActivityAsync()
    {
        if (!IsActivityRunning)
            return;

        IsActivityRunning = false;
        EndTime = DateTime.Now;

        CanStartActivity = false;
        CanStopActivity = false;

        StopTimers();

        _stepCounterService.Stop();

        UpdateEstimatedStats();
        UpdateDisplayTexts();

        await SaveFinishedActivityAsync();

        StatusText = "Activity saved.";
    }

    private void ResetActivity()
    {
        StopTimers();

        StartTime = null;
        EndTime = null;

        DurationSeconds = 0;
        DistanceMeters = 0;

        CurrentSpeedKmh = 0;
        AverageSpeedKmh = 0;
        MaxSpeedKmh = 0;

        Steps = 0;
        Calories = 0;
        CurrentCadenceStepsPerMinute = 0;

        _lastLocation = null;
        _lastLocationTime = null;

        _activitySamples.Clear();

        _isUsingRealStepCounter = false;
        _stepCounterService.Stop();
        _stepCounterService.Reset();

        CanStartActivity = false;
        CanStopActivity = false;

        UpdateDisplayTexts();
    }

    private void StartTimers()
    {
        StopTimers();

        _uiTimer = _dispatcher.CreateTimer();
        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += OnUiTimerTick;
        _uiTimer.Start();

        _locationTimer = _dispatcher.CreateTimer();
        _locationTimer.Interval = TimeSpan.FromSeconds(5);
        _locationTimer.Tick += async (s, e) => await TrackLocationAsync();
        _locationTimer.Start();
    }

    private void StopTimers()
    {
        if (_uiTimer != null)
        {
            _uiTimer.Stop();
            _uiTimer = null;
        }

        if (_locationTimer != null)
        {
            _locationTimer.Stop();
            _locationTimer = null;
        }
    }

    private void OnUiTimerTick(object? sender, EventArgs e)
    {
        if (!IsActivityRunning || StartTime == null)
            return;

        DurationSeconds = (DateTime.Now - StartTime.Value).TotalSeconds;

        UpdateEstimatedStats();
        UpdateDisplayTexts();
    }

    private async Task TrackLocationAsync()
    {
        if (!IsActivityRunning)
            return;

        Location? currentLocation = await _locationService.GetCurrentLocationAsync();

        if (currentLocation == null)
            return;

        if (!IsValidLocation(currentLocation))
            return;

        DateTime currentTime = DateTime.Now;

        if (_lastLocation != null && _lastLocationTime != null)
        {
            double distance = Location.CalculateDistance(
                _lastLocation,
                currentLocation,
                DistanceUnits.Kilometers) * 1000.0;

            double seconds = (currentTime - _lastLocationTime.Value).TotalSeconds;

            if (seconds > 0 && distance > 0)
            {
                double segmentSpeedKmh = (distance / 1000.0) / (seconds / 3600.0);

                if (!IsValidSpeed(segmentSpeedKmh))
                    return;

                DistanceMeters += distance;
                CurrentSpeedKmh = segmentSpeedKmh;

                if (CurrentSpeedKmh > MaxSpeedKmh)
                    MaxSpeedKmh = CurrentSpeedKmh;
            }
        }

        _lastLocation = currentLocation;
        _lastLocationTime = currentTime;

        UpdateAverageSpeed();
        UpdateEstimatedStats();
        AddActivitySample(currentLocation);
        UpdateDisplayTexts();
    }

    private static bool IsValidLocation(Location location)
    {
        if (location.Accuracy.HasValue && location.Accuracy.Value > 50)
            return false;

        return true;
    }

    private bool IsValidSpeed(double speedKmh)
    {
        if (SelectedActivityType == ActivityType.Walking.ToString() && speedKmh > 15)
            return false;

        if (SelectedActivityType == ActivityType.Running.ToString() && speedKmh > 35)
            return false;

        if (SelectedActivityType == ActivityType.Cycling.ToString() && speedKmh > 80)
            return false;

        return true;
    }

    private void UpdateAverageSpeed()
    {
        if (DurationSeconds <= 0)
        {
            AverageSpeedKmh = 0;
            return;
        }

        AverageSpeedKmh = (DistanceMeters / 1000.0) / (DurationSeconds / 3600.0);
    }

    private void AddActivitySample(Location location)
    {
        ActivitySample sample = new()
        {
            RecordedAt = DateTime.Now,
            ElapsedSeconds = DurationSeconds,
            DistanceMeters = DistanceMeters,
            SpeedKmh = CurrentSpeedKmh,
            Steps = Steps,
            CadenceStepsPerMinute = CurrentCadenceStepsPerMinute,
            Latitude = location.Latitude,
            Longitude = location.Longitude
        };

        _activitySamples.Add(sample);
    }

    private void UpdateEstimatedStats()
    {
        UserSettings settings = SettingsService.GetSettings();

        if (SelectedActivityType == ActivityType.Cycling.ToString())
        {
            Steps = 0;
            CurrentCadenceStepsPerMinute = 0;
        }
        else if (_isUsingRealStepCounter)
        {
            Steps = _stepCounterService.CurrentActivitySteps;
        }
        else
        {
            Steps = ActivityCalculationService.EstimateSteps(
                SelectedActivityType,
                DistanceMeters,
                settings);
        }

        Calories = ActivityCalculationService.EstimateCalories(
            SelectedActivityType,
            settings.WeightKg,
            DurationSeconds,
            AverageSpeedKmh);

        if (SelectedActivityType != ActivityType.Cycling.ToString() && DurationSeconds > 0)
        {
            CurrentCadenceStepsPerMinute = Steps / (DurationSeconds / 60.0);
        }
    }

    private void UpdateDisplayTexts()
    {
        DurationText = TimeSpan.FromSeconds(DurationSeconds).ToString(@"hh\:mm\:ss");
        DistanceText = $"{DistanceMeters / 1000.0:0.00} km";
        CurrentSpeedText = $"{CurrentSpeedKmh:0.0} km/h";
        AverageSpeedText = $"{AverageSpeedKmh:0.0} km/h";
        MaxSpeedText = $"{MaxSpeedKmh:0.0} km/h";
        StepsText = $"{Steps:N0}";
        CaloriesText = $"{Calories:0} kcal";
    }

    private async Task SaveFinishedActivityAsync()
    {
        ActivitySession session = new()
        {
            ActivityType = SelectedActivityType,
            StartTime = StartTime ?? DateTime.Now,
            EndTime = EndTime,
            DurationSeconds = DurationSeconds,
            DistanceMeters = DistanceMeters,
            Steps = Steps,
            AverageSpeedKmh = AverageSpeedKmh,
            MaxSpeedKmh = MaxSpeedKmh,
            Calories = Calories,
            IsArchived = true
        };

        await _databaseService.SaveActivitySessionAsync(session);

        foreach (ActivitySample sample in _activitySamples)
        {
            sample.ActivitySessionId = session.Id;
        }

        if (_activitySamples.Count > 0)
        {
            await _databaseService.SaveActivitySamplesAsync(_activitySamples);
        }
    }
}