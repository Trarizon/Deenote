#nullable enable

using Deenote.UIFramework.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.UIFramework
{
    [CreateAssetMenu(
      fileName = nameof(UIResources),
      menuName = $"Deenote.UIFramework/{nameof(UIResources)}")]
    [MovedFrom(true,sourceClassName:"UIThemeResources")]
    public sealed class UIResources : ScriptableObject
    {
        [Header("Font")]
        public string PreferedFontName = default!;
        public string[] FallbackFontNames = default!;
        public TMP_FontAsset FinalFallbackFont = default!;
        [Header("CheckBox")]
        public Sprite CheckBoxCheckedIcon = default!;
        public Sprite CheckBoxIndeterminateIcon = default!;

        [Header("Prefabs")]
        public DropdownItem DropdownItemPrefab = default!;
    }
}