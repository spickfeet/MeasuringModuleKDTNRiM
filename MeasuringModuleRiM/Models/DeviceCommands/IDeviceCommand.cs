using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    internal interface IDeviceCommand
    {
        ICRC CRC { get; }
        IDeviceCommunication DeviceCommunication { get; }
    }
}
