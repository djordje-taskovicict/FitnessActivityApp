using Microsoft.Maui.Storage;
using SQLite;

namespace FitnessActivityApp.Data;

public static class Constants
{
    public const string DatabaseFilename = "FitnessActivityTracker.db3";

    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}