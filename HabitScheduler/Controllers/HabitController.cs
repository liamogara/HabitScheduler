using HabitScheduler.Data;
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
        public async Task<IActionResult> CreateHabit(Habit habit)
        {
            _dbContext.Habits.Add(habit);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHabits), new { id = habit.Id }, habit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(int id, Habit updatedHabit)
        {
            var habit = await _dbContext.Habits.FindAsync(id);
            if (habit == null)
            {
                return NotFound();
            }

            habit.Name = updatedHabit.Name;
            habit.FrequencyPerWeek = updatedHabit.FrequencyPerWeek;
            habit.MinDurationMinutes = updatedHabit.MinDurationMinutes;
            habit.StartHour = updatedHabit.StartHour;
            habit.EndHour = updatedHabit.EndHour;
            habit.IsActive = updatedHabit.IsActive;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
