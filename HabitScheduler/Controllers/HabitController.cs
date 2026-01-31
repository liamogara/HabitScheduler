using HabitScheduler.Data;
using HabitScheduler.DTOs;
using HabitScheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitScheduler.Controllers
{
    [Route("api/habit")]
    [ApiController]
    public class HabitController : ControllerBase
    {
        private readonly HabitSchedulerDbContext _dbContext;

        public HabitController(HabitSchedulerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetHabits()
        {
            var habits = await _dbContext.Habits
                .OrderBy(h => h.Name)
                .ToListAsync();

            return Ok(habits);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHabit(CreateHabitDto dto)
        {
            var habit = new Habit
            {
                Name = dto.Name,
                FrequencyPerWeek = dto.FrequencyPerWeek,
                MinDurationMinutes = dto.MinDurationMinutes,
                StartHour = dto.StartHour,
                EndHour = dto.EndHour,
                IsActive = dto.IsActive
            };
            _dbContext.Habits.Add(habit);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(int id, CreateHabitDto dto)
        {
            var habit = await _dbContext.Habits.FindAsync(id);
            if (habit == null)
            {
                return NotFound();
            }

            habit.Name = dto.Name;
            habit.FrequencyPerWeek = dto.FrequencyPerWeek;
            habit.MinDurationMinutes = dto.MinDurationMinutes;
            habit.StartHour = dto.StartHour;
            habit.EndHour = dto.EndHour;
            habit.IsActive = dto.IsActive;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
