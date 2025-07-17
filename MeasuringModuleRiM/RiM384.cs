using MeasuringModuleRiM.Exceptions;
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
    /// <summary>
    /// Исключения бросаемые данной библиотекой это RiM384Exception
    /// </summary>
    public class RiM384
    {
        private PasswordCommand _passwordCommand;
        private DeviceInformationCommand _deviceInformationCommand;
        private ServiceCommand _serviceCommand;
        private RFSettingsCommand _rfSettingsCommand;
        private CalibrationCommand _calibrationCommand;
        private byte[] _serialNumber;
        private IDeviceCommunication _deviceCommunication;
        private RiM384Parser _rim384Parser;
        /// <summary>
        /// serialNumber от 0 до 16777215
        /// </summary>
        /// <param name="deviceCommunication"></param>
        /// <param name="crc"></param>
        /// <param name="serialNumber"></param>
        public RiM384(IDeviceCommunication deviceCommunication, ICRC crc, int serialNumber)
        {
            try
            {
                if (serialNumber < 0 || serialNumber > 16777215)
                {
                    throw new ArgumentException("Серийный номер должен быть от 0 до 16772987");
                }
                byte[] byteArray = BitConverter.GetBytes(serialNumber);
                _serialNumber = [byteArray[0], byteArray[1], byteArray[2]];
                _deviceCommunication = deviceCommunication;
                _rim384Parser = new RiM384Parser();


                _passwordCommand = new(crc, deviceCommunication);
                _deviceInformationCommand = new(crc, deviceCommunication);
                _serviceCommand = new(crc, deviceCommunication);
                _rfSettingsCommand = new(crc, deviceCommunication);
                _calibrationCommand = new(crc, deviceCommunication);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }
        public void StartCommunication()
        {
            try
            {
                _deviceCommunication.StartCommunication();
            }
            catch (Exception e) 
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public void StopCommunication()
        {
            try
            {
                _deviceCommunication.StopCommunication();
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public byte[] EnterReadPassword(string password) 
        {
            try
            {
                return _passwordCommand.EnterReadPassword(_serialNumber, password);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public byte[] EnterWritePassword(string password)
        {
            try
            {
                return _passwordCommand.EnterWritePassword(_serialNumber, password);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public VersionAndType ReadVersionTypeAndType()
        {
            try
            {
                return _rim384Parser.ParseVersionAndType(_deviceInformationCommand.ReadVersionTypeAndType(_serialNumber));
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public TimeSpan ReadWorkTime()
        {
            try
            {
                return TimeSpan.FromSeconds(_rim384Parser.ParseTimeSeconds(_deviceInformationCommand.ReadWorkTimeSeconds(_serialNumber)));
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
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
            try
            {
                return _rim384Parser.ParseElectricalIndicators(_serviceCommand.ReadElectricalIndicators(_serialNumber, paramType));
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public int ReadSerialNumber()
        {
            return _rim384Parser.ParseSerialNumber(_serviceCommand.ReadSerialNumber());
        }

        /// <summary>
        /// Изменить серийный номер на значение от 0 до 16777215
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] WriteSerialNumber(int serialNumber)
        {
            try
            {
                byte[] data = _serviceCommand.WriteSerialNumber(serialNumber);
                // Замена подстановка серийного номера после записи
                byte[] newSerialNumber = BitConverter.GetBytes(serialNumber);
                Array.Copy(newSerialNumber, 0, _serialNumber, 0, _serialNumber.Length);

                return data;
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public float ReadRFSignalLevel()
        {
            try
            {
                return _rim384Parser.ParseRFSignalLevel(_rfSettingsCommand.ReadRFSignalLevel(_serialNumber));
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public RFSettings ReadRFSettings()
        {
            return _rim384Parser.ParseRFSettings(_rfSettingsCommand.ReadRFSettings(_serialNumber));
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
            try
            {
                return _rfSettingsCommand.WriteRFSettings(_serialNumber, channelNumber, powerCode);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public DateTime ReadCalibrationDate()
        {
            try
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(_rim384Parser.ParseTimeSeconds(_calibrationCommand.ReadCalibrationDate(_serialNumber)));
                DateTime baseDate = new(2000, 1, 1);
                return baseDate.Add(timeSpan);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public byte[] WriteCalibrationDate(DateTime dateTime)
        {
            try
            {
                return _calibrationCommand.WriteCalibrationDate(_serialNumber, dateTime);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
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
            try
            {
                return _rim384Parser.ParseCalibrationConst(_calibrationCommand.ReadCalibrationConst(_serialNumber, constPtr));
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
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
            try
            {
                return _calibrationCommand.WriteCalibrationConst(_serialNumber, constPtr, constValue);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public byte[] RestartMeasurements()
        {
            try
            {
                return _calibrationCommand.RestartMeasurements(_serialNumber);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public ServiceParameters ReadServiceParameters()
        {
            try
            {
                return _rim384Parser.ParseServiceParameters(_serviceCommand.ReadServiceParameters(_serialNumber));
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        /// <summary>
        /// Если не истёк интервал измерения/усреднения параметров (~ 1сек.), то метод вернет null.
        /// </summary>
        /// <returns></returns>
        public MeasuredValues? ReadMeasuredValues()
        {
            try
            {
                return _rim384Parser.ParseMeasuredValues(_calibrationCommand.ReadMeasuredValues(_serialNumber));
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        /// <summary>
        /// Заглушка т.к. результат команды не соответствует документации
        /// Send: 7A AF 00 08 02 5A DF
        /// Результат:
        /// Receive: 7A AF 00 08 03 60 5E 83
        /// Ожидание: Счетчик байт(07H) Статус(1 байт), Время в секундах от 01.01.2000 (4 байта), CRC(2 байта)
        /// </summary>
        /// <param name="serialNumber"></param>
        public void ReadCurrentTimeValue()
        {
            try
            {
                _serviceCommand.ReadCurrentTimeValue(_serialNumber);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }
    }
}
