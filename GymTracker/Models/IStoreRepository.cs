namespace GymTracker.Models;

public interface IStoreRepository
{
    public IQueryable<WorkoutPlan> WorkoutPlans { get; } 
}