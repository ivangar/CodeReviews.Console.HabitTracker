namespace HabitTracker.ivangar;

class Program
{
    private readonly static HabitActions _habitActions = new();

    static void Main(string[] args)
    {
        _habitActions.Run();
    }
}
