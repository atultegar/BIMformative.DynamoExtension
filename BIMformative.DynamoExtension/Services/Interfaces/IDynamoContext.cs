using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Interfaces
{
    public interface IDynamoContext
    {
        Dynamo.ViewModels.DynamoViewModel ViewModel { get; }
        Dynamo.Models.DynamoModel Model { get; }
        System.Windows.Window Window { get; }

        bool HasOpenWorkspace { get; }
        bool IsCurrentWorkspaceDirty { get; }

        Task<bool> EnsureWorkspaceCanCloseAsync();
        void CloseCurrentWorkspace();
        void OpenWorkspace(string filePath);
        
    }
}
