using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class CategoryWithBooks
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Books> Books { get; set; } = new();
    }
}
