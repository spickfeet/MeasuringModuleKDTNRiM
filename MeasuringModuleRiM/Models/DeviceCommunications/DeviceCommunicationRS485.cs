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

        public byte[] SendCommand(byte[] bytesMessage)
        {
            _serial.Write(bytesMessage, 0, bytesMessage.Length);
            return ReceiveResult();
        }

        private byte[] ReceiveResult()
        {
            // Сначала читаем заголовок пакета (5 байт)
            byte[] header = ReceiveFixedSize(5);

            int dataLength = header[4];

            // Если в пакете есть дополнительные данные, читаем их
            if (dataLength > 0)
            {
                byte[] data = ReceiveFixedSize(dataLength);

                // Объединяем заголовок и данные в один массив
                byte[] packet = new byte[5 + dataLength];
                Buffer.BlockCopy(header, 0, packet, 0, 5);
                Buffer.BlockCopy(data, 0, packet, 5, dataLength);

                return packet;
            }
            throw new Exception("The data packet arrived incomplete.");
        }

        private byte[] ReceiveFixedSize(int nbytes)
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
