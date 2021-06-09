using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helper.Checkers
{
    public interface IChecker
    {
        string Name { get; }

        bool IsDisabled { get; }

        TimeSpan? Interval { get; }

        Task Check(CancellationToken cancellationToken);

        ICheckerHistory History { get; }

        void CopyToClipboard();

        EventHandler Notify { get; set; }

        bool NeedNotify { get; }

        event Action<IChecker> NotFound;
    }
}
