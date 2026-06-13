#nullable enable

using Deenote.Contexts;
using Deenote.Core.Audio;
using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.GamePlay.Audio;
using Deenote.GameStage.UI;
using Deenote.Systems;

namespace Deenote.GamePlay
{
    public sealed partial class GamePlayManagerB
    {
        private readonly EnvironmentContext _environment;
        private readonly GamePlayContext _context;
        private readonly ProjectContext _project;
        private readonly GameMusicPlayer _musicPlayer;
        private readonly NoteSoundsPlayManager _noteSoundsPlay;
        private readonly IPerspectiveViewPanelInfoProvider _perspectiveViewPanelInfoProvider;
        private readonly InputInterpreter _inputInterpreter;

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

        internal GamePlayManagerB(EnvironmentContext environment, GamePlayContext context, ProjectContext project, GameMusicPlayer musicPlayer, GamePianoSoundPlayer pianoSoundPlayer, GameHitSoundPlayer hitSoundPlayer, IPerspectiveViewPanelInfoProvider perspectiveViewPanelInfoProvider, InputInterpreter inputInterpreter)
        {
            _environment = environment;
            _project = project;
            _context = context;
            _musicPlayer = musicPlayer;
            _noteSoundsPlay = new NoteSoundsPlayManager(_context, _project, hitSoundPlayer, pianoSoundPlayer);
            _perspectiveViewPanelInfoProvider = perspectiveViewPanelInfoProvider;
            _inputInterpreter = inputInterpreter;

        }

        internal void OnStart()
        {
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
                if (e.MatchProperty(nameof(s.MusicVolume))) {
                    _musicPlayer.Volume = s.MusicVolume;
                }
                if (e.MatchProperty(nameof(s.ActualMusicSpeed))) {
                    _musicPlayer.Pitch = s.ActualMusicSpeed;
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

            RegisterHooks(MainSystem.GlobalHooks);
            RegisterInputs();
        }

        private void RegisterHooks(MonoBehaviourHooks hooks)
        {
            hooks.Tick += (delta) =>
            {
                if (_project.CurrentChart is null)
                    return;

                if (!_musicPlayer.IsPlaying && _manualPlaySpeedMultiplier is { } manual) {
                    _musicPlayer.Nudge(delta * manual);
                }
            };

            hooks.ApplicationFocusChanged += (focus) =>
            {
                if (!focus && _context.PauseWhenLoseFocus) {
                    _musicPlayer.Stop();
                }
            };
        }

        private void Update_MusicPlayerPitch()
        {

        }

        public void TogglePlayingState()
        {
            _musicPlayer.TogglePlayingState();
        }

        public void Play()
        {
            _musicPlayer.Play();
        }

        public void Stop()
        {
            _musicPlayer.Stop();
        }

        public void Nudge(float delta)
        {
            _musicPlayer.Nudge(delta);
        }
    }
}
