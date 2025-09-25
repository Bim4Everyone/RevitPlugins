namespace RevitLintelPlacement.ViewModels;

internal class InfoElement {
    public static InfoElement LintelIsFixedWithoutElement =>
        new() {
            Message = "Под зафиксированной перемычкой отсутствует проем.",
            TypeInfo = TypeInfo.Warning,
            ElementType = ElementType.Lintel
        };

    public static InfoElement LintelGeometricalDisplaced =>
        new() {
            Message = "Перемычка смещена от проема.",
            TypeInfo = TypeInfo.Warning,
            ElementType = ElementType.Lintel
        };

    public static InfoElement MissingLintelParameter =>
        new() {
            Message = "У семейства перемычки отсутствует значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Lintel
        };

    public static InfoElement MissingOpeningParameter =>
        new() {
            Message = "У проема отсутствует значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Opening
        };

    public static InfoElement UnsetLintelParamter =>
        new() {
            Message = "У перемычки не удалось установить значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Opening
        };

    public static InfoElement BlankParamter =>
        new() {
            Message = "В настройках не установлено значение параметра \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.Config
        };

    public static InfoElement LackOfView =>
        new() {
            Message = "В проекте не содержится ни одного вида \"{0}\".",
            TypeInfo = TypeInfo.Error,
            ElementType = ElementType.View
        };

    public string Message { get; set; }
    public TypeInfo TypeInfo { get; set; }
    public ElementType ElementType { get; set; }

    public InfoElement FormatMessage(params string[] args) {
        return new InfoElement {
            TypeInfo = TypeInfo,
            Message = string.Format(Message, args),
            ElementType = ElementType
        };
    }
}
