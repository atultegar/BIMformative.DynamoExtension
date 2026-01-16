using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Contracts.Common
{
    public record PagedResult<T>(
        IReadOnlyList<T> Data,
        int Page,
        int Limit,
        int Total,
        int TotalPages);

    //public List<T> Data { get; set; }

    //public int Page { get; set; }
    //public int Limit { get; set; }
    //public int Total { get; set; }
    //public int TotalPages { get; set; }
}
