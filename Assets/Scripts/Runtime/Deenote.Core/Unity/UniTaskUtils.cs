using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

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

        // UniTask does not cancel the operation, so we need this
        public static AsyncInstantiateOperation<T> AttachCancellation<T>(this AsyncInstantiateOperation<T> operation, CancellationToken cancellationToken)
            where T : UnityEngine.Object
        {
            var reg = cancellationToken.Register(static (s) =>
            {
                var operation = (AsyncInstantiateOperation<T>)s;
                operation.Cancel();
            }, operation);

            operation.completed += DisposeReg;
            return operation;

            void DisposeReg(AsyncOperation e)
            {
                reg.Dispose();
                operation.completed -= DisposeReg;
            }
        }
    }
}
