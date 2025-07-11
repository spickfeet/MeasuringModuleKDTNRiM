using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    public class RFSettingsCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; }
        public RFSettingsCommand(ICRC crc, IDeviceCommunication deviceCommunication)
        {
            CRC = crc;
            DeviceCommunication = deviceCommunication;
        }
        public byte[] ReadRFSignalLevel(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x6B;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes, 15);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception("Error reading RF interface signal level. Error code: " +
                    $"{BitConverter.ToInt32(receive.Skip(5).Take(4).ToArray(), 0)}");
            }
            return receive;
        }
    }
}
