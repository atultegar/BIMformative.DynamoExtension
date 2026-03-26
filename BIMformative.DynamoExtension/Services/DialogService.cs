using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BIMformative.DynamoExtension.Services
{
    public class DialogService : IDialogService
    {
        private readonly Window _owner;

        public DialogService(Window owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public bool? ShowDialog(Window dialog)
        {
            if (dialog == null) 
                throw new ArgumentNullException(nameof(dialog));

            return Application.Current.Dispatcher.Invoke(() =>
            {
                dialog.Owner = GetSafeOwner();
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                return dialog.ShowDialog();
            });
        }

        public void Show(Window dialog)
        {
            if (dialog == null)
                throw new ArgumentNullException(nameof(dialog));

            Application.Current.Dispatcher.Invoke(() =>
            {
                dialog.Owner = GetSafeOwner();
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.Show();
            });
        }

        private Window GetSafeOwner()
        {
            if (_owner != null && _owner.IsLoaded)
                return _owner;

            // fallback
            return Application.Current.MainWindow;
        }
    }
}
