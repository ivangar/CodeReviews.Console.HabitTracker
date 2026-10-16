using FluentAssertions;
using HabitTracker.ivangar.Models;
using HabitTracker.ivangar.Services;
using Moq;
using System.Reflection;

namespace HabitTracker.ivangar.Tests
{
    public class HabitActionsTests
    {
        private readonly Mock<ISqliteService> _sqliteMock;
        private readonly HabitActions _sut;

        public HabitActionsTests()
        {
            _sqliteMock = new Mock<ISqliteService>();
            _sut = new TestableHabitActions(_sqliteMock.Object);

            Console.SetOut(new StringWriter());
            Console.SetIn(new StringReader(""));
        }

        // ------------------------------------------------------------
        // SHOW ALL
        // ------------------------------------------------------------
        [Fact]
        public void ShowAll_WhenRecordsExist_ShouldCallGetAllRecords()
        {
            // Arrange
            _sqliteMock.Setup(s => s.GetAllRecords())
                .Returns(new List<HabitRecord>
                {
                    new HabitRecord { Id = 1, HabitId = 1, HabitName = "Water", Quantity = 2 }
                });

            var method = typeof(HabitActions)
                .GetMethod("ShowAll", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            method.Invoke(_sut, null);

            // Assert
            _sqliteMock.Verify(s => s.GetAllRecords(), Times.Once);
        }

        [Fact]
        public void ShowAll_WhenNoRecords_ShouldPrintNoRowsFound()
        {
            // Arrange
            _sqliteMock.Setup(s => s.GetAllRecords()).Returns(new List<HabitRecord>());

            var output = new StringWriter();
            Console.SetOut(output);

            var method = typeof(HabitActions)
                .GetMethod("ShowAll", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            method.Invoke(_sut, null);

            // Assert
            output.ToString().Should().Contain("No rows found");
        }

        // ------------------------------------------------------------
        // INSERT
        // ------------------------------------------------------------
        [Fact]
        public void Insert_NoHabits_DoesNotCallInsertRecord_And_WaitsForKey()
        {
            // Arrange
            _sqliteMock.Setup(s => s.GetHabits()).Returns(new List<Habit>());

            var method = typeof(HabitActions).GetMethod("Insert", BindingFlags.Instance | BindingFlags.NonPublic);

            // Act
            method.Invoke(_sut, null);

            // Assert
            _sqliteMock.Verify(s => s.InsertRecord(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        // ------------------------------------------------------------
        // DELETE
        // ------------------------------------------------------------
        [Fact]
        public void Delete_WhenNoRecords_ShouldNotCallDeleteRecord()
        {
            // Arrange
            _sqliteMock.Setup(s => s.GetAllRecords()).Returns(new List<HabitRecord>());

            var method = typeof(HabitActions)
                .GetMethod("Delete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            method.Invoke(_sut, null);

            // Assert
            _sqliteMock.Verify(s => s.DeleteRecord(It.IsAny<int>()), Times.Never);
        }

        // ------------------------------------------------------------
        // RECORD EXISTS
        // ------------------------------------------------------------
        [Fact]
        public void RecordExists_WhenRecordFound_ShouldReturnTrue()
        {
            // Arrange
            var records = new List<HabitRecord>
            {
                new HabitRecord { Id = 10 }
            };

            var method = typeof(HabitActions)
                .GetMethod("RecordExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            bool exists = (bool)method.Invoke(_sut, new object[] { records, 10 });

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public void RecordExists_WhenRecordNotFound_ShouldReturnFalse()
        {
            // Arrange
            var records = new List<HabitRecord>
            {
                new HabitRecord { Id = 1 }
            };

            var output = new StringWriter();
            Console.SetOut(output);

            var method = typeof(HabitActions)
                .GetMethod("RecordExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            bool exists = (bool)method.Invoke(_sut, new object[] { records, 99 });

            // Assert
            exists.Should().BeFalse();
            output.ToString().Should().Contain("doesn't exist");
        }

        // Test helper subclass to avoid blocking Console.ReadKey()
        private class TestableHabitActions : HabitActions
        {
            public TestableHabitActions(ISqliteService sqliteService) : base(sqliteService) { }

            protected override void WaitForKey()
            {
                // no-op in tests
            }
        }
    }
}
