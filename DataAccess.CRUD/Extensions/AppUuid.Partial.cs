using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace DataAccess.CRUD.Extensions    /// Pour que la classe ci marche ,
//Placer plu tot le namespace GrpcShrinkageServiceTraining.Protobuf dans le projet DataAccess.CRUD
namespace GrpcShrinkageServiceTraining.Protobuf
{
    public partial class AppUuid
    {
        public static AppUuid FromGuid(Guid guid)
        {
            return new AppUuid
            {
                Value = guid.ToString("D")
            };
        }

        public static AppUuid FromString(string value)
        {
            return FromGuid(Guid.Parse(value));
        }

        public Guid ToGuid()
        {
            return Guid.Parse(Value);
        }

        public bool TryParseToGuid(out Guid guid)
        {
            return Guid.TryParse(Value, out guid);
        }

        public static implicit operator AppUuid(Guid guid)
        {
            return FromGuid(guid);
        }
    }
}
