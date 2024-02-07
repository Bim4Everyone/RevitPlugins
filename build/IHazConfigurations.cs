using System.Collections.Generic;
using System.Linq;

using Nuke.Common;

interface IHazConfigurations : INukeBuild {
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration => TryGetValue(() => Configuration)
                                   ?? (IsLocalBuild ? Configuration.Debug : Configuration.Release);
}
