using MeasuringModuleRiM.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Parsers
{
    public class KDTNParser
    {
        public VersionAndType  ParseVersionAndType(byte[] data)
        {
            
            double versions = int.Parse(data[6].ToString("X2").TrimStart('0')) + int.Parse(data[5].ToString("X2").TrimStart('0')) * 0.01;
            string type = $"РиМ {data[9].ToString("X2").TrimStart('0')}{data[8].ToString("X2")}.{data[7].ToString("X2")}";
            VersionAndType versionAndType = new VersionAndType(versions, type);
            return versionAndType;
        }

        public ElectricalIndicators ParseElectricalIndicators(byte[] data)
        {
            int settingsType = data[5];

            int totalOrNonPhaseIndicationFactValue = BitConverter.ToInt32(data.Skip(6).Take(4).ToArray(), 0);
            int? totalOrNonPhaseIndication = (totalOrNonPhaseIndicationFactValue == -1) ? null : totalOrNonPhaseIndicationFactValue;

            int phaseAOrNonPhaseIndicationFactValue = BitConverter.ToInt32(data.Skip(10).Take(4).ToArray(), 0);
            int? phaseAOrNonPhaseIndication = (phaseAOrNonPhaseIndicationFactValue == -1) ? null : phaseAOrNonPhaseIndicationFactValue;
            int phaseBOrNonPhaseIndicationFactValue = BitConverter.ToInt32(data.Skip(14).Take(4).ToArray(), 0);
            int? phaseBOrNonPhaseIndication = (phaseBOrNonPhaseIndicationFactValue == -1) ? null : phaseBOrNonPhaseIndicationFactValue;
            int phaseCOrNonPhaseIndicationFactValue = BitConverter.ToInt32(data.Skip(18).Take(4).ToArray(), 0);
            int? phaseCOrNonPhaseIndication = (phaseCOrNonPhaseIndicationFactValue == -1) ? null : phaseCOrNonPhaseIndicationFactValue;

            ElectricalIndicators electricalIndicators = new ElectricalIndicators(settingsType,
                totalOrNonPhaseIndication,phaseAOrNonPhaseIndication,
                phaseBOrNonPhaseIndication,phaseCOrNonPhaseIndication);
            return electricalIndicators;
        }

        public int ParseWorkTimeSeconds(byte[] data)
        {
            return BitConverter.ToInt32(data.Skip(5).Take(4).ToArray(), 0);
        }
    }
}
