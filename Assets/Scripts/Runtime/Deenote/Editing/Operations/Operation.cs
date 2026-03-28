#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Api.Operations;
using System;

namespace Deenote.Editing.Operations
{
    internal struct OperationGuard
    {
        private const byte GuardUndone = 1;
        private const byte GuardRedone = 2;
        private byte _guard;

        public OperationGuard(bool enable = true)
        {
            _guard = enable ? GuardUndone : default;
        }

        public readonly void OnRedoing()
        {
            if (_guard == GuardRedone) {
                ThrowHelper.ThrowInvalidOperationException("Redo operation repeatly");
            }
        }

        public void OnRedone()
        {
            if (_guard != 0) {
                _guard = GuardRedone;
            }
        }

        public readonly void OnUndoing()
        {
            if (_guard == GuardUndone) {
                ThrowHelper.ThrowInvalidOperationException("Undo operation repeatly");
            }
        }

        public void OnUndone()
        {
            if (_guard != 0) {
                _guard = GuardUndone;
            }
        }
    }

    internal abstract class NotifiableOperation : IOperation
    {
        private OperationGuard _guard;

        public event Action? Redone;
        public event Action? Undone;

        protected NotifiableOperation(bool enableGuard = true)
        {
            _guard = new OperationGuard(enableGuard);
        }

        public NotifiableOperation OnRedone(Action action)
        {
            Redone += action;
            return this;
        }

        public NotifiableOperation OnUndone(Action action)
        {
            Redone -= action;
            return this;
        }

        public NotifiableOperation OnDone(Action action)
        {
            Redone += action;
            Undone += action;
            return this;
        }

        void IOperation.Redo()
        {
            _guard.OnRedoing();

            Redo();
            Redone?.Invoke();

            _guard.OnRedone();
        }

        void IOperation.Undo()
        {
            _guard.OnUndoing();

            Undo();
            Undone?.Invoke();

            _guard.OnUndone();
        }

        protected abstract void Redo();
        protected abstract void Undo();
    }

    internal abstract class NotifiableOperation<TArgs> : IOperation
    {
        private OperationGuard _guard;

        public event Action<TArgs>? Redone;
        public event Action<TArgs>? Undone;

        protected NotifiableOperation(bool enableGuard = true)
        {
            _guard = new OperationGuard(enableGuard);
        }

        public NotifiableOperation<TArgs> OnRedone(Action<TArgs> action)
        {
            Redone += action;
            return this;
        }

        public NotifiableOperation<TArgs> OnUndone(Action<TArgs> action)
        {
            Redone -= action;
            return this;
        }

        public NotifiableOperation<TArgs> OnDone(Action<TArgs> action)
        {
            Redone += action;
            Undone += action;
            return this;
        }

        void IOperation.Redo()
        {
            _guard.OnRedoing();

            var args = Redo();
            Redone?.Invoke(args);

            _guard.OnRedone();
        }

        void IOperation.Undo()
        {
            _guard.OnUndoing();

            var args = Undo();
            Undone?.Invoke(args);

            _guard.OnUndone();
        }

        protected abstract TArgs Redo();
        protected abstract TArgs Undo();
    }
}
