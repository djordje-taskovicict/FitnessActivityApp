using FitnessActivityApp.Models;

namespace FitnessActivityApp.Services;

public static class ActivityCalculationService
{
    public static int EstimateSteps(
        string activityType,
        double distanceMeters,
        UserSettings settings)
    {
        if (distanceMeters <= 0)
            return 0;

        if (activityType == ActivityType.Cycling.ToString())
            return 0;

        double stepLengthMeters = activityType == ActivityType.Running.ToString()
            ? settings.RunningStepLengthMeters
            : settings.WalkingStepLengthMeters;

        if (stepLengthMeters <= 0)
            stepLengthMeters = 0.75;

        return (int)Math.Round(distanceMeters / stepLengthMeters);
    }

    public static double EstimateCalories(
        string activityType,
        double weightKg,
        double durationSeconds,
        double averageSpeedKmh)
    {
        if (weightKg <= 0 || durationSeconds <= 0)
            return 0;

        double durationHours = durationSeconds / 3600.0;
        double met = GetMetValue(activityType, averageSpeedKmh);

        return Math.Round(met * weightKg * durationHours, 0);
    }

    private static double GetMetValue(string activityType, double averageSpeedKmh)
    {
        if (activityType == ActivityType.Walking.ToString())
        {
            if (averageSpeedKmh < 4)
                return 2.8;

            if (averageSpeedKmh < 5.5)
                return 3.5;

            return 4.3;
        }

        if (activityType == ActivityType.Running.ToString())
        {
            if (averageSpeedKmh < 8)
                return 7.0;

            if (averageSpeedKmh < 11)
                return 9.8;

            return 11.5;
        }

        if (activityType == ActivityType.Cycling.ToString())
        {
            if (averageSpeedKmh < 16)
                return 4.0;

            if (averageSpeedKmh < 22)
                return 6.8;

            return 8.0;
        }

        return 3.0;
    }
}