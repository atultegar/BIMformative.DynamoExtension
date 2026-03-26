using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Api
{
    public class ApiException : Exception
    {
        public string Code { get; }

        public ApiException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}
