using System;
using System.Collections.Generic;
using System.Linq;
using Helper.Models;

namespace Helper.Jobs.Impl
{
    public class JobHistory: IHistory
    {
        private readonly Dictionary<DateTime, object> _dictionary = new Dictionary<DateTime, object>();

        public IReadOnlyDictionary<DateTime, object> Values => _dictionary;

        public DateTime? LastTime
        {
            get
            {
                if (_dictionary.Count == 0)
                    return null;

                return _dictionary.Keys.Max();
            }
        }

        public void Add(object value)
        {
            _dictionary.Add(DateTime.Now, value);
        }
    }
}
