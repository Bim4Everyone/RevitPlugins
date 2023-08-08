using System.Collections.Generic;

using Nuke.Common;

interface IHazConfigurations : INukeBuild {
    IEnumerable<RevitConfiguration> DebugConfigurations => RevitConfiguration.GetDebugConfiguration();
    IEnumerable<RevitConfiguration> ReleaseConfigurations => RevitConfiguration.GetReleaseConfiguration();
}