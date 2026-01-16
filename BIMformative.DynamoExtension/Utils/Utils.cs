using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Utils
{
    public class Utils
    {
        public static string ToTitleCase(string text)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

            if (ti != null)
            {
                return ti.ToTitleCase(text);
            }
            return text;
        }
    }
}
