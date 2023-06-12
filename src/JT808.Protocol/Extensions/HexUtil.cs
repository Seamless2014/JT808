using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JT808.Protocol.Extensions
{
    /// <summary>
	///
	/// </summary>
	public static class HexUtil
    {
        private static readonly char[] HexdumpTable;

        static HexUtil()
        {
            HexdumpTable = new char[1024];
            char[] digits = "0123456789ABCDEF".ToCharArray();
            for (int i = 0; i < 256; i++)
            {
                HexdumpTable[i << 1] = digits[((uint)i >> 4) & 0xF];
                HexdumpTable[(i << 1) + 1] = digits[i & 0xF];
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="fromIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string DoHexDump(ReadOnlySpan<byte> buffer, int fromIndex, int length)
        {
            if (length == 0)
            {
                return "";
            }
            int endIndex = fromIndex + length;
            char[] buf = new char[length << 1];
            int srcIdx = fromIndex;
            int dstIdx = 0;
            while (srcIdx < endIndex)
            {
                Array.Copy(HexdumpTable, buffer[srcIdx] << 1, buf, dstIdx, 2);
                srcIdx++;
                dstIdx += 2;
            }
            return new string(buf);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="array"></param>
        /// <param name="fromIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string DoHexDump(byte[] array, int fromIndex, int length)
        {
            if (length == 0)
            {
                return "";
            }
            int endIndex = fromIndex + length;
            char[] buf = new char[length << 1];
            int srcIdx = fromIndex;
            int dstIdx = 0;
            while (srcIdx < endIndex)
            {
                Array.Copy(HexdumpTable, (array[srcIdx] & 0xFF) << 1, buf, dstIdx, 2);
                srcIdx++;
                dstIdx += 2;
            }
            return new string(buf);
        }
    }
}
