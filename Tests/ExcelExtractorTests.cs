using GymTracker.Models;
using GymTracker.Common.Types;


namespace Tests;

public class ExcelExtractorTests
{
    [Fact(DisplayName = "ExtractOneRepMax successfully")]
    public void ExtractOneRepMax_SuccessfulExtraction_ReturnsExpectedData()
    {
        Dictionary<string, int> oneRepMaxs = ExcelExtractor.ExtractORM(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");
        // Dictionary<string, int> oneRepMaxs = ExcelExtractor.ExtractORM(@"C:\C#\GymTrackerTDD\test.xlsx");

        Dictionary<string, int> expected = new() {
            {"Squat", 102},
            {"Bench", 95},
            {"Deadlift", 113},
            {"OHP", 52}
        };


        Assert.Equal(expected, oneRepMaxs);
    }

    [Fact(DisplayName = "Extract start and end date of the training plan")]
    public void ExtractStartEndDate_SuccessfulExtraction_ReturnsExpectedData()
    {
        (string, string) timespan = ExcelExtractor.ExtractTimespan(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");
        // (string, string) timespan = ExcelExtractor.ExtractTimespan(@"C:\C#\GymTrackerTDD\test.xlsx");

        (string, string) expected = ("May 2024", "July 2024");

        Assert.Equal(expected, timespan);
    }

    // TODO: Change the implementation, stub reading from the filesystem and
    // check that an actual exercise was extracted
    [Fact(DisplayName = "Extract exercise list")]
    public void ExtractExerciseList_SuccessfulExtraction()
    {
        Option<List<Exercise>> result = ExcelExtractor.TryGetExercises(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");
        // Option<List<Exercise>> result = ExcelExtractor.TryGetExercises(@"C:\C#\GymTrackerTDD\test.xlsx");

        Exercise expected = new("BACK SQUAT", "4", "4", "80", "77,5%", "2-4 min");

        Assert.Equal(expected, result.Reduce([]).ElementAt(0));
    }

    [Fact(DisplayName = "Extract exercise list file path is empty")]
    public void ExtractExerciseList_FilePathIsEmpty()
    {
        string expectedMessage = "Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!";

        static void Act() => ExcelExtractor.TryGetExercises("");


        Exception exception = Assert.ThrowsAny<Exception>(Act);

        Assert.Equal(expectedMessage, exception.Message);

    }


    [Fact(DisplayName = "Extract exercise list invalid excel format")]
    public void ExtractExerciseList_InvalidExcelFormat()
    {
        var expectedMessage = "Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!";

        // static void Act() => ExcelExtractor.TryGetExercises(@"C:\C#\GymTrackerTDD\empty excel.xlsx");
        static void Act() => ExcelExtractor.TryGetExercises(@"D:\GymProgressTracker\GymTrackerTDD\empty excel.xlsx");

        Exception exception = Assert.ThrowsAny<Exception>(Act);
        Assert.Equal(expectedMessage, exception.Message);

    }

    [Fact(DisplayName = "Extract exercise list alongside corresponding week and training day")]
    public void ExtractExerciseListWithWeekAndTrainingDay_SuccessfulExtraction()
    {
        // Option<WorkoutPlan> optionWorkoutPlan = ExcelExtractor.TryGetWorkoutPlan(@"C:\C#\GymTrackerTDD\test.xlsx");
        Option<WorkoutPlan> optionWorkoutPlan = ExcelExtractor.TryGetWorkoutPlan(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");
        Exercise expected = new("BACK SQUAT", "4", "4", "80", "77,5%", "2-4 min");
        // Exercise expected = new("DUMBBELL INCLINE PRESS", "3", "8", "26", "RPE8", "2-3 min");

        WorkoutPlan result = optionWorkoutPlan.Reduce(WorkoutPlanFactory.CreateDummy(0));
        
        
        Assert.Equal(expected, result.Value["WEEK 1"]["LOWER FOCUSED FULL BODY"][0]); 

    }
}


