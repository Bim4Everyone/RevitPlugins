using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

namespace PlatformSettings.SharedParams {
    internal class ProjectParams : RevitParams {
        public override RevitParamsConfig GetConfig() {
            return ProjectParamsConfig.Load(GetConfigPath());
        }
    }
}
