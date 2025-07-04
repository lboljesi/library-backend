using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRestModels
{
    public class BooksCreateUpdateREST
    {
        
        public string Title { get; set; }

        public string Isbn { get; set; }

        public int PublishedYear { get; set; }

        public int Price { get; set; }
    }
}
