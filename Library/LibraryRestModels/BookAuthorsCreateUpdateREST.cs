using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRestModels
{
    public class BookAuthorsCreateUpdateREST
    {

        public Guid? BookId { get; set; }
        public Guid? AuthorId { get; set; }
    }
}
