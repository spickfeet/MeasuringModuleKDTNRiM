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
        /// <param name="channelNumber"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public byte[] WriteRFSettings(int channelNumber, int powerCode) 
        {
            return _rfSettingsCommand.WriteRFSettings(_serialNumber, channelNumber, powerCode);
        }

        public DateTime ReadCalibrationDate()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(_kdtnParser.ParseTimeSeconds(_calibrationCommand.ReadCalibrationDate(_serialNumber)));
            DateTime baseDate = new(2000, 1, 1);
            return baseDate.Add(timeSpan);
        }

        public byte[] WriteCalibrationDate(DateTime dateTime)
        {
            return _calibrationCommand.WriteCalibrationDate(_serialNumber, dateTime);
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
        public int ReadCalibrationConst(int constPtr)
        {
            return _kdtnParser.ParseCalibrationConst(_calibrationCommand.ReadCalibrationConst(_serialNumber, constPtr));
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
        public byte[] WriteCalibrationConst(int constPtr, int constValue)
        {
            return _calibrationCommand.WriteCalibrationConst(_serialNumber, constPtr, constValue);
        }

        public byte[] RestartMeasurements()
        {
            return _calibrationCommand.RestartMeasurements(_serialNumber);
        }

        public ServiceParameters ReadServiceParameters()
        {
            return _kdtnParser.ParseServiceParameters(_serviceCommand.ReadServiceParameters(_serialNumber));
        }

        /// <summary>
        /// Если не истёк интервал измерения/усреднения параметров (~ 1сек.), то метод вернет null.
        /// </summary>
        /// <returns></returns>
        public MeasuredValues? ReadMeasuredValues()
        {
            return _kdtnParser.ParseMeasuredValues(_calibrationCommand.ReadMeasuredValues(_serialNumber));
        }
    }
}
