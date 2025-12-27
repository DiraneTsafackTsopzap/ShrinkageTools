using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.Modeles;
    public class ShrinkageUserDailyValuesDataModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid TeamId { get; set; }

        public double PaidTimeOff { get; set; }

        public double Overtime { get; set; }

        public double PaidTime { get; set; }

        public double VacationTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public DateOnly ShrinkageDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }
        public string CreatedByUserEmail { get; init; } = null!;

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByUserEmail { get; init; }
        public DateTime DeletedAt { get; set; }

        public Guid DeletedBy { get; set; }
    }

