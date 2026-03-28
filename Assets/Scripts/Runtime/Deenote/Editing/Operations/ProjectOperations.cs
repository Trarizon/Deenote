#nullable enable

using Deenote.CoreB.Models;
using Deenote.Editing.EditorModels;
using Deenote.Library.Collections;
using UnityEngine;

namespace Deenote.Editing.Operations
{
    internal static class ProjectOperations
    {
        public static InsertTempoOperation InsertTempo(this ProjectEditorModel project, TempoRange tempoRange)
        {
            Debug.Assert(tempoRange.Length >= 0f);
            return new InsertTempoOperation(project, tempoRange);
        }

        public sealed class InsertTempoOperation : NotifiableOperation
        {
            private readonly ProjectEditorModel _project;
            private readonly int _insertIndex;
            private readonly Tempo _insertStartTempo;
            private readonly Tempo? _insertEndTempo;
            private readonly Tempo? _insertContinuingTempo;
            private readonly Tempo[] _removeTempos;

            public InsertTempoOperation(ProjectEditorModel project, TempoRange tempoRange)
            {
                var (tempo, endTime) = tempoRange;

                var tempos = project.Tempos;

                int i = 0;
                for (; i < tempos.Count; i++) {
                    // if |tempo.Start - prevTp.Start| <= MinInterval
                    // we set prevTempo = tempo
                    if (tempo.StartTime < tempos[i].StartTime + Tempo.MinBeatLineInterval)
                        break;
                }
                int removeStartIndex = i;
                for (; i < tempos.Count; i++) {
                    if (endTime < tempos[i].StartTime + Tempo.MinBeatLineInterval)
                        break;
                }
                int removeEndIndex = i;

                if (tempoRange.Length < Tempo.MinBeatLineInterval) {
                    // If given tempo range is to short, we only adjust startTime
                }
                else {
                    // |  .  .  .  . | . . .|   .   .   .   .  |
                    //      ^tempo                ^endTime
                    //               ^rmvStartIndex            ^rmvEndIndex
                    //                      ^prevTempo         ^nextTempoTime
                    //                              ^continuingTempoTime
                    float nextTempoTime = project.GetNonOverflowTempoTime(removeEndIndex);
                    Tempo prevTempo = project.GetActualTempo(removeEndIndex - 1);
                    float continuingTempoTime = prevTempo.GetBeatTime(prevTempo.GetCeilingBeatIndex(endTime));

                    if (continuingTempoTime <= nextTempoTime - Tempo.MinBeatLineInterval) {
                        _insertContinuingTempo = new Tempo(prevTempo.Bpm, continuingTempoTime);
                    }
                    else {
                        // If continuingTempo is near or greater than nextTempo, ignore it
                        _insertContinuingTempo = null;
                    }

                    if (endTime < Mathf.Min(nextTempoTime, continuingTempoTime) - Tempo.MinBeatLineInterval) {
                        _insertEndTempo = new Tempo(bpm: 0, endTime);
                    }
                    else {
                        // If endTime is near to Min(..), ignore endTime
                        // so the actual 'endTime' will be nextTempoTime or continuingTempoTime
                        _insertEndTempo = null;
                    }
                }


                _project = project;
                _insertIndex = removeStartIndex;
                _insertStartTempo = tempo;
                _removeTempos = project.Tempos.AsSpan()[removeStartIndex..removeEndIndex].ToArray();
            }

            protected override void Redo()
            {
                var tempos = _project.Tempos;
                tempos.RemoveRange(_insertIndex, _removeTempos.Length);

                if (_insertContinuingTempo is { } continuing)
                    tempos.Insert(_insertIndex, continuing);
                if (_insertEndTempo is { } endTempo)
                    tempos.Insert(_insertIndex, endTempo);
                tempos.Insert(_insertIndex, _insertStartTempo);
            }

            protected override void Undo()
            {
                var tempos = _project.Tempos;
                tempos.RemoveAt(_insertIndex);
                if (_insertEndTempo is not null)
                    tempos.RemoveAt(_insertIndex);
                if (_insertContinuingTempo is not null)
                    tempos.RemoveAt(_insertIndex);
                tempos.InsertRange(_insertIndex, _removeTempos);
            }
        }
    }
}
