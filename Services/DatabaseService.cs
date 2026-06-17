using FitnessActivityApp.Data;
using FitnessActivityApp.Models;
using SQLite;

namespace FitnessActivityApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

        await _database.CreateTableAsync<ActivitySession>();
        await _database.CreateTableAsync<ActivitySample>();
    }

    public async Task<int> SaveActivitySessionAsync(ActivitySession session)
    {
        await InitAsync();

        if (session.Id != 0)
            return await _database!.UpdateAsync(session);

        return await _database!.InsertAsync(session);
    }

    public async Task SaveActivitySamplesAsync(IEnumerable<ActivitySample> samples)
    {
        await InitAsync();

        await _database!.InsertAllAsync(samples);
    }

    public async Task<List<ActivitySession>> GetArchivedSessionsAsync()
    {
        await InitAsync();

        return await _database!
            .Table<ActivitySession>()
            .Where(x => x.IsArchived)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<ActivitySession?> GetSessionByIdAsync(int id)
    {
        await InitAsync();

        return await _database!
            .Table<ActivitySession>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ActivitySample>> GetSamplesForSessionAsync(int sessionId)
    {
        await InitAsync();

        return await _database!
            .Table<ActivitySample>()
            .Where(x => x.ActivitySessionId == sessionId)
            .OrderBy(x => x.RecordedAt)
            .ToListAsync();
    }

    public async Task DeleteSessionAsync(ActivitySession session)
    {
        await InitAsync();

        List<ActivitySample> samples = await GetSamplesForSessionAsync(session.Id);

        foreach (ActivitySample sample in samples)
        {
            await _database!.DeleteAsync(sample);
        }

        await _database!.DeleteAsync(session);
    }
}