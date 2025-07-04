using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryQuerying
{
    public class AuthorsQuery
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string SortBy { get; set; } = "LastName"; 
        public bool SortDesc { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
