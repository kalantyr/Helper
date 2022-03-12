using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helper.Core.Jobs
{
    public interface IJob
    {
        string Name { get; }

        bool IsDisabled { get; }

        TimeSpan? Interval { get; }

        IHistory History { get; }

        Action<IJob, string> Message { get; set; }

        Task Run(CancellationToken cancellationToken);
    }
}