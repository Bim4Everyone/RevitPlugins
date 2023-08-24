using Nuke.Common;

static class NukeBuildExtensions {
    public static T From<T>(this INukeBuild nukeBuild) where T : INukeBuild {
        return (T) (object) nukeBuild;
    }
}