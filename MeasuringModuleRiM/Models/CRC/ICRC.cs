using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasuringModuleRiM.Models.CRC
{
    public interface ICRC
    {
        void AddCRC(byte[] data);
        bool CheckCRC(byte[] data);
    }
}
