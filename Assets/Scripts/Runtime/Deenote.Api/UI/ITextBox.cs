#nullable enable

using System;

namespace Deenote.Api.UI
{
    public interface ITextBox
    {
        string? PlaceHolder { get; }
        /// <summary>
        /// Input text validation, this is called when the text is changed
        /// </summary>
        Func<string, string?> Validation { get; }
    }
}
