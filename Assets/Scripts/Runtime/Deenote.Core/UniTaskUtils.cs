using Cysharp.Threading.Tasks;
using System;

namespace Deenote.CoreB
{
    public static class UniTaskUtils
    {
        public static async UniTask ContinueWith<T>(this UniTask task, T state, Action<T> action)
        {
            await task;
            action(state);
        }
    }
}
