using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal interface IConfigProvider {
    AddLinksFromFolderConfig GetAddLinksFromFolderConfig();

    AddLinksFromServerConfig GetAddLinksFromServerConfig();

    UpdateLinksConfig GetUpdateLinksConfig();
}
