﻿using RiM384Lib.Models.CRC;
using RiM384Lib.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RiM384Lib.Models.DeviceCommands
{
    internal class RFSettingsCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; }
        public RFSettingsCommand(ICRC crc, IDeviceCommunication deviceCommunication)
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
        public (byte[], byte[]) ReadRFSignalLevel(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x6B;
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
            if (receive.Length != 13 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
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
                throw new Exception($"Ошибка чтения уровня сигнала RF-интерфейса. Код ошибки: {(int)receive[5]}.");
            }
            return (receive, writeBytes);
        }

        /// <summary>
        /// Возвращает полученные и отправленные байты.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (byte[], byte[]) ReadRFSettings(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x78;
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
            if (receive.Length != 6 + CRC.CRCLength)
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
                throw new Exception($"Ошибка чтения настроек RF-канала модуля. Код ошибки: {(int)receive[5]}.");
            }
            return (receive, writeBytes);
        }
        /// <summary>
        /// Возвращает полученные и отправленные байты.
        /// channelNumber от 1 до 8;
        /// Код мощности излучения от 0 до 7:
        /// 0 – [7.8 dBm]
        /// 1 – [-15 dBm]
        /// 2 – [-10 dBm]
        /// 3 – [-5 dBm]
        /// 4 – [0 dBm]
        /// 5 – [5 dBm]
        /// 6 – [7 dBm]
        /// 7 – [10 dBm]
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="channelNumber"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public (byte[], byte[]) WriteRFSettings(byte[] serialNumber, int channelNumber, int powerCode)
        {
            // Проверка входных значений
            if (channelNumber < 1 || channelNumber > 8)
                throw new ArgumentException("Номер канала должен быть от 1 до 8");
            if (powerCode < 0 || powerCode > 7)
                throw new ArgumentException("Код мощности должен быть от 0 до 7");
            channelNumber--; 

            // Формирование данных для отправки
            byte[] writeBytes = new byte[8];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x79;
            writeBytes[4] = 0x03;
            writeBytes[5] = (byte)((powerCode << 4) | channelNumber);

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
                throw new Exception($"Ошибка записи настроек RF-канала модуля. Код ошибки: {(int)receive[5]}.");
            }
            return (receive, writeBytes);
        }
    }
}
