using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface ITwoSourceLinksProvider {
        ISelectLinksResult GetLocalLinks();
        Task<ISelectLinksResult> GetServerLinks();
    }
}
