using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _saveProperty;

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

        Parameters.Add(new ParamViewModel {
            Header = "Параметр 1",
            Description = "Описание параметра 1",
            CommonParamHeader = "Параметр донора",
            CommonParam = "ФОП_Этаж СМР",
            ElementParamHeader = "Параметр элемента",
            ElementParam = "ФОП_Этаж СМР"
        });

        Parameters.Add(new ParamViewModel {
            Header = "Параметр 2",
            Description = "Описание параметра 2",
            CommonParamHeader = "Параметр донора",
            CommonParam = "ФОП_Секция СМР",
            ElementParamHeader = "Параметр элемента",
            ElementParam = "ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Стены",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Перекрытия",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Потолки",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Каркас несущий",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Фундаменты несущей конструкции",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Колонны",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Воздуховоды",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Стены",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });


        Categories.Add(new CategoryViewModel {
            CategoryName = "Стены",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Перекрытия",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Потолки",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Каркас несущий",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Фундаменты несущей конструкции",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Колонны",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Воздуховоды",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Стены",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Стены",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Перекрытия",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Потолки",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Каркас несущий",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Фундаменты несущей конструкции",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Колонны",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Воздуховоды",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });

        Categories.Add(new CategoryViewModel {
            CategoryName = "Стены",
            IsChecked = true,
            HasWarning = true,
            WarningDescription = "Отсутствуют параметры: ФОП_Этаж СМР, ФОП_Секция СМР"
        });
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

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ObservableCollection<ParamViewModel> Parameters { get; } = [];

    public ObservableCollection<CategoryViewModel> Categories { get; } = [];

    /// <summary>
    /// Свойство для примера. (требуется удалить)
    /// </summary>
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
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
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        _pluginConfig.SaveProjectConfig();
    }
}
