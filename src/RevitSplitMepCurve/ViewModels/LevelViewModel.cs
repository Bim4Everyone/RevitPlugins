using System;
using System.Collections.Generic;
using System.Globalization;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitSplitMepCurve.ViewModels;

internal class LevelViewModel : BaseViewModel, IEquatable<LevelViewModel> {
    private bool _isSelected;

    public LevelViewModel(
        Level level,
        bool isSelected,
        double basePointZFt,
        ILocalizationService localization) {
        Level = level ?? throw new ArgumentNullException(nameof(level));
        if(localization is null) {
            throw new ArgumentNullException(nameof(localization));
        }
        _isSelected = isSelected;

        double elevationFt = level.Elevation - basePointZFt;
#if REVIT_2020_OR_LESS
        double elevationMm = UnitUtils.ConvertFromInternalUnits(elevationFt, DisplayUnitType.DUT_MILLIMETERS);
#else
        double elevationMm = UnitUtils.ConvertFromInternalUnits(elevationFt, UnitTypeId.Millimeters);
#endif
        var format = localization.GetLocalizedString("MainWindow.Levels.DisplayFormat");
        DisplayName = string.Format(CultureInfo.CurrentCulture, format, level.Name, elevationMm);
        ElevationTooltip = localization.GetLocalizedString("MainWindow.Levels.ElevationTooltip");
    }

    public string Name => Level.Name;

    public string DisplayName { get; }

    public string ElevationTooltip { get; }

    public Level Level { get; }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool Equals(LevelViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }
        return Level.Id == other.Level.Id;
    }

    public override bool Equals(object obj) => Equals(obj as LevelViewModel);

    public override int GetHashCode() =>
        EqualityComparer<ElementId>.Default.GetHashCode(Level.Id);
}
