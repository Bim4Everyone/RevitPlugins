using System;

using dosymep.Nuke.RevitVersions;

using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;

static class VersioningExtensions {
    public static DotNetBuildSettings SetSimpleVersion(this DotNetBuildSettings settings,
        GitVersion gitVersion,
        RevitVersion revitVersion) {
        return settings
            .SetVersion(InjectRevitVersion(revitVersion, gitVersion.AssemblySemVer))
            .SetFileVersion(InjectRevitVersion(revitVersion, gitVersion.AssemblySemFileVer))
            .SetInformationalVersion(InjectRevitVersion(revitVersion, gitVersion.InformationalVersion));
    }

    public static string InjectRevitVersion(RevitVersion revitVersion, string versionString) {
        int index = versionString.IndexOf('.');
        return revitVersion + versionString.Substring(index);
    }
}
