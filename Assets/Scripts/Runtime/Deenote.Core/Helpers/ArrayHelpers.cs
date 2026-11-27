using System;
using System.Diagnostics;

namespace Deenote.Core.Helpers
{
    internal static class ArrayHelpers
    {
        public static void Grow<T>(ref T[] array, int copyLength, int expectedCapacity)
        {
            Debug.Assert(expectedCapacity > array.Length);
            var newCapacity = GetNextCapacity(array.Length, expectedCapacity);
            var newArray = new T[newCapacity];
            Array.Copy(array, newArray, copyLength);
            array = newArray;
        }

        public static void GrowForInsertion<T>(ref T[] array, int insertStart, int insertLength, int originalCount)
        {
            Debug.Assert(originalCount + insertLength > array.Length);
            var newCapacity = GetNextCapacity(array.Length, originalCount + insertLength);
            var newArray = new T[newCapacity];
            Array.Copy(array, newArray, insertStart);
            Array.Copy(array, insertStart, newArray, insertStart + insertLength, originalCount - insertStart);
            array = newArray;
        }

        private static int GetNextCapacity(int currentCapacity, int expectedCapacity)
        {
            const int ArrayMaxLength = 0x7FFFFFC7;
            var result = currentCapacity == 0 ? 4 : currentCapacity * 2;
            if (result > ArrayMaxLength) result = ArrayMaxLength;
            if (result < expectedCapacity) result = expectedCapacity;
            return result;
        }
    }
}
