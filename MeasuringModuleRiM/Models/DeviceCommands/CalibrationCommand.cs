using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public byte[] ReadMeasuredValues(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x70;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка при чтении значений измеряемых величин. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
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
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка при чтении даты калибровки модуля. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }

        public byte[] WriteCalibrationDate(byte[] serialNumber, DateTime date)
        {
            if(date.Year < 2000)
            {
                throw new ArgumentException("Год должен быть позже 2000.");
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
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка при записи даты калибровки модуля. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }

        /// <summary>
        /// constPtr = 0 - WGAIN
        /// constPtr = 1 - WOFFS
        /// constPtr = 2 - VARGAIN
        /// constPtr = 3 - VAROFFS
        /// constPtr = 4 - PHCAL
        /// constPtr = 5 - VRMSGAIN
        /// constPtr = 6 - IRMSGAIN
        /// constPtr = 7 - CTGAIN
        /// constPtr = 8 - FREQCAL
        /// constPtr = 9 - IRMSOFFS
        /// constPtr = 10 - CTGAIN_ Q 
        /// constPtr = 11 - VDIV_PH
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="constPtr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] ReadCalibrationConst(byte[] serialNumber, int constPtr)
        {
            // 254 и 255 тоже исключены
            if (constPtr < 0 || constPtr > 11)
            {
                throw new ArgumentException("Неизвестный указатель калибровочной константы.");
            }
            // Формирование данных для отправки
            byte[] writeBytes = new byte[8];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x72;
            writeBytes[4] = 0x03;
            writeBytes[5] = (byte)constPtr;

            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка при чтении калибровочной константы. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }

        /// <summary>
        /// constPtr = 0 - WGAIN constValue Диапазон: 0…65535 Примечание: Отсутствуют
        /// constPtr = 1 - WOFFS constValue Диапазон: -32768…32767 Примечание: Отсутствуют
        /// constPtr = 2 - VARGAIN constValue Диапазон: 0…65535 Примечание: Отсутствуют
        /// constPtr = 3 - VAROFFS constValue Диапазон: -32768…32767 Примечание: Отсутствуют
        /// constPtr = 4 - PHCAL constValue Диапазон: 0…255 Примечание: Ст. байт - незначащий
        /// constPtr = 5 - VRMSGAIN constValue Диапазон: 0…65535 Примечание: Отсутствуют
        /// constPtr = 6 - IRMSGAIN constValue Диапазон: 0…65535 Примечание: Отсутствуют
        /// constPtr = 7 - CTGAIN constValue Диапазон: 0…65535 Примечание: CrossTalk
        /// constPtr = 8 - FREQCAL constValue Диапазон: 0…65535 Примечание: Отсутствуют
        /// constPtr = 9 - IRMSOFFS constValue Диапазон: -32768…32767 Примечание: Отсутствуют
        /// constPtr = 10 - CTGAIN_Q constValue Диапазон: 0…65535 Примечание: CrossTalk квадратурная
        /// constPtr = 11 - VDIV_PH constValue Диапазон: 200…5000, 65535-выкл. Примечание: Фазовая корр. делит. напр.
        /// constPtr = 254 - VDIV_PH constValue Диапазон: 0x0000 Примечание: Защита калибровки (запись)
        /// constPtr = 255 - VDIV_PH constValue Диапазон: 0x0000 Примечание: Сброс констант (запись)
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="constPtr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] WriteCalibrationConst(byte[] serialNumber, int constPtr, int constValue)
        {

            if (constPtr < 0 || (constPtr > 11 && constPtr != 254 && constPtr != 255))
            {
                throw new ArgumentException("Неизвестный указатель калибровочной константы.");
            }
            // Формирование данных для отправки
            byte[] writeBytes = new byte[10];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x73;
            writeBytes[4] = 0x05;
            writeBytes[5] = (byte)constPtr;
            byte[] constValueBytes;

            if (constPtr == 1 || constPtr == 3 || constPtr == 9)
            {
                if(constValue < -32768 || constValue > 32767)
                {
                    throw new ArgumentException($"Значение константы по указателю {constPtr} находится в диапазоне от -32768 до 32767.");
                }
                constValueBytes = BitConverter.GetBytes((short)constValue);
                Array.Copy(constValueBytes, 0, writeBytes, 6, constValueBytes.Length);
            }
            else
            {
                if (constPtr == 254 || constPtr == 255 && (constValue != 0))
                {
                    throw new ArgumentException($"Значение константы по указателю {constPtr} должно быть записано только как 0.");
                }
                if (constPtr == 4 && (constValue < 0 || constValue > 255))
                {
                    throw new ArgumentException($"Значение константы по указателю {constPtr} находится в диапазоне от 0 до 255.");
                }
                if (constPtr == 11 && constValue != 65535 && (constValue < 200 || constValue > 5000))
                {
                    throw new ArgumentException($"Значение константы по указателю {constPtr} находится в диапазоне от 200 до 5000 или равно 65535.");
                }
                if (constValue < 0 || constValue > 65535)
                {
                    throw new ArgumentException($"Значение константы по указателю {constPtr} находится в диапазоне от 0 до 65535.");
                }
                constValueBytes = BitConverter.GetBytes((ushort)constValue);
                Array.Copy(constValueBytes, 0, writeBytes, 6, constValueBytes.Length);
            }

            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка при записи калибровочной константы. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }
        public byte[] RestartMeasurements(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x71;
            writeBytes[4] = 0x02;

            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка при чтении измеряемых величин. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }
    }
}
