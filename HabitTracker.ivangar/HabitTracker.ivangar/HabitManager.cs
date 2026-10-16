using HabitTracker.ivangar.Models;
using Spectre.Console;

namespace HabitTracker.ivangar;

public class HabitManager
{
    private readonly SqliteService _sqliteService;

    public HabitManager(SqliteService sqliteService)
    {
        _sqliteService = sqliteService ?? throw new ArgumentNullException(nameof(sqliteService));
    }

    public void Run()
    {
        while (true)
        {
            Menu.DisplayTitle();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Manage habits - choose an action:")
                    .AddChoices(new[] { "View habits", "Create habit", "Update habit", "Delete habit", "Back" })
            );

            if (choice == "Back")
                break;

            switch (choice)
            {
                case "View habits":
                    ViewHabits();
                    break;
                case "Create habit":
                    CreateHabit();
                    break;
                case "Update habit":
                    UpdateHabit();
                    break;
                case "Delete habit":
                    DeleteHabit();
                    break;
            }
        }
    }

    private void ViewHabits()
    {
        var habits = _sqliteService.GetHabits();
        Menu.PrintHabits(habits);
        AnsiConsole.MarkupLine("Press Any Key to Continue.");
        Console.ReadKey();
    }

    private void CreateHabit()
    {
        var name = Menu.GetStringInput("Enter habit name:");
        var unit = Menu.GetStringInput("Enter unit (free-text, optional):", "");

        var id = _sqliteService.CreateHabit(name, unit);
        AnsiConsole.MarkupLine($"[green]Habit '{name}' created with Id {id}.[/]");
        Console.ReadKey();
    }

    private void UpdateHabit()
    {
        var habits = _sqliteService.GetHabits();
        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No habits available to update.[/]");
            Console.ReadKey();
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Habit>()
                .Title("Select habit to update:")
                .UseConverter(h => $"{h.Name} ({h.Unit})")
                .AddChoices(habits)
        );

        var newName = Menu.GetStringInput("Enter new name:", selected.Name);
        var newUnit = Menu.GetStringInput("Enter new unit:", selected.Unit);

        var updated = _sqliteService.UpdateHabit(selected.Id, newName, newUnit);
        if (updated == 1)
            AnsiConsole.MarkupLine($"[green]Habit updated.[/]");
        else
            AnsiConsole.MarkupLine($"[red]Failed to update habit.[/]");

        Console.ReadKey();
    }

    private void DeleteHabit()
    {
        var habits = _sqliteService.GetHabits();
        if (habits.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No habits available to delete.[/]");
            Console.ReadKey();
            return;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Habit>()
                .Title("Select habit to delete:")
                .UseConverter(h => $"{h.Name} ({h.Unit})")
                .AddChoices(habits)
        );

        var confirm = AnsiConsole.Confirm($"Are you sure you want to delete '{selected.Name}' and all its records?");
        if (!confirm)
            return;

        var deleted = _sqliteService.DeleteHabit(selected.Id);
        if (deleted == 1)
            AnsiConsole.MarkupLine($"[green]Habit deleted and associated records removed.[/]");
        else
            AnsiConsole.MarkupLine($"[red]Failed to delete habit.[/]");

        Console.ReadKey();
    }
}
