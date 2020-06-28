using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
    internal static class PecCalculator
    {
        private static byte[] table;

        static PecCalculator()
        {
            table = GenerateTable(0x07);
        }

        public static byte CalculatePec(byte[] buffer, int start, int length)
        {
            byte pec = 0x00;

            for (int i = 0; i < length; i++)
            {
                var data = buffer[start + i];
                pec = table[pec ^ data];
            }

            return pec;
        }

        private static byte[] GenerateTable(byte polynomial)
        {
            byte[] csTable = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                int curr = i;

                for (int j = 0; j < 8; ++j)
                {
                    if ((curr & 0x80) != 0)
                    {
                        curr = (curr << 1) ^ (int)polynomial;
                    }
                    else
                    {
                        curr <<= 1;
                    }
                }

                csTable[i] = (byte)curr;
            }

            return csTable;
        }
    }
}
