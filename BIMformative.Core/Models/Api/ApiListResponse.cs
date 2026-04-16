using System.Collections.Generic;

namespace BIMformative.Core.Models.Api
{
    public class ApiListResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
    }
}
