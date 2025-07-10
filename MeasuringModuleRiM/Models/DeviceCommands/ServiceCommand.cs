using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.DeviceCommunications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommands
{
    internal class ServiceCommand : IDeviceCommand
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
            byte[] receive = DeviceCommunication.SendCommand(writeBytes, 24);

            if (receive[3] != writeBytes[3])
            {
                throw new Exception($"Error getting electrical indicators. Error code: " +
                    $"{BitConverter.ToInt32(receive.Skip(18).Take(4).ToArray(), 0)}");
            }
            return receive;
        }
    }
}
