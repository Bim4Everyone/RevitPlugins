using System.Collections.Generic;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCorrectNamingCheck.Models;

namespace RevitCorrectNamingCheck.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    private List<LinkedFile> _linkedFiles;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    public MainViewModel(RevitRepository revitRepository) {
        _revitRepository = revitRepository;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    public List<LinkedFile> LinkedFiles {
        get => _linkedFiles;
        set => RaiseAndSetIfChanged(ref _linkedFiles, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LinkedFiles = _revitRepository.GetLinkedFiles();
    }
}
