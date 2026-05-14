using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Systems.Calendar
{
    public class CalendarSystem : MonoBehaviour
    {
        public static CalendarSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int daysPerSeason = 28;
        [SerializeField] private CalendarDatabase calendarDatabase;

        [Header("Current State")]
        [SerializeField] private CalendarDate currentDate = new CalendarDate(1, Season.Spring, 1);
        [SerializeField] private DayOfWeek currentDayOfWeek = DayOfWeek.Monday;
        [SerializeField] private int currentHour = 6; // 0-23 range

        [Header("Events")]
        public UnityEvent<CalendarDate> OnDayChanged;
        public UnityEvent<Season> OnSeasonChanged;
        public UnityEvent<int> OnYearChanged;
        public UnityEvent<List<CalendarEvent>> OnEventsTriggered;

         [Header("Visuals")]
         [SerializeField] private TextMeshProUGUI dayText;
         [SerializeField] private TextMeshProUGUI hourText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Trigger initial events for the first day
            CheckDailyEvents();
            UpdateUITexts();
        }

        [ContextMenu("Advance Day")]
        public void AdvanceDay()
        {
            // Increment Day of Week (0-6)
            int nextDayOfWeek = ((int)currentDayOfWeek + 1) % 7;
            currentDayOfWeek = (DayOfWeek)nextDayOfWeek;

            // Increment Day
            currentDate.day++;

            if (currentDate.day > daysPerSeason)
            {
                currentDate.day = 1;
                AdvanceSeason();
            }

            OnDayChanged?.Invoke(currentDate);
            CheckDailyEvents();
            UpdateUITexts();
            
            Debug.Log($"Advanced to: {currentDate} ({currentDayOfWeek})");
        }

        private void AdvanceSeason()
        {
            int nextSeason = (int)currentDate.season + 1;
            
            // Skip Season.All (which is value 4) and loop back to Spring
            if (nextSeason >= (int)Season.All) 
            {
                currentDate.season = Season.Spring;
                AdvanceYear();
            }
            else
            {
                currentDate.season = (Season)nextSeason;
            }

            OnSeasonChanged?.Invoke(currentDate.season);
            Debug.Log($"Advanced to Season: {currentDate.season}");
        }

        private void AdvanceYear()
        {
            currentDate.year++;
            OnYearChanged?.Invoke(currentDate.year);
            Debug.Log($"Advanced to Year: {currentDate.year}");
        }

        private void CheckDailyEvents()
        {
            if (calendarDatabase == null) return;

            var events = calendarDatabase.GetEventsForDate(currentDate, currentDayOfWeek, daysPerSeason);
            if (events.Count > 0)
            {
                OnEventsTriggered?.Invoke(events);
                foreach (var ev in events)
                {
                    Debug.Log($"Event Triggered: {ev.eventName}");
                }
            }
        }

        #region Helpers

        public CalendarDate GetCurrentDate() => currentDate;
        public DayOfWeek GetCurrentDayOfWeek() => currentDayOfWeek;
        public int GetCurrentHour() => currentHour;

        public void SetDate(CalendarDate date)
        {
            currentDate = date;
            UpdateUITexts();
            CheckDailyEvents();
        }

        public void SetDayOfWeek(DayOfWeek day)
        {
            currentDayOfWeek = day;
            UpdateUITexts();
        }

        public void SetHour(int hour)
        {
            currentHour = Mathf.Clamp(hour, 0, 23);
            UpdateUITexts();
        }

        private void UpdateUITexts()
        {
            if (dayText != null)
            {
                dayText.text = $"{currentDayOfWeek} | Day {currentDate.day} of {currentDate.season}";
            }

            if (hourText != null)
            {
                hourText.text = $"{currentHour:D2}:00";
            }
        }

        [ContextMenu("Advance Hour")]
        public void AdvanceHour()
        {
            currentHour = (currentHour + 1) % 24;
            
            if (currentHour == 0)
            {
                AdvanceDay();
            }
            else
            {
                UpdateUITexts();
            }
            
            Debug.Log($"Advanced to: {currentHour:D2}:00");
        }

        public void SetDate(int day, Season season, int year, DayOfWeek dayOfWeek)
        {
            currentDate = new CalendarDate(day, season, year);
            currentDayOfWeek = dayOfWeek;
            OnDayChanged?.Invoke(currentDate);
            CheckDailyEvents();
            UpdateUITexts();
        }

        #endregion

        #region Save/Load

        [Serializable]
        public struct CalendarSaveData
        {
            public int day;
            public Season season;
            public int year;
            public DayOfWeek dayOfWeek;
        }

        public string GetSaveDataJson()
        {
            var data = new CalendarSaveData
            {
                day = currentDate.day,
                season = currentDate.season,
                year = currentDate.year,
                dayOfWeek = currentDayOfWeek
            };
            return JsonUtility.ToJson(data);
        }

        public void LoadFromJson(string json)
        {
            var data = JsonUtility.FromJson<CalendarSaveData>(json);
            SetDate(data.day, data.season, data.year, data.dayOfWeek);
        }

        #endregion
    }
}
