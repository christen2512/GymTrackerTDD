using GymTracker.Models;
using GymTracker.Common.Types;
using OfficeOpenXml;


namespace Tests;

public class ExcelExtractorTests
{   
    [Fact(DisplayName = "ExtractOneRepMax successfully")]
    public void ExtractOneRepMax_SuccessfulExtraction_ReturnsExpectedData()
    {
        Dictionary<string, int> oneRepMaxs = ExcelExtractor.ExtractORM();

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
        (string, string) timespan = ExcelExtractor.ExtractTimespan();

        (string, string) expected = ("May 2024", "July 2024");

        Assert.Equal(expected, timespan);
    }

    [Fact(DisplayName = "Extract exercise list")]
    public void ExtractExerciseList_SuccessfulExtraction()
    {
        Option<List<Exercise>> result = ExcelExtractor.GetExercises();

        int expected = 301;

        Assert.Equal(expected, result.Reduce([]).Count); 
    }
     

}


