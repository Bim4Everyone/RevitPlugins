namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class PlaceholderReplacer {
    public string Replace(string template, object source) {
        if(string.IsNullOrEmpty(template) || source == null) {
            return template ?? string.Empty;
        }

        string result = template;
        foreach(var property in source.GetType().GetProperties()) {
            if(!property.CanRead) {
                continue;
            }

            string placeholder = "{" + property.Name + "}";
            if(!result.Contains(placeholder)) {
                continue;
            }

            object value = property.GetValue(source);
            result = result.Replace(placeholder, value?.ToString() ?? string.Empty);
        }

        return result;
    }
}

