#nullable enable

using Deenote.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.Core.GameStage;
using Deenote.Core.Project;
using Deenote.CoreB.Notification;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.GamePlay;
using Deenote.GamePlay.UI;
using Deenote.GameStage;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    public sealed partial class GamePlayManager : FlagNotifiableMonoBehaviour<GamePlayManager, GamePlayManager.NotificationFlag>
    {
        private NotesManager _notesManager = default!;
        // private SpeedChangeWarningsManager _speedChangeWarningsManager = default!;
        private GridsManager _gridsManager = default!;
        [SerializeField] GameMusicPlayer _musicPlayer = default!;
        private StagePianoSoundPlayer _pianoSoundPlayer = default!;
        [SerializeField] HitSoundSource _hitSoundPlayer = default!;


        public GameStageController? Stage { get; private set; }

        public NotesManager NotesManager => _notesManager;
        public GridsManager Grids => _gridsManager;
        public GameMusicPlayer MusicPlayer => _musicPlayer;

        public event Action<StageLoadedEventArgs>? StageLoaded;


        public ChartModel? CurrentChart
        {
            get => App.ProjectManager.CurrentChart;
            private set => App.ProjectManager.CurrentChart = value;
        }

        /// <remarks>
        /// If music is paused, maually set music time by this value,
        /// <br />
        /// If playing, this value should sync to _musicSource.velocity
        /// </remarks>
        private float? _manualPlaySpeedMultiplier;

        public float StagePlaySpeed
            => _manualPlaySpeedMultiplier ?? MusicPlayer.Pitch;

        public void SetManualPlaySpeed(float? manualPlaySpeed)
        {
            App.GamePlayManager.SetManualPlaySpeed(manualPlaySpeed);
        }

        public void OnStageLoaded(GameStageController stage)
        {
            // App.Logger.LogDebug("StageLoaded");
            // Stage = stage;
            // Stage.OnInstantiate(this);
            // NotesManager.Initialize(
            //     UnityUtils.CreateObjectPool(
            //         Stage.Config.NotePrefab,
            //         Stage.NotePanelTransform,
            //         item => item.OnInstantiate(this)));
            // OnStageLoaded_Properties(stage);

            // if (IsChartLoaded()) {
            //     UpdateNotes(true, true);
            // }

            StageLoaded?.Invoke(new StageLoadedEventArgs(stage, (PerspectiveViewForegroundBase)App.GameStageManager.ThemeManager.CurrentForeground));
        }

        private void Awake()
        {
            _gridsManager = new GridsManager(this, MainSystem.StageChartEditor);
            _pianoSoundPlayer = new StagePianoSoundPlayer(MainSystem.PianoSoundSource);
            _notesManager = new NotesManager(this);
            // _speedChangeWarningsManager = new SpeedChangeWarningsManager(this);

            RegisterConfigurations();
            RegisterCustomPropertiesConfigurations();

            // MusicPlayer.TimeChanged += args =>
            // {
            //     if (!IsChartLoaded())
            //         return;

            //     App.GameStageManager.NotesManager.ShiftActiveNotes(playSounds: !args.IsByJump);

            //     // NotesManager.ShiftStageActiveNotes();
            //     App.GameStageManager.WarningNotesManager.ShiftActiveNotes();
            //     NotifyFlag(NotificationFlag.ActiveNoteUpdated);
            //     // In previous version, note time was controlled by StageNoteController.Update,
            //     // so we have to manually call update when manually change music time.
            //     // In current version, note time is controlled by GamePlayManager.UpdateActiveNotes,
            //     // so the following code is not required
            //     // if (args.IsManuallyChanged) RefreshNotesTimeState();
            // };

            // MusicPlayer.TransportChanged += reason =>
            // {
            //     if (!IsChartLoaded())
            //         return;

            //     switch (reason) {
            //         case TransportChangeReason.Paused:
            //             // Let already-started voices ring out, but discard
            //             // everything that has only been scheduled for the future.
            //             NotesManager.CancelPendingAudio();
            //             return;
            //         case TransportChangeReason.Seeked:
            //             // Time jumps only invalidate scheduled voices. Active
            //             // voices continue as tails.
            //             NotesManager.CancelPendingAudio();
            //             break;
            //         case TransportChangeReason.RateChanged:
            //             NotesManager.CancelPendingAudio();
            //             break;
            //         case TransportChangeReason.Started:
            //             NotesManager.RebaseScheduledAudio();
            //             break;
            //     }

            //     NotesManager.ScheduleAudio();
            // };

            App.ProjectManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.CurrentChart))) {
                    if (CurrentChart is null) {
                        UnloadChart();
                    }
                    else {
                        LoadChartInCurrentProject(CurrentChart);
                    }
                }
            });
        }

        private void OnDestroy()
        {
            _gridsManager.Dispose();
        }

        private void Update()
        {
            if (!IsChartLoaded())
                return;

            // if (!MusicPlayer.IsPlaying && _manualPlaySpeedMultiplier is { } manuallPlaySpeed) {
            //     MusicPlayer.Nudge(Time.deltaTime * manuallPlaySpeed);
            // }

            if (IsStageLoaded()) {
                NotesManager.RefreshStageNoteTimeDisplay();
                App.GameStageManager.WarningNotesManager.RefreshActiveNotes();
            }
        }
        // private void OnApplicationFocus(bool focus)
        // {
        //     if (!focus && PauseWhenLoseFocus) {
        //         MusicPlayer.Stop();
        //     }
        // }

        public void UpdateNotes(bool noteCollectionChangedOrNoteTimeRelatedPropertyChanged, bool notesVisualDataChanged)
        {
            AssertChartLoaded();
            AssertStageLoaded();

            switch (noteCollectionChangedOrNoteTimeRelatedPropertyChanged, notesVisualDataChanged) {
                case (true, false):
                    NotesManager.RefreshStageActiveNotes();
                    App.GamePlayManager.NoteSoundsPlayer.CancelScheduledAudio();
                    App.GamePlayManager.NoteSoundsPlayer.ResetScheduledAudio(MusicPlayer.PreciseTime, MusicPlayer.Transport.Epoch);
                    break;
                case (false, true):
                    foreach (var note in NotesManager.OnStageNotes) {
                        note.RefreshVisual();
                    }
                    break;
                case (true, true):
                    NotesManager.RefreshStageActiveNotes();
                    App.GamePlayManager.NoteSoundsPlayer.CancelScheduledAudio();
                    App.GamePlayManager.NoteSoundsPlayer.ResetScheduledAudio(MusicPlayer.PreciseTime, MusicPlayer.Transport.Epoch);
                    break;
                default:
                    return;
            }
            NotifyFlag(NotificationFlag.ActiveNoteUpdated);
        }

        public void UpdateSpeedChangeWarnings()
        {
            AssertChartLoaded();
            if (IsStageLoaded()) {
                App.GameStageManager.WarningNotesManager.RefreshActiveNotes();
            }
            NotifyFlag(NotificationFlag.ActiveSpeedChangeWarningUpdated);
        }

        public void LoadChartInCurrentProject(ChartModel chart)
        {
            Debug.Assert(App.ProjectManager.CurrentProject2?.Charts.Contains(chart) is true);

            App.GamePlayManager.NoteSoundsPlayer.CancelScheduledAudio();
            MusicPlayer.Stop();
            MusicPlayer.Time = 0f;
            CurrentChart = chart;

            CheckCollision();
            if (IsStageLoaded()) {
                UpdateNotes(true, true);
                UpdateSpeedChangeWarnings();
            }
            NotifyFlag(NotificationFlag.CurrentChart);

            void CheckCollision()
            {
                var chart = CurrentChart;

                for (int i = 0; i < chart.NoteNodes.Count; i++) {
                    if (chart.NoteNodes[i] is not NoteModel note)
                        continue;

                    for (int j = i + 1; j < chart.NoteNodes.Count; j++) {
                        if (chart.NoteNodes[j] is not NoteModel noteCmp)
                            continue;

                        if (!EntityArgs.IsTimeCollided(note, noteCmp))
                            break;
                        if (EntityArgs.IsPositionCollided(note, noteCmp)) {
                            note.CollisionCount++;
                            noteCmp.CollisionCount++;
                        }
                    }
                }
            }
        }

        public void UnloadChart()
        {
            if (CurrentChart is null)
                return;
            App.GamePlayManager.NoteSoundsPlayer.CancelScheduledAudio();
            CurrentChart = null;
            MusicPlayer.Stop();
            NotifyFlag(NotificationFlag.CurrentChart);
        }

        #region Validation

        [MemberNotNull(nameof(CurrentChart))]
        private void ValidateChart()
        {
            if (CurrentChart is null)
                throw new System.InvalidOperationException("Chart not loaded.");
        }

#pragma warning disable CS8774
        /// <summary>
        /// The method do <c>UnityEngine.Debug.Assert()</c>, and could make IDE
        /// provide a better nullable diagnostic in context
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        [MemberNotNull(nameof(CurrentChart))]
        public void AssertChartLoaded() => Debug.Assert(CurrentChart is not null, "Chart not loaded");

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        [MemberNotNull(nameof(Stage))]
        public void AssertStageLoaded() => Debug.Assert(Stage is not null, "Stage not loaded");
#pragma warning restore CS8774

        [MemberNotNullWhen(true, nameof(Stage))]
        public bool IsStageLoaded() => Stage is not null;

        [MemberNotNullWhen(true, nameof(CurrentChart))]
        public bool IsChartLoaded() => CurrentChart is not null;

        #endregion

        public enum NotificationFlag
        {
            HighlightedNoteSpeed,
            IsApplySpeedDifference,
            IsFilterNoteSpeed,

            NoteSpeed,
            MusicSpeed,
            HitSoundVolume,
            MusicVolume,
            PianoVolume,
            IsShowLinkLines,
            SuddenPlus,
            StageEffectOn,
            DistinguishPianoNotes,
            EarlyDisplaySlowNotes,
            PauseWhenLoseFocus,
            WarningNoteDisplay,

            ActiveNoteUpdated,
            ActiveSpeedChangeWarningUpdated,

            CurrentChart,
            ChartName,
            ChartDifficulty,
            ChartLevel,
            ChartSpeed,
            ChartRemapMinVolume,
            ChartRemapMaxVolume,

            CustomSubBeatLineColor,
            CustomBeatLineColor,
            CustomTempoLineColor,
        }

        public readonly record struct StageLoadedEventArgs(
            GameStageController Stage,
            PerspectiveViewForegroundBase PerspectiveViewForegroundPrefab);
    }
}
