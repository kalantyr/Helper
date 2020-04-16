using System.Threading;
using System.Threading.Tasks;
using Helper.Checkers;

namespace Helper.Jobs
{
    public interface IJob
    {
        string Name { get; }

        ICheckerHistory History { get; }

        Task Run(CancellationToken cancellationToken);
    }
}