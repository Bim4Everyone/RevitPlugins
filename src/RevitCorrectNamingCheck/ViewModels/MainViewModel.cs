using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCorrectNamingCheck.Models;
using RevitCorrectNamingCheck.Services;

namespace RevitCorrectNamingCheck.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly LinkedFileEnricher _linkedFileEnricher;
    private readonly ILocalizationService _localization;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    public MainViewModel(
        RevitRepository revitRepository,
        LinkedFileEnricher linkedFileEnricher,
        ILocalizationService localization) {
        _revitRepository = revitRepository;
        _linkedFileEnricher = linkedFileEnricher;
        _localization = localization;
        LinkedFiles = [];

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView);
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public ObservableCollection<LinkedFileViewModel> LinkedFiles { get; }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а также инициализация полей окна.</remarks>
    private void LoadView() {
        var links = _revitRepository.GetLinkedFiles();
        var worksets = _revitRepository.GetUserWorksets().Select(w => new WorksetInfo(w.Key, w.Value)).ToArray();
        foreach(var link in links) {
            var vm = new LinkedFileViewModel(link, worksets);
            _linkedFileEnricher.Enrich(vm);
            LinkedFiles.Add(vm);
        }
    }

    private void AcceptView() {
        if(!_revitRepository.Document.IsWorkshared) {
            return;
        }

        using var transaction = _revitRepository.Document
            .StartTransaction(_localization.GetLocalizedString("Transaction.UpdateLinks"));
        foreach(var vm in LinkedFiles) {
            _revitRepository.UpdateLinkedFile(vm.LinkedFile, vm.TypeWorkset.Id, vm.InstanceWorkset.Id, vm.IsPinned);
        }

        transaction.Commit();
    }
}
