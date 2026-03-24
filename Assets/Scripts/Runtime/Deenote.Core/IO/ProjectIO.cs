#nullable enable

using CommunityToolkit.HighPerformance;
using Deenote.CoreB.Models.Projects;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Deenote.CoreB.IO
{
    public static class ProjectIO
    {
        public const ushort DeenoteProjectFileHeader = 0xDEE0;
        public const byte DeenoteProjectFileVersionMark = 1;

        public static async Task<ProjectModel?> LoadAsync(string projectFilePath, CancellationToken cancellationToken = default)
        {
            if (projectFilePath.EndsWith(".dsproj")) {
                using var fsDsproj = File.OpenRead(projectFilePath);
                var dsprojResult = await DsprojSerializer.Instance.DeserializeAsync(fsDsproj, cancellationToken).ConfigureAwait(false);
                if (dsprojResult is not null)
                    return dsprojResult;
            }

            using var fs = File.OpenRead(projectFilePath);
            var header = fs.Read<ushort>();
            if (header != DeenoteProjectFileHeader)
                return null;

            var version = fs.Read<byte>();
            if (version == ProjectSerializer.FileVersionMark) {
                fs.Seek(0, SeekOrigin.Begin);
                return await ProjectSerializer.Instance.DeserializeAsync(fs, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }

        public static Task SaveAsync(ProjectModel project, string saveFilePath, CancellationToken cancellationToken = default)
        {
            return ProjectSerializer.Instance.SerializeAsync(project, saveFilePath, cancellationToken);
        }
    }
}
