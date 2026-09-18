using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.CoreB.Localization
{
    [Serializable]
    public struct LocalizableText
    {
        [SerializeField] private bool _isLocalized;
        [SerializeField] private string _textOrKey;
        private readonly object? _args;

        public readonly bool IsLocalized => _isLocalized;
        public readonly string TextOrKey => _textOrKey;
        public readonly ReadOnlySpan<string> Args
        {
            get {
                if (_args is null)
                    return ReadOnlySpan<string>.Empty;
                if (_args is string[] arr)
                    return arr.AsSpan();

                System.Diagnostics.Debug.Assert(_args is string);
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<object, string>(ref Unsafe.AsRef(in _args)), 1);
            }
        }

        /// <summary>
        /// Get original format of text, which is not formatted with args.
        /// </summary>
        public readonly LocalizableText Original => new(IsLocalized, TextOrKey, null);

        private LocalizableText(bool isLocalized, string textOrKey, object? args = null)
        {
            _isLocalized = isLocalized;
            _textOrKey = textOrKey;
            System.Diagnostics.Debug.Assert(args is null or string[] or string);
            _args = args;
        }

        public static LocalizableText Raw(string text) => new(false, text);

        public static LocalizableText Localized(string textKey) => new(true, textKey);
        public static LocalizableText Localized(string textKey, string arg0) => new(true, textKey, arg0);
        public static LocalizableText Localized(string textKey, params string[] args) => new(true, textKey, args);

        public override readonly bool Equals(object? obj) => obj is LocalizableText text && this == text;
        public override readonly int GetHashCode()
        {
            var hc = new HashCode();
            hc.Add(IsLocalized);
            hc.Add(TextOrKey);
            foreach (var arg in Args)
                hc.Add(arg);
            return hc.ToHashCode();
        }

        public static bool operator ==(LocalizableText left, LocalizableText right)
        {
            if (left.IsLocalized != right.IsLocalized)
                return false;
            if (left.TextOrKey != right.TextOrKey)
                return false;

            return left.Args.SequenceEqual(right.Args);
        }

        public static bool operator !=(LocalizableText left, LocalizableText right) => !(left == right);

        public static bool operator ==(LocalizableText left, string right) => left == Raw(right);
        public static bool operator !=(LocalizableText left, string right) => left != Raw(right);
    }
}