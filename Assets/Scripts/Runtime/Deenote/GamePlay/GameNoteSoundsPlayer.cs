using Deenote.Audio;
using Deenote.CoreB;
using Deenote.Entities.Models;
using Deenote.GamePlay.Audio;
using System;

namespace Deenote.GamePlay
{
    internal partial class GameNoteSoundsPlayer
    {
        private readonly PianoSoundSource _pianoSoundSource;
        private readonly HitSoundSource _hitSoundSource;

        private ChartModel? Chart => App.ProjectManager.CurrentChart;

        private float _prevTime;

        internal bool Mute { get; set; }

        public float HitSoundVolume { get; set; }
        public float PianoSoundVolume { get; set; }
        public float SoundSpeed { get; set; } = 1f;

        public GameNoteSoundsPlayer(PianoSoundSource pianoSoundSource, HitSoundSource hitSoundSource)
        {
            _pianoSoundSource = pianoSoundSource;
            _hitSoundSource = hitSoundSource;
        }

        public void UpdateTime(float currentTime)
        {
            var prevTime = _prevTime;
            _prevTime = currentTime;

            if (Chart is null)
                return;

            if (currentTime <= prevTime) {
                // Time is backward, ignore.
                return;
            }

            if (Mute)
                return;

            int iLower = Chart.NoteNodes.AsSpan().FindLowerBoundIndex(new NoteTimeComparable(prevTime));
            if (iLower < 0) iLower = ~iLower;

            for (int i = iLower; i < Chart.NoteNodes.Count; i++) {
                var node = Chart.NoteNodes[i];
                if (node.Time >= currentTime)
                    break;

                if (node is NoteModel note) {
                    PlayHitSound(note);
                    PlayPianoSound(note.Sounds.AsSpan());
                }
            }

            int iLowerBg = Chart.BackgroundSoundNotes.AsSpan().FindLowerBoundIndex(new NoteTimeComparable(prevTime));
            if (iLowerBg < 0) iLowerBg = ~iLowerBg;
            for (int i = iLowerBg; i < Chart.BackgroundSoundNotes.Count; i++) {
                var note = Chart.BackgroundSoundNotes[i];
                if (note.Time >= currentTime)
                    break;
                PlayPianoSound(note.Sounds);
            }
        }

        private void PlayHitSound(NoteModel note)
        {
            if (HitSoundVolume <= 0)
                return;

            _hitSoundSource.PlaySound(note.Kind switch {
                NoteModel.NoteKind.Slide => HitSoundSource.SoundKind.Slide,
                _ => HitSoundSource.SoundKind.Click,
            }, HitSoundVolume);
        }

        private void PlayPianoSound(ReadOnlySpan<PianoSoundValueModel> sounds)
        {
            if (PianoSoundVolume <= 0)
                return;
            if (sounds.IsEmpty)
                return;

            foreach (var sound in sounds) {
                _pianoSoundSource.PlaySoundAsync(
                    sound.Pitch,
                    sound.Velocity,
                    sound.Duration,
                    sound.Delay,
                    SoundSpeed,
                    PianoSoundVolume).Forget();
            }
        }

        private readonly struct NoteTimeComparable : IComparable<IStageTimeNode>
        {
            private readonly float _time;
            public NoteTimeComparable(float time) => _time = time;
            public int CompareTo(IStageTimeNode other) => _time.CompareTo(other.Time);
        }
    }
}