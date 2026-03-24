#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Deenote.Library
{
    public static class Utils
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        public static string ReplaceInvalidFileNameChars(string path, char replace = '_')
        {
            if (path.Length > 512) {
                using var own = SpanOwner<char>.Allocate(path.Length);
                var span = own.Span;
                Core(path, span, replace);
                return span.ToString();
            }
            else {
                var span = (stackalloc char[path.Length]);
                Core(path, span, replace);
                return span.ToString();
            }

            static void Core(ReadOnlySpan<char> source, Span<char> span, char replace)
            {
                for (var i = 0; i < source.Length; i++) {
                    var c = source[i];
                    if (InvalidFileNameChars.AsSpan().IndexOf(c) >= 0) {
                        span[i] = replace;
                    }
                    else {
                        span[i] = source[i];
                    }
                }
            }
        }

        public static bool EndsWithOneOf(this string str, ReadOnlySpan<string> ends)
        {
            foreach (var end in ends) {
                if (str.EndsWith(end))
                    return true;
            }
            return false;
        }

        public static bool SetField<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            return true;
        }

        public static bool SetField<T>(ref T field, T value, out T originalValue)
        {
            originalValue = field;
            return SetField(ref field, value);
        }
    }
}