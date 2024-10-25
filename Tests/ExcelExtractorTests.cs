using GymTracker.Models;
using GymTracker.Common.Types;
using OfficeOpenXml;


namespace Tests;

public class ExcelExtractorTests
{   
    [Fact(DisplayName = "ExtractOneRepMax successfully")]
    public void ExtractOneRepMax_SuccessfulExtraction_ReturnsExpectedData()
    {
        Dictionary<string, int> oneRepMaxs = ExcelExtractor.ExtractORM(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");

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

        (string, string) expected = ("May 2024", "July 2024");

        Assert.Equal(expected, timespan);
    }

    [Fact(DisplayName = "Extract exercise list")]
    public void ExtractExerciseList_SuccessfulExtraction()
    {
        Option<List<Exercise>> result = ExcelExtractor.GetExercises(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");

        int expected = 301;

        Assert.Equal(expected, result.Reduce([]).Count); 
    }

    [Fact(DisplayName = "Extract exercise list file path is empty")]
    public void ExtractExerciseList_FilePathIsEmpty()

    {

        var exceptionType = typeof(Exception);

        string expectedMessage = "Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!";

       

        static void act() => ExcelExtractor.GetExercises("");

 

        Exception exception = Assert.ThrowsAny<Exception>(act);

 

        Assert.Equal(expectedMessage, exception.Message);

    }

 

    [Fact(DisplayName= "Extract exercise list invalid excel format")]

    public void ExtractExerciseList_InvalidExcelFormat()

    {

        var exceptionType = typeof(Exception);

        var expectedMessage = "Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!";

 

        static void act() => ExcelExtractor.GetExercises(@"C:\C#\GymTrackerTDD\empty excel.xlsx");

 

        Exception exception = Assert.ThrowsAny<Exception>(act);

 

        Assert.Equal(expectedMessage, exception.Message);

    }

 

    [Fact(DisplayName = "Extract exercise list alongside corresponding week and training day")]

    public void ExtractExerciseListWithWeekAndTrainingDay_SuccessfulExtraction()

    {// WorkoutPlan = new Dictionary<string, Dictionary<string, List<Exercise>>>(),

        Option<WorkoutPlan> result = ExcelExtractor.GetWorkoutPlan(@"C:\C#\GymTrackerTDD\test.xlsx");

        Exercise expected = new Exercise("BACK SQUAT", "4", "4", "80", "2-4 min");

       

        Assert.Equal(expected, result.Reduce(() => new WorkoutPlan(default!)).Value["Week 1"]["LOWER FOCUSED FULL BODY"][0]);

       

    }
     

}


