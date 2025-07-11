using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
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
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception("Error reading RF interface signal level. Error code: " +
                    $"{BitConverter.ToInt32(receive.Skip(5).Take(4).ToArray(), 0)}");
            }
            return receive;
        }

        public byte[] ReadRFSettings(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x78;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error reading RF settings. Error code: {(int)receive[5]}");
            }
            return receive;
        }
        /// <summary>
        /// channelNumber от 1 до 8;
        /// Мощность излучения от 0 до 7:
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
        public byte[] WriteRFSettings(byte[] serialNumber, int channelNumber, int power)
        {
            // Проверка входных значений
            if (channelNumber < 1 || channelNumber > 8)
                throw new ArgumentException("Номер канала должен быть от 0 до 7");
            if (power < 0 || power > 7)
                throw new ArgumentException("Уровень мощности должен быть от 0 до 7");
            channelNumber--;

            // Формирование данных для отправки
            byte[] writeBytes = new byte[8];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x79;
            writeBytes[4] = 0x03;
            writeBytes[5] = (byte)((power << 4) | channelNumber);

            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error write RF settings.");
            }
            return receive;
        }
    }
}
