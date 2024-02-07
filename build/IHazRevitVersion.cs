using System.Collections.Generic;
using System.Linq;

using dosymep.Nuke.RevitVersions;

using Nuke.Common;

interface IHazRevitVersion : INukeBuild {
    /// <summary>
    /// Build versions.
    /// </summary>
    IEnumerable<RevitVersion> BuildRevitVersions { get; set; }
    
    /// <summary>
    /// Min Revit version.
    /// </summary>
    [Parameter("Min Revit version.")]
    RevitVersion MinVersion => TryGetValue(() => MinVersion) ?? RevitVersion.Rv2022;

    /// <summary>
    /// Max Revit version.
    /// </summary>
    [Parameter("Max Revit version.")]
    RevitVersion MaxVersion => TryGetValue(() => MaxVersion) ?? RevitVersion.Rv2024;

    /// <summary>
    /// Build versions.
    /// </summary>
    [Parameter("Build Revit versions.")]
    RevitVersion[] RevitVersions => TryGetValue(() => RevitVersions) ?? new RevitVersion[0];
}
