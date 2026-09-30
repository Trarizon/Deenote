using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace Deenote.Core
{
    public static class Asserts
    {
#pragma warning disable CS8777
        [Conditional("UNITY_ASSERTIONS")]
        public static void NotNull<T>([NotNull] T value, [CallerArgumentExpression("value")] string expression = "")
        {
            Debug.Assert(value != null, $"{expression} is null");

        }
#pragma warning restore CS8777
    }
}
