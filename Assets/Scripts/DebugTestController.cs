using Cysharp.Threading.Tasks;
using TriInspector;
using UnityEngine;

#if UNITY_EDITOR

namespace Deenote
{
    [DeclareFoldoutGroup("GamePlayManager", Expanded = true)]
    public sealed partial class DebugTestController : MonoBehaviour
    {
        [ShowInInspector, Group("GamePlayerManager")]
        float StagePlaySpeed => App.Current is null ? 0 : App.GamePlayManager.StagePlaySpeed;
        
        [ShowInInspector, Group("GamePlayerManager")]
        float CurrentTime => App.Current is null ? 0 : App.GamePlayManager.CurrentTime;

        private async UniTaskVoid Start()
        {
            var proj = await DebugUtils.GetTestProjectAsync();
            Debug.Log("Loaded test project");
            App.ProjectManager.OpenProject(proj.Item1, proj.Item2);
        }
    }
}

#endif
