using dosymep.Revit;
using dosymep.SimpleServices;

using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;
internal class SourceFileViewModel {

    private readonly ILocalizationService _localizationService;

    public SourceFileViewModel(ILocalizationService localizationService, IFileProvider fileProvider) {
        _localizationService = localizationService;
        FileProvider = fileProvider;
        SuorceFileUniqueId = FileProvider.Document.GetUniqId();
        FileName = GetFileName();
    }

    public IFileProvider FileProvider { get; private set; }
    public string SuorceFileUniqueId { get; set; }
    public string FileName { get; set; }

    private string GetFileName() {
        return FileProvider.Type == FileProviderType.CurrentFileProvider
           ? _localizationService.GetLocalizedString("SourceFileViewModel.CurrentFile")
           : SuorceFileUniqueId;
    }
}
