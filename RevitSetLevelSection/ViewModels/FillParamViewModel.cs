using dosymep.Bim4Everyone;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal abstract class FillParamViewModel : BaseViewModel {
        public abstract bool IsEnabled { get; set; }
        public abstract RevitParam RevitParam { get; set; }

        public abstract string GetErrorText(bool fromRevitParam);
        public abstract void UpdateElements(bool fromProjectParam);

        public abstract ParamSettings GetParamSettings();
        public abstract void SetParamSettings(ParamSettings paramSettings);
    }
}