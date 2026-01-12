#nullable enable

using Deenote.Api.UI;
using System.Threading.Tasks;

namespace Deenote.Api.Experimental
{
    public interface IFormManager
    {
        public Task ShowForm(IForm form);
    }
}
