using Deenote.GameStage.Themes;
using UnityEngine;

namespace Deenote
{
    [CreateAssetMenu(
        fileName = "AppGlobalConfigs",
        menuName = "Deenote/AppGlobalConfigs")]
    public sealed class AppGlobalConfigs : ScriptableObject
    {
        [SerializeField] internal GameStageThemeConfig[] BuiltinStageThemes;
    }
}
