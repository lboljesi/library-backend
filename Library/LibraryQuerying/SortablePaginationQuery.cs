using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryQuerying
{
    public class SortablePaginationQuery : PaginationQuery
    {
        public string? SortBy { get; set; }
    }
}
