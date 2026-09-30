using Cysharp.Threading.Tasks;
using Deenote.GamePlay.UI;
using System;
using UnityEngine;

namespace Deenote.GameStage.UI
{
    public class GameStageForegroundView : MonoBehaviour
    {
        [field: SerializeField]
        public GameStageUIArgs Args { get; private set; } = default!;
    }
}
