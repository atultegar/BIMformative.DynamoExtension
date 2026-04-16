using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IDesktopTicketService
    {
        Task<string> CreateTicketAsync(CancellationToken cancellationToken = default);
    }
}
