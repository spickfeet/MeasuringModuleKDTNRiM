using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    internal class ServiceCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; } 
        public ServiceCommand(ICRC crc, IDeviceCommunication deviceCommunication)
        {
            CRC = crc;
            DeviceCommunication = deviceCommunication;
        }
        /// <summary>
        /// Тип параметра: 
        /// 0 – активная мощность, Вт (фаза А);  
        /// 1 - реактивная мощность, Вар (фаза А); 
        /// 4 – линейные напряжения(UAB), В; 
        /// 5 – фазные/линейные токи(IA), А; 
        /// 6 – частота сети, Гц
        /// </summary>
        /// <param name="paramType"></param>
        /// <returns></returns>
        public byte[] ReadElectricalIndicators(byte[] serialNumber, int paramType)
        {
            if(paramType != 0 && paramType != 1 && paramType != 4 && paramType != 5 && paramType != 6)
            {
                throw new ArgumentException($"Ошибка {paramType} неизвестный тип параметра");
            }

            // Формирование данных для отправки
            byte[] writeBytes = new byte[8];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x27;
            writeBytes[4] = 0x03;
            writeBytes[5] = BitConverter.GetBytes(paramType)[0];
            CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка CRC
            if (!CRC.CheckCRC(receive))
            {
                throw new Exception($"Некорректная контрольная сумма полученного пакета. Пакет {BitConverter.ToString(receive)}");
            }

            // Проверка количества байт
            if (receive.Length != 22 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
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
                throw new Exception($"Ошибка чтения электрических показателей. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }

        public byte[] ReadSerialNumber()
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            writeBytes[3] = 0x7F;
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
            if (receive.Length != 8 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
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
                throw new Exception($"Ошибка чтения серийного номера модуля. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }
        /// <summary>
        /// Изменить серийный номер на значение от 0 до 16777215
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] WriteSerialNumber(int serialNumber)
        {
            if(serialNumber < 0 || serialNumber > 16777215)
            {
                throw new ArgumentException("Серийный номер должен быть от 0 до 16777215");
            }
            // Формирование данных для отправки
            byte[] serialNumberBytes = BitConverter.GetBytes(serialNumber);
            byte[] writeBytes = new byte[7];
            writeBytes[0] = serialNumberBytes[0];
            writeBytes[1] = serialNumberBytes[1];
            writeBytes[2] = serialNumberBytes[2];
            writeBytes[3] = 0x7F;
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
            if (receive.Length != 5 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
            {
                throw new Exception($"Количество полученных байт не соответствует ожидаемому.");
            }

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Ошибка записи серийного номера модуля. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }

        public byte[] ReadServiceParameters(byte[] serialNumber)
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x7E;
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
            if (receive.Length != 8 + CRC.CRCLength && receive.Length != 6 + CRC.CRCLength)
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
                throw new Exception($"Ошибка чтения служебных параметров. Код ошибки: {(int)receive[5]}.");
            }
            return receive;
        }

        /// <summary>
        /// Заглушка т.к. результат команды не соответствует документации
        /// Send: 7A AF 00 08 02 5A DF
        /// Результат:
        /// Receive: 7A AF 00 08 03 60 5E 83
        /// Ожидание: Счетчик байт(07H) Статус(1 байт), Время в секундах от 01.01.2000 (4 байта), CRC(2 байта)
        /// </summary>
        /// <param name="serialNumber"></param>
        public void ReadCurrentTimeValue(byte[] serialNumber)
        {
            return;
        }
    }
}
