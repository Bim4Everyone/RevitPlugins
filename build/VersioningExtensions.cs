using System;

using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;

static class VersioningExtensions {
    public static DotNetBuildSettings SetSimpleVersion(this DotNetBuildSettings settings,
        GitVersion gitVersion,
        RevitConfiguration configuration) {
        return settings
            .SetAssemblyVersion(InjectRevitVersion(configuration, gitVersion.AssemblySemVer))
            .SetFileVersion(InjectRevitVersion(configuration, gitVersion.AssemblySemFileVer))
            .SetInformationalVersion(InjectRevitVersion(configuration, gitVersion.InformationalVersion));
    }

    public static string InjectRevitVersion(RevitConfiguration configuration, string versionString) {
        int index = versionString.IndexOf('.');
        return configuration.Version + versionString.Substring(index);
    }
}