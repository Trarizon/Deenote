#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using System;
using System.Runtime.InteropServices;

namespace Deenote.CoreB.Notification
{
    public readonly struct EventRegister<TSender,TArgs>
    {
        private readonly Event<TSender, TArgs> _event;

        public static EventRegister<TSender,TArgs> RegisterAndInvoke<T>() => default;
    }
}
