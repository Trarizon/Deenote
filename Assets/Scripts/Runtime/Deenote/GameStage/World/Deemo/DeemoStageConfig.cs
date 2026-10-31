using TriInspector;
using UnityEngine;

namespace Deenote.GameStage.World.Deemo
{
    [CreateAssetMenu(
        fileName = "DeemoStageConfig",
        menuName = "Deenote/GameStage/Deemo/StageConfig"
    )]
    [DeclareBoxGroup("Prefabs")]
    public class DeemoStageConfig : GameStageConfig
    {
        [Title("Deemo Stage Configs")]
        public Material HoldBodyCullingMaterial;
    }
}
