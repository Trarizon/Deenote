namespace Deenote.GameStage
{
    public enum GameStageWarningNoteVisibility
    {
        Hide = 0,
        ShowNote = 1,
        ShowLine = 2,
        ShowNoteAndLine = 3
    }

    public static class GameStageWarningNoteVisibilityExtensions
    {
        public static bool IsShowNote(this GameStageWarningNoteVisibility visibility)
            => (visibility & GameStageWarningNoteVisibility.ShowNote) != 0;
    }
}