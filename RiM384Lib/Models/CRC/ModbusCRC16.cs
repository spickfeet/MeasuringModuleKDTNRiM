using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RiM384Lib.Models.CRC
{
    public class ModbusCRC16 : ICRC
    {
        public int CRCLength { get; private set; } = 2;
        public void AddCRC(byte[] data)
        {
            (data[data.Length - 2], data[data.Length - 1]) = CalcCRC(data);
        }

        public bool CheckCRC(byte[] data)
        {
            byte crcByte1;
            byte crcByte2;
            (crcByte1, crcByte2) = CalcCRC(data);
            if(crcByte1 == data[data.Length - 2] && crcByte2 == data[data.Length - 1]) return true;
            return false;
        }
        private (byte, byte) CalcCRC(byte[] data)
        {
            ushort crc = 0xFFFF;

            for (int i = 0; i < data.Length - 2; i++)
            {
                crc ^= data[i];

                for (int j = 0; j < 8; j++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;

                    if (lsb)
                        crc ^= 0xA001;
                }
            }
            return ((byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF));
        }
    }
}
