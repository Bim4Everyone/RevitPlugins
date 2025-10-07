using System;

namespace RevitServerFolders.Models;

internal class FileModelObjectExportSettings : ExportSettings {
    private NwcExportViewSettings _viewSettings;

    public bool IsExportRooms { get; set; }


    public void SetNwcExportViewSettings(NwcExportViewSettings viewSettings) {
        if(viewSettings is null) {
            throw new ArgumentNullException(nameof(viewSettings));
        }
        _viewSettings = viewSettings;
    }

    public NwcExportViewSettings GetNwcExportViewSettings() {
        if(_viewSettings is null) {
            throw new InvalidOperationException("Сначала надо задать настройки вида для экспорта в NWC");
        }
        return _viewSettings;
    }
}
