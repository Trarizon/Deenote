#nullable enable

using Deenote.Contexts;
using Deenote.Core.Audio;
using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.GamePlay.Audio;
using Deenote.Systems;
using System;
using UnityEngine;

namespace Deenote.GamePlay
{
    partial class GamePlayManagerB
    {
        private float? _musicAutoResetTime;
        private bool _manualPlaySpeedUp;
        private float _manualPlaySpeedInputMultiplier;
        private void RegisterInputs()
        {
            var action = _inputInterpreter.InputActions.StageGamePlay;
            action.PauseResume.started += _ => TogglePlayingState();
            action.AutoResetPlay.started += _ =>
            {
                _musicAutoResetTime = _context.CurrentTime;
                Play();
            };
            action.AutoResetPlay.canceled += _ =>
            {
                if (_musicAutoResetTime is { } resetTime) {
                    Stop();
                    _context.CurrentTime = resetTime;
                    _musicAutoResetTime = null;
                }
            };
            action.ToMusicStart.started += _ => _context.CurrentTime = 0f;
            action.ToMusicEnd.started += _ => _context.CurrentTime = _context.MusicLength;
            action.ManualPlay.started += ctx =>
            {
                _manualPlaySpeedInputMultiplier = ctx.ReadValue<float>();
                SetManualPlay();
            };
            action.ManualPlay.canceled += _ =>
            {
                _manualPlaySpeedInputMultiplier = 0f;
                SetManualPlay();
            };
            action.ManualPlaySpeedUp.started += _ =>
            {
                _manualPlaySpeedUp = true;
                SetManualPlay();
            };
            action.ManualPlaySpeedUp.canceled += _ =>
            {
                _manualPlaySpeedUp = false;
                SetManualPlay();
            };
            action.ScrollPlay.performed += ctx =>
            {
                if (!_perspectiveViewPanelInfoProvider.IsMouseHovering)
                    return;

                var delta = ctx.ReadValue<Vector2>().y;
                if (delta != 0f) {
                    var deltaTime = delta * 0.001f * _environment.GameViewScrollSensitivity;
                    Nudge(-deltaTime);
                }
            };

            const int MusicSpeedDelta = 1;

            var sActions = _inputInterpreter.InputActions.StageSettings;
            sActions.MusicSpeedUp.started += _ => _context.MusicSpeed += MusicSpeedDelta;
            sActions.MusicSpeedDown.started += _ => _context.MusicSpeed -= MusicSpeedDelta;

            void SetManualPlay()
            {
                if (_manualPlaySpeedInputMultiplier == 0f) {
                    SetManualPlaySpeed(null);
                    return;
                }
                if (_manualPlaySpeedUp) {
                    SetManualPlaySpeed(5f * _manualPlaySpeedInputMultiplier);
                }
                else {
                    SetManualPlaySpeed(2.5f * _manualPlaySpeedInputMultiplier);
                }
            }
        }
    }
}
