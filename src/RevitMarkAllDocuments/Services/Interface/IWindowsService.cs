using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.ViewModels;

namespace RevitMarkAllDocuments.Services;

internal interface IWindowsService {
    bool ShowMarkListWindow(MarkDataByDocument markDataForCurrentDoc, RevitParam markParam);

    bool ShowWarningsWindow(WarningsViewModel warnings);
}
