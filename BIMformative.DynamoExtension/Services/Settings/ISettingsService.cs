using BIMformative.DynamoExtension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Settings
{
    public interface ISettingsService
    {
        AppSettings Current { get; }

        void Load();
        void Save();

        void Reset();
    }
}
