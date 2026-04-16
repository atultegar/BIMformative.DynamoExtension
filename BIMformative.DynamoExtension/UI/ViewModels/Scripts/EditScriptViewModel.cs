using BIMformative.Core.Interfaces;
using Models = BIMformative.Core.Models;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BIMformative.Core.Models.Scripts;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public sealed class EditScriptViewModel : ScriptDetailsViewModel
    {
        private readonly IScriptService _scriptService;
        private readonly IDialogService _dialogService;
        private readonly IScriptLoadService _scriptLoadService;

        private bool _isInitialized;

        public IEnumerable<Models.ScriptType> ScriptTypes =>
            Enum.GetValues(typeof(Models.ScriptType)).Cast<Models.ScriptType>();

        public EditScriptViewModel(
            string slug,
            IScriptService scriptService,
            IScriptLoadService loader,
            IScriptCompareService compareService,
            IDialogService dialogService)
            : base(slug, scriptService, loader, compareService)
        {            
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _scriptLoadService = loader ?? throw new ArgumentNullException(nameof(loader));

            SaveMetadataCommand = new AsyncRelayCommand(SaveMetadataAsync);
            UploadVersionCommand = new AsyncRelayCommand(UploadVersionAsync);
            DeleteScriptCommand = new AsyncRelayCommand(DeleteScriptAsync);

            AddTagCommand = new RelayCommand(AddTag);
            RemoveTagCommand = new RelayCommand<string>(RemoveTag);

            CloseCommand = new RelayCommand(Close);
        }

        #region EditableFields

        private string _editTitle;
        public string EditTitle
        {
            get => _editTitle;
            set => SetProperty(ref _editTitle, value);
        }

        private string _editDescription;
        public string EditDescription
        {
            get => _editDescription;
            set => SetProperty(ref _editDescription, value);
        }

        private Models.ScriptType _editScriptType;
        public Models.ScriptType EditScriptType
        {
            get => _editScriptType;
            set => SetProperty(ref _editScriptType, value);
        }

        private bool _isPublic;
        public bool IsPublic
        {
            get => _isPublic;
            set => SetProperty(ref _isPublic, value);
        }

        private ObservableCollection<string> _editTags = new();
        public ObservableCollection<string> EditTags
        {
            get => _editTags; 
            set => SetProperty(ref _editTags, value);
        }

        private string _newTag;
        public string NewTag
        {
            get => _newTag;
            set => SetProperty(ref _newTag, value);
        }

        private string _editVersion;
        public string EditVersion
        {
            get => _editVersion;
            set => SetProperty(ref _editVersion, value);
        }

        #endregion

        #region Commands

        public AsyncRelayCommand SaveMetadataCommand { get; }
        public AsyncRelayCommand UploadVersionCommand { get; }
        public AsyncRelayCommand DeleteScriptCommand { get; }

        public RelayCommand AddTagCommand { get; }
        public RelayCommand<string> RemoveTagCommand { get; }

        public RelayCommand CloseCommand { get; }

        #endregion

        #region Tag Logic

        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTag))
                return;

            var tag = NewTag.Trim();

            if (!EditTags.Contains(tag))
                EditTags.Add(tag);

            NewTag = string.Empty;
        }

        private void RemoveTag(string tag)
        {
            if (EditTags.Contains(tag))
                EditTags.Remove(tag);
        }

        #endregion

        #region Initialization

        protected override void OnDetailsChanged()
        {
            base.OnDetailsChanged();

            if (_isInitialized)
                return;

            EditTitle = Title;
            EditDescription = Description;
            IsPublic = Details?.Is_Public ?? false;

            EditTags = new ObservableCollection<string>(Tags ?? new List<string>());

            EditVersion = Details?.Current_Version_Number.ToString();

            if (Enum.TryParse<Models.ScriptType>(Details?.Script_Type, out var parsedType))
                EditScriptType = parsedType;
            else
                EditScriptType = Models.ScriptType.Revit;

                _isInitialized = true;
        }

        #endregion

        #region Commands Implementation

        private async Task SaveMetadataAsync()
        {
            var payload = new ScriptUpdateRequest
            {
                Title = EditTitle,
                Description = EditDescription,
                Script_Type = EditScriptType.ToString().ToLower(),
                Tags = EditTags.ToList(),
                Current_Version = EditVersion,
                Is_Public = IsPublic
            };

            await _scriptService.UpdateScriptMetadataAsync(Details.Slug, payload);

            await InitializeAsync(); // refresh UI
        }

        private async Task UploadVersionAsync()
        {
            var vm = new UploadVersionViewModel(Details.Slug, _scriptService, _scriptLoadService);

            var dialog = new UploadVersionDialog
            {
                DataContext = vm,
            };

            // Close dialog from VM
            vm.RequestClose += () => dialog.Close();

            _dialogService.ShowDialog(dialog);

            // Refresh versions after dialog closes
            await LoadVersionCommand.ExecuteAsync();
        }

        private async Task DeleteScriptAsync()
        {
            await _scriptService.DeleteAsync(Details.Slug);
        }

        private void Close()
        {
            Application.Current.Windows
                .OfType<Window>()
                .SingleOrDefault(w => w.IsActive)?
                .Close();
        }

        public async Task InitializeEditAsync()
        {
            await InitializeAsync();
        }

        #endregion
    }
}
