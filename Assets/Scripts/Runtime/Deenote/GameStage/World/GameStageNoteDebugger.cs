using Deenote.Core.GameStage;
using Deenote.Entities.Models;
using TMPro;
using UnityEngine;

namespace Deenote.GameStage.World
{
#if UNITY_EDITOR

    internal class GameStageNoteDebugger : MonoBehaviour
    {
        [SerializeField] DeemoGameStageNoteController _note;
        [SerializeField] TMP_Text _text;

        void Start()
        {
            _text.gameObject.SetActive(true);
        }

        void LateUpdate()
        {
            _text.SetText("{0}", ((IStageNoteNode?)_note.NoteModel)?.Uid ?? 0);
        }
    }

#endif
}
