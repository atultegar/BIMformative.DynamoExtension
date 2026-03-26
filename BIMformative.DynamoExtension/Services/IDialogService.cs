using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BIMformative.DynamoExtension.Services
{
    public interface IDialogService
    {
        bool? ShowDialog(Window dialog);
        void Show(Window dialog);
    }
}
