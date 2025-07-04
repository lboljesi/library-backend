using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class LoanREST
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public DateTime MustReturn { get; set; }
        public Books Book { get; set; }
    }

}
