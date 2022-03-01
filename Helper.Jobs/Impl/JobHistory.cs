using System;
using System.Collections.Generic;

namespace Helper.Jobs.Impl
{
    public class JobHistory: IHistory
    {
        private readonly Dictionary<DateTime, object> _dictionary = new Dictionary<DateTime, object>();

        public IReadOnlyDictionary<DateTime, object> Values => _dictionary;

        public DateTime? LastTime => DateTime.Now.AddHours(-01);

        public void Add(object value)
        {
            _dictionary.Add(DateTime.Now, value);
        }
    }
}
