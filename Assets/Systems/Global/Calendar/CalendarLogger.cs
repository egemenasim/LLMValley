using UnityEngine;
using Systems.Calendar;

namespace Systems.Calendar.Demo
{
    public class CalendarLogger : MonoBehaviour
    {
        private void Start()
        {
            if (CalendarSystem.Instance != null)
            {
                CalendarSystem.Instance.OnDayChanged.AddListener(OnDayChanged);
            }
        }

        private void OnDayChanged(CalendarDate date)
        {
            Debug.Log($"<color=cyan>[Calendar]</color> The date is now {date}. Day of week: {CalendarSystem.Instance.GetCurrentDayOfWeek()}");
        }
    }
}
