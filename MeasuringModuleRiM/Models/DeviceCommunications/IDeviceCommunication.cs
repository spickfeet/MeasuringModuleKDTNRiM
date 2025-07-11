using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommunications
{
    public interface IDeviceCommunication
    {
        byte[] SendCommand(byte[] bytesMessage);
        void StartCommunication();
        void StopCommunication();
    }
}
