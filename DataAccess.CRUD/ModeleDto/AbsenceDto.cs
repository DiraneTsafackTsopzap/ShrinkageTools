using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.EnumsModels;
using GrpcShrinkageServiceTraining.Protobuf;

namespace DataAccess.CRUD.ModeleDto;

    public record AbsenceDto
    {
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? UserEmail { get; init; }
    public Guid TeamId { get; init; }
    public AbsenceTypeDto AbsenceType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = null!;
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
}

