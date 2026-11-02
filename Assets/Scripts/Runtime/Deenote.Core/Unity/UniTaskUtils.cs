using Cysharp.Threading.Tasks;
using System;

namespace Deenote.Core.Unity
{
    public static class UniTaskUtils
    {
        public static async UniTask ContinueWith<T>(this UniTask task, T state, Action<T> action)
        {
            await task;
            action(state);
        }

        public static Action Action(Func<UniTask> asyncAction) => asyncAction._ActionExt;
        private static void _ActionExt(this Func<UniTask> asyncAction) => asyncAction().Forget();
    }
}
