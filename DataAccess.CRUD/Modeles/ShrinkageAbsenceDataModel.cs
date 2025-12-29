using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.Modeles;
    public class ShrinkageAbsenceDataModel
    {
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByUserEmail { get; init; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserEmail { get; init; } = string.Empty;
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; init; }
        public Guid? DeletedBy { get; init; }
        public Guid UserId { get; init; }
        public Guid TeamId { get; init; }
        public string AbsenceType { get; init; } = null!;
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
    }

