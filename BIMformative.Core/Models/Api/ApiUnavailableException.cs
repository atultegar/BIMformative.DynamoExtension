using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.Core.Models.Api
{
    public class ApiUnavailableException : Exception
    {
        public ApiUnavailableException(string message, Exception inner) 
        : base(message, inner) { }
    }

}
