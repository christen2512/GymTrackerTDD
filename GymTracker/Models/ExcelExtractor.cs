using GymTracker.Common.Types;
using OfficeOpenXml;

namespace GymTracker.Models
{
    public class ExcelExtractor
    {
        public static Dictionary<string, int> ExtractORM()
        {
            string filePath = @"D:\GymProgressTracker\GymTrackerTDD\test.xlsx";
            if (string.IsNullOrEmpty(filePath))
                throw new Exception("File path is invalid");

            var ORMs = new Dictionary<string, int>();
            FileInfo existingFile = new(filePath);
            using (ExcelPackage package = new(existingFile))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets["4X"];
                ORMs.Add(sheet.Cells[7, 5].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 5].Value));
                ORMs.Add(sheet.Cells[7, 6].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 6].Value));
                ORMs.Add(sheet.Cells[7, 7].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 7].Value));
                ORMs.Add(sheet.Cells[7, 8].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 8].Value));
            }

            return ORMs;
        }

        public static (string, string) ExtractTimespan()
        {
            string filePath = @"D:\GymProgressTracker\GymTrackerTDD\test.xlsx";
            if (string.IsNullOrEmpty(filePath))
                throw new Exception("File path is invalid");

            string startDate = "N/A";
            string endDate = "N/A";

            FileInfo existingFile = new(filePath);
            using (ExcelPackage package = new(existingFile))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets["4X"];
                try
                {
                    startDate = sheet.Cells[9,5].Text.Split('-')[0].Trim();
                    endDate = sheet.Cells[9,5].Text.Split('-')[1].Trim();   
                }
                catch (Exception e)
                {
                    throw new Exception("Failure when trying to extract workout plan timespan, either the sheet is null or the index is wrong", e);
                }
            }

            return (startDate, endDate);
        }

        public static Option<List<Exercise>> GetExercises()
        {
            FileInfo f = new(@"D:\GymProgressTracker\GymTrackerTDD\test.xlsx");
            using (ExcelPackage p = new(f))
            {                
                ExcelWorksheet sheet = p.Workbook.Worksheets["4X"]; 

                var rawWorkoutPlan = sheet.Cells["B12:J"]
                    .Select(cell => cell.IsValidCell() ? cell.Text : "N/A")
                    .ToList();
                rawWorkoutPlan.RemoveInvalidCells();
                int previousIndex = -1;
                var result = rawWorkoutPlan
                    .Aggregate(
                        new
                        {
                            // need to add some fields to create the final structure 
                            // that will hold the entire workoutplan

                            // WorkoutPlan = new Dictionary<string, Dictionary<string, List<Exercise>>>(),
                            Exercises = new List<Exercise>(),
                            ExerciseData = new List<string>()
                        },
                        (acc, value) =>
                        {
                            if(ExcelRangeBaseExtensionAndHelpers.IsExerciseName(value))
                            {
                                acc.ExerciseData.Add(value);
                                if(acc.ExerciseData.LastIndexOf(value) != previousIndex && previousIndex != -1)
                                {
                                    acc.Exercises.Add(acc.ExerciseData[previousIndex..acc.ExerciseData.LastIndexOf(value)].CreateExercise());
                                }
                                previousIndex = acc.ExerciseData.LastIndexOf(value);
                            }
                            else if(!ExcelRangeBaseExtensionAndHelpers.IsWeekOrDayType(value))
                            {
                                acc.ExerciseData.Add(value);
                            }
                            return acc;
                        },
                        // add the last push up record in the training plan as that doesn't get added 
                        acc => 
                        {
                            acc.Exercises.Add(rawWorkoutPlan[2180..].CreateExercise());
                            return acc.Exercises;
                        }
                    );
                
                return result.Count > 1 ?
                    new Some<List<Exercise>>(result) :
                    new None<List<Exercise>>();
            }
        
        }
    }
    
    public static class ExcelRangeBaseExtensionAndHelpers
    {
        public static bool IsAllUpper(string input) =>
            input.All(x => x.Equals(' ') || x.Equals('[') || x.Equals(']') || x.Equals('-') || x.Equals(':')
            || char.IsUpper(x));
        
        public static bool IsExerciseName(string line)
        {
            return !line.Any(char.IsDigit) && 
            line.Length > 5 &&
            IsAllUpper(line) &&
            !line.Contains("WEEK", StringComparison.OrdinalIgnoreCase) && 
            !line.Contains("FULL BODY", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsWeekOrDayType(string line)
        {
            return line.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase) || 
            line.EndsWith("FULL BODY", StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsValidCell(this ExcelRangeBase cell) =>
        !cell.Text.Equals(string.Empty) && 
        !cell.Text.Equals("workout", StringComparison.OrdinalIgnoreCase) &&
        !cell.Text.Equals("Exercise", StringComparison.OrdinalIgnoreCase) &&
        !cell.Text.Equals("Warmup Sets", StringComparison.OrdinalIgnoreCase) &&
        !cell.Text.Equals("Working Sets", StringComparison.OrdinalIgnoreCase) &&
        !cell.Text.Equals("Reps / Duration", StringComparison.OrdinalIgnoreCase) &&
        !cell.Text.Equals("Load (kg)", StringComparison.OrdinalIgnoreCase) &&
        !cell.Text.Equals("RPE / %", StringComparison.OrdinalIgnoreCase) &&
        !cell.Text.Equals("Rest", StringComparison.OrdinalIgnoreCase);

        public static bool IsWorkout(string cellText) =>
        cellText.Equals("lower focused full body", StringComparison.OrdinalIgnoreCase) ||
        cellText.Equals("chest focused full body", StringComparison.OrdinalIgnoreCase) ||
        cellText.Equals("back focused full body", StringComparison.OrdinalIgnoreCase) ||
        cellText.Equals("lower focused full body 2", StringComparison.OrdinalIgnoreCase);

        public static Exercise CreateExercise(this List<string> exerciseValues) =>
            new(exerciseValues[0], exerciseValues[2], exerciseValues[3], exerciseValues[4], exerciseValues[5]);
        
        public static List<string> RemoveInvalidCells(this List<string> rawWorkoutPlan) 
        {
            for(int i = 0; i < rawWorkoutPlan.Count(); i++)
            {
                if(rawWorkoutPlan[i].Equals("N/A") && rawWorkoutPlan[i + 1].Equals("N/A"))
                {
                    int j = i + 1;
                    while(rawWorkoutPlan[j].Equals("N/A"))
                    {
                        j++;
                    }
                    rawWorkoutPlan.RemoveRange(i, j - i);
                }
            }
            List<string> newList = rawWorkoutPlan;
            return newList;
        }
    }

}