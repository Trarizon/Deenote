using Deenote.Core.Notification;
using System;

namespace Deenote.Contexts.EditorModels
{
    public sealed class ProjectEditorModel : INotifyPropertyChanged<ProjectEditorModel>
    {
        public string MusicName { get; set; }
        public string Composer { get; set; }
        public string ChartDesigner { get; set; }
        public string AudioFileRelativePath { get; set; }

        public event Action<ProjectEditorModel, PropertyEventArgs>? PropertyChanged;
    }
}
