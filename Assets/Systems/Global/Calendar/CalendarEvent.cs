using UnityEngine;

namespace Systems.Calendar
{
    [CreateAssetMenu(fileName = "NewCalendarEvent", menuName = "Calendar/Event")]
    public class CalendarEvent : ScriptableObject
    {
        public string eventName;
        [TextArea]
        public string description;
        public Sprite icon; 
        public OccurrenceType occurrenceType;

        [Header("Trigger Settings")]
        public int triggerDay = 1;
        public Season triggerSeason = Season.Spring;
        public int triggerYear = 1;
        public DayOfWeek triggerDayOfWeek = DayOfWeek.Monday;

        [Header("Recurrence Settings")]
        [Tooltip("Repeat every X days/weeks/months/years. Use 1 for 'Every'")]
        public int repeatInterval = 1;

        public bool IsTriggered(CalendarDate currentDate, DayOfWeek currentDayOfWeek, int daysPerSeason)
        {
            // Ensure interval is at least 1 to avoid division by zero
            int interval = Mathf.Max(1, repeatInterval);

            bool seasonMatches = triggerSeason == Season.All || currentDate.season == triggerSeason;

            switch (occurrenceType)
            {
                case OccurrenceType.OneTime:
                    return currentDate.day == triggerDay && currentDate.season == triggerSeason && currentDate.year == triggerYear;
                
                case OccurrenceType.Daily:
                    return currentDate.ToTotalDays(daysPerSeason) % interval == 0;

                case OccurrenceType.Weekly:
                    if (currentDayOfWeek != triggerDayOfWeek) return false;
                    return currentDate.ToTotalWeeks(daysPerSeason) % interval == 0;

                case OccurrenceType.Monthly:
                    if (currentDate.day != triggerDay) return false;
                    // In this system, 1 season = 1 month. 
                    // To handle "Every X months", we treat seasons as months.
                    int totalMonths = ((currentDate.year - 1) * 4) + (int)currentDate.season;
                    return totalMonths % interval == 0;

                case OccurrenceType.Yearly:
                    if (currentDate.day != triggerDay || !seasonMatches) return false;
                    return (currentDate.year - 1) % interval == 0;

                default:
                    return false;
            }
        }
    }
}
