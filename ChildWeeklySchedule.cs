namespace ChildmindingScheduler
{
    internal class ChildWeeklySchedule
    {
        public required string ChildName { get; set; }

        public required List<ChildDailySchedule> DailySchedules { get; set; }
    }
}