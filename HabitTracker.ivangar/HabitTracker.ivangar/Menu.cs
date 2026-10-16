using HabitTracker.ivangar.Extensions;
using HabitTracker.ivangar.Models;
using Spectre.Console;
using static HabitTracker.ivangar.Enums;

namespace HabitTracker.ivangar
{
    public static class Menu
    {
        public static void DisplayTitle()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Habit Tracker App")
                    .Centered()
                    .Color(Color.Blue));
        }

        public static MainMenu PrintMainMenu()
        {
            DisplayTitle();

            return AnsiConsole.Prompt(
                    new SelectionPrompt<MainMenu>()
                    .Title("What would you like to do?")
                    .UseConverter(x => x.GetDescription())
                    .AddChoices(Enum.GetValues<MainMenu>()));
        }

        public static string GetDateInput(string message, DateTime? date = null)
        {
            DateTime defaultDate = date ?? DateTime.Today;

            var habitDate = AnsiConsole.Prompt(
                new TextPrompt<DateTime>(message)
                    .DefaultValue(defaultDate)
            );

            return habitDate.ToString("yyyy-MM-dd");
        }

        public static int GetNumberInput(string message, int? defaultNumber = null)
        {
            var prompt = new TextPrompt<int>(message);

            if (defaultNumber.HasValue)
                prompt = prompt.DefaultValue(defaultNumber.Value);

            return AnsiConsole.Prompt(prompt);
        }

        public static string GetStringInput(string message, string? defaultValue = null)
        {
            var prompt = new TextPrompt<string>(message)
                .Validate(input =>
                    !string.IsNullOrEmpty(input) && input.All(c => char.IsLetter(c) || char.IsWhiteSpace(c))
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Only alphabetic characters are allowed.[/]"));

            if (!string.IsNullOrEmpty(defaultValue))
                prompt = prompt.DefaultValue(defaultValue!);
            return AnsiConsole.Prompt(prompt);
        }

        public static void PrintHabits(List<Habit> habits)
        {
            if (habits.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No habits available.[/]");
                Console.ReadKey();
                return;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded)
                .AddColumn("[yellow]Id[/]")
                .AddColumn("[yellow]Name[/]")
                .AddColumn("[yellow]Unit[/]");

            foreach (var h in habits)
            {
                table.AddRow(
                    h.Id.ToString(),
                    $"[green]{h.Name}[/]",
                    $"[blue]{h.Unit}[/]"
                );
            }

            AnsiConsole.Write(table);
        }

        public static void PrintRecords(List<HabitRecord> records)
        {
            if (records.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No records available.[/]");
                Console.ReadKey();
                return;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded)
                .AddColumn("[yellow]Id[/]")
                .AddColumn("[yellow]Date[/]")
                .AddColumn("[yellow]Habit[/]")
                .AddColumn("[yellow]Quantity[/]");

            foreach (var r in records)
            {
                var unitText = string.IsNullOrEmpty(r.Unit) ? string.Empty : $" {r.Unit}";
                table.AddRow(
                    r.Id.ToString(),
                    $"[green]{r.Date:yyyy-MM-dd}[/]",
                    $"[blue]{r.HabitName}{unitText}[/]",
                    $"[blue]{r.Quantity}[/]"
                );
            }

            AnsiConsole.Write(table);
        }
    }
}
