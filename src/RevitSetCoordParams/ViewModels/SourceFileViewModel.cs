using dosymep.Revit;
using dosymep.SimpleServices;

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

    // Метод получения имени файла
    private string GetFileName() {
        return !FileProvider.Document.IsLinked
           ? _localizationService.GetLocalizedString("SourceFileViewModel.CurrentFile")
           : SuorceFileUniqueId;
    }
}
