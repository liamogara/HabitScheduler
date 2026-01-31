namespace HabitScheduler.DTOs
{
    public class HabitDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int FrequencyPerWeek { get; set; }
        public int MinDurationMinutes { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
