using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommunications
{
    public interface IDeviceCommunication
    {
        byte[] SendCommand(byte[] bytesMessage, int resultLength);
        void StartCommunication();
        void StopCommunication();
    }
}
