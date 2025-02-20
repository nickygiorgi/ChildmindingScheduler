using System.IO;
using System.Text.Json;
using System.Windows;

namespace ChildmindingScheduler
{
    /// <summary>
    /// Code behind with functionality to:
    /// 1) modify the settings.json file (button 'Add Child')
    /// 2) print out all the monthly fees in .csv spreadsheets (button 'Monthly Fees')
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the panel used to add a new child
        /// to the childminding setting
        /// </summary>
        /// <remarks>
        /// No functionality provided to update an existing child settings.
        /// Modify the settings.json file manually to modify an existing child settings.
        /// </remarks>
        private void btn_new_change_click(object sender, RoutedEventArgs e)
        {
            cnv_add.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Produces the monthly fees for all registered children
        /// </summary>
        private void btn_monthly_fees_click(object sender, RoutedEventArgs e)
        {
            using var reader = new StreamReader("settings.json");
            var json = reader.ReadToEnd();
            var settingSchedule = JsonSerializer.Deserialize<SettingWeeklySchedule>(json);

            if (settingSchedule == null || settingSchedule.Schedules.Count < 1)
            {
                Console.WriteLine("There are no schedules to print");
            }
            else
            {
                DateTime nextMonth = DateTime.Today.AddMonths(1);
                DateTime firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
                DateTime lastDayOfNextMonth = firstDayOfNextMonth.AddMonths(1).AddDays(-1);

                foreach (var childSchedule in settingSchedule.Schedules)
                {
                    var childName = childSchedule.ChildName;

                    if (!Directory.Exists("fees"))
                    {
                        Directory.CreateDirectory("fees");
                    }

                    using (var outputFile = new StreamWriter($"fees\\{childName}.csv"))
                    {
                        var maxSessionsPerDay = FindMaxSessionsPerDay(childSchedule);
                        outputFile.WriteLine(CreateHeadersLine(maxSessionsPerDay));

                        var lineIndex = 2;
                        for (DateTime date = firstDayOfNextMonth; date <= lastDayOfNextMonth; date = date.AddDays(1))
                        {
                            var daySchedule = childSchedule.DailySchedules.FirstOrDefault(schedule => schedule.Day == date.DayOfWeek.ToString());
                            if (daySchedule != null)
                            {
                                var dayLine = CreateDayLine(daySchedule, maxSessionsPerDay, lineIndex, date);
                                outputFile.WriteLine(dayLine);
                                lineIndex++;
                            }
                        }

                        outputFile.WriteLine(CreateTotalMonthlyFeesLine(maxSessionsPerDay, lineIndex - 1));
                    }
                }
            }

            // Show a message saying "Fees Printed!" with an X button to close it
            lbl_popup_schedules_printed.Visibility = Visibility.Visible;
            btn_close_fees_printed_popup.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Closes the 'Fees Printed' dialog
        /// </summary>
        private void btn_close_fees_printed_popup_click(object sender, RoutedEventArgs e)
        {
            lbl_popup_schedules_printed.Visibility = Visibility.Hidden;
            btn_close_fees_printed_popup.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Adds a new child to the settings.json file
        /// </summary>
        private void btn_add_schedule_Click(object sender, RoutedEventArgs e)
        {
            var childName = txt_child_name.Text;
            double hourlyRate = 0;
            if (!string.IsNullOrWhiteSpace(txt_hourly_rate.Text))
            {
                hourlyRate = Convert.ToDouble(txt_hourly_rate.Text);
            }
            double sessionRate = 0;
            if (!string.IsNullOrWhiteSpace(txt_session_rate.Text))
            {
                sessionRate = Convert.ToDouble(txt_session_rate.Text);
            }
            var dailySchedules = new List<ChildDailySchedule>();

            if (!string.IsNullOrWhiteSpace(txt_monday.Text))
            {
                var mondaySchedule = GetChildDailyScheduleFromString(
                    dailyScheduleString: txt_monday.Text,
                    day: "Monday",
                    hourlyRate: hourlyRate,
                    sessionRate: sessionRate
                    );
                dailySchedules.Add(mondaySchedule);
            }

            if (!string.IsNullOrWhiteSpace(txt_tuesday.Text))
            {
                var tuesdaySchedule = GetChildDailyScheduleFromString(
                    dailyScheduleString: txt_tuesday.Text,
                    day: "Tuesday",
                    hourlyRate: hourlyRate,
                    sessionRate: sessionRate
                    );
                dailySchedules.Add(tuesdaySchedule);
            }

            if (!string.IsNullOrWhiteSpace(txt_wednesday.Text))
            {
                var wedSchedule = GetChildDailyScheduleFromString(
                    dailyScheduleString: txt_wednesday.Text,
                    day: "Wednesday",
                    hourlyRate: hourlyRate,
                    sessionRate: sessionRate
                    );
                dailySchedules.Add(wedSchedule);
            }

            if (!string.IsNullOrWhiteSpace(txt_thursday.Text))
            {
                var thursSchedule = GetChildDailyScheduleFromString(
                    dailyScheduleString: txt_thursday.Text,
                    day: "Thursday",
                    hourlyRate: hourlyRate,
                    sessionRate: sessionRate
                    );
                dailySchedules.Add(thursSchedule);
            }

            if (!string.IsNullOrWhiteSpace(txt_friday.Text))
            {
                var fridaySchedule = GetChildDailyScheduleFromString(
                    dailyScheduleString: txt_friday.Text,
                    day: "Friday",
                    hourlyRate: hourlyRate,
                    sessionRate: sessionRate
                    );
                dailySchedules.Add(fridaySchedule);
            }

            var newWeeklySchedule = new ChildWeeklySchedule { ChildName = childName, DailySchedules = dailySchedules };

            // Update the settings file
            var settingSchedule = new SettingWeeklySchedule { Schedules = new List<ChildWeeklySchedule>() };
            using (var reader = new StreamReader("settings.json"))
            {
                var json = reader.ReadToEnd();
                settingSchedule = JsonSerializer.Deserialize<SettingWeeklySchedule>(json);
            }
            settingSchedule?.Schedules.Add(newWeeklySchedule);

            var jsonSchedule = JsonSerializer.Serialize(settingSchedule, new JsonSerializerOptions { WriteIndented = true });
            using (var outputFile = new StreamWriter("settings.json"))
            {
                outputFile.WriteLine(jsonSchedule);
            }

            lbl_child_added.Visibility = Visibility.Visible;
            btn_close_child_added_popup.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Closes the 'Child Added' dialog
        /// </summary>
        private void btn_close_child_added_popup_click(object sender, RoutedEventArgs e)
        {
            lbl_child_added.Visibility = Visibility.Hidden;
            btn_close_child_added_popup.Visibility = Visibility.Hidden;
            cnv_add.Visibility = Visibility.Hidden;
        }

        /* Utility methods */

        /// <summary>
        /// Returns the highest number of sessions per day in a week for a given child
        /// </summary>
        /// <param name="childSchedule"> The child weekly schedule </param>
        /// <returns> The highest number of sessions the child will attend in one single day </returns>
        private int FindMaxSessionsPerDay(ChildWeeklySchedule childSchedule)
        {
            var dailyScheduleWithTheMostSessions = childSchedule.DailySchedules.MaxBy(x => x.Sessions.Count);
            return dailyScheduleWithTheMostSessions is null ? 0 : dailyScheduleWithTheMostSessions.Sessions.Count;
        }

        /// <summary>
        /// Creates a row with the headers of the monthly fees spreadsheet:
        /// Date ¦ Session n Start ¦ Session n End ¦ ... ¦ Total Hours ¦ Fees
        /// </summary>
        /// <param name="maxSessionsPerDay"> The max number of sessions this child has in any day of the month (1-3) </param>
        /// <returns> A row with the headers of the monthly fees spreadsheet: </returns>
        private string CreateHeadersLine(int maxSessionsPerDay)
        {
            var headersLine = "Date, ";
            for (int i = 1; i <= maxSessionsPerDay; i++)
            {
                headersLine += $"Session {i} Start, ";
                headersLine += $"Session {i} End, ";
            }
            headersLine += "Total Hours, Fee";
            return headersLine;
        }

        /// <summary>
        /// Creates a row of data for a day of childminding
        /// </summary>
        /// <param name="daySchedule"> The child's schedule for the day </param>
        /// <param name="maxSessionsPerDay"> The max number of sessions this child has in any day of the month (1-3) </param>
        /// <param name="lineIndex"> The row number in the spreadsheet </param>
        /// <param name="date"> The date </param>
        /// <returns> A row in a child's monthly fees spreadsheet </returns>
        /// <remarks>
        /// Format of a row is:
        /// Date ¦ Session n Start ¦ Session n End ¦ ... ¦ Total Hours ¦ Fees
        /// There can be a maximum of 3 sessions in a day
        /// </remarks>
        private string CreateDayLine(ChildDailySchedule daySchedule, int maxSessionsPerDay, int lineIndex, DateTime date)
        {
            var line = $"{date:dddd d MMMM},";

            // Append data on session times (session start and session end for each session)
            line += CreateSessionTimesColumnData(daySchedule, maxSessionsPerDay);

            // Append total hours column data
            line += CreateTotalHoursColumnData(daySchedule, lineIndex);

            // Append fees column data
            line += CreateFeesColumnData(daySchedule, lineIndex, maxSessionsPerDay);
            return line;
        }

        /// <summary>
        /// Creates the data for the columns that show starting and ending times for each session
        /// </summary>
        /// <param name="daySchedule"> The schedule for the day </param>
        /// <param name="maxSessionsPerDay"> The highest number of sessions per day in a week for this child </param>
        /// <returns> The starting and ending times of sessions for the day </returns>
        private string CreateSessionTimesColumnData(ChildDailySchedule daySchedule, int maxSessionsPerDay)
        {
            var line = "";
            foreach (var session in daySchedule.Sessions)
            {
                line += $"{session.Start},{session.End},";
            }

            if (maxSessionsPerDay > daySchedule.Sessions.Count)
            {
                var difference = maxSessionsPerDay - daySchedule.Sessions.Count;
                for (int i = 0; i < difference; i++)
                {
                    line += $"N/A,";
                }
            }
            return line;
        }

        /// <summary>
        /// Creates the spreadsheet formula that calculates the total billable hours for the day
        /// </summary>
        /// <param name="daySchedule"> The minding schedule for the day </param>
        /// <param name="lineIndex"> The row in the spreadsheet </param>
        /// <returns> The formula to calculate the billable hours </returns>
        private string CreateTotalHoursColumnData(ChildDailySchedule daySchedule, int lineIndex)
        {
            var line = "=";
            var sessionIndex = 1;
            var endTimeColumn = "C";
            var startTimeColumn = "B";
            foreach (var session in daySchedule.Sessions)
            {
                if (sessionIndex == 2)
                {
                    endTimeColumn = "E";
                    startTimeColumn = "D";
                }
                else if (sessionIndex == 3)
                {
                    endTimeColumn = "G";
                    startTimeColumn = "F";
                }

                line += $"+({endTimeColumn}{lineIndex}-{startTimeColumn}{lineIndex})";
                sessionIndex++;
            }
            line += ",";
            return line;
        }

        /// <summary>
        /// Creates the spreadsheet formula that calculates the fees to be paid for the day
        /// </summary>
        /// <param name="daySchedule"> The minding schedule for the day </param>
        /// <param name="lineIndex"> The current row number in the spreadsheet </param>
        /// <param name="maxSessionsPerDay"> The maximum number of sessions in a day for this child (1-3) </param>
        /// <returns> The formula to calculate the fees for the day </returns>
        private string CreateFeesColumnData(ChildDailySchedule daySchedule, int lineIndex, int maxSessionsPerDay)
        {
            var line = "=";

            // Calculate fees for sessions paid at a fixed rate
            var anyFixedRate = false;
            var fixedRateSessions = daySchedule.Sessions.Where(x => x.SessionRate != 0);
            var sessionRate = fixedRateSessions.FirstOrDefault()?.SessionRate;
            if (sessionRate != null && sessionRate != 0)
            {
                anyFixedRate = true;
                line += $"({fixedRateSessions.Count()}*{sessionRate})";
            }

            // Calculate fees for sessions paid hourly
            var hourlySessions = daySchedule.Sessions.Where(x => x.SessionRate == 0);
            if (hourlySessions.Any())
            {
                // The hourly rate will be the same for all sessions paid hourly
                var hourlyRate = hourlySessions.FirstOrDefault()?.HourlyRate;
                if (anyFixedRate)
                {
                    line += "+";
                }
                var totalHoursColumn = "D";
                switch (maxSessionsPerDay)
                {
                    case 2:
                        totalHoursColumn = "F";
                        break;
                    case 3:
                        totalHoursColumn = "H";
                        break;
                    default:
                        break;
                }
                line += $"({hourlyRate}*{totalHoursColumn}{lineIndex}*24)";
            }
            return line;
        }

        /// <summary>
        /// Creates the spreadsheet formula that calculates the fees for the whole month
        /// </summary>
        /// <param name="maxSessionsPerDay"> Max number of sessions per day (1-3) </param>
        /// <param name="lastLineIndex"> The number of the last row containing daily data </param>
        /// <returns> The formula to calculate the monthly fees </returns>
        private string CreateTotalMonthlyFeesLine(int maxSessionsPerDay, int lastLineIndex)
        {
            var billingColumn = "E";
            var totalHoursColumn = "D";
            switch (maxSessionsPerDay)
            {
                case 2:
                    billingColumn = "G";
                    totalHoursColumn = "F";
                    break;
                case 3:
                    billingColumn = "I";
                    totalHoursColumn = "H";
                    break;
                default:
                    break;
            }

            // Leave a column free for the date
            var finalLine = ",";
            // Leave two columns free for all the sessions start-end times
            // apart from one last column where the word 'TOTAL:' will be printed
            for (int i = 0; i < maxSessionsPerDay-1; i++)
            {
                finalLine += ",,";
            }
            finalLine += ",";

            // Type 'TOTAL:' in the column of the last session's end times
            finalLine += "TOTAL:,";
            // Then add the formula to calculate the total hours across the month
            finalLine += $"=SUM({totalHoursColumn}2:{totalHoursColumn}{lastLineIndex}),";
            // Then add the formula to calculate the total cost across the month
            finalLine += $"=SUM({billingColumn}2:{billingColumn}{lastLineIndex})";
            return finalLine;
        }

        /// <summary>
        /// Parses the user's input for a new child schedule and turns it into an object
        /// then stores it in the settings.json file
        /// </summary>
        /// <param name="dailyScheduleString"> The user's input </param>
        /// <param name="day"> The day of the week </param>
        /// <param name="hourlyRate"> The hourly rate for this child </param>
        /// <param name="sessionRate"> The session rate for this child (if paying by session) </param>
        /// <returns></returns>
        private ChildDailySchedule GetChildDailyScheduleFromString(string dailyScheduleString, string day, double hourlyRate, double sessionRate)
        {
            string[] parts = dailyScheduleString.Split(',');
            var sessions = new List<Session>();

            foreach (string part in parts)
            {
                string trimmedPart = part.Trim(); // Remove spaces
                string[] timeParts = trimmedPart.Split(' '); // Separate time range from 'h'

                if (timeParts.Length < 2) continue; // Ensure we have a suffix like 'h'

                string timeRange = timeParts[0]; // "07:30-8:45"
                string suffix = timeParts[1]; // "h"

                string[] times = timeRange.Split('-'); // ["07:30", "8:45"]

                if (times.Length != 2) continue; // Ensure start and end times exist

                // Convert to TimeOnly (handles missing leading zero)
                TimeOnly startTime = TimeOnly.ParseExact(times[0].PadLeft(5, '0'), "HH:mm");
                TimeOnly endTime = TimeOnly.ParseExact(times[1].PadLeft(5, '0'), "HH:mm");

                var session = new Session { Start = startTime, End = endTime };
                if (suffix == "h")
                {
                    session.HourlyRate = hourlyRate;
                }
                else
                {
                    session.SessionRate = sessionRate;
                }
                sessions.Add(session);
            }

            return new ChildDailySchedule { Day = day, Sessions = sessions };
        }
    }
}