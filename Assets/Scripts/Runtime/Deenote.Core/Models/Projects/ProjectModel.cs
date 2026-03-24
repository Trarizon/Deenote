#nullable enable

using Deenote.CoreB.Models.Charts;
using System;
using System.Collections.Generic;

namespace Deenote.CoreB.Models.Projects
{
    public sealed class ProjectModel
    {
        public string MusicName { get; set; } = "";
        public string Composer { get; set; } = "";
        public string ChartDesigner { get; set; } = "";
        public byte[] AudioFileData { get; set; } = Array.Empty<byte>();
        public string AudioFileRelativePath { get; set; } = "";
        public List<ChartModel> Charts { get; } = new List<ChartModel>();
        public List<Tempo> Tempos { get; } = new List<Tempo>();
    }
}
