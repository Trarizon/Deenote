using Deenote.Audio;
using Deenote.Entities.Models;
using System;

namespace Deenote.GamePlay
{
    internal partial class GameNoteSoundsPlayer
    {
        private readonly PianoSoundSource _pianoSoundSource;
        private readonly HitSoundSource _hitSoundSource;

        private ChartModel? Chart => App.ProjectManager.CurrentChart;

        public float HitSoundVolume { get; set; }
        public float PianoSoundVolume { get; set; }
        public float SoundSpeed { get; set; } = 1f;

        public GameNoteSoundsPlayer(PianoSoundSource pianoSoundSource, HitSoundSource hitSoundSource)
        {
            _pianoSoundSource = pianoSoundSource;
            _hitSoundSource = hitSoundSource;
        }

        private static HitSoundSource.SoundKind NoteToSoundKind(NoteModel note)
        {
            return note.Kind switch {
                NoteModel.NoteKind.Slide => HitSoundSource.SoundKind.Slide,
                _ => HitSoundSource.SoundKind.Click,
            };
        }

        private void ScheduleSound(PianoSoundValueModel sound, double targetDspTime)
        {
            if (PianoSoundVolume <= 0)
                return;

            _pianoSoundSource.ScheduleSound(
                sound.Pitch,
                (int)(sound.Velocity * PianoSoundVolume),
                sound.Duration,
                SoundSpeed,
                targetDspTime
            );
        }

        private readonly struct NoteTimeComparable : IComparable<IStageTimeNode>
        {
            private readonly float _time;
            public NoteTimeComparable(float time) => _time = time;
            public int CompareTo(IStageTimeNode other) => _time.CompareTo(other.Time);
        }
    }
}