using RiM384Lib.Exceptions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib.Models.DeviceCommunications
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
            Dispose();
        }

        public byte[] SendCommand(byte[] bytesMessage)
        {
            try
            {
                _serial.DiscardOutBuffer();
                _serial.Write(bytesMessage, 0, bytesMessage.Length);
                _serial.BaseStream.Flush();
                return ReceiveResult();
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        private byte[] ReceiveResult()
        {
            try
            {
                // Сначала читаем заголовок пакета (5 байт)
                byte[] header = ReceiveFixedSize(5);

                int dataLength = header[4];

                // Если в пакете есть дополнительные данные, читаем их
                if (dataLength > 1)
                {
                    byte[] data = ReceiveFixedSize(dataLength);

                    // Объединяем заголовок и данные в один массив
                    byte[] packet = new byte[5 + dataLength];
                    Buffer.BlockCopy(header, 0, packet, 0, 5);
                    Buffer.BlockCopy(data, 0, packet, 5, dataLength);

                    return packet;
                }
                throw new Exception("Пакет данных получен неполным.");
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        private byte[] ReceiveFixedSize(int nbytes)
        {
            try
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
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }
        public void StartCommunication()
        {
            try
            {
                if (!_serial.IsOpen)
                    _serial.Open();
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public void StopCommunication()
        {
            try
            {
                if (_serial.IsOpen)
                    _serial.Close();
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public void Dispose()
        {
            StopCommunication();
        }
    }
}
