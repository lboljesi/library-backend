using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryQuerying
{
    public class LoanDto
    {
        public Guid Id { get; set; }
        public Guid BookId { get; set; }
        public Guid MemberId { get; set; }

        public DateTime LoanDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public DateTime MustReturn { get; set; }

        public string BookTitle { get; set; } = "";
        public string Isbn { get; set; } = "";
        public bool IsActive => ReturnedDate == null;
        public bool IsOverdue => ReturnedDate == null && DateTime.UtcNow.Date > MustReturn.Date;

    }
}
