using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface ITwoSourceLinksProvider {
        ISelectLinksResult GetLocalLinks();
        ISelectLinksResult GetServerLinks();
    }
}
