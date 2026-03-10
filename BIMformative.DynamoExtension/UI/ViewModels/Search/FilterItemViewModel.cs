using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Search
{
    public class FilterItemViewModel : ViewModelBase
    {
        public string Name { get; }
        public string Header { get; }

		private bool _isSelected;

		public bool IsSelected
		{
			get => _isSelected;
			set 
            {
                if (SetProperty(ref _isSelected, value))
                    FilterChanged?.Invoke(this);
            }
		}

        public event Action<FilterItemViewModel> FilterChanged; 

        public FilterItemViewModel(string name, string header)
        {
            Name = name;
            Header = header;
        }
    }
}
