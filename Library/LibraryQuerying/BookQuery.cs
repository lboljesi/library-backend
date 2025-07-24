using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryQuerying
{
    public class BookQuery : SortablePaginationQuery
    {
        public int? PublishedYear { get; set; }
    }
}
