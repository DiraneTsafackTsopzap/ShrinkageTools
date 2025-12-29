using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.ModeleDto;

namespace DataAccess.CRUD.ModelesRequests;
    public class SaveActivityRequest_M
    {
    public Guid CorrelationId { get; init; }
    public ActivityDto Activity { get; init; } = null!;

}

