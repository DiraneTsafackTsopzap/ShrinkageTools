using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.Modeles
{
    public class ShrinkageTeamsDataModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string TeamName { get; set; } = null!;
        public Guid[] TeamLeadIds { get; set; } = null!;

        //public int TeamReference { get; set; } : Que veut dire TeamReference ici ? et pkoi il est un int ?

        public string TeamReference { get; set; } = string.Empty; // ✅
    }
}
