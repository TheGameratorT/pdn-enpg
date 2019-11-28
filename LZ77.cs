using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameratorT.FileTypes.ENPG
{
    class LZ77
    {
        public static void LZ77_Compress_Search(byte[] data, int pos, out int match, out int length)
        {
            int maxMatchDiff = 4096;
            int maxMatchLen = 18;
            match = 0;
            length = 0;

            int start = pos - maxMatchDiff;
            if (start < 0) start = 0;

            for (int thisMatch = start; thisMatch < pos; thisMatch++)
            {
                int thisLength = 0;
                while (thisLength < maxMatchLen
                    && thisMatch + thisLength < pos
                    && pos + thisLength < data.Length
                    && data[pos + thisLength] == data[thisMatch + thisLength])
                    thisLength++;

                if (thisLength > length)
                {
                    match = thisMatch;
                    length = thisLength;
                }

                //We can't improve the max match length again...
                if (length == maxMatchLen)
                    return;
            }
        }

        public static byte[] LZ77_Compress(byte[] data, bool header = false)
        {
            ByteArrayOutputStream res = new ByteArrayOutputStream();
            if (header)
            {
                res.writeUInt(0x37375A4C); //LZ77
            }

            res.writeInt((data.Length << 8) | 0x10);

            byte[] tempBuffer = new byte[16];

            //Current byte to compress.
            int current = 0;

            while (current < data.Length)
            {
                int tempBufferCursor = 0;
                byte blockFlags = 0;
                for (int i = 0; i < 8; i++)

                {
                    //Not sure if this is needed. The DS probably ignores this data.
                    if (current >= data.Length)
                    {
                        tempBuffer[tempBufferCursor++] = 0;
                        continue;
                    }

                    int searchPos = 0;
                    int searchLen = 0;
                    LZ77_Compress_Search(data, current, out searchPos, out searchLen);
                    int searchDisp = current - searchPos - 1;
                    if (searchLen > 2) //We found a big match, let's write a compressed block.
                    {
                        blockFlags |= (byte)(1 << (7 - i));
                        tempBuffer[tempBufferCursor++] = (byte)((((searchLen - 3) & 0xF) << 4) + ((searchDisp >> 8) & 0xF));
                        tempBuffer[tempBufferCursor++] = (byte)(searchDisp & 0xFF);
                        current += searchLen;
                    }
                    else
                    {
                        tempBuffer[tempBufferCursor++] = data[current++];
                    }
                }

                res.writeByte(blockFlags);
                for (int i = 0; i < tempBufferCursor; i++)
                    res.writeByte(tempBuffer[i]);
            }

            return res.getArray();
        }

        //If can't decompress it returns the uncompressed file
        public static byte[] LZ77_Decompress(byte[] source)
        {
            bool WithHeader = false;
            if (source[0] == 'L' &&
                source[1] == 'Z' &&
                source[2] == '7' &&
                source[3] == '7')
                WithHeader = true;

            if (source[0] != 0x10 && !WithHeader)
                throw new Exception();

            // This code converted from Elitemap
            int DataLen;
            DataLen = source[1] | (source[2] << 8) | (source[3] << 16);
            if (WithHeader)
                DataLen = source[5] | (source[6] << 8) | (source[7] << 16);
            byte[] dest = new byte[DataLen];
            int i, j, xin, xout;
            xin = 4;
            if (WithHeader)
                xin = 8;
            xout = 0;
            int length, offset, windowOffset, data;
            byte d;
            while (DataLen > 0)
            {
                d = source[xin++];
                if (d != 0)
                {
                    for (i = 0; i < 8; i++)
                    {
                        if ((d & 0x80) != 0)
                        {
                            data = ((source[xin] << 8) | source[xin + 1]);
                            xin += 2;
                            length = (data >> 12) + 3;
                            offset = data & 0xFFF;
                            windowOffset = xout - offset - 1;
                            for (j = 0; j < length; j++)
                            {
                                dest[xout++] = dest[windowOffset++];
                                DataLen--;
                                if (DataLen == 0)
                                {
                                    return dest;
                                }
                            }
                        }
                        else
                        {
                            dest[xout++] = source[xin++];
                            DataLen--;
                            if (DataLen == 0)
                            {
                                return dest;
                            }
                        }
                        d <<= 1;
                    }
                }
                else
                {
                    for (i = 0; i < 8; i++)
                    {
                        dest[xout++] = source[xin++];
                        DataLen--;
                        if (DataLen == 0)
                        {
                            return dest;
                        }
                    }
                }
            }
            return dest;
        }
    }
}
