using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class CreateLoanDto
    {
      
        public Guid BookId { get; set; }

        [Required]
        public Guid MemberId { get; set; }

        [Required]
        public DateTime LoanDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        [Required]
        public DateTime MustReturn { get; set; }
    }

}
