using GymTracker.Common.Types;
using OfficeOpenXml;

namespace GymTracker.Models
{
    public class ExcelExtractor
    {
        private static bool FileHasRightFormat(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            using ExcelPackage package = new(fileInfo);
            ExcelWorksheet sheet = package.Workbook.Worksheets["4x"];
            try
            {
                return sheet.Cells[1, 2].Text.Equals("JEFF NIPPARD'S FULL BODY - 4X/WEEK SPREADSHEET", StringComparison.OrdinalIgnoreCase) &&
                 sheet.Cells[152, 2].Text.Equals("Week 5 (Block 2)", StringComparison.OrdinalIgnoreCase) &&
                 sheet.Cells[31, 4].Text.Equals("HAMMER CURL", StringComparison.OrdinalIgnoreCase);

            }
            catch (Exception)
            {
                return false;
            }

        }
        private static bool IsFileValid(string filePath) =>
            !string.IsNullOrWhiteSpace(filePath) && FileHasRightFormat(filePath);

        public static Dictionary<string, int> ExtractORM(string filePath)
        {
            if (!IsFileValid(filePath))
            {
                throw new Exception("Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!");
            }

            var ORMs = new Dictionary<string, int>();
            FileInfo existingFile = new(filePath);

            using ExcelPackage package = new(existingFile);

            ExcelWorksheet sheet = package.Workbook.Worksheets["4X"];
            ORMs.Add(sheet.Cells[7, 5].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 5].Value));
            ORMs.Add(sheet.Cells[7, 6].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 6].Value));
            ORMs.Add(sheet.Cells[7, 7].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 7].Value));
            ORMs.Add(sheet.Cells[7, 8].Value.ToString()!.Replace(" ", string.Empty), Convert.ToInt32(sheet.Cells[8, 8].Value));

            return ORMs;
        }

        public static (string, string) ExtractTimespan(string filePath)
        {
            if (!IsFileValid(filePath))
            {
                throw new Exception("Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!");
            }

            FileInfo existingFile = new(filePath);
            using ExcelPackage package = new(existingFile);

            ExcelWorksheet sheet = package.Workbook.Worksheets["4X"];

            string startDate;
            string endDate;
            try
            {
                startDate = sheet.Cells[9, 5].Text.Split('-')[0].Trim();
                endDate = sheet.Cells[9, 5].Text.Split('-')[1].Trim();
            }
            catch (Exception e)
            {
                throw new Exception("Failure when trying to extract workout plan timespan, either the sheet is null or the index is wrong", e);
            }


            return (startDate, endDate);
        }

        public static Option<List<Exercise>> TryGetExercises(string filePath)
        {
            if (!IsFileValid(filePath))
            {
                throw new Exception("Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!");
            }

            FileInfo f = new(filePath);
            using ExcelPackage p = new(f);
            ExcelWorksheet sheet = p.Workbook.Worksheets["4X"];

            var rawWorkoutPlan = sheet.Cells["B12:J"]
                .Select(cell => cell.IsValidCell() ? cell.Text : "N/A")
                .ToList()
                .RemoveInvalidCells();

            int prevExNameIndex = -1;
            var result = rawWorkoutPlan
                .Aggregate(
                    new
                    {
                        Exercises = new List<Exercise>(),
                        ExcelExVals = new List<string>()
                    },
                    (acc, excelCellVal) =>
                    {
                        if (ExcelRangeBaseExtensionAndHelpers.IsExerciseName(excelCellVal))
                        {
                            acc.ExcelExVals.Add(excelCellVal);

                            // Gathering the necessary values to build an Exercise instance
                            if (acc.ExcelExVals.LastIndexOf(excelCellVal) != prevExNameIndex &&
                            prevExNameIndex != -1)
                            {
                                acc.Exercises.Add
                                (acc.ExcelExVals[prevExNameIndex..acc.ExcelExVals.LastIndexOf(excelCellVal)]
                                .CreateExercise()
                                );
                            }
                            prevExNameIndex = acc.ExcelExVals.LastIndexOf(excelCellVal);
                        }
                        else if (!ExcelRangeBaseExtensionAndHelpers.IsWeekOrDayType(excelCellVal))
                        {
                            acc.ExcelExVals.Add(excelCellVal);
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

        public static Option<WorkoutPlan> TryGetWorkoutPlan(string filePath)
        {
            if (!IsFileValid(filePath))
            {
                throw new Exception("Excel file doesn't have the right format, please upload 4X/WEEK Jeff Nippard full body!");
            }

            FileInfo f = new(filePath);
            using ExcelPackage p = new(f);
            ExcelWorksheet sheet = p.Workbook.Worksheets["4X"];

            var weeks = sheet.Cells["B12:J"]
                .Select(cell => cell.IsValidCell() ? cell.Text : "N/A")
                .Where(val => val.Contains("WEEK", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var workoutDays = sheet.Cells["B12:J"]
                .Select((cell, cellAddress) => cell.IsValidCell() ? (cell.Text, cell.Address) : ("N/A", "N/A"))
                .Where(val => val.Item1.Contains("FULL BODY", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int weekIndex = 0;
            var workoutPlan = workoutDays
                .Aggregate(
                    new
                    {
                        WorkoutPlan = new Dictionary<string, Dictionary<string, List<Exercise>>>(),
                        CurrentWorkoutType = new List<string>()
                    },
                    (acc, val) =>
                    {
                        if (val.Item1.Equals("LOWER FOCUSED FULL BODY"))
                        {
                            acc.WorkoutPlan.Add(weeks[weekIndex], []);
                            acc.CurrentWorkoutType.Add(val.Item1);
                            weekIndex++;
                        }

                        else if (val.Item1.Contains("FULL BODY"))
                        {
                            acc.WorkoutPlan[weeks[weekIndex - 1]].Add(val.Item1, []);
                            acc.CurrentWorkoutType.Add(val.Item1);
                        }
                        int currentIndex = workoutDays.FindIndex(x => x == val);

                        if (currentIndex != workoutDays.Count - 1)
                        {
                            (string currentCellAddress, string endOfWorkoutDayAddress) = 
                                GetWorkoutDayAddresses(workoutDays[currentIndex].Item2, workoutDays[currentIndex + 1].Item2);

                            List<Exercise> workoutDay = GetWorkoutDay(sheet.Cells, currentCellAddress, endOfWorkoutDayAddress);

                            acc.WorkoutPlan[weeks[weekIndex - 1]][acc.CurrentWorkoutType.Last()] = workoutDay;
                        }
                        return acc;
                    },
                    acc =>
                    {
                        List<Exercise> workoutDay = GetWorkoutDay(sheet.Cells, "D358", "J366");
                        acc.WorkoutPlan[weeks[weekIndex - 1]][acc.CurrentWorkoutType.Last()] = workoutDay;
                        return new Some<WorkoutPlan>(new WorkoutPlan(0, acc.WorkoutPlan));
                    }
                );

            return workoutPlan.Value.Value.Count > 0 ?
                 new Some<WorkoutPlan>(workoutPlan.Value) : new None<WorkoutPlan>();
        }

        private static (string, string) GetWorkoutDayAddresses(string beginningAddress, string endAddress)
        {
            int endOfWorkoutDayRow = int.Parse(
                new string(endAddress.Where(char.IsDigit).ToArray()))- 1;

            string currentCellAddress = beginningAddress.Replace("C", "D");

            string endOfWorkoutDayAddress = 
                endAddress.Replace("C", "J").Remove(1).Insert(1, endOfWorkoutDayRow.ToString());

            return(currentCellAddress, endOfWorkoutDayAddress);
        }

        private static List<Exercise> GetWorkoutDay(ExcelRange cells, string beginningAddress, string endAddress) =>
            cells[$"{beginningAddress}:{endAddress}"]
                .Select(cell => cell.IsValidCell() ? cell.Text : "N/A")
                .Select((cellValue, index) => new { cellValue, index })
                .GroupBy(x => x.index / 7, x => x.cellValue)
                .Select(g => g.ToList().CreateExercise())
                .ToList()
                .RemoveInvalidExercises();
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
            new(exerciseValues[0], exerciseValues[2], exerciseValues[3], exerciseValues[4], exerciseValues[5], exerciseValues[6]);

        public static List<Exercise> RemoveInvalidExercises(this List<Exercise> exercises) =>
            exercises.Where(ex => ex.IsValidExercise()).ToList();

        public static bool IsValidExercise(this Exercise ex) =>
            !ex.Name.Equals("N/A") && !ex.WorkingSets.Equals("N/A") &&
            !ex.Reps.Equals("N/A") && !ex.Rpe.Equals("N/A") && !ex.Rest.Equals("N/A");
        // ex.Name.Equals("Dummy") && ex.WorkingSets.Equals("Dummy") && ex.Reps.Equals("Dummy") && ex.Load.Equals("Dummy") && ex.Rest.Equals("Dummy");


        public static List<string> RemoveInvalidCells(this List<string> rawWorkoutPlan)
        {
            for (int i = 0; i < rawWorkoutPlan.Count; i++)
            {
                if (rawWorkoutPlan[i].Equals("N/A") && rawWorkoutPlan[i + 1].Equals("N/A"))
                {
                    int j = i + 1;
                    while (rawWorkoutPlan[j].Equals("N/A"))
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