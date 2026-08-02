using System;

namespace Deenote.Core.Notification
{
    public readonly struct PropertyEventArgs
    {
        public static PropertyEventArgs All => new();
        public static PropertyEventArgs None => new("-");

        public string PropertyName { get; }

        public PropertyEventArgs(string propertyName)
            => PropertyName = propertyName;

        public bool Match(string propertyName)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName;

        public bool Match(string propertyName, string propertyName2)
            => string.IsNullOrEmpty(PropertyName) || PropertyName == propertyName || PropertyName == propertyName2;

        public bool Match(ReadOnlySpan<string> propertyNames)
        {
            if (string.IsNullOrEmpty(PropertyName))
                return true;
            foreach (var propertyName in propertyNames) {
                if (PropertyName == propertyName)
                    return true;
            }
            return false;
        }
    }
}
