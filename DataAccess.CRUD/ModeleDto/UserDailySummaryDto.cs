using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.EnumsModels;
using GrpcShrinkageServiceTraining.Protobuf;

namespace DataAccess.CRUD.ModeleDto;
public record UserDailySummaryDto
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public Status Status { get; init; }
    public AbsenceType AbsenceType { get; init; }
    public PublicHolidayDto? PublicHoliday { get; init; }
    public WeekendDto? Weekend { get; init; }
}

