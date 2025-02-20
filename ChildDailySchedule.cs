namespace ChildmindingScheduler
{
    internal class ChildDailySchedule
    {
        public required string Day { get; set; }

        public required List<Session> Sessions { get; set; }
    }
}
