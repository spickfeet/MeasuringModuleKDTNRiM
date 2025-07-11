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
        TimeSpan timeSpan = TimeSpan.FromSeconds(kdtn.ReadWorkTimeSeconds());
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
        timeSpan = TimeSpan.FromSeconds(kdtn.ReadWorkTimeSeconds());
        Console.WriteLine($"WorkTime {timeSpan.Days} дней {timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}");

        // Возвращение старого значения у серийного номера
        // Запись серийного номера
        Console.WriteLine($"WriteSerialNumber {BitConverter.ToString(kdtn.WriteSerialNumber(44922))}");
        // Чтение серийного номера
        Console.WriteLine($"SerialNumber {kdtn.ReadSerialNumber()}");

        Console.WriteLine();

        Console.WriteLine($"RFSignalLevel {kdtn.ReadRFSignalLevel()}");

        kdtn.StartCommunication();

    }
}