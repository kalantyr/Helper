using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helper.Core.Checkers
{
    public interface IChecker
    {
        string Name { get; }

        bool IsDisabled { get; }

        TimeSpan? Interval { get; }

        Task Check(CancellationToken cancellationToken);

        ICheckerHistory History { get; }

        string GetTextForClipboard();

        event Action<IChecker> Notify;

        bool NeedNotify { get; }

        DateTime? LastAvailableTime { get; }

        event Action<IChecker> NotFound;
    }
}
