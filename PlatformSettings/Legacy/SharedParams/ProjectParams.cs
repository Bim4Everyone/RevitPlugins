using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

namespace PlatformSettings.Legacy.SharedParams {
    internal class ProjectParams : RevitParams {
        public override RevitParamsConfig GetConfig() {
            return ProjectParamsConfig.Load(GetConfigPath());
        }
    }
}
