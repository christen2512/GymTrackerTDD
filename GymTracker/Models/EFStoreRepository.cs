namespace GymTracker.Models;

public class EFStoreRepository(StoreDbContext ctx) : IStoreRepository
{
    private StoreDbContext context = ctx;

    public IQueryable<WorkoutPlan> WorkoutPlans => context.WorkoutPlans;
}