using BIMformative.Core.Interfaces;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Services.Settings;
using BIMformative.DynamoExtension.UI.Views.Controls;
using Dynamo.UI;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public sealed class SettingsTabViewModel : TabItemViewModel
    {
        private readonly ISettingsService _settings;
        public SettingsTabViewModel(ISettingsService settings)
            : base(
                  header: "Settings",
                  contentFactory: () =>
                  {
                      var control = new SettingsControl();
                      return control;                      
                  })

        {
            _settings = settings;

            BrowsePathCommand = new RelayCommand(BrowsePath);
            ResetPathCommand = new RelayCommand(ResetPath);

            // Set DataContext after base constructor
            if (Content is SettingsControl control)
            {
                control.DataContext = this;
            }
        }

        public string DefaultScriptSavePath
        {
            get => _settings.Current.DefaultScriptSavePath!;
            set
            {
                if (_settings.Current.DefaultScriptSavePath != value)
                {
                    _settings.Current.DefaultScriptSavePath = value;
                    OnPropertyChanged(nameof(DefaultScriptSavePath));
                    _settings.Save();
                }
            }
        }

        public ICommand BrowsePathCommand { get; }
        public ICommand ResetPathCommand { get; }

        private void BrowsePath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(dialog.SelectedPath))
            {
                DefaultScriptSavePath = dialog.SelectedPath;
            }
        }

        private void ResetPath()
        {
            _settings.Reset();
            RaisePropertyChanged(nameof(DefaultScriptSavePath));
        }

        public bool AskBeforeOverwrite
        {
            get => _settings.Current.AskBeforeOverwrite;
            set
            {
                if (_settings.Current.AskBeforeOverwrite != value)
                {
                    _settings.Current.AskBeforeOverwrite = value;
                    RaisePropertyChanged(nameof(AskBeforeOverwrite));
                    _settings.Save();
                }
            }
        }
    }
}
