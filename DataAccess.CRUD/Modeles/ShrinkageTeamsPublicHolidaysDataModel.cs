using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.Modeles;

    public class ShrinkageTeamsPublicHolidaysDataModel
    {
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? DeletedAt { get; init; }
    public Guid? DeletedBy { get; init; }
    public string Title { get; set; } = null!;
    public DateOnly AffectedDay { get; init; }
    public Guid[] TeamIds { get; set; } = null!;
}

