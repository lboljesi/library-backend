using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class MemberCreateUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime MembershipDate { get; set; }

        public int BirthYear { get; set; }

    }
}
