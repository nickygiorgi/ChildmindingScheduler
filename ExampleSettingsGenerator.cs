using System.IO;
using System.Text.Json;

namespace ChildmindingScheduler
{
    /// <summary>
    /// Use this class to generate an example .json for testing
    /// </summary>
    /// <remarks>
    /// Example usage:
    ///     var jsonGenerator = new ExampleSettingsGenerator();
    ///     jsonGenerator.Generate();
    /// </remarks>
    internal class ExampleSettingsGenerator
    {
        /// <summary>
        /// Generates example .json for testing
        /// </summary>
        public void Generate()
        {
            var tomSchedule = new ChildWeeklySchedule
            {
                ChildName = "Tom Cat",
                DailySchedules = new List<ChildDailySchedule> {
                new ChildDailySchedule {
                    Day = "Monday",
                    Sessions = new List<Session> {
                        new Session { Start = new TimeOnly(7, 30), End = new TimeOnly(9, 0), SessionRate = 7.5 },
                        new Session { Start = new TimeOnly(15, 30), End = new TimeOnly(17, 30), SessionRate = 7.5 },
                    }
                },
                new ChildDailySchedule {
                    Day = "Tuesday",
                    Sessions = new List<Session> {
                        new Session { Start = new TimeOnly(7, 30), End = new TimeOnly(9, 0), SessionRate = 7.5 },
                        new Session { Start = new TimeOnly(15, 30), End = new TimeOnly(16, 30), SessionRate = 7.5 }
                    }
                }
            }
            };

            var aliceSchedule = new ChildWeeklySchedule
            {
                ChildName = "Alice Dog",
                DailySchedules = new List<ChildDailySchedule> {
                new ChildDailySchedule {
                    Day = "Monday",
                    Sessions = new List<Session> {
                        new Session { Start = new TimeOnly(7, 0), End = new TimeOnly(9, 0), SessionRate = 7.5 },
                        new Session { Start = new TimeOnly(15, 30), End = new TimeOnly(17, 0), SessionRate = 7.5 }
                    }
                },
                new ChildDailySchedule {
                    Day = "Tuesday",
                    Sessions = new List<Session> {
                        new Session { Start = new TimeOnly(7, 0), End = new TimeOnly(9, 0), SessionRate = 7.5 },
                        new Session { Start = new TimeOnly(15, 0), End = new TimeOnly(16, 0), SessionRate = 7.5 }
                    }
                }
            }
            };

            var settingSchedule = new SettingWeeklySchedule
            {
                Schedules = new List<ChildWeeklySchedule>
                {
                    tomSchedule,
                    aliceSchedule
                }
            };

            var jsonSchedule = JsonSerializer.Serialize(settingSchedule, new JsonSerializerOptions { WriteIndented = true });

            using (var outputFile = new StreamWriter("test-settings.json"))
            {
                outputFile.WriteLine(jsonSchedule);
            }
        }
    }
}
