#nullable enable

using CommunityToolkit.Diagnostics;

namespace Deenote.Editing.Operations.Components
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
}
