using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filejieyasuo
{
    public static class Export
    {
        public static int Clamp(this int m, int value, int min, int max)
        {
            int result;
            if (value < min)
            {
                result = min;
            }
            else if (value > max)
            {
                result = max;
            }
            else
            {
                result = value;
            }
            return result;
        }
    }
}
