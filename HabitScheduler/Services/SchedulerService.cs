using HabitScheduler.Data;
using HabitScheduler.Enums;
using HabitScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitScheduler.Services
{
    public class SchedulerService
    {
        private readonly HabitSchedulerDbContext _dbContext;

        public SchedulerService(HabitSchedulerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateSchedule(DateOnly weekStartDate)
        {
            var habits = await _dbContext.Habits
                .Where(h => h.IsActive)
                .Include(h => h.ScheduledSlots)
                .ToListAsync();

            var days = Enumerable.Range(0, 7)
                .Select(offset => weekStartDate.AddDays(offset))
                .ToList();

            foreach (var habit in habits)
            {
                int scheduledCount = habit.ScheduledSlots
                    .Count(s => days.Contains(s.Date));
                int slotsToSchedule = habit.FrequencyPerWeek - scheduledCount;
                if (slotsToSchedule <= 0)
                    continue;
                foreach (var day in days)
                {
                    if (slotsToSchedule <= 0)
                        break;

                    bool alreadyScheduled = habit.ScheduledSlots
                        .Any(s => s.Date == day);
                    if (alreadyScheduled)
                        continue;

                    var start = FindAvailableTime(day, habit);
                    if (start == null) continue;

                    var slot = new ScheduleSlot
                    {
                        HabitId = habit.Id,
                        Date = day,
                        StartTime = start.Value,
                        DurationMinutes = habit.MinDurationMinutes,
                    };

                    habit.ScheduledSlots.Add(slot);
                    _dbContext.ScheduleSlots.Add(slot);

                    slotsToSchedule--;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private TimeOnly? FindAvailableTime(DateOnly date, Habit habit)
        {
            var slots = _dbContext.ScheduleSlots
                .Where(s => s.Date == date)
                .OrderBy(s => s.StartTime)
                .ToList();

            for (int hour = habit.StartHour; hour + (habit.MinDurationMinutes / 60) <= habit.EndHour; hour++)
            {
                var start = new TimeOnly(hour, 0);

                if (slots.Any(s =>
                    (start < s.StartTime.AddMinutes(s.DurationMinutes)) &&
                    (s.StartTime < start.AddMinutes(habit.MinDurationMinutes))))
                {
                       continue;
                }

                return start;
            }
            return null;
        }

        public async Task MarkMissed(int slotId)
        {
            var slot = await _dbContext.ScheduleSlots
                .Include(s => s.Habit)
                .FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot != null)
            {
                slot.Status = SlotStatus.Missed;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task Reschedule(int slotId)
        {
            var slot = await _dbContext.ScheduleSlots
                .Include(s => s.Habit)
                .FirstOrDefaultAsync(s => s.Id == slotId);
            
            if (slot != null)
            {
                _dbContext.ScheduleSlots.Remove(slot);
                await AttemptReschedule(slot.Habit, slot.Date);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task AttemptReschedule(Habit habit, DateOnly date)
        {
            var days = Enumerable.Range(1, (int)date.DayOfWeek)
                .Select(offset => date.AddDays(offset))
                .ToList();

            foreach (var day in days)
            {

                var start = FindAvailableTime(day, habit);
                if (start == null) continue;

                var slot = new ScheduleSlot
                {
                    HabitId = habit.Id,
                    Date = day,
                    StartTime = start.Value,
                    DurationMinutes = habit.MinDurationMinutes,
                };
                _dbContext.ScheduleSlots.Add(slot);

                await _dbContext.SaveChangesAsync();
                return;
            }
        }

        public async Task MarkCompleted(int slotId)
        {
            var slot = await _dbContext.ScheduleSlots
                .Include(s => s.Habit)
                .FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot != null)
            {
                slot.Status = SlotStatus.Completed;
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
