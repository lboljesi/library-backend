using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class BookWithAuthorsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public int Price { get; set; }
        public List<AuthorDto> Authors { get; set; } = new();
    }
}
