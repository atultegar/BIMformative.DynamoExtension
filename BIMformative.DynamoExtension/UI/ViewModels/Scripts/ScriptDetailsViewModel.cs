using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Exceptions;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Script;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class ScriptDetailsViewModel : ViewModelBase
    {
        private readonly IScriptLoadService _loader;
        private readonly IScriptService _scriptService;
        private readonly IScriptCompareService _compareService;
        private readonly string _slug;

        private readonly List<ScriptVersionRowViewModel> _selectedVersions = new();

        private ScriptDetailsDto _details;
        public ScriptDetailsDto Details
        {
            get => _details;
            private set
            {
                if (SetProperty(ref _details, value))
                {
                    OnDetailsChanged();
                }
            }
        }

        private bool _hasLiked;
        public bool HasLiked
        {
            get => _hasLiked;
            set => SetProperty(ref _hasLiked, value);
        }

        public ObservableCollection<ScriptVersionRowViewModel> Versions { get; } = new();

        private ViewState _currentState;
        public ViewState CurrentState
        {
            get => _currentState;
            set => SetProperty(ref _currentState, value);
        }

        public void UpdateSelectedVersions(List<ScriptVersionRowViewModel> selected)
        {
            _selectedVersions.Clear();
            _selectedVersions.AddRange(selected);

            OnPropertyChanged(nameof(CanCompare));
            CompareVersionCommand?.RaiseCanExecuteChanged();
        }

        public bool CanCompare => _selectedVersions.Count == 2;

        //COMMANDS
        public ICommand LoadCommand { get; }
        public ICommand LoadDetailsCommand { get; }
        public AsyncRelayCommand LoadVersionCommand { get; }

        public ICommand VersionHistoryCommand { get; }
        public ICommand LikeCommand { get; }
        public ICommand CloseCommand { get; }

        public AsyncRelayCommand CompareVersionCommand { get; }

        public bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            private set => SetProperty(ref _isLoaded, value);
        }

        

        public ScriptDetailsViewModel(
            string slug,
            IScriptService scriptService,
            IScriptLoadService loader,
            IScriptCompareService compareService)
        {
            _slug = slug ?? throw new ArgumentNullException(nameof(slug));
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _compareService = compareService ?? throw new ArgumentNullException(nameof(compareService));

            LoadDetailsCommand = new AsyncRelayCommand(LoadDetailsAsync);
            LoadVersionCommand = new AsyncRelayCommand(LoadVersionsAsync);
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            LikeCommand = new AsyncRelayCommand(ToggleLikeAsync);
            CompareVersionCommand = new AsyncRelayCommand(CompareAsync, () => _selectedVersions.Count == 2);
        }

        private async Task CompareAsync()
        {
            if (_selectedVersions.Count != 2)
                return;

            int ver0 = int.Parse(_selectedVersions[0].VersionNumber.TrimStart('V'));
            int ver1 = int.Parse(_selectedVersions[1].VersionNumber.TrimStart('V'));

            var v1 = Math.Min(ver0, ver1);
            var v2 = Math.Max(ver0, ver1);

            await _compareService.OpenVersionCompareAsync(_slug, v1, v2);
        }

        private async Task ToggleLikeAsync()
        {
            if (Details == null)
                return;

            try
            {
                if (HasLiked)
                    await _scriptService.UnlikeAsync(_slug);
                else
                    await _scriptService.LikeAsync(_slug);

                HasLiked = !HasLiked;

                // Optimistic UI update
                if (HasLiked)
                    Details.Likes_Count++;
                else
                    Details.Likes_Count--;

                OnPropertyChanged(nameof(LikesCount));
            }
            catch 
            {
                // optional : revert UI
            }
        }

        public async Task InitializeAsync()
        {
            await LoadDetailsAsync();
            await LoadVersionsAsync();
        }

        private async Task LoadDetailsAsync()
        {
            try
            {
                CurrentState = ViewState.Loading;

                // Run in parallel
                var detailsTask = _scriptService.GetBySlugAsync(_slug, CancellationToken.None);

                var hasLikedTask = _scriptService.HasLiked(_slug, CancellationToken.None);

                await Task.WhenAll(detailsTask, hasLikedTask);

                var results = await detailsTask;
                HasLiked = await hasLikedTask;

                if (results == null)
                {
                    CurrentState = ViewState.Empty;
                    return;
                }

                Details = results;
                CurrentState = ViewState.Loaded;
            }
            catch (UnauthorizedAccessException)
            {
                CurrentState = ViewState.NotAuthenticated;
            }
            catch (OperationCanceledException)
            {
                CurrentState = ViewState.Error;
            }
            catch (ApiUnavailableException)
            {
                CurrentState = ViewState.ApiUnavailable;
            }
            catch
            {
                CurrentState = ViewState.Error;
            }
        }

        private async Task LoadVersionsAsync()
        {
            try
            {
                var versions = await _scriptService.GetVersionsAsync(_slug);

                Versions.Clear();

                foreach (var v in versions)
                    Versions.Add(new ScriptVersionRowViewModel(v, this));
            }
            catch
            {
                // optional : log
            }            
        }

        private async Task LoadAsync()
        {
            if (Details == null)
                return;

            await _loader.LoadScriptAsync(ToScriptDto(Details), CancellationToken.None);
        }

        public async Task SetCurrentVersionAsync(int versionNumber)
        {
            try
            {
                var res = await _scriptService.SetCurrentVersionAsync(Details.Slug, versionNumber, CancellationToken.None);

                MessageBoxService.Show(res.Message, "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (ApiException ex)
            {
                MessageBoxService.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }            

            await LoadVersionsAsync();
        }

        public async Task DeleteVersionAsync(int versionNumber)
        {
            if (MessageBoxService.Show(
                "Delete this version?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            await _scriptService.DeleteVersionAsync(Details.Slug, versionNumber);

            await LoadVersionsAsync();
        }

        private static ScriptDto ToScriptDto(ScriptDetailsDto d)
        {
            return new ScriptDto
            {
                Id = d.Id,
                Owner_Id = d.Owner_Id,
                Title = d.Title,
                Slug = d.Slug,
                Description = d.Description,
                Script_Type = d.Script_Type,
                Current_Version_Number = d.Current_Version_Number,
                Owner_First_Name = d.Owner_First_Name,
                Owner_Last_Name = d.Owner_Last_Name,
                Owner_Avatar_Url = d.Owner_Avatar_Url,
                Demo_Link = d.Demo_Link,
                Downloads_Count = d.Downloads_Count,
                Likes_Count = d.Likes_Count,
                Updated_At = d.Updated_At
            };
        }

        protected virtual void OnDetailsChanged()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(OwnerFullName));
            OnPropertyChanged(nameof(Owner_Avatar_Url));
            OnPropertyChanged(nameof(DemoLink));
            OnPropertyChanged(nameof(DownloadsCount));
            OnPropertyChanged(nameof(LikesCount));
            OnPropertyChanged(nameof(ScriptType));
            OnPropertyChanged(nameof(CurrentVersionNumber));
            OnPropertyChanged(nameof(DynamoVersion));
            OnPropertyChanged(nameof(IsPlayerReady));
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(ExternalPackages));
            OnPropertyChanged(nameof(UpdatedAt));
        }

        // SAFE UI PROPERTIES
        public string Title => Details?.Title ?? string.Empty;
        public string Description => Details?.Description ?? string.Empty;
        public string OwnerFullName => Details?.OwnerFullName ?? string.Empty;
        public string? Owner_Avatar_Url => Details?.Owner_Avatar_Url;

        public string? DemoLink => Details?.Demo_Link;
        public string DownloadsCount => Details?.Downloads_Count.ToString() ?? "0";
        public string LikesCount => Details?.Likes_Count.ToString() ?? "0";
        public string ScriptType => Details?.Script_Type == "civil3d" ? "Civil 3D" : "Revit";
        public string CurrentVersionNumber => Details != null ? $"V{Details.Current_Version_Number}" : string.Empty;
        public string DynamoVersion => Details?.Dynamo_Version ?? string.Empty;
        public bool IsPlayerReady => Details?.Is_Player_Ready ?? false;
        public IReadOnlyList<string> Tags => Details?.Tags ?? Array.Empty<string>();
        public IReadOnlyList<string> ExternalPackages => Details?.External_Packages ?? Array.Empty<string>();
        public string UpdatedAt => Details?.Updated_At.ToString("dd MMM yyyy") ?? string.Empty;        
    }
}
