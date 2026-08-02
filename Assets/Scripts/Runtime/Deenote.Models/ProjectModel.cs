using Deenote.CoreB.Helpers;
using System;
using System.Collections.Generic;

namespace Deenote.Models
{
    public sealed partial class ProjectModel
    {
        public string MusicName { get; set; } = "";
        public string Composer { get; set; } = "";
        public string ChartDesigner { get; set; } = "";
        public byte[] AudioFileData { get; set; } = Array.Empty<byte>();
        public string AudioFileRelativePath { get; set; } = "";
        public List<ChartModel> Charts { get; } = new List<ChartModel>();
        public List<Tempo> Tempos { get; } = new List<Tempo>();

        public ProjectModel()
        {
            
        }

        public ProjectModel Clone()
        {
            var proj = new ProjectModel {
                MusicName = MusicName,
                Composer = Composer,
                ChartDesigner = ChartDesigner,
                AudioFileData = AudioFileData,
                AudioFileRelativePath = AudioFileRelativePath,
            };

            proj.Charts.EnsureCapacity(Charts.Count);
            foreach (var chart in Charts)
            {
                proj.Charts.Add(chart.Clone());
            }
            
            proj.Tempos.EnsureCapacity(Tempos.Count);
            proj.Tempos.Replace(Tempos.AsSpan());
            return proj;
        }
    }
}
