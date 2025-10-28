using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels;
internal class SheetFormatViewModel : BaseViewModel, IEquatable<SheetFormatViewModel> {
    public SheetFormatViewModel(SheetFormat sheetFormat) {
        SheetFormat = sheetFormat;
    }

    public SheetFormat SheetFormat { get; }

    public bool Equals(SheetFormatViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }
        return SheetFormat == other.SheetFormat;
    }

    public override int GetHashCode() {
        return 736150915 + SheetFormat.GetHashCode();
    }

    public override bool Equals(object obj) {
        return Equals(obj as SheetFormatViewModel);
    }

    public override string ToString() {
        return $"{SheetModel.SheetFormatSizeParam}{SheetFormat.SizeIndex}" +
            $"{SheetModel.SheetFormatMultiplyParam}{SheetFormat.MultiplyIndex}";
    }

    // ГОСТ 2.301
    public static IReadOnlyCollection<SheetFormatViewModel> GetStandardSheetFormats() {
        return new ReadOnlyCollection<SheetFormatViewModel>([
            new SheetFormatViewModel(new SheetFormat(0,1)),
            new SheetFormatViewModel(new SheetFormat(0,2)),
            new SheetFormatViewModel(new SheetFormat(0,3)),
            new SheetFormatViewModel(new SheetFormat(1,1)),
            new SheetFormatViewModel(new SheetFormat(1,3)),
            new SheetFormatViewModel(new SheetFormat(1,4)),
            new SheetFormatViewModel(new SheetFormat(2,1)),
            new SheetFormatViewModel(new SheetFormat(2,3)),
            new SheetFormatViewModel(new SheetFormat(2,4)),
            new SheetFormatViewModel(new SheetFormat(2,5)),
            new SheetFormatViewModel(new SheetFormat(3,1)),
            new SheetFormatViewModel(new SheetFormat(3,3)),
            new SheetFormatViewModel(new SheetFormat(3,4)),
            new SheetFormatViewModel(new SheetFormat(3,5)),
            new SheetFormatViewModel(new SheetFormat(3,6)),
            new SheetFormatViewModel(new SheetFormat(3,7)),
            new SheetFormatViewModel(new SheetFormat(4,1)),
            new SheetFormatViewModel(new SheetFormat(4,3)),
            new SheetFormatViewModel(new SheetFormat(4,4)),
            new SheetFormatViewModel(new SheetFormat(4,5)),
            new SheetFormatViewModel(new SheetFormat(4,6)),
            new SheetFormatViewModel(new SheetFormat(4,7)),
            new SheetFormatViewModel(new SheetFormat(4,8)),
            new SheetFormatViewModel(new SheetFormat(4,9)),
        ]);
    }
}
