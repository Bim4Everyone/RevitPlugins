using Newtonsoft.Json.Linq;

static class JTokenExtensions {
    public static string GetExtensionUrl(this JToken token) {
        return token.Value<string>("url");
    }

    public static string GetExtensionName(this JToken token) {
        return token.Value<string>("name");
    }

    public static string GetExtensionType(this JToken token) {
        return token.Value<string>("type");
    }

    public static bool IsLib(this JToken token) {
        return token.GetExtensionType() == "lib";
    }

    public static string GetExtensionDirName(this JToken token) {
        return token.GetExtensionName() + '.' + token.GetExtensionType();
    }
}
