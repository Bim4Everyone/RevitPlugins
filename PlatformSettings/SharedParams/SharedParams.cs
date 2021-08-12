using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace PlatformSettings.SharedParams {
    internal class SharedParams : RevitParams {
        public override RevitParamsConfig GetConfig() {
            return SharedParamsConfig.Load(GetConfigPath());
        }
    }
}
