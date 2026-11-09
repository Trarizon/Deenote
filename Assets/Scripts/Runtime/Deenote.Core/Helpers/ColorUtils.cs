using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deenote.Core.Helpers
{
    public static class ColorUtils
    {
        /// <param name="code">RGBA, hex, allows # sign</param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool TryParse(ReadOnlySpan<char> code, out Color color)
        {
            if (code.IsEmpty)
                goto Failed;

            if (code[0] == '#')
                code = code[1..];

            if (code.IsEmpty)
                goto Failed;

            const int HalfByteBitCount = 4;

            Span<byte> bytes = stackalloc byte[4];
            bytes[3] = byte.MaxValue;

            int bidx = 0;
            for (int i = 0; i < code.Length; i += 2) {
                var c = code[i];
                if (!TryParseByte(c, out var bl))
                    goto Failed;
                ref var b = ref bytes[bidx];
                b = bl;

                if (i + 1 >= code.Length)
                    break;
                c = code[i + 1];
                if (!TryParseByte(c, out var br))
                    goto Failed;
                b <<= HalfByteBitCount;
                b |= br;
                bidx++;
            }

            color = new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
            return true;

        Failed:
            color = default;
            return false;

            static bool TryParseByte(char c, out byte b)
            {
                if (c is >= '0' and <= '9') {
                    b = (byte)(c - '0');
                    return true;
                }
                if (c is >= 'a' and <= 'f') {
                    b = (byte)(c - ('a' - 10));
                    return true;
                }
                if (c is >= 'A' and <= 'F') {
                    b = (byte)(c - ('A' - 10));
                    return true;
                }

                b = 0;
                return false;
            }
        }

        public static string ToRGBAString(this Color color, bool hashtag = false)
            => ToRGBAString((Color32)color, hashtag);

        public static string ToRGBAString(this Color32 color, bool hashtag = false)
        {
            var buffer = (stackalloc char[8 + 1]);
            if (hashtag) {
                buffer[0] = '#';
                WriteColorRGBAString(color, buffer[1..]);
                return buffer.ToString();
            }
            WriteColorRGBAString(color, buffer);
            return buffer[..8].ToString();
        }

        public static bool WriteColorRGBAString(Color32 color, Span<char> buffer)
        {
            const int HalfByteBitCount = 4;
            const int Written = 8;

            if (buffer.Length < Written) {
                return false;
            }
            ToStr(color.r, ref buffer[0]);
            ToStr(color.g, ref buffer[2]);
            ToStr(color.b, ref buffer[4]);
            ToStr(color.a, ref buffer[6]);
            return true;

            void ToStr(byte b, ref char c)
            {
                var bl = b >> HalfByteBitCount;
                c = ToChar4Bits(bl);

                var br = b & 0xF;
                Unsafe.Add(ref c, 1) = ToChar4Bits(br);
            }

            char ToChar4Bits(int b)
            {
                Debug.Assert(b <= 0xF);
                if (b < 10)
                    return (char)(b + '0');
                else
                    return (char)(b - 10 + 'A');
            }
        }
    }

}