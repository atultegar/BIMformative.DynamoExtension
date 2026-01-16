using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models
{
    public class ApiResponse<T>
    {
        public List<T> Data { get; set; } = new();
    }
}
