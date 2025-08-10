using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class MemberDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // Fix: Initialize with a default value to avoid null  

        public DateTime MembershipDate { get; set; }
        public int BirthYear { get; set; }
    }
}
