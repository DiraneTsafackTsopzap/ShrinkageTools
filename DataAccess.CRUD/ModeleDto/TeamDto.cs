using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.ModeleDto
{
    public class TeamDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public IReadOnlyList<Guid> TeamLeadIds { get; init; } = null!;
    }
}
