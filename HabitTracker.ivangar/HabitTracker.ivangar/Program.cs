using HabitTracker.ivangar.Services;

namespace HabitTracker.ivangar;

class Program
{
    private readonly static ISqliteService _sqliteService = new SqliteService();
    private readonly static HabitActions _habitActions = new(_sqliteService);

    static void Main(string[] args)
    {
        _sqliteService.InitializeDatabase();
        _habitActions.Run();
    }
}
