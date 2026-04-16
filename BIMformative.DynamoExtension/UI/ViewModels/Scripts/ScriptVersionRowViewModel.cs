using BIMformative.Core.Models.Scripts;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using System;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class ScriptVersionRowViewModel : ViewModelBase
    {
        private readonly ScriptVersionDto _dto;
        private readonly ScriptDetailsViewModel _parent;

        public ScriptVersionRowViewModel(ScriptVersionDto dto, ScriptDetailsViewModel parent)
        {
            _dto = dto;
            _parent = parent;

            SetCurrentCommand = new AsyncRelayCommand(SetCurrentAsync, () => !dto.Is_Current);
            DeleteVersionCommand = new AsyncRelayCommand(DeleteVersionAsync);
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler? SelectionChanged;

        // Commands
        public AsyncRelayCommand SetCurrentCommand { get; }
        public AsyncRelayCommand DeleteVersionCommand { get; }

        private async Task SetCurrentAsync()
        {
            await _parent.SetCurrentVersionAsync(_dto.Version_Number);
        }

        private async Task DeleteVersionAsync()
        {
            await _parent.DeleteVersionAsync(_dto.Version_Number);
        }

        // Display Properties
        public string VersionNumber => $"V{_dto.Version_Number}";
        public string? ChangeLog => _dto.Changelog;
        public string UpdatedAt => _dto.Updated_At.ToString("dd MMM yyyy");
        public string IsCurrent => _dto.Is_Current ? "\uEA3B" : "";

        public bool SetCurrentEnabled => !_dto.Is_Current;
    }
}
