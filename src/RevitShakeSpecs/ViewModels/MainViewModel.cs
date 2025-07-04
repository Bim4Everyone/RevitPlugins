using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitShakeSpecs.Models;

namespace RevitShakeSpecs.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private string _errorText = string.Empty;
    private string _selectedProjectSectionParamName = string.Empty;
    private string _selectedProjectSection = string.Empty;



    public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;

        // Подгружаем выбранное при прошлом сеансе
        LoadConfig();
        // Заполняем параметры (в один из них пользователь писал название комплекта)
        GetFilterableParams();
        // Заполняем варианты комплектов, в соответствии с выбранным параметров
        GetProjectSections(null);

        GetProjectSectionsCommand = new RelayCommand(GetProjectSections);
        RegenerateCommand = new RelayCommand(Regenerate, CanShake);
        ShakeCommand = new RelayCommand(Shake, CanShake);
    }



    public ICommand RegenerateCommand { get; }
    public ICommand ShakeCommand { get; }
    public ICommand GetProjectSectionsCommand { get; }



    /// <summary>
    /// Параметры листов, по которым может идти фильтрация
    /// </summary>
    public List<string> ProjectSectionParamNames { get; set; } = [];

    /// <summary>
    /// Разделы проекта (доступные варианты заполнения выбранного параметра из ProjectSectionParamNames)
    /// </summary>
    public ObservableCollection<string> ProjectSections { get; set; } = [];

    /// <summary>
    /// Выбранное пользователем имя параметра, по которому производится фильтрация (раздел проекта)
    /// </summary>
    public string SelectedProjectSectionParamName {
        get => _selectedProjectSectionParamName;
        set => RaiseAndSetIfChanged(ref _selectedProjectSectionParamName, value);
    }

    /// <summary>
    /// Выбранное пользователем значение раздела проекта
    /// </summary>
    public string SelectedProjectSection {
        get => _selectedProjectSection;
        set => RaiseAndSetIfChanged(ref _selectedProjectSection, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }



    /// <summary>
    /// (Вариант 1) Метод команды простой регенерации проекта
    /// </summary>
    /// <param name="p"></param>
    private void Regenerate(object p) {
        using(Transaction transaction = _revitRepository.Document.StartTransaction("Регенирация документа")) {
            _revitRepository.Document.Regenerate();
            transaction.Commit();
        }

        SaveConfig();
    }


    /// <summary>
    /// (Вариант 2) Метод команды перемещающий спецификации на листах выбранного комплекта чертежей (раздела проекта)
    /// </summary>
    /// <param name="p"></param>
    private void Shake(object p) {
        using(Transaction transaction = _revitRepository.Document.StartTransaction("Встряска спецификаций")) {
            foreach(ViewSheet viewSheet in _revitRepository.GetAllExistingSheets) {

                var paramValueTemp = viewSheet.GetParamValueOrDefault(SelectedProjectSectionParamName);
                if(paramValueTemp is null) { continue; }
                string paramValue = paramValueTemp.ToString();

                if(paramValue != SelectedProjectSection) {
                    continue;
                }

                var sheetUtils = new SheetUtils(_revitRepository.Document, viewSheet);
                sheetUtils.FindAndShakeSpecsOnSheet();
            }

            transaction.Commit();
        }

        SaveConfig();
    }
    private bool CanShake(object p) {
        if(ErrorText.Length > 0) {
            return false;
        }
        return SelectedProjectSection is not null;
    }



    /// <summary>
    /// Заполнение списка параметров, где может быть прописан раздел проекта
    /// </summary>
    private void GetFilterableParams() {
        var category = _revitRepository.Document.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);

        var filterableParametersIds = ParameterFilterUtilities.GetFilterableParametersInCommon(_revitRepository.Document, [category.Id]);

        foreach(var id in filterableParametersIds) {
            if(_revitRepository.Document.GetElement(id) is not ParameterElement param) { continue; }

            ProjectSectionParamNames.Add(param.Name);
        }

        ProjectSectionParamNames.Sort();
    }


    /// <summary>
    /// Получение возможных вариантов заполнения параметра раздела проекта
    /// </summary>
    /// <param name="p"></param>
    private void GetProjectSections(object p) {
        if(SelectedProjectSectionParamName is null) {
            return;
        }

        ProjectSections.Clear();
        ErrorText = string.Empty;
        string projectSection;
        foreach(var viewSheet in _revitRepository.GetAllExistingSheets) {
            var projectSectionTemp = viewSheet.GetParamValueOrDefault(SelectedProjectSectionParamName);
            if(projectSectionTemp is null) { continue; }
            projectSection = projectSectionTemp.ToString();

            // Заполнение списка разделов проекта
            if(!ProjectSections.Contains(projectSection)) {
                ProjectSections.Add(projectSection);
            }
        }
        // Сортировка
        ProjectSections = new ObservableCollection<string>(ProjectSections.OrderBy(i => i));
    }



    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        setting ??= _pluginConfig.AddSettings(_revitRepository.Document);

        setting.ProjectSectionParamName = SelectedProjectSectionParamName;
        setting.SelectedProjectSection = SelectedProjectSection;

        _pluginConfig.SaveProjectConfig();
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SelectedProjectSectionParamName = setting?.ProjectSectionParamName;
        SelectedProjectSection = setting?.SelectedProjectSection;
    }
}
