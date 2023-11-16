using System.Collections.Generic;
using System.Linq;

using Nuke.Common;

interface IHazConfigurations : INukeBuild {
    int MinReleaseVersion => ReleaseConfigurations.Min(item => item.Version);
    int MaxReleaseVersion => ReleaseConfigurations.Max(item => item.Version);
    
    IEnumerable<RevitConfiguration> DebugConfigurations => RevitConfiguration.GetDebugConfiguration();
    IEnumerable<RevitConfiguration> ReleaseConfigurations => RevitConfiguration.GetReleaseConfiguration();
}