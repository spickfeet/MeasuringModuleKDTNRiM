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
            CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка CRC
            if (!CRC.CheckCRC(receive))
            {
                throw new Exception($"Некорректная контрольная сумма полученного пакета. Пакет {BitConverter.ToString(receive)}");
            }

            // Проверка количества байт
            if (receive.Length != 10 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
            {
                throw new Exception($"Количество полученных байт не соответствует ожидаемому.");
            }

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка чтения типа устройства и версии ПО. Код ошибки: {(int)receive[5]}.");
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
            CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка CRC
            if (!CRC.CheckCRC(receive))
            {
                throw new Exception($"Некорректная контрольная сумма полученного пакета. Пакет {BitConverter.ToString(receive)}");
            }

            // Проверка количества байт
            if (receive.Length != 9 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
            {
                throw new Exception($"Количество полученных байт не соответствует ожидаемому.");
            }

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка чтения счётчика наработки. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }
    }
}
