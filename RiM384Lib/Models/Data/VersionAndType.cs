using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiM384Lib.Models.Data
{
    public struct VersionAndType
    {
        public readonly double versions;
        public readonly string type;
        public VersionAndType(double versions, string type)
        {
            this.versions = versions;
            this.type = type;
        }
    }
}
