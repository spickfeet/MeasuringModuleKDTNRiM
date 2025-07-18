using RiM384Lib.Exceptions;
using RiM384Lib.Models.CRC;
using RiM384Lib.Models.Data;
using RiM384Lib.Models.DeviceCommands;
using RiM384Lib.Models.DeviceCommunications;
using RiM384Lib.Parsers;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib
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
        private byte[] _lastReceive;
        private byte[] _lastSend;

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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _passwordCommand.EnterReadPassword(_serialNumber, password);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return receiveBytes;
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _passwordCommand.EnterWritePassword(_serialNumber, password);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return receiveBytes;
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _deviceInformationCommand.ReadVersionTypeAndType(_serialNumber);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return _rim384Parser.ParseVersionAndType(receiveBytes);
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _deviceInformationCommand.ReadWorkTimeSeconds(_serialNumber);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return TimeSpan.FromSeconds(_rim384Parser.ParseTimeSeconds(receiveBytes));
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _serviceCommand.ReadElectricalIndicators(_serialNumber, paramType);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return _rim384Parser.ParseElectricalIndicators(receiveBytes);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public int ReadSerialNumber()
        {
            byte[] receiveBytes;
            byte[] sendBytes;
            (receiveBytes, sendBytes) = _serviceCommand.ReadSerialNumber();
            _lastReceive = receiveBytes;
            _lastSend = sendBytes;
            return _rim384Parser.ParseSerialNumber(receiveBytes);
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _serviceCommand.WriteSerialNumber(serialNumber);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;

                // Сравнение желаемого нового серийного номера и фактически установленного
                byte[] receiveSerialNumberBytes = receiveBytes.Take(3).ToArray();
                int receiveSerialNumber = BitConverter.ToInt32([receiveSerialNumberBytes[0], receiveSerialNumberBytes[1], receiveSerialNumberBytes[2], 0]);

                if (serialNumber != receiveSerialNumber)
                {
                    throw new Exception($"Ошибка смены серийного номера: введен {serialNumber}, установлен {receiveSerialNumber}");
                }

                // Замена подстановка серийного номера после записи
                byte[] newSerialNumber = BitConverter.GetBytes(serialNumber);
                Array.Copy(newSerialNumber, 0, _serialNumber, 0, _serialNumber.Length);

                return receiveBytes;
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _rfSettingsCommand.ReadRFSignalLevel(_serialNumber);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return _rim384Parser.ParseRFSignalLevel(receiveBytes);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public RFSettings ReadRFSettings()
        {
            byte[] receiveBytes;
            byte[] sendBytes;
            (receiveBytes, sendBytes) = _rfSettingsCommand.ReadRFSettings(_serialNumber);
            _lastReceive = receiveBytes;
            _lastSend = sendBytes;
            return _rim384Parser.ParseRFSettings(receiveBytes);
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _rfSettingsCommand.WriteRFSettings(_serialNumber, channelNumber, powerCode);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return receiveBytes;
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _calibrationCommand.ReadCalibrationDate(_serialNumber);

                _lastReceive = receiveBytes;
                _lastSend = sendBytes;

                TimeSpan timeSpan = TimeSpan.FromSeconds(_rim384Parser.ParseTimeSeconds(receiveBytes));
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _calibrationCommand.WriteCalibrationDate(_serialNumber, dateTime);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return receiveBytes;
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _calibrationCommand.ReadCalibrationConst(_serialNumber, constPtr);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return _rim384Parser.ParseCalibrationConst(receiveBytes);
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _calibrationCommand.WriteCalibrationConst(_serialNumber, constPtr, constValue);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return receiveBytes;
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _calibrationCommand.RestartMeasurements(_serialNumber);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return receiveBytes;
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _serviceCommand.ReadServiceParameters(_serialNumber);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return _rim384Parser.ParseServiceParameters(receiveBytes);
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
                byte[] receiveBytes;
                byte[] sendBytes;
                (receiveBytes, sendBytes) = _calibrationCommand.ReadMeasuredValues(_serialNumber);
                _lastReceive = receiveBytes;
                _lastSend = sendBytes;
                return _rim384Parser.ParseMeasuredValues(receiveBytes);
            }
            catch (Exception e)
            {
                throw new RiM384Exception(e.Message);
            }
        }

        public (byte[], byte[]) GetLastSendAndReceiveBytes()
        {
            return (_lastSend, _lastReceive);
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
