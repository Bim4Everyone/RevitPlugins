using System.ComponentModel;

namespace RevitDeclarations.Models {
    public enum ContourCheckEnum {
        [Description("Ошибка контура")]
        Error,
        [Description("Да (проверить)")]
        YesCheck,
        [Description("Да")]
        Yes,
        [Description("Нет (проверить)")]
        NoCheck,
        [Description("Нет")]
        No
    }
}
