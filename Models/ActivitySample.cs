using SQLite;

namespace FitnessActivityApp.Models;

public class ActivitySample
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ActivitySessionId { get; set; }

    public DateTime RecordedAt { get; set; }

    public double ElapsedSeconds { get; set; }

    public double DistanceMeters { get; set; }
    public double SpeedKmh { get; set; }

    public int Steps { get; set; }
    public double CadenceStepsPerMinute { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Ignore]
    public string DisplayText =>
        $"{TimeSpan.FromSeconds(ElapsedSeconds):mm\\:ss} • {SpeedKmh:0.0} km/h • {DistanceMeters / 1000.0:0.00} km • Steps: {Steps:N0} • Cadence: {CadenceStepsPerMinute:0} spm";
}