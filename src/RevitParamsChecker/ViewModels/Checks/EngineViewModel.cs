using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Services;

namespace RevitParamsChecker.ViewModels.Checks;

internal class EngineViewModel : BaseViewModel {
    public EngineViewModel(ChecksEngine engine, ILocalizationService localization) {
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        if(localization is null) {
            throw new ArgumentNullException(nameof(localization));
        }

        Name = localization.GetLocalizedString($"{nameof(CheckTargetType)}.{engine.TargetType}");
    }

    public ChecksEngine Engine { get; }

    public string Name { get; }

    public CheckTargetType TargetType => Engine.TargetType;
}
