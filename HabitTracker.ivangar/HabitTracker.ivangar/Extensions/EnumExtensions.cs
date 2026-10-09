using System.ComponentModel;

namespace HabitTracker.ivangar.Extensions
{
    public static class EnumExtensions
    {
        // Helper extension to read an Enum description
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }
    }
}
