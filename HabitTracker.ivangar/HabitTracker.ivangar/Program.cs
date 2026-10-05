using Spectre.Console;
using static HabitTracker.ivangar.Enums;

namespace HabitTracker.ivangar;

class Program
{
    static string connectionString = @"Data Source=habit-Tracker.db";

    private readonly static SqliteService _sqliteService = new(connectionString);

    static void Main(string[] args)
    {

        _sqliteService.InitializeDatabase();
        GetUserInput();
    }

    static void GetUserInput()
    {
        while (true)
        {
            var action = Menu.PrintMainMenu();

            if (action == MainMenu.CloseApplication)
                break;

            switch (action)
            {
                case MainMenu.ViewAllRecords:
                    GetAllRecords();
                    break;
                case MainMenu.InsertRecord:
                    Insert();
                    break;
                case MainMenu.DeleteRecord:
                    Delete();
                    break;
                case MainMenu.UpdateRecord:
                    Update();
                    break;
            }
        }

        AnsiConsole.MarkupLine("\n[green]Thank you for visiting the Habit Tracker App\n[/]");
    }

    public static void GetAllRecords()
    {
        var habits = _sqliteService.GetAll();

        if (habits.Count != 0)
        {
            Menu.PrintItems(habits);
            AnsiConsole.MarkupLine("Press Any Key to Continue.");
            Console.ReadKey();
        }

        else
        {
            AnsiConsole.MarkupLine("No rows found");
            Console.ReadKey();
        }
    }

    public static void Insert()
    {
        string date = Menu.GetDateInput();
        int qty = Menu.GetNumberInput("Please insert [green]number of glasses[/] or other measure of your choice (no decimals allowed)");

        int inserted = _sqliteService.Insert(date, qty);

        AnsiConsole.MarkupLine($"\n[DarkTurquoise]{inserted} {(inserted == 1 ? "row" : "rows")} inserted into the DB![/]");
        Console.ReadKey();
    }

    public static void Delete()
    {
        var habits = _sqliteService.GetAll();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine($"\n[red]No Records found[/]");
            Console.ReadKey();
            return;
        }

        Menu.PrintItems(habits);

        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to delete or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0)
            return;

        if (!_sqliteService.Exists(recordId))
        {
            AnsiConsole.MarkupLine($"\n[red]Record with Id [bold]{recordId}[/] doesn't exist.[/]");
            Console.ReadKey();
            return;
        }

        int deleted = _sqliteService.Delete(recordId);

        if (deleted == 1)
        {
            AnsiConsole.MarkupLine($"\n[green]Record with Id [bold]{recordId}[/] was deleted.[/]");
            Console.ReadKey();
        }
    }

    public static void Update()
    {
        var habits = _sqliteService.GetAll();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine($"\n[red]No Records found[/]");
            Console.ReadKey();
            return;
        }

        Menu.PrintItems(habits);

        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to update or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0)
            return;

        if (!_sqliteService.Exists(recordId))
        {
            AnsiConsole.MarkupLine($"\n[red]Record with Id [bold]{recordId}[/] doesn't exist.[/]");
            Console.ReadKey();
            return;
        }

        string date = Menu.GetDateInput();
        int qty = Menu.GetNumberInput("Please insert [green]number of glasses[/] or other measure of your choice (no decimals allowed)");

        int updated = _sqliteService.Update(recordId, date, qty);

        if (updated == 1)
        {
            AnsiConsole.MarkupLine($"\n[green]Record with Id [bold]{recordId}[/] was updated.[/]");
            Console.ReadKey();
        }
    }
}