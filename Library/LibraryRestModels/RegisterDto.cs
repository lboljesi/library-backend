using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRestModels
{
    public class RegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
    }
}
