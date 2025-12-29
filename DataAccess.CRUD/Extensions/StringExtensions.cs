using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.Extensions;

    public static class StringExtensions
    {
    public static string? NullIfEmpty(this string? str) => string.IsNullOrEmpty(str)
       ? null
       : str;

}

