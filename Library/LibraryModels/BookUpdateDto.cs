using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class BookUpdateDto
    {
        public required string Title { get; set; }
        public required string Isbn { get; set; }  
        public required int PublishedYear { get; set; }
        public int? Price { get; set; }
    }
}
