using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Utils
    {
        public static byte[] CombineByteArrays(byte[][] arrays)
        {
            int totalLength = arrays.Sum(a => a.Length);
            byte[] result = new byte[totalLength];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        public static byte[][] SplitByteArray(byte[] source, int numberOfParts)
        {
            if (source == null || numberOfParts <= 0 || source.Length < numberOfParts)
                throw new ArgumentException("Invalid argument.");

            byte[][] result = new byte[numberOfParts][];
            int partSize = source.Length / numberOfParts;
            int remainingBytes = source.Length % numberOfParts;
            int offset = 0;

            for (int i = 0; i < numberOfParts; i++)
            {
                int size = partSize + (i < remainingBytes ? 1 : 0);
                result[i] = new byte[size];
                Array.Copy(source, offset, result[i], 0, size);
                offset += size;
            }

            return result;
        }
    }
}
