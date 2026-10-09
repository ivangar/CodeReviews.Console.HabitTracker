using System.ComponentModel;

namespace HabitTracker.ivangar
{
    public class Enums
    {
        public enum MainMenu
        {
            [Description("View all records")]
            ViewAllRecords,

            [Description("Insert a record")]
            InsertRecord,

            [Description("Delete a record")]
            DeleteRecord,

            [Description("Update a record")]
            UpdateRecord,

            [Description("Close application")]
            CloseApplication
        }
    }
}
