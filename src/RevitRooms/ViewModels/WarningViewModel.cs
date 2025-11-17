using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels;
/// <summary>
/// Класс для представления информации об ошибке или предупреждении.
/// Содержит описание ошибки и список элементов Revit с этой ошибкой.
/// </summary>
internal class WarningViewModel : BaseViewModel {
    public string Message { get; set; }
    public string Description { get; set; }

    public WarningTypeInfo TypeInfo { get; set; }
    public string TypeInfoText {
        get {
            return TypeInfo switch {
                WarningTypeInfo.Error => "Ошибка",
                WarningTypeInfo.Info => "Информация",
                WarningTypeInfo.Warning => "Предупреждение",
                _ => "Неизвестно",
            };
        }
    }

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

