using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.CoreB
{
    public static class UnityUtils
    {
        public static ObjectPool<T> CreateObjectPool<T>(Func<T> createFunc, int defaultCapacity = 10,
            int maxSize = 10000) where T : Component
            => new(createFunc,
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                obj =>
                {
                    obj.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(obj);
                },
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
    }
}
