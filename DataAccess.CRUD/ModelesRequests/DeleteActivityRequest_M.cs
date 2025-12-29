using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.ModelesRequests;
public class DeleteActivityRequest_M
{
    public Guid CorrelationId { get; init; }
    public Guid ActivityId { get; init; }
    public Guid DeletedBy { get; init; }
}
