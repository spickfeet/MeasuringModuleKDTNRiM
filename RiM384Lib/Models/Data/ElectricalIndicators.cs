using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib.Models.Data
{
    public struct ElectricalIndicators
    {
        public readonly int settingsType;
        public readonly int? totalOrNonPhaseIndication;
        public readonly int? phaseAOrNonPhaseIndication;
        public readonly int? phaseBOrNonPhaseIndication;
        public readonly int? phaseCOrNonPhaseIndication;
        public ElectricalIndicators(int settingsType, 
            int? totalOrNonPhaseIndication, int? phaseAOrNonPhaseIndication, 
            int? phaseBOrNonPhaseIndication, int? phaseCOrNonPhaseIndication) 
        {
            this.settingsType = settingsType;
            this.totalOrNonPhaseIndication = totalOrNonPhaseIndication;
            this.phaseAOrNonPhaseIndication = phaseAOrNonPhaseIndication;
            this.phaseBOrNonPhaseIndication = phaseBOrNonPhaseIndication;
            this.phaseCOrNonPhaseIndication = phaseCOrNonPhaseIndication;
        }
    }
}
