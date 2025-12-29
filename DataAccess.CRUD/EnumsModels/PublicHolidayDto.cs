using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.EnumsModels;

public record PublicHolidayDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public DateOnly AffectedDate { get; init; }
}