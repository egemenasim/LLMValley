namespace Systems.Calendar
{
    public enum Season
    {
        Spring,
        Summer,
        Fall,
        Winter,
        All // For events that can happen in any season
    }

    public enum OccurrenceType
    {
        OneTime,
        Daily,
        Weekly,
        Monthly, // Every month/season on the same day
        Yearly   // Every year on the same season and day
    }

    public enum DayOfWeek
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
}
