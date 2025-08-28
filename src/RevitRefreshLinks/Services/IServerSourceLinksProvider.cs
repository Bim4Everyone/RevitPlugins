using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal interface IServerSourceLinksProvider {
    Task<ILinksFromSource> GetServerLinksAsync();
}
