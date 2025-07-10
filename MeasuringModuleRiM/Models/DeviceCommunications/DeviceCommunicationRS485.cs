using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommunications
{
    public class DeviceCommunicationRS485 : IDeviceCommunication
    {
        private SerialPort _serial;
        public DeviceCommunicationRS485(SerialPort serial) 
        { 
            _serial = serial;
        }

        ~DeviceCommunicationRS485()
        {
            StopCommunication();
        }

        public byte[] SendCommand(byte[] bytesMessage, int resultLength)
        {
            _serial.Write(bytesMessage, 0, bytesMessage.Length);
            return ReceiveResult(resultLength);
        }
        private byte[] ReceiveResult(int nbytes)
        {
            var buf = new byte[nbytes];
            var readPos = 0;
            while (readPos < nbytes)
            {
                int read = _serial.BaseStream.Read(buf, readPos, nbytes - readPos);
                if (read == 0)
                    throw new EndOfStreamException();
                readPos += read;
            }
            return buf;
        }
        public void StartCommunication()
        {
            if (!_serial.IsOpen)
                _serial.Open();
        }

        public void StopCommunication()
        {
            if (_serial.IsOpen)
                _serial.Close();
        }
    }
}
