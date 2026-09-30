using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Deenote.GameStage.UI
{
    public interface IGameStagePerspectiveViewPanel
    {
        RenderTexture ViewRendererTexture { get; }
        UniTask ApplyStageAsync(Func<Transform, UniTask<GameStageForegroundView>> instantiateForegroundViewAsync);
    }
}