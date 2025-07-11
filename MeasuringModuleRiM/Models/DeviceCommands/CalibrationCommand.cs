using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    public class CalibrationCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; }
        public CalibrationCommand(ICRC crc, IDeviceCommunication deviceCommunication)
        {
            CRC = crc;
            DeviceCommunication = deviceCommunication;
        }
        public byte[] ReadCalibrationDate(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x74;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes, 11);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error reading calibration date. Error code: " +
                    $"{BitConverter.ToInt32(receive.Skip(5).Take(4).ToArray(), 0)}");
            }
            return receive;
        }

        public byte[] WriteCalibrationDate(byte[] serialNumber, DateTime date)
        {
            if(date.Year < 2000)
            {
                throw new ArgumentException("Error year must be later than 2000");
            }

            // Формирование данных для отправки
            byte[] writeBytes = new byte[11];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x75;
            writeBytes[4] = 0x06;

            DateTime baseDate = new(2000, 1, 1);
            TimeSpan timeSpan = date - baseDate;
            uint calibrationDateSeconds = (uint)timeSpan.TotalSeconds;
            byte[] calibrationDateSecondsBytes = BitConverter.GetBytes(calibrationDateSeconds);
            Array.Copy(calibrationDateSecondsBytes, 0, writeBytes, 5, calibrationDateSecondsBytes.Length);

            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes, 7);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error write calibration date.");
            }
            return receive;
        }
    }
}
