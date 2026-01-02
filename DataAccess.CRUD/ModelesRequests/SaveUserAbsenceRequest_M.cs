

using DataAccess.CRUD.ModeleDto;

namespace DataAccess.CRUD.ModelesRequests;

    public class SaveUserAbsenceRequest_M
    {
        public Guid CorrelationId { get; init; }
        public AbsenceDto Absence { get; init; } = null!;
    }

