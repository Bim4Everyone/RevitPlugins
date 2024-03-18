using System;

using dosymep.Nuke.RevitVersions;

using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;

static class VersioningExtensions {
    public static DotNetBuildSettings SetSimpleVersion(this DotNetBuildSettings settings,
        Build.BuildParams buildParams,
        RevitVersion revitVersion) {
        return settings
            .SetVersion(InjectRevitVersion(revitVersion, $"{revitVersion}.2.4"))
            .SetFileVersion(InjectRevitVersion(revitVersion, $"{revitVersion}.2.4"))
            .SetInformationalVersion(InjectRevitVersion(revitVersion, $"{revitVersion}.2.4" +
                                                                      $"{buildParams.BranchTag}+{buildParams.BranchCommitCount}" +
                                                                      $".Branch.{buildParams.BranchName}.Sha")); // Sha само добавляется (хз почему)
    }

    public static string InjectRevitVersion(RevitVersion revitVersion, string versionString) {
        int index = versionString.IndexOf('.');
        return revitVersion + versionString.Substring(index);
    }
}
