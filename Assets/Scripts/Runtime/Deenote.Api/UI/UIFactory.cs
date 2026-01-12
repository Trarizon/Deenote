#nullable enable

using System;

namespace Deenote.Api.UI
{
    public static class UIFactory
    {
        public static IForm Form(string title)
        {
            return new Form {
                Title = title,
            };
        }

        public static IButton Button(string contentText, Action? onClick = null)
            => new Button(contentText, onClick);
    }
}