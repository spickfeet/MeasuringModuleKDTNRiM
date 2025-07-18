using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib.Models.Data
{
    public struct ServiceParameters
    {
        public readonly float supercapacitorVoltage;
        public readonly float supplyVoltage;
        public readonly sbyte temperature;
        public ServiceParameters(float supercapacitorVoltage, float supplyVoltage, sbyte temperature)
        {
            this.supercapacitorVoltage = supercapacitorVoltage;
            this.supplyVoltage = supplyVoltage;
            this.temperature = temperature;
        }
    }
}
