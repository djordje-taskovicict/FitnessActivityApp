using FitnessActivityApp.Models;
using FitnessActivityApp.Services;
using FitnessActivityApp.ViewModels;
using System.Globalization;
using System.Windows.Input;

namespace FitnessActivityApp.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private string _fullNameText = string.Empty;
    private string _weightKgText = "70";
    private string _heightCmText = "175";
    private string _selectedGender = Gender.Other.ToString();
    private string _walkingStepLengthText = "0.75";
    private string _runningStepLengthText = "1.10";
    private string _statusText = "Unesi osnovne podatke.";

    public SettingsViewModel()
    {
        GenderOptions = new List<string>
        {
            Gender.Male.ToString(),
            Gender.Female.ToString(),
            Gender.Other.ToString()
        };

        EstimateStepLengthsCommand = new Command(EstimateStepLengthsFromUserData);

        LoadSettings();
    }

    public List<string> GenderOptions { get; }

    public ICommand EstimateStepLengthsCommand { get; }

    public string FullNameText
    {
        get => _fullNameText;
        set => SetProperty(ref _fullNameText, value);
    }

    public string WeightKgText
    {
        get => _weightKgText;
        set => SetProperty(ref _weightKgText, value);
    }

    public string HeightCmText
    {
        get => _heightCmText;
        set => SetProperty(ref _heightCmText, value);
    }

    public string SelectedGender
    {
        get => _selectedGender;
        set => SetProperty(ref _selectedGender, value);
    }

    public string WalkingStepLengthText
    {
        get => _walkingStepLengthText;
        set => SetProperty(ref _walkingStepLengthText, value);
    }

    public string RunningStepLengthText
    {
        get => _runningStepLengthText;
        set => SetProperty(ref _runningStepLengthText, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public void LoadSettings()
    {
        UserSettings settings = SettingsService.GetSettings();

        FullNameText = settings.FullName;
        WeightKgText = FormatNumber(settings.WeightKg);
        HeightCmText = FormatNumber(settings.HeightCm);
        SelectedGender = settings.Gender;
        WalkingStepLengthText = FormatNumber(settings.WalkingStepLengthMeters);
        RunningStepLengthText = FormatNumber(settings.RunningStepLengthMeters);
    }

    public bool TrySaveSettings(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!TryReadSettings(out UserSettings settings, out errorMessage))
            return false;

        settings.IsSetupCompleted = true;

        SettingsService.SaveSettings(settings);

        StatusText = "Podešavanja su sačuvana.";

        return true;
    }

    private void EstimateStepLengthsFromUserData()
    {
        if (!TryParseNumber(HeightCmText, out double heightCm))
        {
            StatusText = "Unesi ispravnu visinu.";
            return;
        }

        if (heightCm < 100 || heightCm > 230)
        {
            StatusText = "Visina mora biti između 100 i 230 cm.";
            return;
        }

        WalkingStepLengthText = FormatNumber(
            SettingsService.EstimateWalkingStepLength(heightCm, SelectedGender));

        RunningStepLengthText = FormatNumber(
            SettingsService.EstimateRunningStepLength(heightCm, SelectedGender));

        StatusText = "Dužina koraka je procenjena.";
    }

    private bool TryReadSettings(out UserSettings settings, out string errorMessage)
    {
        settings = new UserSettings();
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FullNameText))
        {
            errorMessage = "Unesi ime ili nadimak.";
            return false;
        }

        if (!TryParseNumber(WeightKgText, out double weightKg))
        {
            errorMessage = "Unesi ispravnu težinu.";
            return false;
        }

        if (!TryParseNumber(HeightCmText, out double heightCm))
        {
            errorMessage = "Unesi ispravnu visinu.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedGender))
        {
            errorMessage = "Izaberi pol.";
            return false;
        }

        if (!TryParseNumber(WalkingStepLengthText, out double walkingStepLength))
        {
            errorMessage = "Unesi ispravnu dužinu koraka za hodanje.";
            return false;
        }

        if (!TryParseNumber(RunningStepLengthText, out double runningStepLength))
        {
            errorMessage = "Unesi ispravnu dužinu koraka za trčanje.";
            return false;
        }

        if (weightKg < 20 || weightKg > 250)
        {
            errorMessage = "Težina mora biti između 20 i 250 kg.";
            return false;
        }

        if (heightCm < 100 || heightCm > 230)
        {
            errorMessage = "Visina mora biti između 100 i 230 cm.";
            return false;
        }

        if (walkingStepLength < 0.30 || walkingStepLength > 1.50)
        {
            errorMessage = "Dužina koraka za hodanje mora biti između 0.30 i 1.50 m.";
            return false;
        }

        if (runningStepLength < 0.50 || runningStepLength > 2.50)
        {
            errorMessage = "Dužina koraka za trčanje mora biti između 0.50 i 2.50 m.";
            return false;
        }

        settings.FullName = FullNameText.Trim();
        settings.WeightKg = weightKg;
        settings.HeightCm = heightCm;
        settings.Gender = SelectedGender;
        settings.WalkingStepLengthMeters = walkingStepLength;
        settings.RunningStepLengthMeters = runningStepLength;

        return true;
    }

    private static bool TryParseNumber(string text, out double value)
    {
        text = text.Trim().Replace(',', '.');

        return double.TryParse(
            text,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}