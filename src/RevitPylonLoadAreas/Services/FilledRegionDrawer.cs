using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

internal sealed class FilledRegionDrawer {
    private readonly RevitRepository _repo;
    private readonly ILocalizationService _localization;
    private readonly SystemConfig _config;
    private readonly FilledRegionType _type;

    public FilledRegionDrawer(RevitRepository repo, ILocalizationService localization, SystemConfig config) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        _type = repo.GetFilledRegionTypeOrDefault(_config.FilledRegionTypeName)
                ?? throw new InvalidOperationException(
                    _localization.GetLocalizedString("Error.FilledRegionTypeNotFound"));
        _config.FilledRegionTypeName = _type.Name;
    }

    /// <summary>
    /// Строит цветовую область по заданной грузовой площади
    /// </summary>
    public FilledRegion Draw(LoadArea area) {
        return FilledRegion.Create(_repo.Document, _type.Id, _repo.ActiveView.Id, area.Circuits);
    }
}
