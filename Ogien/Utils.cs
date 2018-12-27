using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ogien
{
    class Utils
    {
        public static string FormatByteCount(long byteCount)
        {
            if (byteCount < 1024) {
                return $"{byteCount} B";
            }
            
            if (byteCount < 1024 * 1024) {
                double v = (double)byteCount / 1024;
                return v.ToString("F3") + " kB";
            }

            if (byteCount < 1024 * 1024 * 1024) {
                double v = (double)byteCount / (1024 * 1024);
                return v.ToString("F3") + " MB";
            }

            double x = (double)byteCount / (1024 * 1024 * 1024);
            return x.ToString("F3") + " GB";
        }

        public static byte[] CreateBuffer(int size, bool randomData)
        {
            Console.WriteLine("Create buffer of size {0} B", size);
            var ret = new byte[size];
            if (randomData) {
                Console.WriteLine("Fill buffer with random data");
                throw new NotImplementedException();
            }
            return ret;
        }
    }
}
