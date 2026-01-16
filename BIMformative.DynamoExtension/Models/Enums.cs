using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models
{
    public enum ScriptSortField
    {
        title,
        updated_at,
        downloads_count,
        likes_count,
        owner_first_name
    }

    public enum SortOrder
    {
        asc,
        desc
    }
}
