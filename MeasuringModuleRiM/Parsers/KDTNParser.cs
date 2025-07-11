using MeasuringModuleRiM.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public uint ParseTimeSeconds(byte[] data)
        {
            return BitConverter.ToUInt32(data.Skip(5).Take(4).ToArray(), 0);
        }

        public int ParseSerialNumber(byte[] data)
        {
            byte[] serialNumberBytes = data.Skip(5).Take(3).ToArray();
            Array.Resize(ref serialNumberBytes, 4);
            return BitConverter.ToInt32(serialNumberBytes);
        }

        public float ParseRFSignalLevel(byte[] data)
        {
            return BitConverter.ToSingle(data.Skip(5).Take(4).ToArray(), 0);
        }

        public RFSettings ParseRFSettings(byte[] data)
        {
            int channel = data[5] & 0b0000_0111;
            channel++;

            int powerLevel = (data[5] & 0b0111_0000) >> 4;

            float power = powerLevel switch
            {
                0 => 7.8f,
                1 => -15,
                2 => -10,
                3 => -5,
                4 => 0,
                5 => 5,
                6 => 7,
                7 => 10,
                _ => throw new Exception("Error unknown power")
            };
            RFSettings rFSettings = new(channel, power);
            return rFSettings;
        }

        public int ParseCalibrationConst(byte[] data)
        {
            if (data[5] == 1 || data[5] == 3 || data[5] == 9)
            {
                return (int)BitConverter.ToInt16(data.Skip(6).Take(2).ToArray(), 0);
            }
            else 
            {
                return (int)BitConverter.ToUInt16(data.Skip(6).Take(2).ToArray(), 0);
            }
        }
    }
}
