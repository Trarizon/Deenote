#nullable enable

using Deenote.Contexts;
using Deenote.Core.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.GamePlay.Audio;
using Deenote.GameStage;
using Deenote.GameStage.Stage;
using Deenote.GameStage.UI;
using Deenote.Library.Components;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    [Obsolete]
    public sealed partial class GamePlayManager : MonoBehaviour
    {
        //internal GamePlayContext _context;
        internal GameStageContext _stageContext;
        internal ProjectContext _projectContext;

        //private NotesManager _notesManager = default!;
        //private GridsManager _gridsManager = default!;
        //[SerializeField] GameMusicPlayer _musicPlayer = default!;
        //private GamePianoSoundPlayer _pianoSoundPlayer = default!;
        //[SerializeField] GameHitSoundPlayer _hitSoundPlayer = default!;

        // public GameStageController? Stage { get => _stageContext.GameStage; private set { } }

        //private NotesManager NotesManager => _notesManager;
        //[Obsolete]
        //private GridsManager Grids => _gridsManager;
        //[Obsolete]
        //internal GameMusicPlayer MusicPlayer => _musicPlayer;

        public event Action<StageLoadedEventArgs>? StageLoaded;

        public void UpdateNotes(bool noteCollectionChangedOrNoteTimeRelatedPropertyChanged, bool notesVisualDataChanged)
        {
            //AssertChartLoaded();
            //AssertStageLoaded();
            _stageContext.NotesContext.RefreshActiveVisibleNotes();
            return;
            //switch (noteCollectionChangedOrNoteTimeRelatedPropertyChanged, notesVisualDataChanged) {
            //    case (true, false):
            //        NotesManager.RefreshStageActiveNotes();
            //        break;
            //    case (false, true):
            //        foreach (var note in NotesManager.OnStageNotes) {
            //            note.RefreshVisual();
            //        }
            //        break;
            //    case (true, true):
            //        NotesManager.RefreshStageActiveNotes();
            //        break;
            //    default:
            //        return;
            //}
            //NotifyFlag(NotificationFlag.ActiveNoteUpdated);
        }

        //Update()
        //{
        //    if (IsStageLoaded()) {
        //        NotesManager.RefreshStageNoteTimeDisplay();
        //    }
        //}

        public readonly record struct StageLoadedEventArgs(
            GameStageController Stage,
            ForegroundPerspectiveViewUI PerspectiveViewForegroundPrefab);
    }
}