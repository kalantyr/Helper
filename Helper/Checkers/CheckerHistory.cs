using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Helper.Checkers
{
    public interface ICheckerHistory
    {
        void AddResult(DateTime dateTime, object value);

        EventHandler Changed { get; set; }

        object LastValue { get; }

        DateTime? LastTime { get; }

        IReadOnlyDictionary<DateTime, object> Values { get; }
    }

    public class CheckerHistory : ICheckerHistory
    {
        private readonly IDictionary<DateTime, object> _history = new ConcurrentDictionary<DateTime, object>();

        public object LastValue
        {
            get
            {
                if (!_history.Any())
                    return default;

                var lastTime = _history.Keys.OrderBy(k => k).Last();
                return _history[lastTime];
            }
        }

        public DateTime? LastTime
        {
            get
            {
                if (!_history.Any())
                    return default;

                return _history.Keys.OrderBy(k => k).Last();
            }
        }

        public IReadOnlyDictionary<DateTime, object> Values => new ReadOnlyDictionary<DateTime, object>(_history);

        public void AddResult(DateTime dateTime, object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _history.Add(dateTime, value);

            Changed?.Invoke(this, EventArgs.Empty);
        }

        public EventHandler Changed { get; set; }
    }
}
