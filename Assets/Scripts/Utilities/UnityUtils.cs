using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Deenote.Utilities
{
    public static class UnityUtils
    {
        public static Color WithAlpha(this in Color color, float alpha)
            => new(color.r, color.g, color.b, alpha);

        public static void AddListener(this UnityEvent ev, Func<UniTaskVoid> uniTaskFunc)
            => ev.AddListener(UniTask.UnityAction(uniTaskFunc));

        public static ObjectPool<T> CreateObjectPool<T>(T prefab, Transform parentTransform = null, int defaultCapacity = 10, int maxSize = 10000) where T : Component
            => new ObjectPool<T>(
                () => GameObject.Instantiate(prefab, parentTransform),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                obj => GameObject.Destroy(obj),
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);

        public static void SetSolidColor(this LineRenderer lineRenderer, Color color)
        {
            var gradient = lineRenderer.colorGradient;
            var keys = gradient.colorKeys;
            var akeys = gradient.alphaKeys;
            foreach (ref var key in keys.AsSpan()) {
                key.color = color;
            }
            foreach (ref var key in akeys.AsSpan()) {
                key.alpha = color.a;
            }
            gradient.colorKeys = keys;
            gradient.alphaKeys = akeys;
            lineRenderer.colorGradient = gradient;
        }

        public static bool IsFunctionalKeyHolding(bool ctrl = false, bool shift = false, bool alt = false)
        {
            var ctrlOk = ctrl == (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            var shiftOk = shift == (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            var altOk = alt == (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
            return ctrlOk && shiftOk && altOk;
        }

        public static bool IsKeyDown(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKeyDown(key) && (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }

        public static bool IsKeyUp(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKeyUp(key) && (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }

        public static bool IsKeyHolding(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKey(key) && (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }
    }
}