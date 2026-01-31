using HabitScheduler.Data;
using HabitScheduler.DTOs;
using HabitScheduler.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitScheduler.Controllers
{
    [Route("api/schedule")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly SchedulerService _schedulerService;
        private readonly HabitSchedulerDbContext _dbContext;

        public ScheduleController(SchedulerService schedulerService, HabitSchedulerDbContext dbContext)
        {
            _schedulerService = schedulerService;
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSchedule()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStartDate = today.AddDays(-(int)today.DayOfWeek);

            await _schedulerService.CreateSchedule(weekStartDate);
            return Ok();
        }

        [HttpPost("{slotId}/miss")]
        public async Task<IActionResult> Miss(int slotId)
        {
            await _schedulerService.MarkMissed(slotId);
            return Ok();
        }

        [HttpGet("week")]
        public async Task<IActionResult> GetWeek()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStartDate = today.AddDays(-(int)today.DayOfWeek);
            var days = Enumerable.Range(0, 7)
                .Select(offset => weekStartDate.AddDays(offset))
                .ToList();

            var slots = await _dbContext.ScheduleSlots
                .Include(s => s.Habit)
                .Where(s => days.Contains(s.Date))
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Select(s => new ScheduleSlotDto
                {
                    Id = s.Id,
                    Date = s.Date,
                    StartTime = s.StartTime,
                    DurationMinutes = s.DurationMinutes,
                    Status = s.Status,
                    HabitId = s.HabitId,
                    HabitName = s.Habit.Name
                })
                .ToListAsync();

            return Ok(slots);
        }
    }
}
