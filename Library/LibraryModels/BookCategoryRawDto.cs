using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class BookCategoryRawDto
    {
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = null!;
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid? RelationId { get; set; }
    }
}
