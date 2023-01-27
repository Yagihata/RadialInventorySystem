using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YagihataItems.RadialInventorySystemV4
{
    public class SafeParser
    {
        public static string ParseFileName(string rawName)
        {
            var fileName = rawName;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '-');
            }
            return fileName;
        }
    }
}
