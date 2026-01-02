using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.ModelesRequests;

    public class DeleteUserAbsenceRequest_M
    {
        public Guid CorrelationId { get; init; }
        public Guid Id { get; init; }
        public Guid DeletedBy { get; init; }
    }
