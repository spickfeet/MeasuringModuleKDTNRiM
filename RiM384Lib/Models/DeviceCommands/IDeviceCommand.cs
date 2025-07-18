using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiM384Lib.Models.CRC;
using RiM384Lib.Models.DeviceCommunications;

namespace RiM384Lib.Models.DeviceCommands
{
    internal interface IDeviceCommand
    {
        ICRC CRC { get; }
        IDeviceCommunication DeviceCommunication { get; }
    }
}
