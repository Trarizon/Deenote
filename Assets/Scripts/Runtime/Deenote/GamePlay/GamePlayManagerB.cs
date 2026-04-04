#nullable enable

using Deenote.Contexts;
using Deenote.Core.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Notification;
using System;

namespace Deenote.GamePlay
{
    public sealed class GamePlayManagerB : INotifyPropertyChanged<GamePlayManagerB>
    {
        private readonly GamePlayContext _context;
        private readonly ProjectContext _project;
        private readonly GameMusicPlayer _musicPlayer;

        private readonly NoteSoundsPlayManager _noteSoundsPlay;

        private float? _manualPlaySpeedMultiplier;

        public float StagePlaySpeed => _manualPlaySpeedMultiplier ?? _musicPlayer.Pitch;

        public void SetManualPlaySpeed(float? manualPlaySpeed)
        {
            if (manualPlaySpeed is { } speed) {
                if (speed == 0f) {
                    _musicPlayer.Pitch = _context.ActualMusicSpeed;
                    _musicPlayer.Stop();
                }
                else {
                    _musicPlayer.Pitch = speed;
                }
                _manualPlaySpeedMultiplier = speed;
            }
            else {
                _musicPlayer.Pitch = _context.ActualMusicSpeed;
                _manualPlaySpeedMultiplier = null;
            }
        }

        public event Action<GamePlayManagerB, PropertyEventArgs>? PropertyChanged;

        internal GamePlayManagerB(GamePlayContext context, ProjectContext project, GameMusicPlayer musicPlayer)
        {
            _project = project;
            _context = context;
            _musicPlayer = musicPlayer;
            _noteSoundsPlay = new NoteSoundsPlayManager(_project, _context, new(), new(null!));

            _musicPlayer.TimeChanged += (args) =>
            {
                _context.CurrentTime = args.NewTime;
            };
            _context.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentTime))) {
                    _musicPlayer.Time = s.CurrentTime;
                    _noteSoundsPlay.UpdateTime(s.CurrentTime, true);
                }
            });

            _project.RegisterNestedPropertyChangedAndInvokeNullable(s => s.CurrentProject, nameof(_project.CurrentProject), (s, e) =>
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

            _project.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentChart))) {
                    var chart = s.CurrentChart!;
                    if (chart is null) {
                        _musicPlayer.Stop();
                    }
                }
            });
            MainSystem.GlobalHook.Tick += (delta) =>
            {
                if (_project.CurrentChart is null)
                    return;

                if (!_musicPlayer.IsPlaying && _manualPlaySpeedMultiplier is { } manual) {
                    _musicPlayer.Nudge(delta * manual);
                }
            };

            MainSystem.GlobalHook.ApplicationFocusChanged += (focus) =>
            {
                if (!focus && _context.PauseWhenLoseFocus) {
                    _musicPlayer.Stop();
                }
            };
        }
    }
}
