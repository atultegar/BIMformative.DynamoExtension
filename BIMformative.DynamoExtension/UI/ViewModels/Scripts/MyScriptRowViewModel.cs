using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models.Api;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Script;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.Utils;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class MyScriptRowViewModel : ViewModelBase
    {
        private readonly MyScriptDto _myScript;
        private readonly IScriptService _scriptService;

        private readonly Action<MyScriptRowViewModel>? _viewDetailsAction;
        private readonly Func<MyScriptRowViewModel, Task>? _editAction;
        private readonly Action<MyScriptRowViewModel>? _deletedCallback;
        private readonly Func<MyScriptRowViewModel, Task>? _loadAction;
        private readonly Action<MyScriptRowViewModel>? _uploadVersionAction;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                    RaisePropertyChanged(nameof(CanLoad));
            }
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                if (SetProperty(ref _isLoaded, value))
                {
                    RaisePropertyChanged(nameof(CanLoad));
                }
            }
        }

        public bool CanLoad => !IsLoaded && !IsLoading;

        public ICommand ViewDetailsCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand EditScriptCommand { get; }
        public ICommand DeleteScriptCommand { get; }
        public ICommand UploadVersionCommand { get; }
        public ICommand ChangeVisibilityCommand { get; }

        public MyScriptRowViewModel(
            MyScriptDto myScript, 
            IScriptService scriptService,
            Action<MyScriptRowViewModel>? viewDetailsAction = null,
            Func<MyScriptRowViewModel, Task>? editAction = null,
            Func<MyScriptRowViewModel, Task>? loadAction = null,
            Action<MyScriptRowViewModel>? deletedCallback = null,
            Action<MyScriptRowViewModel>? uploadVersionAction = null)
        {
            _myScript = myScript ?? throw new ArgumentNullException(nameof(myScript));
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(_scriptService));

            _viewDetailsAction = viewDetailsAction;
            _editAction = editAction;
            _loadAction = loadAction;
            _deletedCallback = deletedCallback;
            _uploadVersionAction = uploadVersionAction;

            ViewDetailsCommand = new RelayCommand(() => _viewDetailsAction?.Invoke(this));

            EditScriptCommand = new AsyncRelayCommand(async () =>
            {
                if (_editAction != null)
                    await _editAction?.Invoke(this);
            });

            LoadCommand = new AsyncRelayCommand(async () =>
            {
                if (_loadAction != null)
                    await _loadAction.Invoke(this);
            });

            DeleteScriptCommand = new AsyncRelayCommand(DeleteAsync);
            UploadVersionCommand = new RelayCommand(() => 
                _uploadVersionAction?.Invoke(this));

            ChangeVisibilityCommand = new AsyncRelayCommand(ChangeVisibilityAsync);
        }

        private async Task ChangeVisibilityAsync()
        {
            try
            {
                IsLoading = true;

                var newVisibility = !_myScript.Is_Public;

                await _scriptService.UpdateScriptVisibilityAsync(Slug, !_myScript.Is_Public);

                _myScript.Is_Public = newVisibility;

                RaisePropertyChanged(nameof(IsPublic));
                RaisePropertyChanged(nameof(MakeButtonText));
            }
            catch (ApiException ex)
            {
                MessageBoxService.Show(
                    ex.Message,
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(
                    $"Unexpected error:\n{ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAsync()
        {
            var confirm = MessageBoxService.Show(
                $"Delete script '{Title}'?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            await _scriptService.DeleteAsync(_myScript.Slug);

            _deletedCallback?.Invoke(this);
        }

        // DISPLAY PROPERTIES
        public string Title => _myScript.Title;
        public string Slug => _myScript.Slug;
        public string Description => _myScript.Description;
        public string ScriptType => _myScript.Script_Type == "revit" ? "Revit" : _myScript.Script_Type == "civil3d" ? "Civil 3D" : "";
        public string CurrentVersion => $"V{_myScript.Current_Version_Number}";
        public int DownloadsCount => _myScript.Downloads_Count;
        public int LikesCount => _myScript.Likes_Count;
        public string IsPublic => _myScript.Is_Public ? "Public" : "Private";
        public IReadOnlyList<string> Tags => _myScript.Tags;
        public string UpdatedAt => TimeAgoHelper.Format(_myScript.Updated_At);

        public string MakeButtonText => _myScript.Is_Public ? "Make Private" : "Make Public";

        public MyScriptDto GetDto() => _myScript;

        public void MarkAsLoaded()
        {
            IsLoaded = true;
            IsLoading = false;
        }

        public ScriptDto GetScriptDto()
        {
            return new ScriptDto
            {
                Id = _myScript.Id,
                Owner_Id = _myScript.Owner_Id,
                Title = _myScript.Title,
                Slug = _myScript.Slug,
                Description = _myScript.Description,
                Script_Type = _myScript.Script_Type,
                Current_Version_Number = _myScript.Current_Version_Number,
                Owner_Avatar_Url = "",
                Demo_Link = _myScript.Demo_Link,
                Downloads_Count = _myScript.Downloads_Count,
                Likes_Count = _myScript.Likes_Count,
                Updated_At = _myScript.Updated_At
            };
        }
    }
}
