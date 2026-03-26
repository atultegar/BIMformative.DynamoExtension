using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Api
{
    public class ApiListResponse<T>
    {
        public List<T> Data { get; set; } = new();
    }
}
