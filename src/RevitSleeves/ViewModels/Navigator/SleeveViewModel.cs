using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Models.Navigator;
using RevitSleeves.Models.Placing;

namespace RevitSleeves.ViewModels.Navigator;
internal class SleeveViewModel : BaseViewModel, IEquatable<SleeveViewModel> {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly SleeveModel _sleeve;

    public SleeveViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        SleeveModel sleeve) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _sleeve = sleeve ?? throw new ArgumentNullException(nameof(sleeve));

        Status = _localizationService.GetLocalizedString($"{nameof(SleeveStatus)}.{_sleeve.Status}");
        StatusValue = _sleeve.Status;
        Diameter = _revitRepository.ConvertFromInternal(_sleeve.Diameter);
        Length = Math.Round(_revitRepository.ConvertFromInternal(_sleeve.Length), 1);
        Comment = _sleeve.GetFamilyInstance()
            .GetParamValueOrDefault(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, string.Empty);
        Id = _sleeve.Id.GetIdValue();
    }


    public string Status { get; }

    public SleeveStatus StatusValue { get; }

    public double Diameter { get; }

    public double Length { get; }

    public string Comment { get; }

    public long Id { get; }


    public bool Equals(SleeveViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }

        return _sleeve.Id == other._sleeve.Id;
    }

    public override int GetHashCode() {
        return -589532986 + EqualityComparer<ElementId>.Default.GetHashCode(_sleeve.Id);
    }

    public override bool Equals(object obj) {
        return Equals(obj as SleeveViewModel);
    }

    public SleeveModel GetSleeve() {
        return _sleeve;
    }
}
