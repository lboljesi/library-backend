using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryQuerying
{
    public class BooksQuery
    {

        public string? Title { get; set; }
        public int? PublishedYear { get; set; }

        public string SortBy { get; set; } = "Title";
        public bool SortDesc { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
