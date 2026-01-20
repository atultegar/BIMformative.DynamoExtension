using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Interfaces
{
    public interface IDynamoService
    {
        bool HasUnsavedChanges();
        Task<bool> SaveCurrentWorkspaceAsync();
        void CloseCurrentWorkspace();
        void OpenWorkspace(string filePath);
    }
}
