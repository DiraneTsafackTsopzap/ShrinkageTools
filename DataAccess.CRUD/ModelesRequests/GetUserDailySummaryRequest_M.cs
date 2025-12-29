using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.ModelesRequests
{
    public class GetUserDailySummaryRequest_M
    {
        public Guid CorrelationId { get; init; }
        public Guid UserId { get; init; }
    }
}
