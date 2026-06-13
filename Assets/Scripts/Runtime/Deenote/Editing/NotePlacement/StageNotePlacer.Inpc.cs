#nullable enable

using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library;
using System;

namespace Deenote.Editing.NotePlacement
{
    public sealed partial class StageNotePlacer2 : INotifyPropertyChanged<StageNotePlacer2>
    {
        public bool SnapToPositionGrids
        {
            get => _snapToPositionGrids_bf;
            set {
                if (Utils.SetField(ref _snapToPositionGrids_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(SnapToPositionGrids)));
                }
            }
        }

        public bool SnapToTimeGrids
        {
            get => _snapToTimeGrids_bf;
            set {
                if (Utils.SetField(ref _snapToTimeGrids_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(SnapToTimeGrids)));
                }
            }
        }

        // Runtime status

        internal bool? IndicatorsForceVisible
        {
            get => _indicatorsForceVisible_bf;
            set {
                if (Utils.SetField(ref _indicatorsForceVisible_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IndicatorsForceVisible)));
                }
            }
        }

        internal float? ForceDisplayPlacementNoteSpeed
        {
            get => _forceDisplayPlacementNoteSpeed_bf;
            set {
                if (Utils.SetField(ref _forceDisplayPlacementNoteSpeed_bf, value)) {
                    _metaPrototype.Speed = value ?? _context.PlacementNoteSpeed;
                    NotifyMetaPrototypeChanged();
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(ForceDisplayPlacementNoteSpeed)));
                }
            }
        }

        internal bool IsPastingRequested
        {
            get => _isPastingRequested_bf;
            set {
                if (Utils.SetField(ref _isPastingRequested_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsPastingRequested)));
                }
            }
        }

        internal bool IsPlacingSlidesRequested
        {
            get => _isPlacingSlidesRequested_bf;
            set {
                if (Utils.SetField(ref _isPlacingSlidesRequested_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsPlacingSlidesRequested)));
                }
            }
        }

        internal bool IsPastingRemeberPosition
        {
            get => _isPastingRemeberPosition_bf;
            set {
                if (Utils.SetField(ref _isPastingRemeberPosition_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsPastingRemeberPosition)));
                }
            }
        }

        internal bool PlaceSoundNoteByDefault
        {
            get => _placeSoundNoteByDefault_bf;
            set {
                if (Utils.SetField(ref _placeSoundNoteByDefault_bf, value)) {
                    _metaPrototype.ReplaceSounds(value ? NoteSoundsHelpers.EditorDefaultSounds : ReadOnlySpan<PianoSoundData>.Empty);
                    NotifyMetaPrototypeChanged();

                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(PlaceSoundNoteByDefault)));
                }
            }
        }
    }
}