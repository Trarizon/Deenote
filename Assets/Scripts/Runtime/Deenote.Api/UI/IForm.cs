#nullable enable

using System.Collections.Immutable;

namespace Deenote.Api.UI
{
    public interface IForm
    {
        public string? Title { get; }
    }

    public sealed class Form : IForm
    {
        public string? Title { get; set; }
        public ImmutableArray<IFormColumn> Content { get; }
    }
}
