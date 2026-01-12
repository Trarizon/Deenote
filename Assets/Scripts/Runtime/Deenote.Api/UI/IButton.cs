#nullable enable

using System;

namespace Deenote.Api.UI
{
    public interface IButton : IFormColumn
    {
        string Text { get; }
        void OnClicked() { }
    }

    internal sealed class Button : IButton
    {
        public string Text { get; }
        private readonly Action? _clicked;

        public Button(string text, Action? clicked)
        {
            Text = text;
            _clicked = clicked;
        }

        public void OnClicked() => _clicked?.Invoke();
    }
}
