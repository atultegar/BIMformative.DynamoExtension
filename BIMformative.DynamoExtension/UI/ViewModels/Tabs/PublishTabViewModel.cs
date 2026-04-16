using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.Core.Models.Scripts;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.UI.Views.Controls;
using Dynamo.Wpf.Utilities;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public sealed class PublishTabViewModel : TabItemViewModel
    {
        private readonly IScriptService _scriptService;
        private readonly IScriptLoadService _scriptLoadService;

        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _demoLink = string.Empty;
        private string _selectedScriptType = "Revit";
        private string _uploadedFilePath = string.Empty;
        private string _parsedJson;
        private bool _isPublishing;
        private bool _isAnalyzing;
        private bool _isPublic;
        private string _newTag = string.Empty;
        private ScriptSourceType _sourceType = ScriptSourceType.None;
        private string _uploadId = string.Empty;
        private string _storagePath = string.Empty;
        private double _publishProgress;

        public PublishTabViewModel(
            IAuthService auth, 
            IScriptService scriptService, 
            IScriptLoadService scriptLoadService)
            : base(
                  header: "Publish Script",
                  contentFactory: () => new PublishControl())
        {
            if (Content is PublishControl control)
                control.DataContext = this;

            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _scriptLoadService = scriptLoadService ?? throw new ArgumentNullException(nameof(scriptLoadService));

            UploadScriptCommand = new RelayCommand(UploadScript);
            AnalyzeScriptCommand = new AsyncRelayCommand(AnalyzeAsync, () => CanAnalyzeFile);
            AnalyzeWorkspaceCommand = new AsyncRelayCommand(AnalyzeWorkspace, () => CanAnalyzeWorkspace);
            PublishCommand = new AsyncRelayCommand(PublishAsync, () => CanPublish);
            CancelCommand = new RelayCommand(Cancel);
            AddTagCommand = new RelayCommand(AddTag);
            RemoveTagCommand = new RelayCommand<string>(RemoveTag);
            SelectScriptTypeCommand = new RelayCommand<string>(type =>
            {
                if (!string.IsNullOrWhiteSpace(type))
                    SelectedScriptType = type;
            });

            RefreshState();            
        }

        #region Display Text

        public string AnalyzeButtonText => IsAnalyzing ? "Analyzing..." : "Analyze";
        public string AnalyzeWorkspaceButtonText => IsAnalyzing ? "Analyzing..." : "Analyze Current Script";
        public string PublishButtonText => IsPublishing ? "Publishing..." : "Publish";

        #endregion

        #region Properties
        public string Title 
        {
            get => _title; 
            set 
            { 
                _title = value; 
                RaisePropertyChanged(); 
                RaisePropertyChanged(nameof(CanPublish));
                (PublishCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            } 
        }

        public string Description 
        { 
            get => _description; 
            set 
            {
                if (_description == value) return;
                _description = value; 
                RaisePropertyChanged(); 
            } 
        }

        public string DemoLink 
        { 
            get => _demoLink; 
            set { _demoLink = value; RaisePropertyChanged(); } 
        }

        public ObservableCollection<string> ScriptTypes { get; } = 
            new ObservableCollection<string>  { "Revit", "Civil 3D" };

        public string SelectedScriptType 
        { 
            get => _selectedScriptType; 
            set { _selectedScriptType = value; RaisePropertyChanged(); } 
        }

        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();
        
        public bool IsPublic 
        { 
            get => _isPublic; 
            set { _isPublic = value; RaisePropertyChanged(); } 
        }

        public string ParsedJson 
        { 
            get => _parsedJson; 
            set 
            { 
                _parsedJson = value; 
                RaisePropertyChanged(); 
                RefreshState();
            } 
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
                    RefreshState();
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
                    RaisePropertyChanged(nameof(AnalyzeWorkspaceButtonText));
                    RefreshState();
                }
            } 
        }

        public string NewTag
        {
            get => _newTag;
            set { _newTag = value; RaisePropertyChanged(); }
        }

        public string UploadId
        {
            get => _uploadId;
            set 
            { 
                _uploadId = value; 
                RaisePropertyChanged(); 
            }
        }

        public string StoragePath
        {
            get => _storagePath;
            set 
            { 
                _storagePath = value;
                RaisePropertyChanged();
                RefreshState();
            }
        }

        public double PublishProgress
        {
            get => _publishProgress;
            set { _publishProgress = value; RaisePropertyChanged(); }
        }
        
        public ScriptSourceType SourceType
        {
            get => _sourceType;
            set
            {
                _sourceType = value; 
                RaisePropertyChanged();
                RefreshState();
            }
        }

        public bool HasOpenWorkspace => _scriptLoadService.HasOpenWorkspace();

        public string ShowFileName =>
            string.IsNullOrEmpty(_uploadedFilePath)
            ? "No file selected"
            : Path.GetFileName(_uploadedFilePath);

        // State
        public bool CanAnalyzeFile =>
            !IsAnalyzing &&
            SourceType == ScriptSourceType.File &&
            !string.IsNullOrEmpty(_uploadedFilePath);

        public bool CanAnalyzeWorkspace =>
            !IsAnalyzing &&
            HasOpenWorkspace;

        public bool CanPublish =>
            !IsPublishing &&
            !IsAnalyzing &&
            !string.IsNullOrWhiteSpace(Title) &&
            !string.IsNullOrWhiteSpace(ParsedJson) &&
            !string.IsNullOrWhiteSpace(StoragePath);

        #endregion

        #region Commands

        public ICommand UploadScriptCommand { get; }
        public ICommand AnalyzeScriptCommand { get; }
        public ICommand AnalyzeWorkspaceCommand { get; }
        public ICommand PublishCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }
        public ICommand SelectScriptTypeCommand { get; }

        #endregion

        #region Actions

        private void UploadScript()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Dynamo Script (*.dyn)|*.dyn"
            };

            if (dialog.ShowDialog() != true)
                return;

            _uploadedFilePath = dialog.FileName;
            SourceType = ScriptSourceType.File;

            RefreshState();
        }

        private async Task AnalyzeAsync()
        {
            if (!CanAnalyzeFile) return;

            IsAnalyzing = true;

            try
            {
                var result = await _scriptService.AnalyzeAsync(_uploadedFilePath);

                ApplyAnalyzeResult(result);
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(
                    ex.Message,
                    "Analyze failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsAnalyzing = false;
                RefreshState();
            }
        }

        private async Task AnalyzeWorkspace()
        {
            SourceType = ScriptSourceType.Workspace;
            _uploadedFilePath = string.Empty;
            RefreshState();

            IsAnalyzing = true;

            try
            {
                var result = await _scriptLoadService.AnalyzeWorkspaceAsync();

                ApplyAnalyzeResult(result);
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
                RefreshState();
            }

        }

        private async Task PublishAsync()
        {
            if (!CanPublish) 
                return;

            IsPublishing = true;
            PublishProgress = 0;

            try
            {
                var request = new ScriptPublishRequestDto
                {
                    StoragePath = StoragePath,
                    ParsedJson = ParsedJson,
                    Title = Title,
                    Description = Description,
                    ScriptType = SelectedScriptType == "Revit" ? "revit" : "civil3d",
                    Tags = Tags,
                    DemoLink = DemoLink,
                    IsPublic = IsPublic
                };

                var progress = new Progress<double>(p => PublishProgress = p * 100);                

                await _scriptService.PublishAsync(request);

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
                RefreshState();
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
            RefreshState();
        }

        private void RemoveTag(string tag)
        {
            if (tag == null)
                return;

            if (Tags.Contains(tag))
                Tags.Remove(tag);

            RaisePropertyChanged(nameof(Tags));
            RefreshState();
        }

        private void Cancel()
        {
            // Core fields
            _uploadedFilePath = string.Empty;
            SourceType = ScriptSourceType.None;

            Title = string.Empty;
            Description = string.Empty;
            DemoLink = string.Empty;
            SelectedScriptType = "Revit";
            IsPublic = false;

            // Analyze state
            ParsedJson = null;
            UploadId = string.Empty;
            StoragePath = string.Empty;
            NewTag = string.Empty;

            // Flags
            IsAnalyzing = false;
            IsPublishing = false;

            // Collections
            Tags.Clear();
            RaisePropertyChanged(nameof(Tags));

            RefreshState();
        }

        private void ApplyAnalyzeResult(ScriptAnalyzeResponseDto result)
        {
            ParsedJson = JsonConvert.SerializeObject(result.ScriptData);
            UploadId = result.UploadId;
            StoragePath = result.StoragePath;

            if (string.IsNullOrWhiteSpace(Title))
                Title = result.ScriptData?.Name ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Description))
                Description = result.ScriptData?.Description ?? string.Empty;

            Tags.Clear();
            RaisePropertyChanged(nameof(Tags));
        }

        private void RefreshState()
        {
            RaisePropertyChanged(nameof(ShowFileName));
            RaisePropertyChanged(nameof(HasOpenWorkspace));
            RaisePropertyChanged(nameof(CanAnalyzeFile));
            RaisePropertyChanged(nameof(CanAnalyzeWorkspace));
            RaisePropertyChanged(nameof(CanPublish));

            (AnalyzeScriptCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (AnalyzeWorkspaceCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (PublishCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
