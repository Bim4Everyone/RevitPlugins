using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitFamilyParameterAdder.Models;

internal class SharedParam : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private string _paramName;
    private ExternalDefinition _paramInShPF;
    private string _paramGroupInShPF;
    private bool _isInstanceParam = true;
    private Dictionary<string, bool> _levelOfParam;
    private List<ParameterGroupHelper> _paramGroupsInFM;
    private ParameterGroupHelper _selectedParamGroupInFM;
    private bool _isDefaultParam = false;
    private string _formula = string.Empty;
    private bool _isSelected = false;

    public SharedParam(ExternalDefinition externalDefinition, List<ParameterGroupHelper> bINParameterGroups,
                       ILocalizationService localizationService) {
        _localizationService = localizationService;
        LevelOfParam = new Dictionary<string, bool>() {
            { _localizationService.GetLocalizedString("MainWindow.Instance"), true},
            { _localizationService.GetLocalizedString("MainWindow.Type"), false}
        };
        ParamName = externalDefinition.Name;
        ParamInShPF = externalDefinition;
        ParamGroupInShPF = externalDefinition.OwnerGroup.Name;
        ParamGroupsInFM = bINParameterGroups;
    }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public string ParamName {
        get => _paramName;
        set => RaiseAndSetIfChanged(ref _paramName, value);
    }

    /// <summary>
    /// Объект параметра в ФОП
    /// </summary>
    public ExternalDefinition ParamInShPF {
        get => _paramInShPF;
        set => RaiseAndSetIfChanged(ref _paramInShPF, value);
    }

    /// <summary>
    /// Группа параметров в ФОП
    /// </summary>
    public string ParamGroupInShPF {
        get => _paramGroupInShPF;
        set => RaiseAndSetIfChanged(ref _paramGroupInShPF, value);
    }

    /// <summary>
    /// Уровень размещения параметра - экземпляр/тип
    /// </summary>
    public bool IsInstanceParam {
        get => _isInstanceParam;
        set => RaiseAndSetIfChanged(ref _isInstanceParam, value);
    }

    public Dictionary<string, bool> LevelOfParam {
        get => _levelOfParam;
        set => RaiseAndSetIfChanged(ref _levelOfParam, value);
    }

    /// <summary>
    /// Список групп параметров в семействе
    /// </summary>
    public List<ParameterGroupHelper> ParamGroupsInFM {
        get => _paramGroupsInFM;
        set => RaiseAndSetIfChanged(ref _paramGroupsInFM, value);
    }

    /// <summary>
    /// Выбранная группа для группировки параметра в семействе
    /// </summary>
    public ParameterGroupHelper SelectedParamGroupInFM {
        get => _selectedParamGroupInFM;
        set => RaiseAndSetIfChanged(ref _selectedParamGroupInFM, value);
    }

    /// <summary>
    /// Указывает является ли текущий параметр дефолтным
    /// </summary>
    public bool IsDefaultParam {
        get => _isDefaultParam;
        set => RaiseAndSetIfChanged(ref _isDefaultParam, value);
    }

    /// <summary>
    /// Строка формулы, которую нужно попытаться вставить при добавлении параметра в семейство
    /// </summary>
    public string Formula {
        get => _formula;
        set => RaiseAndSetIfChanged(ref _formula, value);
    }
}
