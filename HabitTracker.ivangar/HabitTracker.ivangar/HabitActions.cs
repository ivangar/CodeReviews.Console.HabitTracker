using HabitTracker.ivangar.Models;
using Spectre.Console;

namespace HabitTracker.ivangar;

public class HabitActions
{
    private readonly SqliteService _sqliteService;
    private readonly HabitManager _habitManager;

    public HabitActions(SqliteService sqliteService)
    {
        _sqliteService = sqliteService ?? throw new ArgumentNullException(nameof(sqliteService));
        _habitManager = new HabitManager(_sqliteService);
    }

    public void Run()
    {
        while (true)
        {
            var action = Menu.PrintMainMenu();

            if (action == Enums.MainMenu.CloseApplication)
                break;

            switch (action)
            {
                case Enums.MainMenu.ManageHabits:
                    _habitManager.Run();
                    break;
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
        var records = _sqliteService.GetAllRecords();

        if (records.Count != 0)
        {
            Menu.PrintRecords(records);
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
        var habits = _sqliteService.GetHabits();

        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No habits found. Please create a habit first from 'Manage habits'.[/]");
            Console.ReadKey();
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Habit>()
                .Title("Select a habit to add a record to:")
                .UseConverter(h => $"{h.Name} ({h.Unit})")
                .AddChoices(habits)
        );

        string date = Menu.GetDateInput($"Please insert the [green]habit date[/] for {selected.Name}:");
        int qty = Menu.GetNumberInput($"Please insert [green]quantity[/] for {selected.Name} (no decimals allowed):");

        int inserted = _sqliteService.InsertRecord(selected.Id, date, qty);

        AnsiConsole.MarkupLine($"\n[DarkTurquoise]{inserted} {(inserted == 1 ? "row" : "rows")} inserted into the DB![/]");
        Console.ReadKey();
    }

    private void Delete()
    {
        var records = _sqliteService.GetAllRecords();

        if (records.Count == 0)
        {
            AnsiConsole.MarkupLine($"\n[red]No Records found[/]");
            Console.ReadKey();
            return;
        }

        Menu.PrintRecords(records);

        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to delete or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0 || !RecordExists(records, recordId))
            return;

        int deleted = _sqliteService.DeleteRecord(recordId);

        if (deleted == 1)
        {
            AnsiConsole.MarkupLine($"\n[green]Record with Id [bold]{recordId}[/] was deleted.[/]");
            Console.ReadKey();
        }
    }

    private void Update()
    {
        var records = _sqliteService.GetAllRecords();

        if (records.Count == 0)
        {
            AnsiConsole.MarkupLine($"\n[red]No Records found[/]");
            Console.ReadKey();
            return;
        }

        Menu.PrintRecords(records);

        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to update or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0 || !RecordExists(records, recordId))
            return;

        HabitRecord record = records.SingleOrDefault(r => r.Id == recordId)!;
        string date = Menu.GetDateInput($"Please insert the [green]habit date[/] for {record.HabitName}:", record.Date);
        int qty = Menu.GetNumberInput($"Please insert [green]quantity[/] for {record.HabitName} (no decimals allowed):", record.Quantity);

        int updated = _sqliteService.UpdateRecord(recordId, date, qty);

        if (updated == 1)
        {
            AnsiConsole.MarkupLine($"\n[green]Record with Id [bold]{recordId}[/] was updated.[/]");
            Console.ReadKey();
        }
    }

    private bool RecordExists(List<HabitRecord> habitRecords, int recordId)
    {
        var record = habitRecords.Find(r => r.Id == recordId);

        if (record == null)
        {
            AnsiConsole.MarkupLine($"[red]Record with Id [bold]{recordId}[/] doesn't exist.[/]");
            Console.ReadKey();
            return false;
        }

        return true;
    }
}
