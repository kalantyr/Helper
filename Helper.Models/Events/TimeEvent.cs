using System;
using System.Collections.Generic;
using System.Linq;

namespace Helper.Models.Events
{
    public class TimeEvent: IEvent
    {
        private readonly ICollection<DateTime> _history = new List<DateTime>();

        public string Name { get; set; }

        public string Schedule { get; set; }

        public TimeSpan WarningPeriod { get; set; }

        public bool NeedNotify
        {
            get
            {
                var lastTime = DateTime.MinValue;
                if (_history.Any())
                    lastTime = _history.OrderBy(h => h).Last();
                
                if (DateTime.Now - lastTime <= TimeSpan.FromMinutes(1))
                    return false;

                // округляем до минут
                var now = TimeSpan.FromMinutes((int)DateTime.Now.TimeOfDay.TotalMinutes);

                foreach(var q in GetSchedule(Schedule))
                    if (now == q || now == q.Add(-WarningPeriod))
                    {
                        _history.Add(DateTime.Now);
                        return true;
                    }
                
                return false;
            }
        }

        private static IReadOnlyCollection<TimeSpan> GetSchedule(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new TimeSpan[0];

            return value
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => TimeSpan.Parse(s.Trim()))
                .ToArray();
        }
    }
}
