#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage
{
    public sealed class GameStageThemeEntry : MonoBehaviour
    {
        [SerializeField] private GameStageController _stage;
        [SerializeField] private GameStageController _foreground;
    }
}