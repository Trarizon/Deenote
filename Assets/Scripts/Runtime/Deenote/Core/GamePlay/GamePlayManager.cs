#nullable enable

using Deenote.Contexts;
using Deenote.Core.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.Core.GameStage;
using Deenote.Core.GameStage.Foreground;
using Deenote.Core.Project;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    public sealed partial class GamePlayManager : FlagNotifiableMonoBehaviour<GamePlayManager, GamePlayManager.NotificationFlag>
    {
        private ProjectContext _projectContext;

        private NotesManager _notesManager = default!;
        private GridsManager _gridsManager = default!;
        [SerializeField] GameMusicPlayer _musicPlayer = default!;
        private StagePianoSoundPlayer _pianoSoundPlayer = default!;
        [SerializeField] HitSoundPlayer _hitSoundPlayer = default!;


        public GameStageController? Stage { get; private set; }

        internal NotesManager NotesManager => _notesManager;
        public GridsManager Grids => _gridsManager;
        public GameMusicPlayer MusicPlayer => _musicPlayer;
        public StagePianoSoundPlayer PianoSoundPlayer => _pianoSoundPlayer;
        public HitSoundPlayer HitSoundPlayer => _hitSoundPlayer;

        public event Action<StageLoadedEventArgs>? StageLoaded;


        public ChartEditorModel? CurrentChart => _projectContext?.CurrentChart;

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
            if (manualPlaySpeed is { } speed) {
                if (speed == 0f) {
                    MusicPlayer.Pitch = ActualMusicSpeed;
                    MusicPlayer.Stop();
                }
                else {
                    _musicPlayer.Pitch = speed;
                }
                _manualPlaySpeedMultiplier = speed;
            }
            else {
                _musicPlayer.Pitch = ActualMusicSpeed;
                _manualPlaySpeedMultiplier = null;
            }
        }

        private void Awake()
        {
            _projectContext = MainSystem.Contexts.Project;

            _gridsManager = new GridsManager(this, MainSystem.StageChartEditor, _projectContext);
            _pianoSoundPlayer = new StagePianoSoundPlayer(MainSystem.PianoSoundSource);
            _notesManager = new NotesManager(this);

            RegisterConfigurations();
            RegisterCustomPropertiesConfigurations();

            GameStageSceneLoader.StageLoaded += loader =>
            {
                Debug.Log("GameStage loaded");
                try {

                    Stage = loader.StageController;
                    Stage.Initialize(this, _projectContext);
                    NotesManager.Initialize(
                        UnityUtils.CreateObjectPool(
                            Stage.Args.GamePlayNotePrefab,
                            Stage.NotePlane.ContentTransform,
                            item => item.OnInstantiate(Stage.NotePlane)));
                    OnStageLoaded_Properties(loader);

                    if (IsChartLoaded()) {
                        UpdateNotes(true, true);
                    }

                    StageLoaded?.Invoke(new StageLoadedEventArgs(Stage, loader.PerspectiveViewForeground));
                } catch (Exception ex) {
                    Debug.LogError(ex.Message + ex.StackTrace);
                    throw;
                }
            };

            MusicPlayer.TimeChanged += args =>
            {
                if (!IsChartLoaded())
                    return;

                var forward = args.NewTime > args.OldTime;
                NotesManager.ShiftStageActiveNotes(!args.IsByJump && _manualPlaySpeedMultiplier is null);
                NotifyFlag(NotificationFlag.ActiveNoteUpdated);
                // In previous version, note time was controlled by StageNoteController.Update,
                // so we have to manually call update when manually change music time.
                // In current version, note time is controlled by GamePlayManager.UpdateActiveNotes,
                // so the following code is not required
                // if (args.IsManuallyChanged) RefreshNotesTimeState();
            };

            _projectContext.RegisterNestedPropertyChangedAndInvokeNullable(s => s.CurrentProject, nameof(ProjectContext.CurrentProject), (s, e) =>
            {
                if (e.MatchProperty(nameof(s.AudioClip))) {
                    if (s?.AudioClip is null) {
                        _musicPlayer.ReplaceClip(NoClipProvider.Instance);
                    }
                    else {
                        // TODO: try out streaming clip provider
                        _musicPlayer.ReplaceClip(new DecodedClipProvider(s.AudioClip!));
                    }
                }
            });
            _projectContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentChart))) {
                    var chart = s.CurrentChart;
                    if (chart is null) {
                        UnloadChart();
                        return;
                    }
                    LoadChartInCurrentProject(chart);
                }
            });

            //MainSystem.ProjectManager.RegisterNotification(
            //    ProjectManager.NotificationFlag.CurrentProject,
            //    manager =>
            //    {
            //        var proj = manager.CurrentProject;
            //        if (proj is null) {
            //            UnloadChart();
            //            return;
            //        }

            //        if (proj.Charts.Count == 0) {
            //            UnloadChart();
            //        }
            //        else {
            //            LoadChartInCurrentProject(proj.Charts[0]);
            //        }

            //        _musicPlayer.ReplaceClip(new DecodedClipProvider(manager.AudioClip!));
            //    });
            //MainSystem.ProjectManager.RegisterNotification(
            //    ProjectManager.NotificationFlag.ProjectAudio,
            //    manager =>
            //    {
            //        manager.AssertProjectLoaded();
            //        _musicPlayer.ReplaceClip(new DecodedClipProvider(manager.AudioClip!));
            //    });

            //MainSystem.ProjectManager.RegisterNotification(
            //    ProjectManager.NotificationFlag.ProjectCharts,
            //    manager =>
            //    {
            //        if (CurrentChart is null)
            //            return;
            //        if (manager.CurrentProject is null)
            //            return;
            //        // Simple check, if current chart is removed from project, unload this and load the first chart
            //        if (!manager.CurrentProject.Charts.Contains(CurrentChart)) {
            //            LoadChartInCurrentProject(manager.CurrentProject.Charts[0]);
            //        }
            //    });
        }

        private void OnDestroy()
        {
            _gridsManager.Dispose();
        }

        private void Update()
        {
            if (!IsChartLoaded())
                return;
            if (!MusicPlayer.IsPlaying && _manualPlaySpeedMultiplier is { } manuallPlaySpeed)
                MusicPlayer.Nudge(Time.deltaTime * manuallPlaySpeed);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus && PauseWhenLoseFocus) {
                MusicPlayer.Stop();
            }
        }

        public void UpdateNotes(bool noteCollectionChangedOrNoteTimeRelatedPropertyChanged, bool notesVisualDataChanged)
        {
            AssertChartLoaded();
            AssertStageLoaded();

            switch (noteCollectionChangedOrNoteTimeRelatedPropertyChanged, notesVisualDataChanged) {
                case (true, false):
                    NotesManager.RefreshStageActiveNotes();
                    break;
                case (false, true):
                    foreach (var note in NotesManager.OnStageNotes) {
                        note.RefreshVisual();
                    }
                    break;
                case (true, true):
                    NotesManager.RefreshStageActiveNotes();
                    break;
                default:
                    return;
            }
            NotifyFlag(NotificationFlag.ActiveNoteUpdated);
        }

        public void LoadChartInCurrentProject(ChartEditorModel chart)
        {
            MusicPlayer.Stop();
            MusicPlayer.Time = 0f;

            if (IsStageLoaded()) {
                UpdateNotes(true, true);
            }
            Debug.Log("Chart loaded");
            NotifyFlag(NotificationFlag.CurrentChart);
        }

        public void UnloadChart()
        {
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

            ActiveNoteUpdated,

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
            ForegroundPerspectiveViewUI PerspectiveViewForegroundPrefab);
    }
}