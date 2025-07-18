using MeasuringModuleRiM;
using MeasuringModuleRiM.Exceptions;
using MeasuringModuleRiM.Models.CRC;
using MeasuringModuleRiM.Models.Data;
using MeasuringModuleRiM.Models.DeviceCommunications;
using System;
using System.IO.Ports;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SerialPortExample
{
    private static void Main(string[] args)
    {
        RiM384 rim384;
        while (true)
        {
            Console.WriteLine("Команды");
            Console.WriteLine("1 - GSM");
            Console.WriteLine("2 - RS-485");
            Console.Write("Команда: ");
            int protocolCode = int.Parse(Console.ReadLine());

            if (protocolCode == 1)
            {
                var serial = new SerialPort("COM6", 4800);
                serial.Handshake = Handshake.None;
                serial.Parity = Parity.None;
                serial.DataBits = 8;
                serial.StopBits = StopBits.One;
                serial.ReadTimeout = 4000;

                ICRC crc = new ModbusCRC16();
                IDeviceCommunication deviceCommunication = new DeviceCommunicationGSM(serial, "89069965121");

                rim384 = new(deviceCommunication, crc, 44922);
                break;
            }
            else if (protocolCode == 2)
            {
                var serial = new SerialPort("COM5", 57600);
                serial.Handshake = Handshake.None;
                serial.Parity = Parity.None;
                serial.DataBits = 8;
                serial.StopBits = StopBits.One;
                serial.ReadTimeout = 2000;

                ICRC crc = new ModbusCRC16();
                IDeviceCommunication deviceCommunication = new DeviceCommunicationRS485(serial);

                rim384 = new(deviceCommunication, crc, 44922);
                break;
            }
            else
            {
                continue;
            }
        }
        try
        {
            for (int i = 0; i < 1000; i++)
            {
                byte[] data;
                byte[] lastSend;
                byte[] lastReceive;
                Console.WriteLine("Подключение");
                rim384.StartCommunication();
                Console.WriteLine("Успешно");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                //Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(rim384.WriteSerialNumber(44922))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                //// Ввод пароля для чтения
                data = rim384.EnterReadPassword("");
                Console.WriteLine($"Получено {data.Length} байт: {BitConverter.ToString(data)}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");


                // Ввод пароля для записи
                data = rim384.EnterWritePassword("");
                Console.WriteLine($"Получено {data.Length} байт: {BitConverter.ToString(data)}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine();

                // Чтение версии и типа устройства
                double versions;
                string type;
                VersionAndType versionAndType = rim384.ReadVersionTypeAndType();
                Console.WriteLine($"Versions: {versionAndType.versions}");
                Console.WriteLine($"Type: {versionAndType.type}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine();

                // Чтение счётчика наработки
                TimeSpan timeSpan = rim384.ReadWorkTime();
                Console.WriteLine($"WorkTime {timeSpan.Days} дней {timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine();

                // Чтение электрических показателей
                ElectricalIndicators electricalIndicators = rim384.ReadElectricalIndicators(6);
                Console.WriteLine("settingsType " + electricalIndicators.settingsType);
                Console.WriteLine("totalOrNonPhaseIndication " + electricalIndicators.totalOrNonPhaseIndication);
                Console.WriteLine("phaseAOrNonPhaseIndication " + electricalIndicators.phaseAOrNonPhaseIndication);
                Console.WriteLine("phaseBOrNonPhaseIndication " + electricalIndicators.phaseBOrNonPhaseIndication);
                Console.WriteLine("phaseCOrNonPhaseIndication " + electricalIndicators.phaseCOrNonPhaseIndication);
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine();

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Обновление серийного номера
                // Запись серийного номера
                //Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(rim384.WriteSerialNumber(123))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Чтение серийного номера
                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Проверка работы с новым серийным номером
                timeSpan = rim384.ReadWorkTime();
                Console.WriteLine($"WorkTime {timeSpan.Days} дней {timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Возвращение старого значения у серийного номера
                // Запись серийного номера
                //Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(rim384.WriteSerialNumber(44922))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Чтение серийного номера
                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine();

                Console.WriteLine($"RFSignalLevel {rim384.ReadRFSignalLevel()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                Console.WriteLine();

                // Обновление настроек RF-канала модуля
                // Запись настроек RF-канала модуля
                Console.WriteLine($"WriteRFSettings {BitConverter.ToString(rim384.WriteRFSettings(1, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                Console.WriteLine();

                // Чтение настроек RF-канала модуля
                RFSettings rFSettings = rim384.ReadRFSettings();
                Console.WriteLine("ReadRFSettings");
                Console.WriteLine($"channelNumber {rFSettings.channelNumber}");
                Console.WriteLine($"power {rFSettings.power} dBm");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                Console.WriteLine();

                // Возврат настроек RF-канала модуля
                // Запись настроек RF-канала модуля
                Console.WriteLine($"WriteRFSettings {BitConverter.ToString(rim384.WriteRFSettings(1, 7))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                Console.WriteLine();

                // Чтение настроек RF-канала модуля
                rFSettings = rim384.ReadRFSettings();
                Console.WriteLine("ReadRFSettings");
                Console.WriteLine($"channelNumber {rFSettings.channelNumber}");
                Console.WriteLine($"power {rFSettings.power} dBm");
                Console.WriteLine();

                // Чтение даты калибровки модуля
                DateTime calibrationDate = rim384.ReadCalibrationDate();
                Console.WriteLine($"CalibrationDate {calibrationDate}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Обновление даты калибровки модуля
                Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(rim384.WriteCalibrationDate(DateTime.Now))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"CalibrationDate {rim384.ReadCalibrationDate()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Возврат даты калибровки модуля
                Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(rim384.WriteCalibrationDate(calibrationDate))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                Console.WriteLine( );
                Console.WriteLine($"CalibrationDate {rim384.ReadCalibrationDate()}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                Console.WriteLine();

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(0, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(1, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(2, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(3, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(4, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(5, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(6, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(7, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(8, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(9, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(10, 0))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(11, 200))}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                Console.WriteLine();

                Console.WriteLine("Чтение калибровочных констант (калибровка)");
                for (int j = 0; j < 12; j++)
                {
                    Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                    Console.WriteLine($"ReadCalibrationConst {j}: {rim384.ReadCalibrationConst(j)}");
                    (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                    Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                    Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                }

                Console.WriteLine();

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine("Сброс констант");
                Console.WriteLine($"WriteCalibrationConst {BitConverter.ToString(rim384.WriteCalibrationConst(255, 0))}");
                Console.WriteLine();

                for (int j = 0; j < 12; j++)
                {
                    Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                    Console.WriteLine($"ReadCalibrationConst {j}: {rim384.ReadCalibrationConst(j)}");
                    (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                    Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                    Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                }

                Console.WriteLine();

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                Console.WriteLine($"RestartMeasurements {BitConverter.ToString(rim384.RestartMeasurements())}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");
                Console.WriteLine();

                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                ServiceParameters serviceParameters = rim384.ReadServiceParameters();
                Console.WriteLine("serviceParameters");
                Console.WriteLine($"supercapacitorVoltage {serviceParameters.supercapacitorVoltage}");
                Console.WriteLine($"supercapacitorVoltage {serviceParameters.supplyVoltage}");
                Console.WriteLine($"supercapacitorVoltage {serviceParameters.temperature}");
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");


                Console.WriteLine($"SerialNumber {rim384.ReadSerialNumber()}");
                MeasuredValues? measuredValues = rim384.ReadMeasuredValues();
                if (measuredValues != null)
                {
                    Console.WriteLine($"activePower {measuredValues.Value.activePower}");
                    Console.WriteLine($"reactivePower {measuredValues.Value.reactivePower}");
                    Console.WriteLine($"rmsVoltage {measuredValues.Value.rmsVoltage}");
                    Console.WriteLine($"rmsCurrent {measuredValues.Value.rmsCurrent}");
                    Console.WriteLine($"frequency {measuredValues.Value.frequency}");
                }
                else
                {
                    Console.WriteLine("measuredValues = NULL");
                }
                (lastSend, lastReceive) = rim384.GetLastSendAndReceiveBytes();
                Console.WriteLine($"Send = {BitConverter.ToString(lastSend)}");
                Console.WriteLine($"Receive = {BitConverter.ToString(lastReceive)}");

                // Заглушка
                rim384.ReadCurrentTimeValue();
            }
        }
        catch (RiM384Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally 
        {
            rim384.StopCommunication();
        }

    }
}