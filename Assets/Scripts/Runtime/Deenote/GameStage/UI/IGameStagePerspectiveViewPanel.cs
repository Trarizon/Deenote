using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Deenote.GameStage.UI
{
    public interface IGameStagePerspectiveViewPanel
    {
        bool IsFullScreen { get; set; }
        RenderTexture ViewRendererTexture { get; }
        UniTask ApplyStageAsync(Func<Transform, UniTask<GameStageForegroundView?>> instantiateForegroundViewAsync);
        
        event Action<IGameStagePerspectiveViewPanel>? ViewSizeChanged;
        bool TryConvertScreenPointToViewportPoint(Vector2 screenPoint, out Vector2 viewportPoint);
    }
}