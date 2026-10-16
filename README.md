# Habit Tracker (Console)

Small console application to track multiple habits with user-defined units. Built with .NET 10 and Microsoft.Data.Sqlite, and uses Spectre.Console for a friendly CLI experience.

## What the app does
- Lets users create named habits and assign a free-text unit (e.g., "glasses", "steps", "pages").
- Lets users insert, view, update and delete records for habits (date + integer quantity).
- Provides a dedicated "Manage habits" menu to create/update/delete habits. Deleting a habit asks for confirmation and cascades to remove its records.
- Uses a local SQLite database file (habit-Tracker.db). On first run the app seeds the database with a few sample habits and 100 randomly generated records.

## How it works (high level)
- Data layer: `SqliteService` encapsulates all database work (initialize, create tables, seed data, CRUD for habits and records). All SQL uses parameterized queries.
- Models: `Habit` and `HabitRecord` represent data. HabitRecord includes denormalized habit name and unit for display.
- UI layer: `Menu` (Spectre.Console prompts) and `HabitActions` / `HabitManager` classes implement the CLI flows.
- Program entry: `Program.cs` initializes the `SqliteService` and starts the `HabitActions` loop.

Database schema (SQLite)
- habits: Id, Name, Unit
- habit_records: Id, HabitId (FK -> habits.Id, ON DELETE CASCADE), Date (yyyy-MM-dd), Quantity (INTEGER)

Notes about legacy data
- The project previously used a `drinking_water` table. The current implementation explicitly drops `drinking_water` (if present) and creates the new normalized tables. No migration is performed.

## Architectural choices
- Single responsibility: database operations are encapsulated in `SqliteService` instead of being mixed with UI logic.
- Small POCO models for data transfer and simple mapping from SQL results.
- CLI UI uses Spectre.Console for better prompts and tables while remaining a console application (no GUI).
- Seeding on first run gives a ready-to-explore dataset while keeping production behavior deterministic (seed only if no habits exist).

## Requirements
- .NET 10 SDK
- Visual Studio 2026 or any editor that can build .NET 10 projects

## Setup & run (quick)
1. Clone the repository and open a terminal in the workspace root.
2. Build the project:

   dotnet build HabitTracker.ivangar/HabitTracker.ivangar.csproj

3. Run the app:

   dotnet run --project HabitTracker.ivangar/HabitTracker.ivangar.csproj

Or open the solution in Visual Studio and start the project. If you want Console.Clear() to behave as expected in the IDE, run the app in an external console (Project > Properties > Debug > Enable external console) or run from a terminal.

The SQLite file `habit-Tracker.db` will be created in the project working directory. On first run the app will seed sample habits and 100 records.

## Usage summary (CLI)
- Main menu: View all records, Insert a record, Manage habits, Delete a record, Update a record, Close application.
- Manage habits: View, Create, Update, Delete (delete asks for confirmation and cascades records).

## Where to change things
- Connection string: set in `Program.cs` (variable `connectionString`).
- Seed data: `SqliteService.SeedDatabase` controls which habits are seeded and the number/ranges of generated records.
- Date format: `Menu.GetDateInput` stores dates as `yyyy-MM-dd`.

## Development notes / reflection
I refactored the original single-file implementation to separate concerns: database access lives in `SqliteService`, CLI flows live in `HabitActions` and `HabitManager`, and `Menu` centralizes all prompts and table rendering. Spectre.Console made prompts and tables straightforward and improved usability with minimal code.

Design tradeoffs and lessons:
- I chose a normalized schema (habits + habit_records) instead of creating one table per habit. This keeps the model flexible and avoids schema changes when users add habits.
- I implemented seeding so the app is immediately useful for development and manual testing. Seeding runs only when the habits table is empty to avoid overwriting user data.
- I used parameterized SQL and transactions for seeding to reduce SQL-injection risk and improve correctness.

Overall the work was straightforward. The main focus was to move DB logic out of the UI and add a small domain model. Spectre.Console reduced boilerplate for user interaction.

---
