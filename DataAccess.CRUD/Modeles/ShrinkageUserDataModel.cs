using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.Modeles
{
    public class ShrinkageUserDataModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public Guid? TeamId { get; set; }
        public DateTime UserCreatedAt { get; set; }
        public DateTime PaidTimeCreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public double PaidTimeMonday { get; set; }
        public double PaidTimeTuesday { get; set; }
        public double PaidTimeWednesday { get; set; }
        public double PaidTimeThursday { get; set; }
        public double PaidTimeFriday { get; set; }
        public double PaidTimeSaturday { get; set; }
        public DateOnly ValidFrom { get; set; }  // Type ici est DateOnly
        public string PaidTimeCreatedByUserEmail { get; set; } = string.Empty;
        public Guid PaidTimeId { get; set; }
    }
}


// Comprendre comment le ShrinkageUserDataModel est definis : Questions qu'on doit se poser pour la suite

// le id cest quoi ?  ya t'il un ForeinKey ? ya un til un id non null ? ya t'il de DateTime ? 

// Ya til d'autre variables? par exemple les int , les double , les floats ?