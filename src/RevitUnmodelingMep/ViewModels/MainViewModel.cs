using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _saveProperty;
    private ObservableCollection<ConsumableTypeItem> _consumableTypes;
    
    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        AddConsumableTypeCommand = RelayCommand.Create(AddConsumableType);
        RemoveConsumableTypeCommand = RelayCommand.Create<ConsumableTypeItem>(RemoveConsumableType, CanRemoveConsumableType);

        ConsumableTypes = new ObservableCollection<ConsumableTypeItem> {
            new ConsumableTypeItem { Title = "Расходник 1", Name = "Тестовый расходник" }
        };
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }
    
    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    public ICommand AddConsumableTypeCommand { get; }

    public ICommand RemoveConsumableTypeCommand { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Свойство для примера. (требуется удалить)
    /// </summary>
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }

    public ObservableCollection<ConsumableTypeItem> ConsumableTypes {
        get => _consumableTypes;
        set => RaiseAndSetIfChanged(ref _consumableTypes, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(SaveProperty)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
            return false;
        }

        ErrorText = null;
        return true;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        _pluginConfig.SaveProjectConfig();
    }

    private void AddConsumableType() {
        int index = ConsumableTypes.Count + 1;
        ConsumableTypes.Add(new ConsumableTypeItem {
            Title = $"Расходник {index}",
            Name = "Тестовый расходник"
        });
        CommandManager.InvalidateRequerySuggested();
    }

    private bool CanRemoveConsumableType(ConsumableTypeItem item) {
        return ConsumableTypes?.Count > 0 && (item == null || ConsumableTypes.Contains(item));
    }

    private void RemoveConsumableType(ConsumableTypeItem item) {
        if(ConsumableTypes == null || ConsumableTypes.Count == 0) {
            return;
        }

        if(item != null && ConsumableTypes.Contains(item)) {
            ConsumableTypes.Remove(item);
        } else {
            ConsumableTypes.RemoveAt(ConsumableTypes.Count - 1);
        }

        CommandManager.InvalidateRequerySuggested();
    }

    internal class ConsumableTypeItem : BaseViewModel {
        private string _title;
        private string _selectedType;
        private string _name;
        private string _category;
        private string _grouping;
        private string _naming;
        private string _brand;
        private string _code;
        private string _unit;
        private string _factory;
        private string _numberFormula;
        private string _noteFormat;
        private string _enamel;
        private string _primer;

        public string Title {
            get => _title;
            set => RaiseAndSetIfChanged(ref _title, value);
        }

        public string SelectedType {
            get => _selectedType;
            set => RaiseAndSetIfChanged(ref _selectedType, value);
        }

        public string Name {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        public string Category {
            get => _category;
            set => RaiseAndSetIfChanged(ref _category, value);
        }

        public string Grouping {
            get => _grouping;
            set => RaiseAndSetIfChanged(ref _grouping, value);
        }

        public string Naming {
            get => _naming;
            set => RaiseAndSetIfChanged(ref _naming, value);
        }

        public string Brand {
            get => _brand;
            set => RaiseAndSetIfChanged(ref _brand, value);
        }

        public string Code {
            get => _code;
            set => RaiseAndSetIfChanged(ref _code, value);
        }

        public string Unit {
            get => _unit;
            set => RaiseAndSetIfChanged(ref _unit, value);
        }

        public string Factory {
            get => _factory;
            set => RaiseAndSetIfChanged(ref _factory, value);
        }

        public string NumberFormula {
            get => _numberFormula;
            set => RaiseAndSetIfChanged(ref _numberFormula, value);
        }

        public string NoteFormat {
            get => _noteFormat;
            set => RaiseAndSetIfChanged(ref _noteFormat, value);
        }

        public string Enamel {
            get => _enamel;
            set => RaiseAndSetIfChanged(ref _enamel, value);
        }

        public string Primer {
            get => _primer;
            set => RaiseAndSetIfChanged(ref _primer, value);
        }
    }
}
