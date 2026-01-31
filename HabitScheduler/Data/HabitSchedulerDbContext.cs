using HabitScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitScheduler.Data
{
    public class HabitSchedulerDbContext : DbContext
    {
        public HabitSchedulerDbContext(DbContextOptions<HabitSchedulerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Habit> Habits => Set<Habit>();
        public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Habit>()
                .HasMany(h => h.ScheduledSlots)
                .WithOne(s => s.Habit)
                .HasForeignKey(s => s.HabitId);
        }
    }
}
