using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class BookCategoryJOIN
    {
        public Guid Id { get; set; }
        public string BookTitle { get; set; } = string.Empty; // Fix: Initialize with a default value
        public string CategoryName { get; set; } = string.Empty; // Fix: Initialize with a default value
    }
}
