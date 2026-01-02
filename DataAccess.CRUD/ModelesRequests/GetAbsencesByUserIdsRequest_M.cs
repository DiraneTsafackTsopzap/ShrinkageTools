using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.ModelesRequests;

    public class GetAbsencesByUserIdsRequest_M
    {
    public Guid CorrelationId { get; init; }
    public IReadOnlyList<Guid> UserIds { get; init; } = null!;
     }

