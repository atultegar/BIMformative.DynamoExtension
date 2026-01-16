using BIMformative.DynamoExtension.UI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BIMformative.DynamoExtension.UI.ViewModels
{
    public class TabItemViewModel : ViewModelBase
    {
        private readonly Func<UserControl> _contentFactory;
        private UserControl? _content;
        public TabItemViewModel(string header, Func<UserControl> contentFactory)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            _contentFactory = contentFactory ?? throw new ArgumentNullException(nameof(contentFactory));
        }

        // Header
        private string _header;
        public string Header
        {
            get => _header;
            set => SetProperty(ref  _header, value);
        }

        // Content
        
        /// <summary>
        /// UserControl shown inside the tab
        /// </summary>
        public UserControl Content
        {
            get
            {
                if (_content == null)
                {
                    _content = _contentFactory();
                }
                return _content;
            }
        }                

        // UI State
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        // Optional (future)
        public string? IconKey { get; }
        public int? BadgeCount { get; }
    }
}
