using System;
using UnityEngine;

namespace Systems.Calendar
{
    [Serializable]
    public struct CalendarDate : IEquatable<CalendarDate>
    {
        public int day;
        public Season season;
        public int year;

        public CalendarDate(int day, Season season, int year)
        {
            this.day = day;
            this.season = season;
            this.year = year;
        }

        public bool Equals(CalendarDate other)
        {
            return day == other.day && season == other.season && year == other.year;
        }

        public override bool Equals(object obj)
        {
            return obj is CalendarDate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(day, season, year);
        }

        public override string ToString()
        {
            return $"{season} {day}, Year {year}";
        }

        public int ToTotalDays(int daysPerSeason)
        {
            // Calculation based on Year 1, Spring 1 being index 0
            // Since we only have 4 seasons, we use 4 as the multiplier
            int yearOffset = (year - 1) * 4 * daysPerSeason;
            int seasonOffset = (int)season * daysPerSeason;
            int dayOffset = day - 1;

            return yearOffset + seasonOffset + dayOffset;
        }

        public int ToTotalWeeks(int daysPerSeason)
        {
            return ToTotalDays(daysPerSeason) / 7;
        }

        public static bool operator ==(CalendarDate left, CalendarDate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CalendarDate left, CalendarDate right)
        {
            return !left.Equals(right);
        }
    }
}
