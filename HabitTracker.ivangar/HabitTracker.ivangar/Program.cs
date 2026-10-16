namespace HabitTracker.ivangar;

class Program
{
    private readonly static SqliteService _sqliteService = new();
    private readonly static HabitActions _habitActions = new(_sqliteService);

    static void Main(string[] args)
    {
        _sqliteService.InitializeDatabase();
        _habitActions.Run();
    }
}
