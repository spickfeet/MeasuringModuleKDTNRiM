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
    static void Main(string[] args)
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

        kdtn.StartCommunication();

    }
}