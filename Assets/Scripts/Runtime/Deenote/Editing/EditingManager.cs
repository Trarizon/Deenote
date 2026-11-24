using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Project;

namespace Deenote.Editing;

public sealed class EditingManager
{
    public ChartNotesSelector NotesSelector { get; }

    internal StageDragSelector DragSelector { get; }

    public EditingManager(ProjectManager2 projectManager, GameStageManager gameStageManager, GamePlayManager2 gamePlayManager)
    {
        NotesSelector = new ChartNotesSelector(projectManager);
        DragSelector = new StageDragSelector(NotesSelector, 
            projectManager, gameStageManager, gamePlayManager);
    }
}
