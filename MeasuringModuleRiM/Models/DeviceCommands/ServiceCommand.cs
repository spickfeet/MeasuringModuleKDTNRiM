using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    public class ServiceCommand : IDeviceCommand
    {
        public ICRC CRC { get; private set; }
        public IDeviceCommunication DeviceCommunication { get; private set; } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="deviceCommunication"></param>
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
                throw new ArgumentException($"Error {paramType} unknown parameter type");
            }

            // Формирование данных для отправки
            byte[] writeBytes = new byte[8];
            Array.Copy(serialNumber, 0, writeBytes, 0, serialNumber.Length);
            writeBytes[3] = 0x27;
            writeBytes[4] = 0x03;
            writeBytes[5] = BitConverter.GetBytes(paramType)[0];
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error getting electrical indicators. Error code: " +
                    $"{BitConverter.ToInt32(receive.Skip(5).Take(4).ToArray(), 0)}");
            }
            return receive;
        }

        public byte[] ReadSerialNumber()
        {
            // Формирование данных для отправки
            byte[] writeBytes = new byte[7];
            writeBytes[3] = 0x7F;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                byte[] errorCodeBytes = receive.Skip(5).Take(3).ToArray();
                Array.Resize(ref errorCodeBytes, 4);
                throw new Exception($"Error getting serial number. Error code: {BitConverter.ToInt32(errorCodeBytes)}");
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
                throw new ArgumentException("Error serial number must be between 0 and 16777215");
            }
            // Формирование данных для отправки
            byte[] serialNumberBytes = BitConverter.GetBytes(serialNumber);
            byte[] writeBytes = new byte[7];
            writeBytes[0] = serialNumberBytes[0];
            writeBytes[1] = serialNumberBytes[1];
            writeBytes[2] = serialNumberBytes[2];
            writeBytes[3] = 0x7F;
            writeBytes[4] = 0x02;
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error write serial number.");
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
            writeBytes = CRC.AddCRC(writeBytes);

            // Отправка данных
            byte[] receive = DeviceCommunication.SendCommand(writeBytes);

            // Проверка кода операции 
            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error reading service parameters. Error code: {(int)receive[5]}");
            }
            return receive;
        }
    }
}
