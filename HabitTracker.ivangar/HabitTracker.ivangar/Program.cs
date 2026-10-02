using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Globalization;
using static HabitTracker.ivangar.Enums;

namespace HabitTracker.ivangar;

class Program
{
    static string connectionString = @"Data Source=habit-Tracker.db";

    static void Main(string[] args)
    {

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText =
                @"CREATE TABLE IF NOT EXISTS drinking_water (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Quantity INTEGER
                    )";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }

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
                    AnsiConsole.MarkupLine($"\n[red]{MainMenu.InsertRecord} not implemented yet![/]");
                    Console.ReadKey();
                    break;
                case MainMenu.UpdateRecord:
                    AnsiConsole.MarkupLine($"\n[red]{MainMenu.UpdateRecord} not implemented yet![/]");
                    Console.ReadKey();
                    break;
            }
        }

        AnsiConsole.MarkupLine("\n[green]Thank you for visiting the Habit Tracker App\n[/]");
    }

    public static void GetAllRecords()
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            List<DrinkingWater> records = [];

            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = @"SELECT * FROM drinking_water";

            SqliteDataReader dataReader = tableCmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    records.Add(new DrinkingWater
                    {
                        Id = dataReader.GetInt32(0),
                        Date = DateTime.ParseExact(dataReader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                        Quantity = dataReader.GetInt32(2)
                    }
                    );
                }

                Menu.PrintItems(records);
            }
            else
            {
                Console.WriteLine("No rows found");
                Console.ReadKey();
            }

            connection.Close();
        }
    }

    private static void Insert()
    {
        int inserted = 0;
        string date = Menu.GetDateInput();
        int qty = Menu.GetNumberInput("Please insert [green]number of glasses[/] or other measure of your choice (no decimals allowed)");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $@"INSERT INTO drinking_water(Date, Quantity) 
                VALUES('{date}', {qty})";

            inserted = tableCmd.ExecuteNonQuery();

            connection.Close();
        }

        /* Move all DB transactions info into a new file helper */
        AnsiConsole.MarkupLine($"\n[DarkTurquoise]{inserted} {(inserted == 1 ? "row" : "rows")} inserted into the DB![/]");
        Console.ReadKey();
    }
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}
