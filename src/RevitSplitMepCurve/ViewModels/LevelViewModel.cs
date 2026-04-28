using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSplitMepCurve.ViewModels;

internal class LevelViewModel : BaseViewModel, IEquatable<LevelViewModel> {
    private bool _isSelected;

    public LevelViewModel(Level level, bool isSelected) {
        Level = level ?? throw new ArgumentNullException(nameof(level));
        _isSelected = isSelected;
    }

    public string Name => Level.Name;

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
