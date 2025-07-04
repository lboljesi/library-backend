using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class LoanResult
    {
        public List<Loan> Loans { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
