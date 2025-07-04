using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class BookCategory
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
