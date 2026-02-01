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
            return NoContent();
        }

        [HttpPost("{slotId}/complete")]
        public async Task<IActionResult> Complete(int slotId)
        {
            await _schedulerService.MarkCompleted(slotId);
            return NoContent();
        }

        [HttpPost("{slotId}/reschedule")]
        public async Task<IActionResult> Reschedule(int slotId)
        {
            await _schedulerService.Reschedule(slotId);
            return NoContent();
        }

        [HttpPost("{slotId}/move")]
        public async Task<IActionResult> Move(int slotId, MoveSlotDto dto)
        {
            await _schedulerService.Move(slotId, dto.day);
            return NoContent();
        }

        [HttpDelete("{slotId}")]
        public async Task<IActionResult> DeleteSlot (int slotId)
        {
            var slot = await _dbContext.ScheduleSlots.FindAsync(slotId);
            if (slot == null)
            {
                return NotFound();
            }

            _dbContext.ScheduleSlots.Remove(slot);
            await _dbContext.SaveChangesAsync();

            return NoContent();
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

        [HttpDelete("week")]
        public async Task<IActionResult> ClearWeek()
        {
            
            await _dbContext.ScheduleSlots.ExecuteDeleteAsync();
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
