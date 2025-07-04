using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class AddCategoriesToBookDto
    {
        public Guid BookId { get; set; }
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    }
}
