using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.Data
{
    public struct RFSettings
    {
        public readonly int channelNumber;
        /// <summary>
        /// Единицы измерения dBm
        /// </summary>
        public readonly float power;
        public RFSettings(int channelNumber, float power) 
        {
            this.channelNumber = channelNumber;
            this.power = power;
        }
    }
}
