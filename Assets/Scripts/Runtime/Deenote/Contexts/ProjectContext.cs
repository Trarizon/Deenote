using Deenote.Contexts.EditorModels;
using Deenote.Models;

namespace Deenote.Contexts
{
    public sealed class ProjectContext
    {
        public ProjectEditorModel? CurrentProject { get; set; }
    }
}
