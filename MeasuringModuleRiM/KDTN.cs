using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.Data;
using MeasuringModuleRiM.Models.DeviceCommands;
using MeasuringModuleRiM.Models.DeviceCommunications;
using MeasuringModuleRiM.Parsers;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM
{
    public class KDTN
    {
        private PasswordCommand _passwordCommand;
        private DeviceInformationCommand _deviceInformationCommand;
        private ServiceCommand _serviceCommand;
        private byte[] _serialNumber;
        private IDeviceCommunication _deviceCommunication;
        private KDTNParser _kdtnParser;
        public KDTN(IDeviceCommunication deviceCommunication, ICRC crc, int serialNumber)
        {
            byte[] byteArray = BitConverter.GetBytes(serialNumber);
            _serialNumber = [byteArray[0], byteArray[1], byteArray[2]];
            _deviceCommunication = deviceCommunication;
            _kdtnParser = new KDTNParser();


            _passwordCommand = new(crc, deviceCommunication);
            _deviceInformationCommand = new(crc, deviceCommunication);
            _serviceCommand = new(crc, deviceCommunication);
        }
        public void StartCommunication()
        {
            _deviceCommunication.StartCommunication();
        }

        public void StopCommunication()
        {
            _deviceCommunication.StopCommunication();
        }

        public byte[] EnterReadPassword(string password) 
        {
            return _passwordCommand.EnterReadPassword(_serialNumber, password);
        }

        public byte[] EnterWritePassword(string password)
        {
            return _passwordCommand.EnterWritePassword(_serialNumber, password);
        }

        public VersionAndType ReadVersionTypeAndType()
        {
            return _kdtnParser.ParseVersionAndType(_deviceInformationCommand.ReadVersionTypeAndType(_serialNumber));
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
        public ElectricalIndicators ReadElectricalIndicators(int paramType)
        {
            return _kdtnParser.ParseElectricalIndicators(_serviceCommand.ReadElectricalIndicators(_serialNumber, paramType));
        }
        public int ReadWorkTimeSeconds()
        {
            return _kdtnParser.ParseWorkTimeSeconds(_deviceInformationCommand.ReadWorkTime(_serialNumber));
        }
    }
}
