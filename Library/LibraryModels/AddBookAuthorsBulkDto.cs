using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class AddBookAuthorsBulkDto
    {
        public Guid BookId { get; set; }
        public List<Guid> AuthorIds { get; set; } = new List<Guid>();
    }
}
