using HabitScheduler.Enums;

namespace HabitScheduler.DTOs
{
    public class ScheduleSlotDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public SlotStatus Status { get; set; }
        public int HabitId { get; set; }
        public string HabitName { get; set; } = null!;
    }
}
