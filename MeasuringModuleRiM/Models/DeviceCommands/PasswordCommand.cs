using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    internal class PasswordCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; }
        public PasswordCommand(ICRC crc, IDeviceCommunication deviceCommunication)
        {
            CRC = crc;
            DeviceCommunication = deviceCommunication;
        }

        /// <summary>
        /// Возвращает полученные и отправленные байты.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (byte[], byte[]) EnterReadPassword(byte[] serialNumber, string password)
        {
            // Формирование данных для отправки
            byte[] writeBytes = AddSerialAndPasswordBytes(serialNumber, password);
            writeBytes[3] = 0x04;
            writeBytes[4] = 0x08;
            CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка CRC
            if (!CRC.CheckCRC(receive))
            {
                throw new Exception($"Некорректная контрольная сумма полученного пакета. Пакет {BitConverter.ToString(receive)}");
            }

            // Проверка количества байт
            if (receive.Length != 5 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
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
                throw new Exception($"Ошибка при вводе пароля для чтения пароля. Код ошибки: {(int)receive[5]}.");
            }
            return (receive, writeBytes);
        }

        /// <summary>
        /// Возвращает полученные и отправленные байты.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (byte[], byte[]) EnterWritePassword(byte[] serialNumber, string password)
        {
            // Формирование данных для отправки
            byte[] writeBytes = AddSerialAndPasswordBytes(serialNumber, password);
            writeBytes[3] = 0x02;
            writeBytes[4] = 0x08;
            CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка CRC
            if (!CRC.CheckCRC(receive))
            {
                throw new Exception($"Некорректная контрольная сумма полученного пакета. Пакет {BitConverter.ToString(receive)}");
            }

            // Проверка количества байт
            if (receive.Length != 5 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
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
                throw new Exception($"Ошибка при вводе пароля для записи пароля. Код ошибки: {(int)receive[5]}.");
            }
            return (receive, writeBytes);
        }

        private byte[] AddSerialAndPasswordBytes(byte[] serialNumber, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] writeBytes = new byte[13];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            Array.Copy(passwordBytes, 0, writeBytes, 5, Math.Min(passwordBytes.Length, 6));
            return writeBytes;
        }
    }
}
