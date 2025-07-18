using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib.Models.DeviceCommunications
{
    public interface IDeviceCommunication : IDisposable
    {
        byte[] SendCommand(byte[] bytesMessage);
        void StartCommunication();
        void StopCommunication();
    }
}
