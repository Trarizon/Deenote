using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using UnityEngine;

namespace Deenote.GamePlay
{
    partial class GameNoteSoundsPlayer
    {
        private const double AudioLookaheadSeconds = 0.25d;
        private const double MinAudioScheduleLeadSeconds = 0.01d;

        private long _scheduledAudioEpoch;
        private int _nextScheduledNoteIndex;
        private int _nextScheduledBackgroundNoteIndex;
        private bool _scheduledAudioInitialized;

        /// <summary>
        /// Schedules chart audio ahead of the DSP clock. This is deliberately
        /// separate from the render-time update path.
        /// </summary>
        internal void ScheduleAudio()
        {
            if (Chart is null)
                return;

            var musicPlayer = _musicPlayer;
            var transport = musicPlayer.Transport;

            if (!transport.CanSchedule)
                return;

            double dspNow = AudioSettings.dspTime;
            if (!_scheduledAudioInitialized) {
                ResetScheduledAudio(
                    transport.ChartTimeAtDsp(dspNow),
                    transport.Epoch);
            }

            var chart = Chart;
            double horizonChartTime = transport.ChartTimeAtDsp(dspNow + AudioLookaheadSeconds);

            while (_nextScheduledNoteIndex < chart.NoteNodes.Count) {
                var node = chart.NoteNodes[_nextScheduledNoteIndex];
                if (node.Time > horizonChartTime)
                    break;

                // Holds intentionally trigger only at their head. NoteTailNode
                // participates in display/combo logic but never schedules audio.
                if (node is NoteModel note) {
                    double targetDspTime = transport.DspTimeAtChart(note.Time);
                    if (targetDspTime >= dspNow + MinAudioScheduleLeadSeconds) {
                        this._hitSoundSource.ScheduleSound(NoteToSoundKind(note), targetDspTime, HitSoundVolume);
                        foreach (var sound in note.Sounds) {
                            double soundDspTime = transport.DspTimeAtChart(note.Time + sound.Delay);
                            this.ScheduleSound(sound, soundDspTime);
                        }
                    }
                }

                _nextScheduledNoteIndex++;
            }

            while (_nextScheduledBackgroundNoteIndex < chart.BackgroundSoundNotes.Count) {
                var note = chart.BackgroundSoundNotes[_nextScheduledBackgroundNoteIndex];
                if (note.Time > horizonChartTime)
                    break;

                double targetDspTime = transport.DspTimeAtChart(note.Time);
                if (targetDspTime >= dspNow + MinAudioScheduleLeadSeconds) {
                    foreach (var sound in note.Sounds) {
                        double soundDspTime = transport.DspTimeAtChart(note.Time + sound.Delay);
                        this.ScheduleSound(sound, soundDspTime);
                    }
                }

                _nextScheduledBackgroundNoteIndex++;
            }
        }

        // TODO: r1.1 临时公开供旧 GamePlayManager 调用
        internal void ResetScheduledAudio(double chartTime, long epoch)
        {
            _scheduledAudioEpoch = epoch;
            _scheduledAudioInitialized = true;

            if (Chart is null) {
                _nextScheduledNoteIndex = 0;
                _nextScheduledBackgroundNoteIndex = 0;
                return;
            }

            var chart = Chart;
            _nextScheduledNoteIndex = chart.NoteNodes.AsSpan()
                .FindLowerBoundIndex(new NodeTimeComparable((float)chartTime));
            _nextScheduledBackgroundNoteIndex = chart.BackgroundSoundNotes.AsSpan()
                .FindLowerBoundIndex(new NodeTimeComparable((float)chartTime));
        }

        internal void CancelScheduledAudio()
        {
            _scheduledAudioInitialized = false;
            _hitSoundSource.CancelScheduledSounds();
            _pianoSoundSource.CancelScheduledSounds();
        }

        internal void CancelPendingAudio()
        {
            _scheduledAudioInitialized = false;
            _hitSoundSource.CancelPendingSounds();
            _pianoSoundSource.CancelPendingSounds();
        }

        internal void RebaseScheduledAudio()
        {
            _scheduledAudioInitialized = false;
        }
    }
}
