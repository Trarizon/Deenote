#nullable enable

using System;
using System.IO;

namespace Deenote.Library.IO;

public static class PathUtils
{
    private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

    public static string ReplaceInvalidFileNameChars(string path, char replace = '_')
    {
        var span = (stackalloc char[path.Length]);
        for (var i = 0; i < path.Length; i++) {
            var c = path[i];
            if (Array.IndexOf(_invalidFileNameChars, c) >= 0) {
                span[i] = replace;
            }
            else {
                span[i] = c;
            }
        }
        return span.ToString();
    }

    public static bool IsValidFileName(string? fileName)
        => fileName is not null && fileName.IndexOfAny(_invalidFileNameChars) < 0;

    public static bool IsValidPath(string path)
        => path.IndexOfAny(_invalidPathChars) < 0;
}