using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.DeviceCommunications
{
    public class DeviceCommunicationGSM : IDeviceCommunication, IDisposable
    {
        private SerialPort _serial;
        private string _phoneNumber;
        private byte _requestNumber;
        public DeviceCommunicationGSM(SerialPort serial, string phoneNumber)
        {
            _serial = serial;
            _phoneNumber = phoneNumber;
        }

        ~DeviceCommunicationGSM()
        {
            Dispose();
        }

        public byte[] SendCommand(byte[] bytesMessage)
        {
            // --- Шаг 1: Добавляем номер запроса ---
            byte[] packetWithNumber = new byte[bytesMessage.Length + 1];
            packetWithNumber[0] = _requestNumber;
            Buffer.BlockCopy(bytesMessage, 0, packetWithNumber, 1, bytesMessage.Length);

            // --- Шаг 2: Кодируем каждый байт в ASCII-тетрады (младшая первой) ---
            byte[] encodedPacket = EncodeTetrad(packetWithNumber);

            // --- Шаг 3: Добавляем CR+LF в начало и конец ---
            byte[] finalPacket = AddCrLf(encodedPacket); // или encodedPacket, если без реверса

            // --- Шаг 4: Отправляем ---
            _serial.Write(finalPacket, 0, finalPacket.Length);

            // --- Шаг 5: Формируем ответ ---

            byte[] header = ReceiveFixedSize(14);
            int dataLength = DecodeTetrad([header[12], header[13]])[0] * 2 + 2;
             
            if (dataLength > 5)
            {
                byte[] data = ReceiveFixedSize(dataLength);

                // Объединяем заголовок и данные в один массив
                byte[] packet = new byte[header.Length + dataLength];
                Buffer.BlockCopy(header, 0, packet, 0, header.Length);
                Buffer.BlockCopy(data, 0, packet, header.Length, dataLength);

                // --- Шаг 6: Декодируем ответ ---
                byte[] decodedResponse = DecodeResponse(packet);

                // --- Шаг 7: Проверяем номер запроса ---
                if (decodedResponse.Length > 0 && decodedResponse[0] != _requestNumber)
                {
                    throw new Exception("Несовпадение номера запроса в ответе.");
                }

                byte[] result = new byte[decodedResponse.Length - 1];
                Array.Copy(decodedResponse, 1, result, 0, result.Length);

                // --- Шаг 8: Увеличиваем номер запроса ---
                _requestNumber = (byte)((_requestNumber + 1) % 256);

                return result;
            }
            throw new Exception("Пакет данных получен неполным.");
        }

        private byte[] ReceiveFixedSize(int nbytes)
        {
            var buf = new byte[nbytes];
            var readPos = 0;
            while (readPos < nbytes)
            {
                int read = _serial.BaseStream.Read(buf, readPos, nbytes - readPos);
                readPos += read;
            }
            return buf;
        }

        private byte[] EncodeTetrad(byte[] data)
        {
            byte[] result = new byte[data.Length * 2];

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                result[i * 2] = (byte)"0123456789ABCDEF"[b & 0x0F];     // младшая тетрада
                result[i * 2 + 1] = (byte)"0123456789ABCDEF"[(b >> 4)]; // старшая тетрада
            }

            return result;
        }

        private byte[] DecodeTetrad(byte[] data)
        {
            if (data.Length % 2 != 0)
                throw new ArgumentException("Длина данных для декодера тетрад должна быть чётной.");

            byte[] result = new byte[data.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                byte high = HexToByte(data[i * 2 + 1]);
                byte low = HexToByte(data[i * 2]);
                result[i] = (byte)((high << 4) | low);
            }

            return result;
        }

        private byte HexToByte(byte value)
        {
            if (value >= 0x30 && value <= 0x39) return (byte)(value - 0x30);
            if (value >= 0x41 && value <= 0x46) return (byte)(value - 0x37);
            if (value >= 0x61 && value <= 0x66) return (byte)(value - 0x57);
            throw new ArgumentException("Некорректный шестнадцатеричный символ.");
        }

        private byte[] AddCrLf(byte[] data)
        {
            byte[] result = new byte[data.Length + 4]; // 2 байта в начале и 2 в конце
            result[0] = 0x0D; // \r
            result[1] = 0x0A; // \n

            Buffer.BlockCopy(data, 0, result, 2, data.Length);

            result[^2] = 0x0D;
            result[^1] = 0x0A;

            return result;
        }


        private byte[] DecodeResponse(byte[] rawData)
        {
            // Удаляем CR+LF
            List<byte> cleaned = new();
            for (int i = 0; i < rawData.Length; i++)
            {
                byte b = rawData[i];
                if (b != 0x0D && b != 0x0A)
                    cleaned.Add(b);
            }

            // Декодируем тетрады
            return DecodeTetrad(cleaned.ToArray());
        }
        public void StartCommunication()
        {
            if (!_serial.IsOpen)
            {
                _serial.Open();
                SendAtCommand("AT+CBST=0,0,1\r\n", "OK", 10000);
                SendAtCommand($"ATD{_phoneNumber}\r\n", "CONNECT", 30000);
            }
        }

        private void SendAtCommand(string command, string expectedResponse, int timeoutMs)
        {
            byte[] cmdBytes = Encoding.ASCII.GetBytes(command);
            _serial.Write(cmdBytes, 0, cmdBytes.Length);
            if(expectedResponse == "")
            {
                return;
            }

            DateTime startTime = DateTime.Now;
            StringBuilder responseBuilder = new StringBuilder();

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                Thread.Sleep(100); // Небольшая пауза между попытками

                int available = _serial.BytesToRead;
                if (available == 0) continue;

                byte[] buffer = new byte[available];
                _serial.Read(buffer, 0, available);

                string response = Encoding.ASCII.GetString(buffer);
                responseBuilder.Append(response);

                string fullResponse = responseBuilder.ToString().ToUpper();

                if (fullResponse.Contains(expectedResponse.ToUpper()))
                {
                    Console.WriteLine($"Команда '{command.Trim()}' выполнена успешно.");
                    return;
                }

                if (fullResponse.Contains("ERROR"))
                {
                    throw new Exception($"Ошибка при выполнении команды: {command}.");
                }
            }

            throw new TimeoutException($"Таймаут ожидания ответа на команду: {command}. Ожидалось: {expectedResponse}.");
        }

        public void StopCommunication()
        {
            if (_serial.IsOpen)
            {
                Thread.Sleep(1500);
                SendAtCommand("+++", "OK", 10000);
                Thread.Sleep(1500);
                SendAtCommand("ATH\r\n", "", 10000);
                _serial.Close();
            }
        }

        public void Dispose()
        {
            StopCommunication();
        }
    }
}
