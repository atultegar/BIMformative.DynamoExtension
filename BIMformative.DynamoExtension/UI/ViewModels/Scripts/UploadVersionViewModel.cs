using BIMformative.Core.Interfaces;
using BIMformative.Core.Models.Api;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class UploadVersionViewModel : ViewModelBase
    {
        private readonly IScriptService _scriptService;
        private readonly IScriptLoadService _scriptLoadService;

        public string Slug { get; }

        public event Action? RequestClose;

        public UploadVersionViewModel(string slug, IScriptService scriptService, IScriptLoadService scriptLoadService)
        {
            Slug = slug ?? throw new ArgumentNullException(nameof(slug));
            _scriptService = scriptService;
            _scriptLoadService = scriptLoadService;

            BrowseFileCommand = new RelayCommand(BrowseFile);
            UseWorkspaceCommand = new AsyncRelayCommand(UseCurrentWorkspaceAsync);
            SubmitCommand = new AsyncRelayCommand(UploadAsync, () => CanUpload);
            _scriptLoadService = scriptLoadService;
        }

        #region Mode Selection

        private bool _useFile = true;
        public bool UseFile
        {
            get => _useFile;
            set
            {
                if (SetProperty(ref _useFile, value))
                {
                    if (value) UseWorkspace = false;
                    OnPropertyChanged(nameof(CanUpload));
                    SubmitCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _useWorkspace;
        public bool UseWorkspace
        {
            get => _useWorkspace;
            set
            {
                if (SetProperty(ref _useWorkspace, value))
                {
                    if (value) UseFile = false;
                    OnPropertyChanged(nameof(CanUpload));
                    SubmitCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Inputs

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (SetProperty(ref _filePath, value))
                {
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(CanUpload));
                    SubmitCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string FileName =>
            string.IsNullOrEmpty(FilePath)
            ? "No file selected"
            : System.IO.Path.GetFileName(FilePath);

        private string _changeLog;
        public string ChangeLog
        {
            get => _changeLog;
            set 
            {
                if (SetProperty(ref _changeLog, value))
                {
                    OnPropertyChanged(nameof(CanUpload));
                    SubmitCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Status

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (SetProperty(ref _statusMessage, value))
                    OnPropertyChanged(nameof(HasStatus));
            }
        }

        public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

        private Brush _statusColor = Brushes.Gray;
        public Brush StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    OnPropertyChanged(nameof(CanUpload));
                    SubmitCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Computed

        public bool CanUpload =>
            !IsBusy &&
            !string.IsNullOrWhiteSpace(ChangeLog) &&
            (
                (UseFile && !string.IsNullOrWhiteSpace(FilePath)) ||
                UseWorkspace
            );

        #endregion

        #region Commands

        public ICommand BrowseFileCommand { get; }
        public ICommand UseWorkspaceCommand { get; }
        public AsyncRelayCommand SubmitCommand { get; }

        #endregion

        #region Actions
                
        private void BrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Dynamo Script",
                Filter = "Dynamo Script (*.dyn)|*.dyn",
                Multiselect = false,
                CheckFileExists = true,
            };

            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;

                // Optional: auto-fill changelog
                if (string.IsNullOrWhiteSpace(ChangeLog))
                    ChangeLog = $"Uploaded version on {DateTime.Now:dd MMM yyyy}";
            }
        }

        

        private async Task UploadAsync()
        {
            if (UseFile)
            {
                await UploadFileAsync();
            }
            else
            {
                await UseCurrentWorkspaceAsync();
            }
        }

        private async Task UploadFileAsync()
        {
            
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                StatusMessage = "Please select a file.";
                StatusColor = Brushes.Red;
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Uploading...";
                StatusColor = Brushes.Gray;

                var parsed = await _scriptService.AnalyzeAsync(FilePath);

                await _scriptService.PublishVersionAsync(Slug, parsed, ChangeLog);

                StatusMessage = "✔ Version uploaded successfully";
                StatusColor = Brushes.Green;

                await Task.Delay(800);
                RequestClose?.Invoke();
            }
            catch (ApiException ex)
            {
                StatusMessage = ex.Message;
                StatusColor = Brushes.Red;
            }            
            catch (Exception ex)
            {
                StatusMessage = $"❌ Upload failed: {ex.Message}";
                StatusColor = Brushes.Red;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UseCurrentWorkspaceAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Exporting workspace...";
                StatusColor = Brushes.Gray;

                await _scriptLoadService.UploadVersionFromWorkspaceAsync(Slug, ChangeLog);

                StatusMessage = "✔ Version uploaded successfully";
                StatusColor = Brushes.Green;

                await Task.Delay(800);
                RequestClose?.Invoke();
            }
            catch (ApiException ex)
            {
                StatusMessage = ex.Message;
                StatusColor = Brushes.Red;
            }            
            catch (Exception ex)
            {
                StatusMessage = $"❌ Upload failed: {ex.Message}";
                StatusColor = Brushes.Red;
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}
