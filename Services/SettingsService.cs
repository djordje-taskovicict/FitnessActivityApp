using FitnessActivityApp.Models;
using FitnessActivityApp.Models;
using Microsoft.Maui.Storage;

namespace FitnessActivityApp.Services;

public static class SettingsService
{
    private const string FullNameKey = "FullName";
    private const string WeightKgKey = "WeightKg";
    private const string HeightCmKey = "HeightCm";
    private const string GenderKey = "Gender";
    private const string WalkingStepLengthMetersKey = "WalkingStepLengthMeters";
    private const string RunningStepLengthMetersKey = "RunningStepLengthMeters";
    private const string IsSetupCompletedKey = "IsSetupCompleted";

    public static UserSettings GetSettings()
    {
        return new UserSettings
        {
            FullName = Preferences.Get(FullNameKey, string.Empty),
            WeightKg = Preferences.Get(WeightKgKey, 70.0),
            HeightCm = Preferences.Get(HeightCmKey, 175.0),
            Gender = Preferences.Get(GenderKey, Gender.Other.ToString()),
            WalkingStepLengthMeters = Preferences.Get(WalkingStepLengthMetersKey, 0.75),
            RunningStepLengthMeters = Preferences.Get(RunningStepLengthMetersKey, 1.10),
            IsSetupCompleted = Preferences.Get(IsSetupCompletedKey, false)
        };
    }

    public static void SaveSettings(UserSettings settings)
    {
        Preferences.Set(FullNameKey, settings.FullName.Trim());
        Preferences.Set(WeightKgKey, settings.WeightKg);
        Preferences.Set(HeightCmKey, settings.HeightCm);
        Preferences.Set(GenderKey, settings.Gender);
        Preferences.Set(WalkingStepLengthMetersKey, settings.WalkingStepLengthMeters);
        Preferences.Set(RunningStepLengthMetersKey, settings.RunningStepLengthMeters);
        Preferences.Set(IsSetupCompletedKey, settings.IsSetupCompleted);
    }

    public static bool IsSetupCompleted()
    {
        return Preferences.Get(IsSetupCompletedKey, false);
    }

    public static void ClearSetup()
    {
        Preferences.Set(IsSetupCompletedKey, false);
    }

    public static double EstimateWalkingStepLength(double heightCm, string gender)
    {
        double heightMeters = heightCm / 100.0;

        double multiplier = gender switch
        {
            "Male" => 0.415,
            "Female" => 0.413,
            _ => 0.414
        };

        return Math.Round(heightMeters * multiplier, 2);
    }

    public static double EstimateRunningStepLength(double heightCm, string gender)
    {
        double heightMeters = heightCm / 100.0;

        double multiplier = gender switch
        {
            "Male" => 0.65,
            "Female" => 0.62,
            _ => 0.63
        };

        return Math.Round(heightMeters * multiplier, 2);
    }
}