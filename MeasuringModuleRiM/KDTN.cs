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
        private RFSettingsCommand _rfSettingsCommand;
        private CalibrationCommand _calibrationCommand;
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
            _rfSettingsCommand = new(crc, deviceCommunication);
            _calibrationCommand = new(crc, deviceCommunication);
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

        public TimeSpan ReadWorkTime()
        {
            return TimeSpan.FromSeconds(_kdtnParser.ParseTimeSeconds(_deviceInformationCommand.ReadWorkTimeSeconds(_serialNumber)));
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

        public int ReadSerialNumber()
        {
            return _kdtnParser.ParseSerialNumber(_serviceCommand.ReadSerialNumber());
        }

        public byte[] WriteSerialNumber(int serialNumber)
        {
            byte[] data = _serviceCommand.WriteSerialNumber(serialNumber);
            // Замена подстановка серийного номера после записи
            byte[] newSerialNumber = BitConverter.GetBytes(serialNumber);
            Array.Copy(newSerialNumber, 0, _serialNumber, 0, _serialNumber.Length);

            return data;
        }

        public float ReadRFSignalLevel()
        {
            return _kdtnParser.ParseRFSignalLevel(_rfSettingsCommand.ReadRFSignalLevel(_serialNumber));
        }

        public RFSettings ReadRFSettings()
        {
            return _kdtnParser.ParseRFSettings(_rfSettingsCommand.ReadRFSettings(_serialNumber));
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
        /// <param name="channelNumber"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public byte[] WriteRFSettings(int channelNumber, int power) 
        {
            return _rfSettingsCommand.WriteRFSettings(_serialNumber, channelNumber, power);
        }

        public int ReadCalibrationTimeSeconds()
        {
            return _kdtnParser.ParseTimeSeconds(_deviceInformationCommand.ReadWorkTimeSeconds(_serialNumber));
        }
        public DateTime ReadCalibrationDate()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_kdtnParser.ParseTimeSeconds(_calibrationCommand.ReadCalibrationDate(_serialNumber)));
            DateTime baseDate = new(2000, 1, 1);
            return baseDate.Add(timeSpan);

        }
    }
}
