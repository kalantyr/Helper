using System;
using System.Collections.Generic;

namespace Helper.Core
{
    public interface IHistory
    {
        IReadOnlyDictionary<DateTime, object> Values { get; }

        DateTime? LastTime { get; }
    }
}
