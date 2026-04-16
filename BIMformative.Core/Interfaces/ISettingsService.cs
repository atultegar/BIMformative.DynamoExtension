using BIMformative.Core.Models;

namespace BIMformative.Core.Interfaces
{
    public interface ISettingsService
    {
        AppSettings Current { get; }

        void Load();
        void Save();

        void Reset();
    }
}
