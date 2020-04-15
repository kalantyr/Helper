using System.Threading;
using System.Threading.Tasks;

namespace Helper.Jobs
{
    public interface IJob
    {
        Task Run(CancellationToken cancellationToken);
    }
}