using BIMformative.DynamoExtension.Services.Interfaces;
using Dynamo.Extensions;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services
{
    public class DynamoService : IDynamoService
    {
        

        public void CloseCurrentWorkspace()
        {
            throw new NotImplementedException();
        }

        public bool HasUnsavedChanges()
        {
            throw new NotImplementedException();
        }

        public void OpenWorkspace(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveCurrentWorkspaceAsync()
        {
            throw new NotImplementedException();
        }
    }
}
