using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.Data
{
    public struct MeasuredValues
    {
        public readonly float activePower;
        public readonly float reactivePower;
        public readonly int rmsVoltage;
        public readonly float rmsCurrent;
        public readonly float frequency;
        public MeasuredValues(float activePower, float reactivePower, int rmsVoltage, float rmsCurrent, float frequency)
        {
            this.activePower = activePower;
            this.reactivePower = reactivePower;
            this.rmsVoltage = rmsVoltage;
            this.rmsCurrent = rmsCurrent;
            this.frequency = frequency;
        }
    }
}
