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
            AnsiConsole.Write(
                new FigletText("Habit Tracker App")
                    .Centered()
                    .Color(Color.Blue));
        }

        public static MainMenu PrintMainMenu()
        {
            Console.Clear();
            DisplayTitle();

            return AnsiConsole.Prompt(
                    new SelectionPrompt<MainMenu>()
                    .Title("What would you like to do?")
                    .UseConverter(x => x.GetDescription())
                    .AddChoices(Enum.GetValues<MainMenu>()));
        }

        public static string GetDateInput(DateTime? date = null)
        {
            DateTime defaultDate = date ?? DateTime.Today;

            var habitDate = AnsiConsole.Prompt(
                new TextPrompt<DateTime>("Please insert the [green]Habit date[/]:")
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

        public static void PrintItems(List<DrinkingWater> habits)
        {
            if (habits.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No records available.[/]");
                Console.ReadKey();
                return;
            }

            var table = new Table();

            table.Border(TableBorder.Rounded)
                .AddColumn("[yellow]Id[/]")
                .AddColumn("[yellow]Date[/]")
                .AddColumn("[yellow]Quantity[/]");

            foreach (var habit in habits)
            {
                table.AddRow(
                    habit.Id.ToString(),
                    $"[green]{habit.Date:yyyy-MM-dd}[/]",
                    $"[blue]{habit.Quantity}[/]"
                );
            }

            AnsiConsole.Write(table);
        }
    }
}
