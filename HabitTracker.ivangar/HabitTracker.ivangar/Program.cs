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
        using (var connection = new SqliteConnection(connectionString))
        {
            List<DrinkingWater> records = [];

            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "SELECT * FROM drinking_water";

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

    public static void Insert()
    {
        int inserted = 0;
        string date = Menu.GetDateInput();
        int qty = Menu.GetNumberInput("Please insert [green]number of glasses[/] or other measure of your choice (no decimals allowed)");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"INSERT INTO drinking_water(Date, Quantity) VALUES('{date}', {qty})";

            inserted = tableCmd.ExecuteNonQuery();

            connection.Close();
        }

        /* Move all DB transactions info into a new file helper */
        AnsiConsole.MarkupLine($"\n[DarkTurquoise]{inserted} {(inserted == 1 ? "row" : "rows")} inserted into the DB![/]");
        Console.ReadKey();
    }

    public static void Delete()
    {
        GetAllRecords();
        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to delete or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0)
            return;

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"DELETE FROM drinking_water WHERE Id = {recordId}";

            int rowCount = tableCmd.ExecuteNonQuery();

            if (rowCount == 0)
            {
                AnsiConsole.MarkupLine($"\n[red]Record with Id [bold]{recordId}[/] doesn't exist.[/]");
                Console.ReadKey();
                connection.Close();
                return;
            }

            AnsiConsole.MarkupLine($"\n[green]Record with Id [bold]{recordId}[/] was deleted.[/]");
            Console.ReadKey();

            connection.Close();
        }
    }

    public static void Update()
    {
        GetAllRecords();
        var recordId = Menu.GetNumberInput("Please type the [yellow]Id[/] of the record you want to update or type [green]0[/] to go back to Main Menu:");

        if (recordId == 0)
            return;

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
            int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (checkQuery == 0)
            {
                AnsiConsole.MarkupLine($"\n[red]Record with Id [bold]{recordId}[/] doesn't exist.[/]");
                Console.ReadKey();
                connection.Close();
                return;
            }

            string date = Menu.GetDateInput();
            int qty = Menu.GetNumberInput("Please insert [green]number of glasses[/] or other measure of your choice (no decimals allowed)");

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {qty} WHERE Id = {recordId}";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}
