using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class MyScriptRowViewModel : ViewModelBase
    {
        private readonly MyScriptDto _myScript;

        public ICommand ViewDetailsCommand { get; }

        public MyScriptRowViewModel(MyScriptDto myScript, Action<MyScriptRowViewModel> viewDetailsAction)
        {
            _myScript = myScript ?? throw new ArgumentNullException(nameof(myScript));

            ViewDetailsCommand = new RelayCommand(() => viewDetailsAction?.Invoke(this));
        }

        // DISPLAY PROPERTIES
        public string Title => _myScript.Title;
        public string Slug => _myScript.Slug;
        public string Description => _myScript.Description;
        public string ScriptType => _myScript.Script_Type == "revit" ? "Revit" : _myScript.Script_Type == "civil3d" ? "Civil 3D" : "";
        public string CurrentVersion => $"V{_myScript.Current_Version_Number.ToString()}";
        public int DownloadsCount => _myScript.Downloads_Count;
        public int LikesCount => _myScript.Likes_Count;
        public string IsPublic => _myScript.Is_Public ? "Public" : "Private";
        public IReadOnlyList<string> Tags => _myScript.Tags;
        public string UpdatedAt => TimeAgoHelper.Format(_myScript.Updated_At);

        public string MakeButtonText => _myScript.Is_Public ? "Make Private" : "Make Public";

        public MyScriptDto GetDto() => _myScript;
    }
}
