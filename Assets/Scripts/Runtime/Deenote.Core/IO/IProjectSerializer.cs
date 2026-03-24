#nullable enable

using Deenote.CoreB.Models.Projects;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Deenote.CoreB.IO
{
    internal interface IProjectSerializer
    {
        Task SerializeAsync(ProjectModel project, string path, CancellationToken cancellationToken = default);
        Task<ProjectModel?> DeserializeAsync(FileStream stream, CancellationToken cancellationToken = default);
    }
}
