using System.Threading;
using System.Threading.Tasks;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
internal interface INwcExportViewSettingsService {
    Task<NwcExportViewSettingsDialogResult> ShowDialogAsync(
        NwcExportViewSettings currentSettings,
        CancellationToken ct = default);
}
