using HabitTracker.ivangar.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace HabitTracker.ivangar.Services;

public class SqliteService : ISqliteService
{
    private readonly string _connectionString;

    public SqliteService()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _connectionString = configuration.GetConnectionString("HabitTracker")!;
    }

    public void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var tableCmd = connection.CreateCommand();

        // Remove legacy table if present
        tableCmd.CommandText = "DROP TABLE IF EXISTS drinking_water;";
        tableCmd.ExecuteNonQuery();

        // Create habits table
        tableCmd.CommandText =
            @"CREATE TABLE IF NOT EXISTS habits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Unit TEXT
                );";
        tableCmd.ExecuteNonQuery();

        // Create habit_records table with FK and cascade delete
        tableCmd.CommandText =
            @"CREATE TABLE IF NOT EXISTS habit_records (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    HabitId INTEGER NOT NULL,
                    Date TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    FOREIGN KEY(HabitId) REFERENCES habits(Id) ON DELETE CASCADE
                );";
        tableCmd.ExecuteNonQuery();

        // Seed data when no habits exist
        if (GetHabits().Count == 0)
            SeedDatabase(connection);
    }

    public void SeedDatabase(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        var seedHabits = new List<(string Name, string Unit)>
        {
            ("Drinking Water", "glasses"),
            ("Walking", "steps"),
            ("Reading", "pages")
        };

        var habitIds = new List<int>();

        foreach (var (name, unit) in seedHabits)
        {
            using var insertHabit = connection.CreateCommand();
            insertHabit.CommandText = "INSERT INTO habits(Name, Unit) VALUES($name, $unit); SELECT last_insert_rowid();";
            insertHabit.Parameters.AddWithValue("$name", name);
            insertHabit.Parameters.AddWithValue("$unit", unit);
            var idObj = insertHabit.ExecuteScalar();
            var id = Convert.ToInt32(idObj);
            habitIds.Add(id);
        }

        var rnd = new Random();
        var today = DateTime.Today;
        var start = today.AddDays(-90);

        for (int i = 0; i < 100; i++)
        {
            int habitId = habitIds[rnd.Next(habitIds.Count)];

            var date = start.AddDays(rnd.Next((today - start).Days + 1)).ToString("yyyy-MM-dd");
            int qty = rnd.Next(1, 11);

            using var insertRecord = connection.CreateCommand();
            insertRecord.CommandText = "INSERT INTO habit_records(HabitId, Date, Quantity) VALUES($hid, $date, $qty);";
            insertRecord.Parameters.AddWithValue("$hid", habitId);
            insertRecord.Parameters.AddWithValue("$date", date);
            insertRecord.Parameters.AddWithValue("$qty", qty);
            insertRecord.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    // Habit CRUD
    public List<Habit> GetHabits()
    {
        var list = new List<Habit>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Unit FROM habits ORDER BY Name";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Habit
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Unit = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
            });
        }

        return list;
    }

    public int CreateHabit(string name, string unit)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO habits(Name, Unit) VALUES($name, $unit); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$unit", unit ?? string.Empty);
        var idObj = cmd.ExecuteScalar();
        return Convert.ToInt32(idObj);
    }

    public int UpdateHabit(int id, string name, string unit)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE habits SET Name = $name, Unit = $unit WHERE Id = $id";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$unit", unit ?? string.Empty);
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery();
    }

    public int DeleteHabit(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM habits WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery();
    }

    // Record CRUD
    public List<HabitRecord> GetAllRecords()
    {
        var list = new List<HabitRecord>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT hr.Id, hr.HabitId, hr.Date, hr.Quantity, h.Name, h.Unit
                            FROM habit_records hr
                            JOIN habits h ON hr.HabitId = h.Id
                            ORDER BY hr.Date DESC";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var dateText = reader.GetString(2);
            DateTime parsedDate;
            if (!DateTime.TryParseExact(dateText, "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out parsedDate))
                DateTime.TryParse(dateText, out parsedDate);

            list.Add(new HabitRecord
            {
                Id = reader.GetInt32(0),
                HabitId = reader.GetInt32(1),
                Date = parsedDate,
                Quantity = reader.GetInt32(3),
                HabitName = reader.GetString(4),
                Unit = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
            });
        }

        return list;
    }

    public int InsertRecord(int habitId, string date, int quantity)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO habit_records(HabitId, Date, Quantity) VALUES($hid, $date, $qty)";
        cmd.Parameters.AddWithValue("$hid", habitId);
        cmd.Parameters.AddWithValue("$date", date);
        cmd.Parameters.AddWithValue("$qty", quantity);
        return cmd.ExecuteNonQuery();
    }

    public int UpdateRecord(int id, string date, int quantity)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE habit_records SET Date = $date, Quantity = $qty WHERE Id = $id";
        cmd.Parameters.AddWithValue("$date", date);
        cmd.Parameters.AddWithValue("$qty", quantity);
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery();
    }

    public int DeleteRecord(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM habit_records WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery();
    }
}
