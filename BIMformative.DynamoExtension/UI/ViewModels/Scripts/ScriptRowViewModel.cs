using BIMformative.DynamoExtension.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BIMformative.DynamoExtension.Infrastructure;
using System;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class ScriptRowViewModel : ViewModelBase
    {        
        private readonly ScriptDto _script;

        public ScriptRowViewModel(ScriptDto script, Func<ScriptRowViewModel, Task>? loadAction = null, ICommand? versionHistoryCommand = null)
        {
            _script = script ?? throw new ArgumentNullException(nameof(script));

            // Command for loading script
            LoadCommand = new RelayCommand(async () =>
            {
                if (!CanLoad || loadAction == null) return;

                try
                {
                    IsLoading = true;
                    await loadAction(this);
                    IsLoaded = true;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Failed to load script: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }

            });

            VersionHistoryCommand = versionHistoryCommand;
            ToggleExpandCommand = new RelayCommand<object>(_ => IsExpanded = !IsExpanded);
        }

        /* ------------- UI STATE -------------*/
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                if (SetProperty(ref _isLoaded, value))
                {
                    RaisePropertyChanged(nameof(LoadButtonText));
                    RaisePropertyChanged(nameof(CanLoad));
                }
                    
            }
        }

        private bool _isDeprecated;
        public bool IsDeprecated
        {
            get => _isDeprecated;
            set
            {
                if (SetProperty(ref _isDeprecated, value))
                    OnPropertyChanged(nameof(LoadButtonText));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    RaisePropertyChanged(nameof(CanLoad));
                }
            }
        }


        public bool CanLoad => !IsLoaded && !IsDeprecated && !IsLoading;

        public string LoadButtonText =>
            IsLoaded ? "Loaded" :
            IsDeprecated ? "Deprecated" :
            IsLoading ? "Loading..." :
            "Load";

        /* ------------- DISPLAY PROPERTIES -------------*/        
        public string Title => _script.Title;
        public string Description => _script.Description;
        public string OwnerFullName => $"{_script.Owner_First_Name} {_script.Owner_Last_Name}";
        public int LikesCount => _script.Likes_Count;
        public int DownloadsCount => _script.Downloads_Count;
        public string ScriptType => Utils.Utils.ToTitleCase(_script.Script_Type);
        public int CurrentVersionNumber => _script.Current_Version_Number;
        public DateTime UpdatedAt => _script.Updated_At;
        public string UpdatedAtDisplay => UpdatedAt.ToString("dd/MMM/yyyy");

        public string ScriptLabel => $"V{_script.Current_Version_Number.ToString()}";

        /* ------------- COMMANDS -------------*/
        public ICommand LoadCommand { get; }
        public ICommand VersionHistoryCommand { get; }
        public ICommand ToggleExpandCommand { get; }

        public void MarkAsLoaded()
        {
            IsLoaded = true;
            IsLoading = false;
        }

        public ScriptDto GetDto()
        {
            return _script; 
        }
    }
}
