using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.ModelesRequests;
    public class SaveUsersShrinkageStatusRequest_M
    {
    public Guid CorrelationId { get; init; }
    public IReadOnlyList<UserDailyValueStatus_M> DailyValueStatuses { get; init; } = null!;
}

