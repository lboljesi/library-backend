using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class BooksPagedDto
    {
        public List<BookDto> Books { get; set; } = new List<BookDto>();
        public int TotalCount { get; set; }
    }
}
