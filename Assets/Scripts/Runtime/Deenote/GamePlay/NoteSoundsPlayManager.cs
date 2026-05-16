#nullable enable

using Deenote.Contexts;
using Deenote.CoreB.Models.Notes.Comparers;
using Deenote.CoreB.Notification;
using Deenote.GamePlay.Audio;
using Deenote.Library.Collections;
using System;

namespace Deenote.GamePlay
{
    internal sealed class NoteSoundsPlayManager
    {
        private readonly ProjectContext _project;
        private readonly GamePlayContext _gamePlay;
        private readonly GameHitSoundPlayer _hitSoundPlayer;
        private readonly GamePianoSoundPlayer _pianoSoundPlayer;
        private int _nextHitNoteIndex;
        private int _nextHitBackgroundNoteIndex;

        private float _time;

        public NoteSoundsPlayManager(GamePlayContext gamePlay, ProjectContext project, GameHitSoundPlayer hitSoundPlayer, GamePianoSoundPlayer pianoSoundPlayer)
        {
            _project = project;
            _gamePlay = gamePlay;
            _hitSoundPlayer = hitSoundPlayer;
            _pianoSoundPlayer = pianoSoundPlayer;

            _gamePlay.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.HitSoundVolume))) {
                    _hitSoundPlayer.Volume = s.HitSoundVolume;
                }
                if (e.MatchProperty(nameof(s.PianoVolume))) {
                    _pianoSoundPlayer.Volume = s.PianoVolume;
                }
                if (e.MatchProperty(nameof(s.ActualMusicSpeed))) {
                    _pianoSoundPlayer.Speed = s.ActualMusicSpeed;
                }
            });
        }

        public void UpdateTime(float time, bool playSounds)
        {
            if (_project.CurrentChart is null)
                return;

            int prevHitNoteIndex = _nextHitNoteIndex;
            int prevHitBackgroundNoteIndex = _nextHitBackgroundNoteIndex;
            var chart = _project.CurrentChart;

            var forward = time > _time;

            if (forward) {
                while (_nextHitNoteIndex < chart.Notes.Count && chart.Notes[_nextHitNoteIndex].Time <= time) {
                    _nextHitNoteIndex++;
                }
                while (_nextHitBackgroundNoteIndex < chart.BackgroundNotes.Count && chart.BackgroundNotes[_nextHitBackgroundNoteIndex].Time <= time) {
                    _nextHitBackgroundNoteIndex++;
                }
            }
            else {
                while (_nextHitNoteIndex > 0 && chart.Notes[_nextHitNoteIndex - 1].Time > time) {
                    _nextHitNoteIndex--;
                }
                while (_nextHitBackgroundNoteIndex > 0 && chart.BackgroundNotes[_nextHitBackgroundNoteIndex - 1].Time > time) {
                    _nextHitBackgroundNoteIndex--;
                }
            }
            _time = time;

            if (forward && playSounds) {
                for (int i = prevHitNoteIndex; i < _nextHitNoteIndex; i++) {
                    var note = chart.Notes[i];
                    _hitSoundPlayer.PlaySound(note.Kind);
                    _pianoSoundPlayer.PlaySounds(note.Sounds.AsSpan());
                }
                for (int i = prevHitBackgroundNoteIndex; i < _nextHitBackgroundNoteIndex; i++) {
                    var note = chart.BackgroundNotes[i];
                    _pianoSoundPlayer.PlaySounds(note.Sounds.AsSpan());
                }
            }
        }

        public void SetTime(float time)
        {
            var chart = _project.CurrentChart!;
            _time = time;

            _nextHitNoteIndex = chart.Notes.AsSpan().FindUpperBoundIndex(new NoteTimeComparable(_time));
            _nextHitBackgroundNoteIndex = chart.BackgroundNotes.AsSpan().FindUpperBoundIndex(new NoteTimeComparable(_time));
        }
    }
}
