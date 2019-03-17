using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDR2
{
    class EnhancedRobertsonHDRSolver:RobertsonHDRSolver
    {
        protected override double w(byte z, double exposure)
        {
            return (Math.Max(z, 255 - z) + 1) * exposure;
        }
    }
}
