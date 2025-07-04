using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryModels
{
    public class DeleteRelationIdList
    {
        [Required(ErrorMessage ="RelationIds are required.")]
        [MinLength(1, ErrorMessage ="At least one ID must be provided.")]
        public List<Guid> RelationIds { get; set; } = new();
    }
}
