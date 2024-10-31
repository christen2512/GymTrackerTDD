using GymTracker.Common.Types;
using Microsoft.EntityFrameworkCore;

namespace GymTracker.Models;

public class SeedData
{
    public static void EnsurePopulated(IApplicationBuilder app)
    {
        StoreDbContext context = app.ApplicationServices
            .CreateScope().ServiceProvider.GetRequiredService<StoreDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        if (!context.WorkoutPlans.Any())
        {
            Option<WorkoutPlan> optionWorkoutPlan = ExcelExtractor.TryGetWorkoutPlan(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");
            context.WorkoutPlans.Add(optionWorkoutPlan.Reduce(WorkoutPlanFactory.CreateDummy(0)));
            context.SaveChanges();
        }
    }
}