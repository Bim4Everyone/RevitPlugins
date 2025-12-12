using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;

using Wpf.Ui.Controls;

namespace RevitRooms.ViewModels;
/// <summary>
/// Класс для представления информации об ошибке или предупреждении.
/// Содержит описание ошибки и список элементов Revit с этой ошибкой.
/// </summary>
internal class WarningViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;

    public WarningViewModel(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public string Message { get; set; }
    public string Description { get; set; }

    public WarningTypeInfo TypeInfo { get; set; }
    public string TypeInfoText {
        get {
            return TypeInfo switch {
                WarningTypeInfo.Error => _localizationService.GetLocalizedString("WarningType.Error"),
                WarningTypeInfo.Info => _localizationService.GetLocalizedString("WarningType.Info"),
                WarningTypeInfo.Warning => _localizationService.GetLocalizedString("WarningType.Warning"),
                _ => _localizationService.GetLocalizedString("WarningType.Unknown"),
            };
        }
    }
    public string FullMessage => $"{TypeInfoText}. {Message}";
    public ObservableCollection<WarningElementViewModel> Elements { get; set; }
}

/// <summary>
/// Класс для представления элемента Revit с ошибкой.
/// Содержит информацию об этом элементе.
/// </summary>
internal class WarningElementViewModel {
    public string Description { get; set; }
    public IElementViewModel<Element> Element { get; set; }

    public ElementId ElementId => Element.ElementId;

    public string Name => Element.Name;
    public string PhaseName => Element.PhaseName;
    public string LevelName => Element.LevelName;
    public string CategoryName => Element.CategoryName;

    public ICommand ShowElementCommand => Element.ShowElementCommand;
    public ICommand SelectElementCommand => Element.SelectElementCommand;
}

