namespace HabitScheduler.Data
{
    public class SeedData
    {
        public static void Initialize(HabitSchedulerDbContext dbContext)
        {
            if (dbContext.Habits.Any())
            {
                return;
            }

            var habits = new List<Models.Habit>
            {
                new Models.Habit
                {
                    Name = "Workout",
                    FrequencyPerWeek = 4,
                    MinDurationMinutes = 30,
                    StartHour = 6,
                    EndHour = 22
                },
                new Models.Habit
                {
                    Name = "Read",
                    FrequencyPerWeek = 5,
                    MinDurationMinutes = 30,
                    StartHour = 8,
                    EndHour = 22
                },
                new Models.Habit
                {
                    Name = "Code",
                    FrequencyPerWeek = 7,
                    MinDurationMinutes = 30,
                    StartHour = 7,
                    EndHour = 19
                }
            };

            dbContext.Habits.AddRange(habits);
            dbContext.SaveChanges();
        }
    }
}
