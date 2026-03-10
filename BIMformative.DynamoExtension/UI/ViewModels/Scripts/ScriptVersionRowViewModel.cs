using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class ScriptVersionRowViewModel : ViewModelBase
    {
        private readonly ScriptVersionDto _dto;

        public ScriptVersionRowViewModel(ScriptVersionDto dto)
        {
            _dto = dto;
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

        // Display Properties
        public string VersionNumber => $"V{_dto.Version_Number}";
        public string? ChangeLog => _dto.Changelog;
        public string UpdatedAt => _dto.Updated_At.ToString("dd MMM yyyy");
        public string IsCurrent => _dto.Is_Current ? "\uEA3B" : "";
    }
}
