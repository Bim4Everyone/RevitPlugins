using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal interface ILocalSourceLinksProvider {
    ILinksFromSource GetLocalLinks();
}
