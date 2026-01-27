using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.EnumsModels;

namespace DataAccess.CRUD.ModelesRequests;
    public class UserDailyValueStatus_M
    {
        public Guid DailyValuesId { get; init; }
        public Guid UserId { get; init; }
        public StatusDto Status { get; init; }
        public string? Comment { get; init; }
    }
