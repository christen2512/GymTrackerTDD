using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymTracker.Models
{
    public record WorkingSet(int SetNumbers, int RepNumber, int LoadNumber, string RPE);
}