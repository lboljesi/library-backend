using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class LoanUpdateDTO
    {
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public DateTime MustReturn { get; set; }
    }
}
