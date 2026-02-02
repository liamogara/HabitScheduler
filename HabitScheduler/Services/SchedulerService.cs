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

                    await _dbContext.SaveChangesAsync();
                    slotsToSchedule--;
                }
            }
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

        public async Task<ScheduleSlot> Reschedule(int slotId)
        {
            var slot = await _dbContext.ScheduleSlots
                .Include(s => s.Habit)
                .FirstOrDefaultAsync(s => s.Id == slotId);

            if (slot == null)
            {
                return null;
            }

            var success = await AttemptReschedule(slot.Habit, slot.Date);
            if (!success)
            {
                return null;
            }
            else 
            {
                slot.Habit.ScheduledSlots.Remove(slot);
                _dbContext.ScheduleSlots.Remove(slot);
                await _dbContext.SaveChangesAsync();
                return slot; 
            }
            
        }

        private async Task<Boolean> AttemptReschedule(Habit habit, DateOnly date)
        {
            var days = Enumerable.Range(0, (int)date.DayOfWeek)
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

                habit.ScheduledSlots.Add(slot);
                _dbContext.ScheduleSlots.Add(slot);

                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
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

        public async Task<ScheduleSlot> Move(int slotId, string day)
        {
            var slot = await _dbContext.ScheduleSlots
                .Include(s => s.Habit)
                .FirstOrDefaultAsync(s => s.Id == slotId);
            if (slot == null)
            {
                return null;
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStartDate = today.AddDays(-(int)today.DayOfWeek);
            var date = (day) switch
            {
                "Sat" => weekStartDate,
                "Sun" => weekStartDate.AddDays(1),
                "Mon" => weekStartDate.AddDays(2),
                "Tue" => weekStartDate.AddDays(3),
                "Wed" => weekStartDate.AddDays(4),
                "Thu" => weekStartDate.AddDays(5),
                "Fri" => weekStartDate.AddDays(6),
                _ => weekStartDate
            };
            var start = FindAvailableTime(date, slot.Habit);
            if (start == null) return null;
            slot.StartTime = start.Value;
            slot.Date = date;

            await _dbContext.SaveChangesAsync();
            return slot;
        }

        public async Task<ScheduleSlot> AddHabit(int habitId, string day)
        {
            var habit = await _dbContext.Habits.FindAsync(habitId);
            if (habit == null)
            {
                return null;
            }

            if (habit.ScheduledSlots.Count() >= habit.FrequencyPerWeek)
                return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStartDate = today.AddDays(-(int)today.DayOfWeek);
            var date = (day) switch
            {
                "Sat" => weekStartDate,
                "Sun" => weekStartDate.AddDays(1),
                "Mon" => weekStartDate.AddDays(2),
                "Tue" => weekStartDate.AddDays(3),
                "Wed" => weekStartDate.AddDays(4),
                "Thu" => weekStartDate.AddDays(5),
                "Fri" => weekStartDate.AddDays(6),
                _ => weekStartDate
            };
            var start = FindAvailableTime(date, habit);
            if (start == null) return null;

            var slot = new ScheduleSlot
            {
                HabitId = habit.Id,
                Date = date,
                StartTime = start.Value,
                DurationMinutes = habit.MinDurationMinutes,
            };

            habit.ScheduledSlots.Add(slot);
            _dbContext.ScheduleSlots.Add(slot);

            await _dbContext.SaveChangesAsync();
            return slot;
        }

    }
}
