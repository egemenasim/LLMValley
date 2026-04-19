using System.Collections.Generic;
using UnityEngine;

namespace Systems.Calendar
{
    [CreateAssetMenu(fileName = "CalendarDatabase", menuName = "Calendar/Database")]
    public class CalendarDatabase : ScriptableObject
    {
        public List<CalendarEvent> allEvents = new List<CalendarEvent>();

        public List<CalendarEvent> GetEventsForDate(CalendarDate date, DayOfWeek dayOfWeek, int daysPerSeason)
        {
            List<CalendarEvent> triggeredEvents = new List<CalendarEvent>();
            foreach (var ev in allEvents)
            {
                if (ev != null && ev.IsTriggered(date, dayOfWeek, daysPerSeason))
                {
                    triggeredEvents.Add(ev);
                }
            }
            return triggeredEvents;
        }
    }
}
