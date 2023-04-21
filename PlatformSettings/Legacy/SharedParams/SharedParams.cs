using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace PlatformSettings.Legacy.SharedParams {
    internal class SharedParams : RevitParams {
        public override RevitParamsConfig GetConfig() {
            return SharedParamsConfig.Load(GetConfigPath());
        }
    }
}
