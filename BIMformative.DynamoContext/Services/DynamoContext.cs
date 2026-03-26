using Dynamo.Models;
using Dynamo.ViewModels;
using System.Windows;

namespace BIMformative.DynamoContext.Services
{
    public class DynamoContext : IDynamoContext
    {
        public DynamoViewModel ViewModel { get; }

        public DynamoModel Model { get; }

        public Window Window { get; }

        public DynamoContext(DynamoViewModel viewModel, Window window)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Model = viewModel.Model ?? throw new ArgumentNullException(nameof(window));
            Window = window ?? throw new ArgumentNullException(nameof(window));
        }


        public bool HasOpenWorkspace => Model.CurrentWorkspace != null;

        public bool IsCurrentWorkspaceDirty =>
            Model.CurrentWorkspace?.HasUnsavedChanges ?? false;



        public void CloseCurrentWorkspace()
        {
            ViewModel.CloseHomeWorkspaceCommand.Execute(null);
        }

        public async Task<bool> EnsureWorkspaceCanCloseAsync()
        {
            if (!IsCurrentWorkspaceDirty)
                return true;

            //ViewModel.ShowSaveDialogAndSaveResult(Model.CurrentWorkspace);

            ViewModel.ShowSaveDialogAndSaveResult(Model.CurrentWorkspace);

            return true;
        }

        public void OpenWorkspace(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid file path", nameof(filePath));

            ViewModel.OpenCommand.Execute(filePath);
        }
    }
}
