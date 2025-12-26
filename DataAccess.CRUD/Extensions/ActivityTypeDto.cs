using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD.Extensions;
public enum ActivityTypeDto
{
    Unspecified = 0,
    Meeting = 1,
    Projects = 2,
    BusinessInterruption = 3,
    TrainingOrCoaching = 4,
    ProductiveNotMeasurable = 5,
    Others = 6
}

public enum ActivityTrackTypeDto
{
    Unspecified = 0,
    Timer = 1,
    Manual = 2,
    ManualStopped = 3
}