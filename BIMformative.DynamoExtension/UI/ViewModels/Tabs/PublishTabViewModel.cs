using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Script;
using BIMformative.DynamoExtension.UI.Views.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public sealed class PublishTabViewModel : TabItemViewModel
    {
        private readonly IScriptService _scriptService;

        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _demoLink = string.Empty;
        private string _selectedScriptType = "Revit";
        private string _uploadedFilePath = string.Empty;
        private string? _parsedJson;
        private bool _isPublishing;
        private bool _isAnalyzing;
        private bool _isPublic;
        private string _newTag = string.Empty;

        private ScriptSourceType _sourceType = ScriptSourceType.None;
        private string? _workspaceJson;

        private string _uploadId = string.Empty;
        private string _storagePath = string.Empty;

        private double _publishProgress;

        public string AnalyzeButtonText =>
            IsAnalyzing ? "Analyzing..." : "Analyze";

        public string AnalyzeWrokspaceButtonText =>
            IsAnalyzing ? "Analyzing..." : "Analyze Current Script";

        public string PublishButtonText =>
            IsPublishing ? "Publishing..." : "Publish";

        public string Title 
        {
            get => _title; 
            set { _title = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(CanPublish)); } 
        }

        public string Description 
        { 
            get => _description; 
            set { _description = value; RaisePropertyChanged(); } 
        }

        public string DemoLink 
        { 
            get => _demoLink; 
            set { _demoLink = value; RaisePropertyChanged(); } 
        }

        public ObservableCollection<string> ScriptTypes { get; } = 
            new()  { "Revit", "Civil 3D" };

        public string SelectedScriptType 
        { 
            get => _selectedScriptType; 
            set { _selectedScriptType = value; RaisePropertyChanged(); } 
        }

        public ObservableCollection<string> Tags { get; } = new();
        
        public bool IsPublic 
        { 
            get => _isPublic; 
            set { _isPublic = value; RaisePropertyChanged(); } 
        }

        public string? ParsedJson 
        { 
            get => _parsedJson; 
            set { _parsedJson = value; RaisePropertyChanged(); } 
        }

        public bool IsPublishing 
        { 
            get => _isPublishing; 
            set 
            {
                if (_isPublishing != value)
                {
                    _isPublishing = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(PublishButtonText));
                    RaisePropertyChanged(nameof(CanPublish));
                }                
            } 
        }

        public bool IsAnalyzing 
        { 
            get => _isAnalyzing; 
            set 
            { 
                if (_isAnalyzing != value)
                {
                    _isAnalyzing = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(AnalyzeButtonText));
                    RaisePropertyChanged(nameof(AnalyzeWrokspaceButtonText));
                    RaisePropertyChanged(nameof(CanAnalyze));
                    RaisePropertyChanged(nameof(CanPublish));
                }
            } 
        }

        public string NewTag
        {
            get => _newTag;
            set { _newTag = value; RaisePropertyChanged(); }
        }

        public string UplaodId
        {
            get => _uploadId;
            set { _uploadId = value; RaisePropertyChanged(); }
        }

        public string StoragePath
        {
            get => _storagePath;
            set { _storagePath = value; RaisePropertyChanged(); }
        }

        public double PublishProgress
        {
            get => _publishProgress;
            set { _publishProgress = value; RaisePropertyChanged(); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; RaisePropertyChanged(); }
        }

        public ScriptSourceType SourceType
        {
            get => _sourceType;
            set
            {
                _sourceType = value; 
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanAnalyze));
                RaisePropertyChanged(nameof(CanPublish));
            }
        }

        public string ShowFileName =>
            string.IsNullOrEmpty(_uploadedFilePath)
            ? "No file selected"
            : Path.GetFileName(_uploadedFilePath);

        // State
        public bool CanAnalyze => 
            !IsAnalyzing && 
            (
                (SourceType == ScriptSourceType.File && !string.IsNullOrEmpty(_uploadedFilePath)) ||
                (SourceType == ScriptSourceType.Workspace && !string.IsNullOrEmpty(_workspaceJson))
            );

        public bool CanPublish => !IsPublishing && CanAnalyze && !string.IsNullOrEmpty(Title);

        // Commands
        public ICommand UploadScriptCommand { get; }
        public ICommand AnalyzeScriptCommand { get; }
        public ICommand PublishCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }
        public ICommand SelectScriptTypeCommand { get; }
        public ICommand AnalyzeWorkspaceCommand { get; }

        public PublishTabViewModel(IAuthService auth, IScriptService scriptService)
            : base(
                  header: "Publish Script",
                  contentFactory: () => new PublishControl())
        {
            if (Content is PublishControl control)
                control.DataContext = this;

            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));

            UploadScriptCommand = new RelayCommand(UploadScript);
            AnalyzeScriptCommand = new AsyncRelayCommand(AnalyzeAsync);
            PublishCommand = new AsyncRelayCommand(PublishAsync);
            CancelCommand = new RelayCommand(Cancel);
            AddTagCommand = new RelayCommand(AddTag);
            RemoveTagCommand = new RelayCommand<string>(RemoveTag);
            SelectScriptTypeCommand = new RelayCommand<string>(type =>
            {
                SelectedScriptType = type;
            });
            AnalyzeWorkspaceCommand = new AsyncRelayCommand(AnalyzeWorkspace);
        }

        

        private void UploadScript()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Dynamo Script (*.dyn)|*.dyn"
            };

            if (dialog.ShowDialog() == true)
            {
                _uploadedFilePath = dialog.FileName;
                RaisePropertyChanged(nameof(ShowFileName));
                RaisePropertyChanged(nameof(CanAnalyze));
                RaisePropertyChanged(nameof(CanPublish));
            }
        }

        private async Task AnalyzeAsync()
        {
            if (!CanAnalyze) return;

            IsAnalyzing = true;

            try
            {
                var result = await _scriptService.AnalyzeAsync(_uploadedFilePath);

                ParsedJson = JsonSerializer.Serialize(result.ScriptData);

                _uploadId = result.UploadId;
                _storagePath = result.StoragePath;

                // PREFILL - NOT LOCK
                if (string.IsNullOrWhiteSpace(Title))
                    Title = result.ScriptData?.Name ?? string.Empty;

                if (string.IsNullOrWhiteSpace(Description))
                    Description = result.ScriptData?.Description ?? string.Empty;
              
                Tags.Clear();
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        private async Task AnalyzeWorkspace()
        {
            IsAnalyzing = true;

            try
            {
                var result = await _scriptService.AnalyzeWorkspaceAsync();

                ParsedJson = JsonSerializer.Serialize(result.ScriptData);

                // PREFILL - NOT LOCK
                if (string.IsNullOrWhiteSpace(Title))
                    Title = result.ScriptData?.Name ?? string.Empty;

                if (string.IsNullOrWhiteSpace(Description))
                    Description = result.ScriptData?.Description ?? string.Empty;

                Tags.Clear();
            }
            catch (OperationCanceledException)
            {
                // user cancelled - ignore silently
            }
            catch (InvalidOperationException ioex)
            {
                MessageBoxService.Show(ioex.Message, "Analyze failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(ex.Message, "Analyze failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally 
            { 
                IsAnalyzing = false; 
            }

        }

        private async Task PublishAsync()
        {
            if (!CanPublish) return;

            IsPublishing = true;
            PublishProgress = 0;

            try
            {
                var request = new ScriptPublishRequestDto
                {
                    StoragePath = _storagePath,
                    ParsedJson = ParsedJson!,
                    Title = Title,
                    Description = Description,
                    ScriptType = SelectedScriptType == "Revit" ? "revit" : "civil3d",
                    Tags = Tags,
                    DemoLink = DemoLink,
                    IsPublic = IsPublic
                };

                var progress = new Progress<double>(p =>
                {
                    PublishProgress = p * 100;
                });

                await _scriptService.PublishAsync(request, progress);

                // SUCCESS FEEDBACK
                MessageBoxService.Show(
                    "Your script has been published successfully 🎉",
                    "Publish Successful",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information 
                );

                Cancel();
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(
                    ex.Message,
                    "Publish Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            finally
            {
                IsPublishing = false;
                PublishProgress = 0;
            }            
        }
        
        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTag))
                return;

            var tag = NewTag.Trim();

            if (!Tags.Contains(tag))
                Tags.Add(tag);

            NewTag = string.Empty;

            RaisePropertyChanged(nameof(Tags));
            RaisePropertyChanged(nameof(CanPublish));
        }

        private void RemoveTag(string tag)
        {
            if (tag == null)
                return;

            if (Tags.Contains(tag))
                Tags.Remove(tag);

            RaisePropertyChanged(nameof(Tags));
            RaisePropertyChanged(nameof(CanPublish));
        }

        private void Cancel()
        {
            // Core fields
            _uploadedFilePath = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            DemoLink = string.Empty;
            SelectedScriptType = "Revit";
            IsPublic = false;

            // Analyze state
            ParsedJson = null;
            NewTag = string.Empty;

            // Flags
            IsAnalyzing = false;
            IsPublishing = false;

            // Collections
            Tags.Clear();

            // Notify derived UI
            RaisePropertyChanged(nameof(ShowFileName));
            RaisePropertyChanged(nameof(CanAnalyze));
            RaisePropertyChanged(nameof(CanPublish));
        }
    }
}
