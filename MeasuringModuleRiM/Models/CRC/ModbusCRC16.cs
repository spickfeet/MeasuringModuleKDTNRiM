using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.CRC
{
    public class ModbusCRC16 : ICRC
    {
        public byte[] AddCRC(byte[] data)
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
            data[data.Length - 2] = (byte)(crc & 0xFF);
            data[data.Length - 1] = (byte)((crc >> 8) & 0xFF);
            return data;
        }
    }
}
