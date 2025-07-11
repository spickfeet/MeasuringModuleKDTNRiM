using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    public class DeviceInformationCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; }
        public DeviceInformationCommand(ICRC crc, IDeviceCommunication deviceCommunication)
        {
            CRC = crc;
            DeviceCommunication = deviceCommunication;
        }
        public byte[] ReadVersionTypeAndType(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x00;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes, 12);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error getting device type and software version. Error code: " +
                    $"{BitConverter.ToInt32(receive.Skip(5).Take(4).ToArray(), 0)}");
            }
            return receive;
        }
        public byte[] ReadWorkTimeSeconds(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x01;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes, 11);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error getting work time. Error code: " +
                    $"{BitConverter.ToInt32(receive.Skip(5).Take(4).ToArray(), 0)}");
            }
            return receive;
        }
    }
}
