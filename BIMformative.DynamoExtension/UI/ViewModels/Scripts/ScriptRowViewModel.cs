using BIMformative.DynamoExtension.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BIMformative.DynamoExtension.Infrastructure;
using System;
using BIMformative.DynamoExtension.UI.ViewModels.Base;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class ScriptRowViewModel : ViewModelBase
    {        
        private readonly ScriptDto _script;

        public ScriptRowViewModel(
            ScriptDto script,
            ICommand downloadCommand,
            ICommand versionHistoryCommand)
        {
            _script = script ?? throw new ArgumentNullException(nameof(script));

            DownloadCommand = new RelayCommand<object>(_ => downloadCommand?.Execute(this));
            VersionHistoryCommand = new RelayCommand<object>(_ => versionHistoryCommand?.Execute(this));

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
                    OnPropertyChanged(nameof(LoadButtonText));
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


        public bool CanLoad => !IsLoaded && !IsDeprecated;

        public string LoadButtonText =>
            IsLoaded ? "Loaded" :
            IsDeprecated ? "Deprecated" :
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
        public ICommand DownloadCommand { get; }
        public ICommand VersionHistoryCommand { get; }
        public ICommand ToggleExpandCommand { get; }

        public void MarkAsLoaded()
        {
            IsLoaded = true;

            RaisePropertyChanged(nameof(CanLoad));
            RaisePropertyChanged(nameof(LoadButtonText));
        }
    }
}
