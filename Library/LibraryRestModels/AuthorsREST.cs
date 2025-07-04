using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRestModels
{
    public class AuthorsREST
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string BookTitles { get; set; }


    }
}
