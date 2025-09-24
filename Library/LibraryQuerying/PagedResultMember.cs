using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryQuerying
{
    public record PagedResultMember<T>(IEnumerable<T> Items, int TotalCount);
}
