using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrpcShrinkageServiceTraining.Protobuf;

namespace DataAccess.CRUD.Extensions
{
    public static class ApiExtensions
    {
        public static bool TryParseToGuidNotNullOrEmpty(this AppUuid? uuid, out Guid guid)
        {
            if (uuid is null)
            {
                guid = Guid.Empty;
                return false;
            }

            return uuid.TryParseToGuid(out guid) && guid != Guid.Empty;
        }
    }
}
