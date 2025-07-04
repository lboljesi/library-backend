using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class MemberREST
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime MembershipDate { get; set; }
        public int BirthYear { get; set; }
        public List<LoanREST> Loans { get; set; }
    }
}
