using System.Collections.ObjectModel;

using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.Validation;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;

/// <summary>
/// Базовый модуль комплекта (лист или компонент листа): галочка включения в генерацию, имя, комментарий
/// и общий порядок создания элементов в модели
/// </summary>
internal abstract class ModuleVM : BaseParamContainerVM, ICheckableModule {
    private bool _isModuleCheck = false;
    private string _moduleName = string.Empty;
    private string _moduleComment = string.Empty;
    private string _moduleCode = string.Empty;
    private string _moduleTypeName = string.Empty;

    protected ModuleVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        ILocalizationService localizationService)
        : base(repository, stringParamSetService, sheetSetParams, localizationService) {
    }

    /// <summary>
    /// Модуль включен в генерацию
    /// </summary>
    public bool IsModuleCheck {
        get => _isModuleCheck;
        set => RaiseAndSetIfChanged(ref _isModuleCheck, value);
    }

    /// <summary>
    /// Имя модуля, задаваемое пользователем
    /// </summary>
    public string ModuleName {
        get => _moduleName;
        set => RaiseAndSetIfChanged(ref _moduleName, value);
    }

    /// <summary>
    /// Комментарий к модулю
    /// </summary>
    public string ModuleComment {
        get => _moduleComment;
        set => RaiseAndSetIfChanged(ref _moduleComment, value);
    }

    /// <summary>
    /// Код модуля.
    /// </summary>
    /// <remarks>
    /// Сейчас не используется: не отображается в окне и не сохраняется в конфигурацию. Значения задаются
    /// фабрикой моделей представления. Нужен для следующей итерации переработки плагина по ТЗ - не удалять.
    /// </remarks>
    public string ModuleCode {
        get => _moduleCode;
        set => RaiseAndSetIfChanged(ref _moduleCode, value);
    }

    /// <summary>
    /// Локализованное название типа модуля для заголовка карточки
    /// </summary>
    public string ModuleTypeName {
        get => _moduleTypeName;
        set => RaiseAndSetIfChanged(ref _moduleTypeName, value);
    }

    /// <summary>
    /// Создает элементы модуля в отдельной транзакции (команда кнопки модуля)
    /// </summary>
    public abstract void CreateComponent();

    /// <summary>
    /// Проверяет, можно ли создать модуль (нет ошибок заполнения)
    /// </summary>
    public abstract bool CanCreateComponent();

    /// <summary>
    /// Создает или находит элементы модуля в модели. Должен вызываться внутри открытой транзакции
    /// </summary>
    /// <param name="processDependent">Обработать также зависимые модули (для листа - его компоненты)</param>
    public abstract void Process(bool processDependent = false);
}
