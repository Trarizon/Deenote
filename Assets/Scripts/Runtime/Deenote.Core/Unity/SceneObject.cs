using System;
using UnityEngine;

namespace Deenote.Core.Unity
{
    internal interface ISceneObject { }

    public abstract class SceneObject<TSelf> : MonoBehaviour, ISceneObject where TSelf : SceneObject<TSelf>
    {
        internal static Action<SceneObject<TSelf>>? _loadedCallback;

        protected void Awake()
        {
            _loadedCallback?.Invoke(this);
        }
    }
}