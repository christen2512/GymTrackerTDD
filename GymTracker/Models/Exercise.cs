public record Exercise(string Name, string WorkingSets, string Reps, string Load, string Rpe, string Rest);

public static class ExerciseFactory
{
    public static Exercise CreateDummy() =>
        new("DUMMY", "DUMMY", "DUMMY", "DUMMY", "DUMMY", "DUMMY");
}