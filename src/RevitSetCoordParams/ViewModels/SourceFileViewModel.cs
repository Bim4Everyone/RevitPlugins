using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;
internal class SourceFileViewModel : BaseViewModel {

    private readonly ILocalizationService _localizationService;
    private string _sourceFileUniqueId;
    private string _fileName;

    public SourceFileViewModel(ILocalizationService localizationService, IFileProvider fileProvider) {
        _localizationService = localizationService;
        FileProvider = fileProvider;
        SourceFileUniqueId = FileProvider.Document.GetUniqId();
        FileName = GetFileName();
    }

    public IFileProvider FileProvider { get; private set; }

    public string SourceFileUniqueId {
        get => _sourceFileUniqueId;
        set => RaiseAndSetIfChanged(ref _sourceFileUniqueId, value);
    }
    public string FileName {
        get => _fileName;
        set => RaiseAndSetIfChanged(ref _fileName, value);
    }

    // Метод получения имени файла
    private string GetFileName() {
        return !FileProvider.Document.IsLinked
           ? _localizationService.GetLocalizedString("SourceFileViewModel.CurrentFile")
           : SourceFileUniqueId;
    }
}
