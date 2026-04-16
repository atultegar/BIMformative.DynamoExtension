using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync();
    }
}
