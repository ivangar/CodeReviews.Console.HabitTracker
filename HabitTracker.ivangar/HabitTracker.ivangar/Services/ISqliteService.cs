using HabitTracker.ivangar.Models;
using Microsoft.Data.Sqlite;

namespace HabitTracker.ivangar.Services
{
    public interface ISqliteService
    {
        List<HabitRecord> GetAllRecords();
        List<Habit> GetHabits();
        void InitializeDatabase();
        void SeedDatabase(SqliteConnection connection);
        int CreateHabit(string name, string unit);
        int UpdateHabit(int id, string name, string unit);
        int DeleteHabit(int id);
        int InsertRecord(int habitId, string date, int quantity);
        int DeleteRecord(int id);
        int UpdateRecord(int id, string date, int quantity);
    }
}
