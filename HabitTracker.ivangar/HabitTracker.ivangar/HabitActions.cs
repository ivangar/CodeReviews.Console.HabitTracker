using HabitTracker.ivangar.Models;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace HabitTracker.ivangar;

public class HabitActions
{
    private readonly SqliteService _sqliteService;

    public HabitActions()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string connectionString = configuration.GetConnectionString("HabitTracker")!;

        _sqliteService = new SqliteService(connectionString);
        _sqliteService.InitializeDatabase();
    }

    public void Run()
    {
        while (true)
        {
            var action = Menu.PrintMainMenu();

            if (action == Enums.MainMenu.CloseApplication)
            {
                _sqliteService.ResetDatabase();
                AnsiConsole.MarkupLine("\n[green]The Database has been flushed\n[/]");
                break;
            }

            switch (action)
            {
                case Enums.MainMenu.ViewAllRecords:
                    ShowAll();
                    break;
                case Enums.MainMenu.InsertRecord:
                    Insert();
                    break;
                case Enums.MainMenu.DeleteRecord:
                    Delete();
                    break;
                case Enums.MainMenu.UpdateRecord:
                    Update();
                    break;
            }
        }

        AnsiConsole.MarkupLine("\n[green]Thank you for visiting the Habit Tracker App\n[/]");
    }

    private void ShowAll()
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

    private void Insert()
    {
        string date = Menu.GetDateInput();
        int qty = Menu.GetNumberInput("Please insert [green]number of glasses[/] or other measure of your choice (no decimals allowed)");

        int inserted = _sqliteService.Insert(date, qty);

        AnsiConsole.MarkupLine($"[DarkTurquoise]{inserted} {(inserted == 1 ? "row" : "rows")} inserted into the DB![/]");
        Console.ReadKey();
    }

    private void Delete()
    {
        var habits = GetHabits();

        if (habits.Count == 0)
            return;

        Menu.PrintItems(habits);
        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to delete or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0 || !HabitExists(recordId))
            return;

        if (_sqliteService.Delete(recordId) == 1)
        {
            AnsiConsole.MarkupLine($"[green]Record with Id [bold]{recordId}[/] was deleted.[/]");
            Console.ReadKey();
        }
    }

    private void Update()
    {
        var habits = GetHabits();

        if (habits.Count == 0)
            return;

        Menu.PrintItems(habits);
        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to update or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0 || !HabitExists(recordId))
            return;

        string date = Menu.GetDateInput();
        int qty = Menu.GetNumberInput("Please insert [green]number of glasses[/] or other measure of your choice (no decimals allowed)");

        if (_sqliteService.Update(recordId, date, qty) == 1)
        {
            AnsiConsole.MarkupLine($"[green]Record with Id [bold]{recordId}[/] was updated.[/]");
            Console.ReadKey();
        }
    }

    private List<DrinkingWater> GetHabits()
    {
        var habits = _sqliteService.GetAll();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No Records found[/]");
            Console.ReadKey();
        }

        return habits;
    }

    private bool HabitExists(int habitId)
    {
        var habitExists = _sqliteService.Exists(habitId);

        if (!habitExists)
        {
            AnsiConsole.MarkupLine($"[red]Record with Id [bold]{habitId}[/] doesn't exist.[/]");
            Console.ReadKey();
        }

        return habitExists;
    }
}
