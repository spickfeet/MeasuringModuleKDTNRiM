using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib.Models.CRC
{
    public interface ICRC
    {
        int CRCLength { get; }
        void AddCRC(byte[] data);
        bool CheckCRC(byte[] data);
    }
}
