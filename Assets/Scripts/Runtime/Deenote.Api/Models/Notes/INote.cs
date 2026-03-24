#nullable enable

using System.ComponentModel;

namespace Deenote.Api.Models.Notes
{
    public interface INote : INotifyPropertyChanged
    {
        float Position { get; }
        float Time { get; }
        float Size { get; }
        float Shift { get; }
        float Speed { get; }
        float Duration { get; }
        bool Vibrate { get; }
        NoteKind Kind { get; }
        WarningType WarningType { get; }
        string EventId { get; }

        INote? PrevLink { get; }
        INote? NextLink { get; }
    }
}
