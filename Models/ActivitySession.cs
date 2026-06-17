using SQLite;

namespace FitnessActivityApp.Models;

public class ActivitySession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string ActivityType { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public double DurationSeconds { get; set; }
    public double DistanceMeters { get; set; }

    public int Steps { get; set; }

    public double AverageSpeedKmh { get; set; }
    public double MaxSpeedKmh { get; set; }

    public double Calories { get; set; }

    public bool IsArchived { get; set; } = true;

    [Ignore]
    public string Title => $"{ActivityType} - {StartTime:dd.MM.yyyy HH:mm}";

    [Ignore]
    public string Summary
    {
        get
        {
            string duration = TimeSpan.FromSeconds(DurationSeconds).ToString(@"hh\:mm\:ss");

            string activityExtra = ActivityType == Models.ActivityType.Cycling.ToString()
                ? $"Max {MaxSpeedKmh:0.0} km/h"
                : $"{Steps:N0} steps";

            return $"{DistanceMeters / 1000.0:0.00} km • {duration} • Avg {AverageSpeedKmh:0.0} km/h • {activityExtra} • {Calories:0} kcal";
        }
    }
}