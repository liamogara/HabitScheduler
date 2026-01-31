using HabitScheduler.Enums;

namespace HabitScheduler.Models
{
    public class ScheduleSlot
    {
        public int Id { get; set; }

        public int HabitId { get; set; }
        public Habit Habit { get; set; } = null!;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }

        public int DurationMinutes { get; set; }

        public SlotStatus Status { get; set; } = SlotStatus.Scheduled;
    }
}
