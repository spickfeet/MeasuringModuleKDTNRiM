using RiM384Lib.Models.CRC;
using RiM384Lib.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib.Models.DeviceCommands
{
    internal class DeviceInformationCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; }
        public DeviceInformationCommand(ICRC crc, IDeviceCommunication deviceCommunication)
        {
            CRC = crc;
            DeviceCommunication = deviceCommunication;
        }

        /// <summary>
        /// Возвращает полученные и отправленные байты.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (byte[], byte[]) ReadVersionTypeAndType(byte[] serialNumber)
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

            // Проверка адреса устройства
            int sendSerialNumber = BitConverter.ToInt32(new byte[] { writeBytes[0], writeBytes[1], writeBytes[2], 0 }, 0);
            int receiveSerialNumber = BitConverter.ToInt32(new byte[] { receive[0], receive[1], receive[2], 0 }, 0);

            if (sendSerialNumber != receiveSerialNumber)
            {
                throw new Exception($"Адрес устройства {receiveSerialNumber} " +
                    $"не соответствует ожидаемому {sendSerialNumber}");
            }

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка чтения типа устройства и версии ПО. Код ошибки: {(int)receive[5]}.");
            }

            return (receive, writeBytes);
        }

        /// <summary>
        /// Возвращает полученные и отправленные байты.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (byte[], byte[]) ReadWorkTimeSeconds(byte[] serialNumber)
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

            // Проверка адреса устройства
            int sendSerialNumber = BitConverter.ToInt32(new byte[] { writeBytes[0], writeBytes[1], writeBytes[2], 0 }, 0);
            int receiveSerialNumber = BitConverter.ToInt32(new byte[] { receive[0], receive[1], receive[2], 0 }, 0);

            if (sendSerialNumber != receiveSerialNumber)
            {
                throw new Exception($"Адрес устройства {receiveSerialNumber} " +
                    $"не соответствует ожидаемому {sendSerialNumber}");
            }

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка чтения счётчика наработки. Код ошибки: {(int)receive[5]}.");
            }
            return (receive, writeBytes);
        }
    }
}
