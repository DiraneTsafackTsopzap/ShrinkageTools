using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.ModelesRequests;
    public class GetUserShrinkageRequest_M
    {
        public Guid CorrelationId { get; init; }
        public Guid UserId { get; init; }
        public DateOnly ShrinkageDate { get; init; }
    }

