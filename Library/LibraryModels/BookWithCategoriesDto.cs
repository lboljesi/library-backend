using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class BookWithCategoriesDto
    {
        public Guid BookId { get; set; }
        public required string BookTitle { get; set; } 
        public List<CategoryWithRelation> Categories { get; set; } = new();
    }
}
