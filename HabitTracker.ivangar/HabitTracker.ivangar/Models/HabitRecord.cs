namespace HabitTracker.ivangar.Models
{
    public class HabitRecord
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }

        // Denormalized for display
        public string HabitName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }
}
