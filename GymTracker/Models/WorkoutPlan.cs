public record WorkoutPlan(long? Id, Dictionary<string, Dictionary<string, List<Exercise>>> Value);

//TODO: Make EFCore accept record types and fix pushing to Azure DB
// public class WorkoutPlan
// {
//     public long? Id { get; set; }
//     
//     public Dictionary<string, Dictionary<string, List<Exercise>>> Value;
//     public WorkoutPlan(long? id, Dictionary<string, Dictionary<string, List<Exercise>>> exercises)
//     {
//         Id = id;
//         Value = exercises;
//     }
//     public WorkoutPlan(){}
//     
// }
public static class WorkoutPlanFactory
{
    public static WorkoutPlan CreateDummy(long? id)
    {
        string[] weeks = ["WEEK 1", "WEEK 2", "Week 3", "WEEK 4", "Week 5 (Block 2)", "Week 6 (Block 2)", 
            "Week 7 (Block 2)", "Week 8 (Block 2)", "WEEK 9 (Block 3) - Deload", "Week 10 (Block 3) - AMRAPs"];
        
        string[] workoutTypes = ["LOWER FOCUSED FULL BODY", "CHEST FOCUSED FULL BODY", "BACK FOCUSED FULL BODY", 
            "LOWER FOCUSED FULL BODY 2"];

        Exercise dummyEx = ExerciseFactory.CreateDummy();
        List<Exercise> exListDummy = [];

        for(int i = 0; i < 10; i++)
        {
            exListDummy.Add(dummyEx);
        }
        
        Dictionary<string, List<Exercise>> workoutDaysWithExs = new()
        {
            {workoutTypes[0], exListDummy},
            {workoutTypes[1], exListDummy},
            {workoutTypes[2], exListDummy},
            {workoutTypes[3], exListDummy},
        };

        Dictionary<string, Dictionary<string, List<Exercise>>> keyValuePairs = new()
        {
            {weeks[0], workoutDaysWithExs},
            {weeks[1], workoutDaysWithExs},
            {weeks[2], workoutDaysWithExs},
            {weeks[3], workoutDaysWithExs},
            {weeks[4], workoutDaysWithExs},
            {weeks[5], workoutDaysWithExs},
            {weeks[6], workoutDaysWithExs},
            {weeks[7], workoutDaysWithExs},
            {weeks[8], workoutDaysWithExs},
            {weeks[9], workoutDaysWithExs},
        };

        return new(id, keyValuePairs);
    }
}