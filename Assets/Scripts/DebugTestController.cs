using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR

namespace Deenote
{
    public sealed partial class DebugTestController : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            var proj = await DebugUtils.GetTestProjectAsync();
            Debug.Log("Loaded test project");
            App.ProjectManager.OpenProject(proj.Item1, proj.Item2);
        }
    }
}

#endif
