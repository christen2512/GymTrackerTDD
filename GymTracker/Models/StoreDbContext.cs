using Microsoft.EntityFrameworkCore;
namespace GymTracker.Models;

public class StoreDbContext(DbContextOptions<StoreDbContext> opts) : DbContext(opts)
{
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
}