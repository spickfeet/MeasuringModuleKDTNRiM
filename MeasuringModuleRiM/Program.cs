using MeasuringModuleRiM;
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
        var serial = new SerialPort("COM5", 57600);
        serial.Handshake = Handshake.None;
        serial.Parity = Parity.None;
        serial.DataBits = 8;
        serial.StopBits = StopBits.One;
        serial.ReadTimeout = 2000;

        ICRC crc = new ModbusCRC16();
        IDeviceCommunication deviceCommunication = new DeviceCommunicationRS485(serial);

        KDTN kdtn = new(deviceCommunication, crc, 44922);

        byte[] data;
        
        kdtn.StartCommunication();

        Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(kdtn.WriteSerialNumber(44922))}");

        // Ввод пароля для чтения
        data =  kdtn.EnterReadPassword("");
        Console.WriteLine($"Получено {data.Length} байт: {BitConverter.ToString(data)}");

        // Ввод пароля для записи
        data = kdtn.EnterWritePassword("");
        Console.WriteLine($"Получено {data.Length} байт: {BitConverter.ToString(data)}");

        Console.WriteLine();

        // Чтение версии и типа устройства
        double versions;
        string type;
        VersionAndType versionAndType = kdtn.ReadVersionTypeAndType();
        Console.WriteLine($"Versions: {versionAndType.versions}");
        Console.WriteLine($"Type: {versionAndType.type}");

        Console.WriteLine();

        // Чтение счётчика наработки
        TimeSpan timeSpan = kdtn.ReadWorkTime();
        Console.WriteLine($"WorkTime {timeSpan.Days} дней {timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");

        Console.WriteLine();

        // Чтение электрических показателей
        ElectricalIndicators electricalIndicators = kdtn.ReadElectricalIndicators(6);
        Console.WriteLine("settingsType " + electricalIndicators.settingsType);
        Console.WriteLine("totalOrNonPhaseIndication " + electricalIndicators.totalOrNonPhaseIndication);
        Console.WriteLine("phaseAOrNonPhaseIndication " + electricalIndicators.phaseAOrNonPhaseIndication);
        Console.WriteLine("phaseBOrNonPhaseIndication " + electricalIndicators.phaseBOrNonPhaseIndication);
        Console.WriteLine("phaseCOrNonPhaseIndication " + electricalIndicators.phaseCOrNonPhaseIndication);

        Console.WriteLine();

        // Обновление серийного номера
        // Запись серийного номера
        Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(kdtn.WriteSerialNumber(123))}");
        // Чтение серийного номера
        Console.WriteLine($"SerialNumber {kdtn.ReadSerialNumber()}");
        // Проверка работы с новым серийным номером
        timeSpan = kdtn.ReadWorkTime();
        Console.WriteLine($"WorkTime {timeSpan.Days} дней {timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");

        // Возвращение старого значения у серийного номера
        // Запись серийного номера
        Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(kdtn.WriteSerialNumber(44922))}");
        // Чтение серийного номера
        Console.WriteLine($"SerialNumber {kdtn.ReadSerialNumber()}");

        Console.WriteLine();

        Console.WriteLine($"RFSignalLevel {kdtn.ReadRFSignalLevel()}");
        Console.WriteLine();

        // Обновление настроек RF-канала модуля
        // Запись настроек RF-канала модуля
        Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(kdtn.WriteRFSettings(1,0))}");
        Console.WriteLine();

        // Чтение настроек RF-канала модуля
        RFSettings rFSettings = kdtn.ReadRFSettings();
        Console.WriteLine("ReadRFSettings");
        Console.WriteLine($"channelNumber {rFSettings.channelNumber}");
        Console.WriteLine($"power {rFSettings.power} dBm");
        Console.WriteLine();

        // Возврат настроек RF-канала модуля
        // Запись настроек RF-канала модуля
        Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(kdtn.WriteRFSettings(1, 7))}");
        Console.WriteLine();

        // Чтение настроек RF-канала модуля
        rFSettings = kdtn.ReadRFSettings();
        Console.WriteLine("ReadRFSettings");
        Console.WriteLine($"channelNumber {rFSettings.channelNumber}");
        Console.WriteLine($"power {rFSettings.power} dBm");
        Console.WriteLine();

        // Чтение даты калибровки модуля
        DateTime calibrationDate = kdtn.ReadCalibrationDate();
        Console.WriteLine($"CalibrationDate {calibrationDate}");

        // Обновление даты калибровки модуля
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationDate(DateTime.Now))}");
        Console.WriteLine($"CalibrationDate {kdtn.ReadCalibrationDate()}");

        // Возврат даты калибровки модуля
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationDate(calibrationDate))}");
        Console.WriteLine($"CalibrationDate {kdtn.ReadCalibrationDate()}");
        Console.WriteLine();


        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(0, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(1, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(2, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(3, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(4, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(5, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(6, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(7, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(8, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(9, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(10, 0))}");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(11, 200))}");
        Console.WriteLine();

        Console.WriteLine("Чтение калибровочных констант (калибровка)");
        Console.WriteLine($"ReadCalibrationConst 0: {kdtn.ReadCalibrationConst(0)}");
        Console.WriteLine($"ReadCalibrationConst 1: {kdtn.ReadCalibrationConst(1)}");
        Console.WriteLine($"ReadCalibrationConst 2: {kdtn.ReadCalibrationConst(2)}");
        Console.WriteLine($"ReadCalibrationConst 3: {kdtn.ReadCalibrationConst(3)}");
        Console.WriteLine($"ReadCalibrationConst 4: {kdtn.ReadCalibrationConst(4)}");
        Console.WriteLine($"ReadCalibrationConst 5: {kdtn.ReadCalibrationConst(5)}");
        Console.WriteLine($"ReadCalibrationConst 6: {kdtn.ReadCalibrationConst(6)}");
        Console.WriteLine($"ReadCalibrationConst 7: {kdtn.ReadCalibrationConst(7)}");
        Console.WriteLine($"ReadCalibrationConst 8: {kdtn.ReadCalibrationConst(8)}");
        Console.WriteLine($"ReadCalibrationConst 9: {kdtn.ReadCalibrationConst(9)}");
        Console.WriteLine($"ReadCalibrationConst 10: {kdtn.ReadCalibrationConst(10)}");
        Console.WriteLine($"ReadCalibrationConst 11: {kdtn.ReadCalibrationConst(11)}");
        Console.WriteLine();

        Console.WriteLine("Сброс констант");
        Console.WriteLine($"WriteCalibrationDate {BitConverter.ToString(kdtn.WriteCalibrationConst(255, 0))}");
        Console.WriteLine();

        Console.WriteLine("Чтение калибровочных констант (калибровка)");
        Console.WriteLine($"ReadCalibrationConst 0: {kdtn.ReadCalibrationConst(0)}");
        Console.WriteLine($"ReadCalibrationConst 1: {kdtn.ReadCalibrationConst(1)}");
        Console.WriteLine($"ReadCalibrationConst 2: {kdtn.ReadCalibrationConst(2)}");
        Console.WriteLine($"ReadCalibrationConst 3: {kdtn.ReadCalibrationConst(3)}");
        Console.WriteLine($"ReadCalibrationConst 4: {kdtn.ReadCalibrationConst(4)}");
        Console.WriteLine($"ReadCalibrationConst 5: {kdtn.ReadCalibrationConst(5)}");
        Console.WriteLine($"ReadCalibrationConst 6: {kdtn.ReadCalibrationConst(6)}");
        Console.WriteLine($"ReadCalibrationConst 7: {kdtn.ReadCalibrationConst(7)}");
        Console.WriteLine($"ReadCalibrationConst 8: {kdtn.ReadCalibrationConst(8)}");
        Console.WriteLine($"ReadCalibrationConst 9: {kdtn.ReadCalibrationConst(9)}");
        Console.WriteLine($"ReadCalibrationConst 10: {kdtn.ReadCalibrationConst(10)}");
        Console.WriteLine($"ReadCalibrationConst 11: {kdtn.ReadCalibrationConst(11)}");
        Console.WriteLine();

        Console.WriteLine($"RestartMeasurements {BitConverter.ToString(kdtn.RestartMeasurements())}");




        kdtn.StartCommunication();

    }
}