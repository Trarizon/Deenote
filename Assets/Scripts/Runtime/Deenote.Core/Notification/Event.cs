#nullable enable

using System;
using System.Collections.Generic;

namespace Deenote.CoreB.Notification
{
    public sealed class Event<TSender, TArgs>
    {
        private readonly List<Action<TSender, TArgs>> _handlers = new();

        public void AddHandler(Action<TSender, TArgs> handler) { }

        public void RemoveHandler(Action<TSender, TArgs> handler) { }

        public void Invoke(TSender sender, TArgs args)
        {
            foreach (var handler in _handlers) {
                handler(sender, args);
            }
        }
    }
}
