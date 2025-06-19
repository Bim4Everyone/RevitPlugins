using System;
using System.Collections.Generic;

namespace RevitRoughFinishingDesign.ViewModels;
internal class PairViewModel : IEquatable<PairViewModel> {

    public PairViewModel(WallTypeViewModel wallTypeViewModel, LineStyleViewModel lineStyleViewModel) {
        WallTypeVM = wallTypeViewModel ?? throw new ArgumentNullException(nameof(wallTypeViewModel));
        LineStyleVM = lineStyleViewModel ?? throw new ArgumentNullException(nameof(lineStyleViewModel));
    }
    public WallTypeViewModel WallTypeVM { get; set; }
    public LineStyleViewModel LineStyleVM { get; set; }

    public override bool Equals(object obj) {
        return Equals(obj as PairViewModel);
    }

    public bool Equals(PairViewModel other) {
        if(other is null) { return false; }
        ;
        if(ReferenceEquals(this, other)) { return true; }
        ;
        return WallTypeVM?.Equals(other.WallTypeVM) ?? false;
    }

    public override int GetHashCode() {
        return -1816055660 + EqualityComparer<WallTypeViewModel>.Default.GetHashCode(WallTypeVM);
    }
}
