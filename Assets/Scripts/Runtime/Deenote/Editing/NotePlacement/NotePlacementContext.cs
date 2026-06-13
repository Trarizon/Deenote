#nullable enable

using Deenote.Core;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using System;

namespace Deenote.Editing.NotePlacement
{
    public sealed partial class NotePlacementContext : INotifyPropertyChanged<NotePlacementContext>
    {
        private const float ZeroAvoidPlacementNoteSpeed = 0.1f;

        private float _placementNoteSpeed_bf;
        public float PlacementNoteSpeed
        {
            get => _placementNoteSpeed_bf;
            set {
                if (value <= 0f)
                    value = ZeroAvoidPlacementNoteSpeed;
                if (Utils.SetField(ref _placementNoteSpeed_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(PlacementNoteSpeed)));
                }
            }
        }

        public NotePlacementContext(SaveSystem storage)
        {
            MetaPrototype = new NotePrototypeModel {
                Speed = 1,
                Size = 1,
            };

            storage.SavingConfigurations += configs =>
            {
                configs.Set("stage/highlight_note_speed", PlacementNoteSpeed);
            };

            storage.LoadedConfigurations += configs =>
            {
                PlacementNoteSpeed = configs.GetSingle("stage/highlight_note_speed", 1f);
            };
        }

        public event Action<NotePlacementContext, PropertyEventArgs>? PropertyChanged;

    }
}
