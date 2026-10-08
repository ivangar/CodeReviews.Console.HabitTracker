using HabitTracker.ivangar.Models;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace HabitTracker.ivangar;

public class SqliteService
{
    private readonly string _connectionString;

    public SqliteService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var tableCmd = connection.CreateCommand();
        tableCmd.CommandText =
            @"CREATE TABLE IF NOT EXISTS drinking_water (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Quantity INTEGER
                    )";

        tableCmd.ExecuteNonQuery();

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var dbRecords = GetAll();

        if (dbRecords.Count == 0)
        {
            foreach (var _ in Enumerable.Range(1, 5))
                Insert(GetRandomDate(), GetRandomQty());
        }
    }

    public List<DrinkingWater> GetAll()
    {
        var records = new List<DrinkingWater>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = "SELECT Id, Date, Quantity FROM drinking_water";

        using var dataReader = tableCmd.ExecuteReader();

        if (dataReader.HasRows)
        {
            while (dataReader.Read())
            {
                var dateText = dataReader.GetString(1);
                DateTime parsedDate;
                if (!DateTime.TryParseExact(dateText, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out parsedDate))
                {
                    // Fallback to general parse if format differs
                    DateTime.TryParse(dateText, out parsedDate);
                }

                records.Add(new DrinkingWater
                {
                    Id = dataReader.GetInt32(0),
                    Date = parsedDate,
                    Quantity = dataReader.GetInt32(2)
                });
            }
        }

        return records;
    }

    public int Insert(string date, int quantity)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = "INSERT INTO drinking_water(Date, Quantity) VALUES($date, $qty)";
        tableCmd.Parameters.AddWithValue("$date", date);
        tableCmd.Parameters.AddWithValue("$qty", quantity);

        return tableCmd.ExecuteNonQuery();
    }

    public int Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = "DELETE FROM drinking_water WHERE Id = $id";
        tableCmd.Parameters.AddWithValue("$id", id);

        return tableCmd.ExecuteNonQuery();
    }

    public bool Exists(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = $id)";
        checkCmd.Parameters.AddWithValue("$id", id);

        var result = checkCmd.ExecuteScalar();
        return Convert.ToInt32(result) == 1;
    }

    public int Update(int id, string date, int quantity)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = "UPDATE drinking_water SET Date = $date, Quantity = $qty WHERE Id = $id";
        tableCmd.Parameters.AddWithValue("$date", date);
        tableCmd.Parameters.AddWithValue("$qty", quantity);
        tableCmd.Parameters.AddWithValue("$id", id);

        return tableCmd.ExecuteNonQuery();
    }

    public void ResetDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        using (var cmdDeleteRows = new SqliteCommand(
            "DELETE FROM drinking_water;", connection, transaction))
        {
            cmdDeleteRows.ExecuteNonQuery();
        }

        using (var cmdResetSeq = new SqliteCommand(
            "DELETE FROM sqlite_sequence WHERE name = @tableName;", connection, transaction))
        {
            cmdResetSeq.Parameters.AddWithValue("@tableName", "drinking_water");
            cmdResetSeq.ExecuteNonQuery();
        }

        transaction.Commit();
    }


    private static string GetRandomDate()
    {
        var random = new Random();

        DateTime start = new DateTime(2015, 1, 1);
        DateTime end = DateTime.Today;

        int range = (end - start).Days;

        DateTime randomDate = start.AddDays(random.Next(range + 1));

        return randomDate.ToString("yyyy-MM-dd");
    }

    private static int GetRandomQty()
    {
        var random = new Random();
        return random.Next(8);
    }
}
