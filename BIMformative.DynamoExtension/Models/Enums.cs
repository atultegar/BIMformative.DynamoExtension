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

    public enum ScriptSyncStatus
    {
        Downloaded = 0,     
        UpToDate = 1,           
        ModifiedLocally = 2,    
        UpdateAvailable = 3,          
        Conflict = 4,
        NotTracked = 5,
        MissingFile = 6
    }

    public enum ScriptSourceType
    {
        None = 0,
        File = 1,
        Workspace = 2
    }

    public enum ViewState
    {
        Loading,
        Empty,
        Error,
        ApiUnavailable,
        NotAuthenticated,
        Loaded
    }

    public enum ScriptSortBy
    {
        Title,
        UpdatedAt,
        Downloads,
        Likes,
        Author
    }

    public enum ScriptSortOrder
    {
        Ascending, 
        Descending 
    }
}
