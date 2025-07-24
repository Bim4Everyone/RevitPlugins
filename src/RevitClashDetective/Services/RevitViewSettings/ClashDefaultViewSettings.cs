using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class ClashDefaultViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ClashModel _clashModel;

    public ClashDefaultViewSettings(RevitRepository revitRepository, ClashModel clashModel) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _clashModel = clashModel ?? throw new ArgumentNullException(nameof(clashModel));
    }


    public void Apply(View3D view3D) {
        var bboxSettings = new BboxViewSettings(_revitRepository,
            [_clashModel.MainElement, _clashModel.OtherElement],
            10);
        var filterSettings = new EmptyFiltersViewSettings(_revitRepository);
        bboxSettings.Apply(view3D);
        filterSettings.Apply(view3D);
    }
}
