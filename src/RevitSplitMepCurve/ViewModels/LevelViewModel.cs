using System;
using System.Collections.Generic;
using System.Globalization;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitSplitMepCurve.ViewModels;

internal class LevelViewModel : BaseViewModel, IEquatable<LevelViewModel> {
    private readonly ILocalizationService _localization;
    private bool _isSelected;

    /// <summary>
    /// Вью модель уровня
    /// </summary>
    /// <param name="level">Уровень</param>
    /// <param name="isSelected">Флаг для чекбокса выбора</param>
    /// <param name="basePointZFt">Отметка базовой точки в единицах ревита для отображения отметки уровня</param>
    /// <param name="localization">Сервис локализации</param>
    public LevelViewModel(
        Level level,
        bool isSelected,
        double basePointZFt,
        ILocalizationService localization) {
        Level = level ?? throw new ArgumentNullException(nameof(level));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _isSelected = isSelected;

        double elevationFt = level.Elevation - basePointZFt;
        double elevationMm = UnitUtils.ConvertFromInternalUnits(elevationFt, UnitTypeId.Millimeters);
        DisplayName = _localization.GetLocalizedString("MainWindow.Levels.DisplayFormat", level.Name, elevationMm);
    }

    public string Name => Level.Name;

    public string DisplayName { get; }

    public Level Level { get; }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool Equals(LevelViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Level.Id == other.Level.Id;
    }

    public override bool Equals(object obj) {
        return Equals(obj as LevelViewModel);
    }

    public override int GetHashCode() {
        return EqualityComparer<ElementId>.Default.GetHashCode(Level.Id);
    }
}
