using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DataAccess.CRUD.Extensions
{
    public class SqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly date) => parameter.Value = date.ToDateTime(new TimeOnly(0, 0));
        public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
    }
}
